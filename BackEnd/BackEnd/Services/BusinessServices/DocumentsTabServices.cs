using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.DocumentsTabModels;
using BackEnd.Models.Options;
using BackEnd.Models.OutputModels;
using BackEnd.Models.DocumentsTabModelModels;
using BackEnd.Models.CustomerModels;

namespace BackEnd.Services.BusinessServices
{
    public class DocumentsTabServices : IDocumentsTabServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentsTabServices> _logger;
        public DocumentsTabServices(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DocumentsTabServices> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;

        }
        public async Task<DocumentsTabSelectModel> Create(DocumentsTabCreateModel dto)
        {
            try
            {
                var entityClass = _mapper.Map<DocumentsTab>(dto);
                await _unitOfWork.DocumentsTabRepository.InsertAsync(entityClass);
                _unitOfWork.Save();

                DocumentsTabSelectModel response = new DocumentsTabSelectModel();
                _mapper.Map(entityClass, response);

                _logger.LogInformation(nameof(Create));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore in fase creazione");
            }
        }

        public async Task<DocumentsTab> Delete(int id)
        {
            try
            {
                IQueryable<DocumentsTab> query = _unitOfWork.dbContext.DocumentsTabs;

                if (id == 0)
                    throw new NullReferenceException("L'id non può essere 0");

                query = query.Where(x => x.Id == id);

                DocumentsTab EntityClasses = await query.FirstOrDefaultAsync();

                if (EntityClasses == null)
                    throw new NullReferenceException("Record non trovato!");

                _unitOfWork.DocumentsTabRepository.Delete(EntityClasses);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation(nameof(Delete));

                return EntityClasses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'eliminazione del documento con ID {id}: {ex.Message}");
                
                // Gestione specifica per DbUpdateException (errori database)
                if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                {
                    if (dbEx.InnerException != null && 
                        (dbEx.InnerException.Message.Contains("DELETE statement conflicted") || 
                         dbEx.InnerException.Message.Contains("REFERENCE constraint")))
                    {
                        throw new Exception("Impossibile eliminare il documento perché è utilizzato come chiave esterna in un'altra tabella.");
                    }
                }

                // Gestione per InnerException (per compatibilità con codice esistente)
                if (ex.InnerException != null && 
                    ex.InnerException.Message.Contains("DELETE statement conflicted with the REFERENCE constraint"))
                {
                    throw new Exception("Impossibile eliminare il documento perché è utilizzato come chiave esterna in un'altra tabella.");
                }

                // Gestione NullReferenceException
                if (ex is NullReferenceException)
                {
                    throw new Exception(ex.Message);
                }

                // Errore generico
                throw new Exception("Si è verificato un errore in fase di eliminazione. Riprova più tardi.");
            }
        }

        public async Task<DocumentsTabSelectModel> GetById(int id)
        {
            try
            {
                if (id is not > 0)
                    throw new Exception("Si è verificato un errore!");

                var query = await _unitOfWork.dbContext.Customers
                    .Include(x => x.CustomerNotes)
                    .FirstOrDefaultAsync(x => x.Id == id);

                DocumentsTabSelectModel result = _mapper.Map<DocumentsTabSelectModel>(query);

                _logger.LogInformation(nameof(GetById));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<DocumentsTabSelectModel> Update(DocumentsTabUpdateModel dto)
        {
            try
            {
                var EntityClass =
                    await _unitOfWork.DocumentsTabRepository.FirstOrDefaultAsync(q => q.Where(x => x.Id == dto.Id));

                if (EntityClass == null)
                    throw new NullReferenceException("Record non trovato!");

                EntityClass = _mapper.Map(dto, EntityClass);

                _unitOfWork.DocumentsTabRepository.Update(EntityClass);
                await _unitOfWork.SaveAsync();

                DocumentsTabSelectModel response = new DocumentsTabSelectModel();
                _mapper.Map(EntityClass, response);

                _logger.LogInformation(nameof(Update));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                if (ex is NullReferenceException)
                {
                    throw new Exception(ex.Message);
                }
                else
                {
                    throw new Exception("Si è verificato un errore in fase di modifica");
                }
            }
        }
    }
}
