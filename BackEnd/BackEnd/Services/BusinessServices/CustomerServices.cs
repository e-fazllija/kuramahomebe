using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.CustomerModels;
using BackEnd.Models.Options;
using BackEnd.Models.OutputModels;

namespace BackEnd.Services.BusinessServices
{
    public class CustomerServices : ICustomerServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerServices> _logger;
        private readonly IOptionsMonitor<PaginationOptions> options;
        private readonly AccessControlService _accessControl;
        
        public CustomerServices(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CustomerServices> logger, IOptionsMonitor<PaginationOptions> options, AccessControlService accessControl)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            this.options = options;
            _accessControl = accessControl;
        }
        public async Task<CustomerSelectModel> Create(CustomerCreateModel dto)
        {
            try
            {
                var entityClass = _mapper.Map<Customer>(dto);
                
                // Imposta sempre CreationDate e UpdateDate in UTC quando si crea una nuova entità
                var now = DateTime.UtcNow;
                entityClass.CreationDate = now;
                entityClass.UpdateDate = now;
                
                await _unitOfWork.CustomerRepository.InsertAsync(entityClass);
                _unitOfWork.Save();

                CustomerSelectModel response = new CustomerSelectModel();
                _mapper.Map(entityClass, response);

                _logger.LogInformation(nameof(Create));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante la creazione del cliente: {ex.Message}");
                throw new Exception("Si è verificato un errore in fase creazione");
            }
        }

        public async Task<Customer> Delete(int id)
        {
            try
            {
                IQueryable<Customer> query = _unitOfWork.dbContext.Customers;

                if (id == 0)
                    throw new NullReferenceException("L'id non può essere 0");

                query = query.Where(x => x.Id == id);

                Customer EntityClasses = await query.FirstOrDefaultAsync();

                if (EntityClasses == null)
                    throw new NullReferenceException("Record non trovato!");

                // Verifica preventiva se ci sono record collegati
                var hasEvents = await _unitOfWork.dbContext.Calendars.AnyAsync(x => x.CustomerId == id);
                if (hasEvents)
                {
                    throw new Exception("Impossibile eliminare il cliente perché è collegato a uno o più appuntamenti nel calendario.");
                }

                var hasRequests = await _unitOfWork.dbContext.Requests.AnyAsync(x => x.CustomerId == id);
                if (hasRequests)
                {
                    throw new Exception("Impossibile eliminare il cliente perché è collegato a una o più richieste.");
                }

                var hasProperties = await _unitOfWork.dbContext.RealEstateProperties.AnyAsync(x => x.CustomerId == id);
                if (hasProperties)
                {
                    throw new Exception("Impossibile eliminare il cliente perché è collegato a uno o più immobili.");
                }

                _unitOfWork.CustomerRepository.Delete(EntityClasses);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation(nameof(Delete));

                return EntityClasses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'eliminazione del cliente con ID {id}: {ex.Message}");
                
                // Se è già un'eccezione con messaggio personalizzato, rilanciala
                if (ex.Message.Contains("Impossibile eliminare il cliente"))
                {
                    throw;
                }

                // Gestione specifica per DbUpdateException (errori database)
                if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                {
                    if (dbEx.InnerException != null && 
                        (dbEx.InnerException.Message.Contains("DELETE statement conflicted") || 
                         dbEx.InnerException.Message.Contains("REFERENCE constraint")))
                    {
                        throw new Exception("Impossibile eliminare il cliente perché è utilizzato come chiave esterna in un'altra tabella.");
                    }
                }

                // Gestione per InnerException (per compatibilità con codice esistente)
                if (ex.InnerException != null && 
                    ex.InnerException.Message.Contains("DELETE statement conflicted with the REFERENCE constraint"))
                {
                    throw new Exception("Impossibile eliminare il cliente perché è utilizzato come chiave esterna in un'altra tabella.");
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

        public async Task<ListViewModel<CustomerSelectModel>> Get(string? userId, string? filterRequest, char? fromName, char? toName)
        {
            try
            {
                IQueryable<Customer> query = _unitOfWork.dbContext.Customers.OrderByDescending(x => x.Id);

                // Filtra per cerchia usando AccessControlService
                if (!string.IsNullOrEmpty(userId))
                {
                    var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                    query = query.Where(x => circleUserIds.Contains(x.UserId));
                }

                if (!string.IsNullOrEmpty(filterRequest))
                    query = query.Where(x => x.FirstName.Contains(filterRequest) || x.LastName.Contains(filterRequest));

                if (fromName != null)
                {
                    string fromNameString = fromName.ToString();
                    query = query.Where(x => string.Compare(x.FirstName.Substring(0, 1), fromNameString) >= 0);
                }

                if (toName != null)
                {
                    string toNameString = toName.ToString();
                    query = query.Where(x => string.Compare(x.FirstName.Substring(0, 1), toNameString) <= 0);
                }

                ListViewModel<CustomerSelectModel> result = new ListViewModel<CustomerSelectModel>();

                result.Total = await query.CountAsync();

                List<Customer> queryList = await query
                    //.Include(x => x.CustomerType)
                    .ToListAsync();

                result.Data = _mapper.Map<List<CustomerSelectModel>>(queryList);

                _logger.LogInformation(nameof(Get));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<CustomerSelectModel> GetById(int id)
        {
            try
            {
                if (id is not > 0)
                    throw new Exception("Si è verificato un errore!");

                var query = await _unitOfWork.dbContext.Customers
                    .Include(x => x.CustomerNotes)
                    .FirstOrDefaultAsync(x => x.Id == id);

                CustomerSelectModel result = _mapper.Map<CustomerSelectModel>(query);

                _logger.LogInformation(nameof(GetById));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<CustomerSelectModel> Update(CustomerUpdateModel dto)
        {
            try
            {
                var EntityClass =
                    await _unitOfWork.CustomerRepository.FirstOrDefaultAsync(q => q.Where(x => x.Id == dto.Id));

                if (EntityClass == null)
                    throw new NullReferenceException("Record non trovato!");

                EntityClass = _mapper.Map(dto, EntityClass);
                
                // Aggiorna sempre UpdateDate in UTC quando si modifica un'entità
                EntityClass.UpdateDate = DateTime.UtcNow;

                _unitOfWork.CustomerRepository.Update(EntityClass);
                await _unitOfWork.SaveAsync();

                CustomerSelectModel response = new CustomerSelectModel();
                _mapper.Map(EntityClass, response);

                _logger.LogInformation(nameof(Update));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'aggiornamento del cliente: {ex.Message}");
                if (ex is NullReferenceException)
                {
                    throw new Exception(ex.Message);
                }
                throw new Exception("Si è verificato un errore in fase di modifica");
            }
        }
    }
}
