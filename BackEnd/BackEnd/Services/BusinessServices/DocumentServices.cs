using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.CustomerModels;
using BackEnd.Models.Options;
using BackEnd.Models.OutputModels;
using BackEnd.Models.InputModels;

namespace BackEnd.Services.BusinessServices
{
    public class DocumentServices : IDocumentServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentServices> _logger;
        private readonly IOptionsMonitor<PaginationOptions> options;
        private readonly IStorageServices _storageServices;
        public DocumentServices(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DocumentServices> logger, IOptionsMonitor<PaginationOptions> options, IStorageServices storageServices)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            this.options = options;
            _storageServices = storageServices;
        }
        public async Task<Documentation> UploadDocument(SendFileModel dto)
        {
            try
            {
                Stream stream = dto.File.OpenReadStream();
                string fileName = $"{dto.FolderName}/{dto.File.FileName.Replace(" ", "-")}";
                string fileUrl = await _storageServices.UploadFile(stream, fileName);

                Documentation document = new Documentation()
                {
                    FileName = fileName,
                    FileUrl = fileUrl
                };

               var result = await _unitOfWork.dbContext.Documentation.AddAsync(document);
                _unitOfWork.Save();
                return result.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore in fase creazione");
            }
        }

        public async Task DeleteDocument(int id)
        {
            try
            {
                Documentation document = await _unitOfWork.dbContext.Documentation.FirstAsync(x => x.Id == id);
                await _storageServices.DeleteFile(document.FileName);
                _unitOfWork.dbContext.Documentation.Remove(document);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation($"Documento {id} elimitnato con successo");

            }
            catch (Exception ex)
    {
                _logger.LogError(ex, $"Error deleting document {id}");
                throw new Exception("Si è verificato un errore durante l'eliminazione del documento");
            }
        }


    }
}
