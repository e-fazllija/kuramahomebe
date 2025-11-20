using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.InputModels;
using BackEnd.Models.OutputModels;
using BackEnd.Models.ResponseModel;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BackEnd.Controllers
{
    [Authorize(Policy = "ActiveSubscription")]
    [ApiController]
    [Route("/api/[controller]/")]
    [Authorize]
    public class BlobStorageController : ControllerBase
    {
        private readonly IStorageServices _storageServices;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AccessControlService _accessControl;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISubscriptionLimitService _subscriptionLimitService;
        private const string FOLDER_PLACEHOLDER_FILE = ".folder_placeholder";
        
        public BlobStorageController(
            IStorageServices storageServices, 
            IUnitOfWork unitOfWork,
            AccessControlService accessControl,
            UserManager<ApplicationUser> userManager,
            ISubscriptionLimitService subscriptionLimitService) 
        {
            _storageServices = storageServices;
            _unitOfWork = unitOfWork;
            _accessControl = accessControl;
            _userManager = userManager;
            _subscriptionLimitService = subscriptionLimitService;
        }

        /// <summary>
        /// Ottiene le informazioni dell'utente corrente dal token JWT
        /// </summary>
        private async Task<(string userId, string adminId, ApplicationUser user, IList<string> roles)> GetCurrentUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Utente non autenticato");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("Utente non trovato");

            var roles = await _userManager.GetRolesAsync(user);
            var adminId = user.AdminId ?? userId;

            return (userId, adminId, user, roles);
        }

        [HttpPost]
        [Route(nameof(CreateFolder))]
        [Authorize]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderModel request)
        {
            try
            {
                var (userId, adminId, user, roles) = await GetCurrentUserInfo();
                
                // Costruisci il path della cartella usando l'ID utente come root
                // Struttura: {userId}/{subfolder}/{subfolder2}/...
                string folderPath = string.IsNullOrEmpty(request.ParentPath) 
                    ? $"{userId}/{request.FolderName}"
                    : $"{request.ParentPath}/{request.FolderName}";

                // Verifica che il ParentPath appartenga all'utente corrente o alla sua cerchia
                if (!string.IsNullOrEmpty(request.ParentPath))
                {
                    var parentUserId = request.ParentPath.Split('/')[0];
                    var canAccess = await _accessControl.CanAccessEntity(userId, parentUserId);
                    if (!canAccess && parentUserId != userId)
                    {
                        return StatusCode(403, new AuthResponseModel() 
                        { 
                            Status = "Error", 
                            Message = "Non hai i permessi per creare cartelle in questo path" 
                        });
                    }
                }

                // Crea un file placeholder per rappresentare la cartella nel blob storage
                string placeholderFileName = $"{folderPath}/{FOLDER_PLACEHOLDER_FILE}";
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("folder")))
                {
                    await _storageServices.UploadFile(stream, placeholderFileName);
                }

                // Salva la cartella nel database
                Documentation folder = new Documentation()
                {
                    FileName = placeholderFileName,
                    FileUrl = "", // Le cartelle non hanno URL visualizzabile
                    DisplayName = request.FolderName,
                    IsFolder = true,
                    IsPrivate = false,
                    ParentPath = request.ParentPath,
                    AgencyId = adminId,
                    UserId = userId,
                    CreationDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };

                await _unitOfWork.dbContext.Documentation.AddAsync(folder);
                _unitOfWork.Save();
                
                return Ok(new DocumentationSelectModel
                {
                    Id = folder.Id,
                    FileName = folder.FileName,
                    FileUrl = folder.FileUrl,
                    DisplayName = folder.DisplayName,
                    IsFolder = folder.IsFolder,
                    IsPrivate = folder.IsPrivate,
                    ParentPath = folder.ParentPath,
                    AgencyId = folder.AgencyId,
                    UserId = folder.UserId,
                    CreationDate = folder.CreationDate
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(InsertDocument))]
        [Authorize]
        public async Task<IActionResult> InsertDocument([FromForm] SendFileModel request)
        {
            try
            {
                if (request.File == null)
                    return BadRequest("Nessun file selezionato");

                var (userId, adminId, user, _) = await GetCurrentUserInfo();

                // Verifica limite storage prima dell'upload
                long fileSizeBytes = request.File.Length;
                
                // Ottieni l'admin per verificare e aggiornare lo storage in un'unica query
                var admin = userId == adminId ? user : await _userManager.FindByIdAsync(adminId);
                if (admin == null)
                    return StatusCode(500, new AuthResponseModel() { Status = "Error", Message = "Admin non trovato" });

                // Verifica limite storage prima del caricamento
                var storageLimitCheck = await _subscriptionLimitService.CheckFeatureLimitAsync(userId, "storage_limit", adminId);
                
                // Se il limite è già stato raggiunto, blocca immediatamente
                if (storageLimitCheck.LimitReached)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, 
                        new AuthResponseModel() 
                        { 
                            Status = "Error", 
                            Message = storageLimitCheck.Message ?? "Limite storage raggiunto. Elimina alcuni file prima di caricarne altri."
                        });
                }
                
                // Se c'è un limite, verifica che il nuovo file non lo supererebbe
                if (storageLimitCheck.Limit != null && 
                    int.TryParse(storageLimitCheck.Limit, out int limitGB) && 
                    limitGB > 0)
                {
                    long limitBytes = (long)limitGB * 1024L * 1024L * 1024L;
                    long currentStorageBytes = admin.StorageUsedBytes;
                    long futureStorageBytes = currentStorageBytes + fileSizeBytes;
                    
                    // Se dopo il caricamento supererebbe il limite, blocca
                    if (futureStorageBytes > limitBytes)
                    {
                        double currentGB = currentStorageBytes / (1024.0 * 1024.0 * 1024.0);
                        double fileGB = fileSizeBytes / (1024.0 * 1024.0 * 1024.0);
                        double futureGB = futureStorageBytes / (1024.0 * 1024.0 * 1024.0);
                        
                        return StatusCode(StatusCodes.Status500InternalServerError, 
                            new AuthResponseModel() 
                            { 
                                Status = "Error", 
                                Message = $"Limite storage raggiunto. Utilizzo attuale: {currentGB:F2} GB su {limitGB} GB. " +
                                         $"Il file da caricare ({fileGB:F2} GB) porterebbe il totale a {futureGB:F2} GB, superando il limite. " +
                                         $"Elimina alcuni file o aggiorna il piano per aumentare lo storage disponibile."
                            });
                    }
                }

                // Costruisci il path del file
                string sanitizedFileName = request.File.FileName.Replace(" ", "-");
                string filePath;
                
                if (string.IsNullOrEmpty(request.ParentPath))
                {
                    // Root dell'utente
                    filePath = request.IsPrivate 
                        ? $"{userId}/private/{sanitizedFileName}"
                        : $"{userId}/{sanitizedFileName}";
                }
                else
                {
                    // Verifica permessi sul ParentPath
                    var parentUserId = request.ParentPath.Split('/')[0];
                    if (parentUserId != userId)
                    {
                        var canAccess = await _accessControl.CanAccessEntity(userId, parentUserId);
                        if (!canAccess)
                        {
                            return StatusCode(403, new AuthResponseModel() 
                            { 
                                Status = "Error", 
                                Message = "Non hai i permessi per caricare file in questo path" 
                            });
                        }
                    }
                    
                    filePath = request.IsPrivate
                        ? $"{request.ParentPath}/private/{sanitizedFileName}"
                        : $"{request.ParentPath}/{sanitizedFileName}";
                }

                // Upload del file
                Stream stream = request.File.OpenReadStream();
                string fileUrl = await _storageServices.UploadFile(stream, filePath);

                // Crea il documento nel database
                Documentation document = new Documentation()
                {
                    FileName = filePath,
                    FileUrl = fileUrl,
                    DisplayName = request.File.FileName,
                    IsFolder = false,
                    IsPrivate = request.IsPrivate,
                    ParentPath = request.ParentPath,
                    AgencyId = adminId,
                    UserId = userId,
                    FileSizeBytes = fileSizeBytes,
                    CreationDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };

                await _unitOfWork.dbContext.Documentation.AddAsync(document);
                
                // Aggiorna lo storage dell'admin (già caricato in memoria)
                admin.StorageUsedBytes = Math.Max(0, admin.StorageUsedBytes + fileSizeBytes);
                admin.UpdateDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(admin);
                
                _unitOfWork.Save();
                
                return Ok(new DocumentationSelectModel
                {
                    Id = document.Id,
                    FileName = document.FileName,
                    FileUrl = document.FileUrl,
                    DisplayName = document.DisplayName,
                    IsFolder = document.IsFolder,
                    IsPrivate = document.IsPrivate,
                    ParentPath = document.ParentPath,
                    AgencyId = document.AgencyId,
                    UserId = document.UserId,
                    CreationDate = document.CreationDate
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetDocuments))]
        [Authorize]
        public async Task<IActionResult> GetDocuments([FromQuery] string? parentPath = null)
        {
            try
            {
                var (userId, adminId, user, roles) = await GetCurrentUserInfo();

                // Ottieni tutti gli utenti nella cerchia (modello collaborativo)
                var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);

                // Query base: documenti degli utenti nella cerchia
                var query = _unitOfWork.dbContext.Documentation
                    .Where(d => circleUserIds.Contains(d.UserId ?? ""));

                // Filtra per parentPath
                if (string.IsNullOrEmpty(parentPath))
                {
                    // Root: mostra solo le root degli utenti nella cerchia
                    query = query.Where(d => string.IsNullOrEmpty(d.ParentPath) || d.ParentPath == "");
                }
                else
                {
                    // Path specifico: verifica permessi
                    var pathUserId = parentPath.Split('/')[0];
                    var canAccess = await _accessControl.CanAccessEntity(userId, pathUserId);
                    
                    if (!canAccess && pathUserId != userId)
                    {
                        return StatusCode(403, new AuthResponseModel() 
                        { 
                            Status = "Error", 
                            Message = "Non hai i permessi per accedere a questo path" 
                        });
                    }
                    
                    query = query.Where(d => d.ParentPath == parentPath);
                }

                var documents = await query.ToListAsync();

                // Filtra i file privati basandosi sulla logica superiore:
                // - Owner: vede tutto
                // - Superiore (Admin/Agency): vede anche i file privati dei sottoposti
                // - Colleghi: NON vedono i file privati
                var filteredDocs = new List<DocumentationSelectModel>();
                
                foreach (var document in documents)
                {
                    bool canView = true;
                    bool isOwner = document.UserId == userId;
                    bool isSuperior = await _accessControl.IsSuperiorOf(userId, document.UserId ?? "");
                    
                    if (document.IsPrivate)
                    {
                        // File privato: verifica se può vederlo
                        canView = isOwner || isSuperior;
                    }
                    
                    if (canView)
                    {
                        // Ottieni informazioni sul creatore
                        string creatorName = "Sconosciuto";
                        if (!string.IsNullOrEmpty(document.UserId))
                        {
                            var creator = await _userManager.FindByIdAsync(document.UserId);
                            if (creator != null)
                            {
                                creatorName = $"{creator.FirstName} {creator.LastName}";
                                if (isOwner) creatorName += " (Tu)";
                            }
                        }
                        
                        // Verifica permessi di modifica
                        bool canModify = await _accessControl.CanModifyEntity(userId, document.UserId ?? "");
                        
                        // Genera URL con SAS token per file (non per cartelle)
                        string fileUrl = document.FileUrl;
                        if (!document.IsFolder && !string.IsNullOrEmpty(document.FileName))
                        {
                            try
                            {
                                fileUrl = _storageServices.GenerateSasUrl(document.FileName, expirationMinutes: 60);
                            }
                            catch (Exception ex)
                            {
                                // In caso di errore, usa l'URL originale (fallback)
                                fileUrl = document.FileUrl;
                            }
                        }
                        
                        filteredDocs.Add(new DocumentationSelectModel
                        {
                            Id = document.Id,
                            FileName = document.FileName,
                            FileUrl = fileUrl, // URL con SAS token per accesso temporaneo
                            DisplayName = document.DisplayName ?? GetDisplayNameFromPath(document.FileName),
                            IsFolder = document.IsFolder,
                            IsPrivate = document.IsPrivate,
                            ParentPath = document.ParentPath,
                            AgencyId = document.AgencyId,
                            UserId = document.UserId,
                            CreationDate = document.CreationDate,
                            IsOwner = isOwner,
                            CanModify = canModify,
                            CreatorName = creatorName
                        });
                    }
                }

                var result = filteredDocs
                    .OrderByDescending(d => d.IsFolder) // Cartelle prima
                    .ThenBy(d => d.DisplayName)
                    .ToList();

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpDelete]
        [Route(nameof(DeleteModule))]
        [Authorize]
        public async Task<IActionResult> DeleteModule(int id)
        {
            try
            {
                var (userId, adminId, user, roles) = await GetCurrentUserInfo();
                
                Documentation document = await _unitOfWork.dbContext.Documentation
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (document == null)
                    return NotFound("Documento non trovato");

                // Verifica permessi di eliminazione usando AccessControlService
                // - Admin: può eliminare tutto nella sua cerchia
                // - Agency: può eliminare propri + Agent
                // - Agent: solo propri
                bool canDelete = await _accessControl.CanModifyEntity(userId, document.UserId ?? "");
                
                if (!canDelete)
                {
                    return StatusCode(403, new AuthResponseModel() 
                    { 
                        Status = "Error", 
                        Message = "Non hai i permessi per eliminare questo documento" 
                    });
                }

                // Verifica aggiuntiva per file privati:
                // Solo owner e superiori possono eliminarli
                if (document.IsPrivate)
                {
                    bool isOwner = document.UserId == userId;
                    bool isSuperior = await _accessControl.IsSuperiorOf(userId, document.UserId ?? "");
                    
                    if (!isOwner && !isSuperior)
                    {
                        return StatusCode(403, new AuthResponseModel() 
                        { 
                            Status = "Error", 
                            Message = "Non hai i permessi per eliminare questo documento privato" 
                        });
                    }
                }

                // Se è una cartella, elimina ricorsivamente tutti i file al suo interno
                if (document.IsFolder)
                {
                    string folderPath = document.ParentPath != null 
                        ? $"{document.ParentPath}/{document.DisplayName}"
                        : $"{document.UserId}/{document.DisplayName}";
                    
                    var childDocuments = await _unitOfWork.dbContext.Documentation
                        .Where(d => d.ParentPath != null && d.ParentPath.StartsWith(folderPath))
                        .ToListAsync();

                    foreach (var child in childDocuments)
                    {
                        // Verifica permessi anche per i file figli
                        bool canDeleteChild = await _accessControl.CanModifyEntity(userId, child.UserId ?? "");
                        if (!canDeleteChild)
                        {
                            return StatusCode(403, new AuthResponseModel() 
                            { 
                                Status = "Error", 
                                Message = $"Non hai i permessi per eliminare il file: {child.DisplayName}" 
                            });
                        }
                        
                        await _storageServices.DeleteFile(child.FileName);
                        _unitOfWork.dbContext.Documentation.Remove(child);
                    }
                }

                // Recupera la dimensione del file prima di eliminarlo (se salvata)
                long fileSizeBytes = 0;
                if (document.FileSizeBytes.HasValue)
                {
                    fileSizeBytes = document.FileSizeBytes.Value;
                }
                else
                {
                    // Se non abbiamo la dimensione salvata, proviamo a recuperarla da Azure Blob Storage
                    // Nota: Questo richiede un metodo aggiuntivo nell'interfaccia IStorageServices
                    // Per ora, assumiamo 0 se non disponibile (non ideale ma funzionale)
                }

                // Elimina il file dal blob storage
                await _storageServices.DeleteFile(document.FileName);

                _unitOfWork.dbContext.Documentation.Remove(document);
                
                // Decrementa lo storage utilizzato dell'Admin root
                if (fileSizeBytes > 0)
                {
                    await UpdateAdminStorageAsync(adminId, -fileSizeBytes);
                }
                
                await _unitOfWork.SaveAsync();
                
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        private string GetDisplayNameFromPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "";

            var parts = fileName.Split('/');
            return parts.Length > 0 ? parts[^1] : fileName;
        }

        /// <summary>
        /// Aggiorna lo storage utilizzato dell'Admin root
        /// </summary>
        /// <param name="adminId">ID dell'Admin root</param>
        /// <param name="bytesToAdd">Bytes da aggiungere (negativo per decrementare)</param>
        private async Task UpdateAdminStorageAsync(string adminId, long bytesToAdd)
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin != null)
            {
                admin.StorageUsedBytes = Math.Max(0, admin.StorageUsedBytes + bytesToAdd);
                admin.UpdateDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(admin);
            }
        }
    }
}
