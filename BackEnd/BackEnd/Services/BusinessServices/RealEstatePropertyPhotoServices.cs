using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.Options;
using BackEnd.Models.OutputModels;
using BackEnd.Models.RealEstatePropertyPhotoModels;

namespace BackEnd.Services.BusinessServices
{
    public class RealEstatePropertyPhotoServices : IRealEstatePropertyPhotoServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RealEstatePropertyPhotoServices> _logger;
        private readonly IOptionsMonitor<PaginationOptions> options;
        private readonly IStorageServices _storageServices;
        public RealEstatePropertyPhotoServices(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RealEstatePropertyPhotoServices> logger, IOptionsMonitor<PaginationOptions> options, IStorageServices storageServices)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            this.options = options;
            _storageServices = storageServices;
        }
        public async Task<RealEstatePropertyPhotoSelectModel> Create(RealEstatePropertyPhotoCreateModel dto)
        {
            try
            {
                var entityClass = _mapper.Map<RealEstatePropertyPhoto>(dto);
                await _unitOfWork.RealEstatePropertyPhotoRepository.InsertAsync(entityClass);
                _unitOfWork.Save();

                RealEstatePropertyPhotoSelectModel response = new RealEstatePropertyPhotoSelectModel();
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

        public async Task Delete(int id)
        {
            try
            {
                IQueryable<RealEstatePropertyPhoto> query = _unitOfWork.dbContext.RealEstatePropertyPhotos;

                if (id == 0)
                    throw new NullReferenceException("L'id non può essere 0");

                query = query.Where(x => x.Id == id);

                RealEstatePropertyPhoto EntityClasses = await query.FirstOrDefaultAsync();

                if (EntityClasses == null)
                    throw new NullReferenceException("Record non trovato!");

                _unitOfWork.RealEstatePropertyPhotoRepository.Delete(EntityClasses);
                await _unitOfWork.SaveAsync();

                await _storageServices.DeleteFile(EntityClasses.FileName);

                _logger.LogInformation(nameof(Delete));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                if (ex.InnerException.Message.Contains("DELETE statement conflicted with the REFERENCE constraint"))
                {
                    throw new Exception("Impossibile eliminare il record perché è utilizzato come chiave esterna in un'altra tabella.");
                }
                if (ex is NullReferenceException)
                {
                    throw new Exception(ex.Message);
                }
                else
                {
                    throw new Exception("Si è verificato un errore in fase di eliminazione");
                }
            }
        }

        public async Task<ListViewModel<RealEstatePropertyPhotoSelectModel>> Get(int currentPage, string? filterRequest, char? fromName, char? toName)
        {
            try
            {
                IQueryable<RealEstatePropertyPhoto> query = _unitOfWork.dbContext.RealEstatePropertyPhotos;

                if (!string.IsNullOrEmpty(filterRequest))
                    query = query.Where(x => x.FileName.Contains(filterRequest));

                if (fromName != null)
                {
                    string fromNameString = fromName.ToString();
                    query = query.Where(x => string.Compare(x.FileName.Substring(0, 1), fromNameString) >= 0);
                }

                if (toName != null)
                {
                    string toNameString = toName.ToString();
                    query = query.Where(x => string.Compare(x.FileName.Substring(0, 1), toNameString) <= 0);
                }

                ListViewModel<RealEstatePropertyPhotoSelectModel> result = new ListViewModel<RealEstatePropertyPhotoSelectModel>();

                result.Total = await query.CountAsync();

                if (currentPage > 0)
                {
                    query = query
                    .Skip((currentPage * options.CurrentValue.RealEstatePropertyPhotoItemPerPage) - options.CurrentValue.RealEstatePropertyPhotoItemPerPage)
                            .Take(options.CurrentValue.RealEstatePropertyPhotoItemPerPage);
                }

                List<RealEstatePropertyPhoto> queryList = await query
                    //.Include(x => x.RealEstatePropertyPhotoType)
                    .ToListAsync();

                result.Data = _mapper.Map<List<RealEstatePropertyPhotoSelectModel>>(queryList);

                _logger.LogInformation(nameof(Get));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<RealEstatePropertyPhotoSelectModel> GetById(int id)
        {
            try
            {
                if (id is not > 0)
                    throw new Exception("Si è verificato un errore!");

                var query = await _unitOfWork.dbContext.RealEstateProperties
                    //.Include(x => x.RealEstatePropertyPhotoType)
                    .FirstOrDefaultAsync(x => x.Id == id);

                RealEstatePropertyPhotoSelectModel result = _mapper.Map<RealEstatePropertyPhotoSelectModel>(query);

                _logger.LogInformation(nameof(GetById));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<RealEstatePropertyPhotoSelectModel> Update(RealEstatePropertyPhotoUpdateModel dto)
        {
            try
            {
                var EntityClass =
                    await _unitOfWork.RealEstatePropertyPhotoRepository.FirstOrDefaultAsync(q => q.Where(x => x.Id == dto.Id));

                if (EntityClass == null)
                    throw new NullReferenceException("Record non trovato!");

                EntityClass = _mapper.Map(dto, EntityClass);

                _unitOfWork.RealEstatePropertyPhotoRepository.Update(EntityClass);
                await _unitOfWork.SaveAsync();

                RealEstatePropertyPhotoSelectModel response = new RealEstatePropertyPhotoSelectModel();
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

        public async Task<List<RealEstatePropertyPhotoSelectModel>> UpdateOrder(List<RealEstatePropertyPhotoUpdateModel> dto)
        {
            try
            {
                var EntityClasses =
                    await _unitOfWork.dbContext.RealEstatePropertyPhotos.Where(x => dto.Select(y => y.Id).Contains(x.Id)).ToListAsync();

                foreach (var EntityClass in EntityClasses)
                {
                    EntityClass.Position = dto.FindIndex(x => x.Id == EntityClass.Id);
                }
                
                _unitOfWork.RealEstatePropertyPhotoRepository.UpdateRange(EntityClasses);
                await _unitOfWork.SaveAsync();

                List<RealEstatePropertyPhotoSelectModel> response = new List<RealEstatePropertyPhotoSelectModel>();
                _mapper.Map(EntityClasses, response);

                _logger.LogInformation(nameof(UpdateOrder));

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

        public async Task SetHighlighted(int realEstatePropertyPhotoId)
        {
            try
            {
                IQueryable<RealEstatePropertyPhoto> query = _unitOfWork.dbContext.RealEstatePropertyPhotos.Where(x => x.Id == realEstatePropertyPhotoId);
                RealEstatePropertyPhoto photo = await query.FirstAsync();
                IQueryable<RealEstatePropertyPhoto> queryHighlighted = _unitOfWork.dbContext.RealEstatePropertyPhotos
                    .Where(x => x.Highlighted == true && x.RealEstatePropertyId == photo.RealEstatePropertyId);

                RealEstatePropertyPhoto? photoHighlighted = await queryHighlighted.FirstOrDefaultAsync();
                photo.Highlighted = true;
                _unitOfWork.dbContext.RealEstatePropertyPhotos.Update(photo);
                await _unitOfWork.SaveAsync();

                if(photoHighlighted != null)
                {
                    photoHighlighted.Highlighted = false;
                    _unitOfWork.dbContext.RealEstatePropertyPhotos.Update(photoHighlighted);
                    await _unitOfWork.SaveAsync();
                }

                _logger.LogInformation(nameof(SetHighlighted));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }
    }
}
