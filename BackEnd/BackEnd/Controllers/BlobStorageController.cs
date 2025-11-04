using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Models.InputModels;
using BackEnd.Models.OutputModels;
using BackEnd.Models.ResponseModel;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
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
        private const string FOLDER_PLACEHOLDER_FILE = ".folder_placeholder";
        
        public BlobStorageController(IStorageServices storageServices, IUnitOfWork unitOfWork) 
        {
            _storageServices = storageServices;
            _unitOfWork = unitOfWork;
        }

        private async Task<(string userId, string? agencyId)> GetCurrentUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Utente non autenticato");

            var user = await _unitOfWork.dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return (userId, user?.AdminId ?? userId);
        }

        [HttpPost]
        [Route(nameof(CreateFolder))]
        [Authorize]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderModel request)
        {
            try
            {
                var (userId, agencyId) = await GetCurrentUserInfo();
                
                // Costruisci il path della cartella
                string folderPath = string.IsNullOrEmpty(request.ParentPath) 
                    ? $"{agencyId}/{request.FolderName}"
                    : $"{request.ParentPath}/{request.FolderName}";

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
                    AgencyId = agencyId,
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

                var (userId, agencyId) = await GetCurrentUserInfo();

                // Costruisci il path del file
                string filePath = string.IsNullOrEmpty(request.ParentPath)
                    ? $"{agencyId}/{request.File.FileName.Replace(" ", "-")}"
                    : $"{request.ParentPath}/{request.File.FileName.Replace(" ", "-")}";

                Stream stream = request.File.OpenReadStream();
                string fileUrl = await _storageServices.UploadFile(stream, filePath);

                Documentation document = new Documentation()
                {
                    FileName = filePath,
                    FileUrl = fileUrl,
                    DisplayName = request.File.FileName,
                    IsFolder = false,
                    IsPrivate = request.IsPrivate,
                    ParentPath = request.ParentPath,
                    AgencyId = agencyId,
                    UserId = request.IsPrivate ? userId : null,
                    CreationDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };

                await _unitOfWork.dbContext.Documentation.AddAsync(document);
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
                var (userId, agencyId) = await GetCurrentUserInfo();

                // Filtra i documenti per agencyId e parentPath
                var query = _unitOfWork.dbContext.Documentation
                    .Where(d => d.AgencyId == agencyId);

                // Filtra per parentPath
                if (string.IsNullOrEmpty(parentPath))
                {
                    query = query.Where(d => string.IsNullOrEmpty(d.ParentPath) || d.ParentPath == "");
                }
                else
                {
                    query = query.Where(d => d.ParentPath == parentPath);
                }

                var documents = await query.ToListAsync();

                // Filtra i file privati: mostra solo quelli dell'utente corrente
                var filteredDocs = documents
                    .Where(d => !d.IsPrivate || d.UserId == userId)
                    .Select(document => new DocumentationSelectModel
                    {
                        Id = document.Id,
                        FileName = document.FileName,
                        FileUrl = document.FileUrl,
                        DisplayName = document.DisplayName ?? GetDisplayNameFromPath(document.FileName),
                        IsFolder = document.IsFolder,
                        IsPrivate = document.IsPrivate,
                        ParentPath = document.ParentPath,
                        AgencyId = document.AgencyId,
                        UserId = document.UserId,
                        CreationDate = document.CreationDate
                    })
                    .OrderByDescending(d => d.IsFolder) // Cartelle prima
                    .ThenBy(d => d.DisplayName)
                    .ToList();

                return Ok(filteredDocs);
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
                var (userId, agencyId) = await GetCurrentUserInfo();
                
                Documentation document = await _unitOfWork.dbContext.Documentation
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (document == null)
                    return NotFound("Documento non trovato");

                // Verifica che l'utente possa eliminare il documento
                if (document.AgencyId != agencyId)
                    return Forbid("Non hai i permessi per eliminare questo documento");

                if (document.IsPrivate && document.UserId != userId)
                    return Forbid("Non hai i permessi per eliminare questo documento privato");

                // Se è una cartella, elimina ricorsivamente tutti i file al suo interno
                if (document.IsFolder)
                {
                    string folderPath = document.ParentPath != null 
                        ? $"{document.ParentPath}/{document.DisplayName}"
                        : $"{agencyId}/{document.DisplayName}";
                    
                    var childDocuments = await _unitOfWork.dbContext.Documentation
                        .Where(d => d.ParentPath != null && d.ParentPath.StartsWith(folderPath))
                        .ToListAsync();

                    foreach (var child in childDocuments)
                    {
                        await _storageServices.DeleteFile(child.FileName);
                        _unitOfWork.dbContext.Documentation.Remove(child);
                    }
                }

                // Elimina il file dal blob storage
                await _storageServices.DeleteFile(document.FileName);

                _unitOfWork.dbContext.Documentation.Remove(document);
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
    }
}
