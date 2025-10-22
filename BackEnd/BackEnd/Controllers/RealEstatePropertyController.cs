using Microsoft.AspNetCore.Mvc;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.OutputModels;
using BackEnd.Models.RealEstatePropertyModels;
using BackEnd.Models.RealEstatePropertyPhotoModels;
using BackEnd.Models.CalendarModels;
using Microsoft.AspNetCore.Authorization;

namespace BackEnd.Controllers
{
    [Authorize(Policy = "ActiveSubscription")]
    [ApiController]
    [Route("/api/[controller]/")]
    public class RealEstatePropertyController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IRealEstatePropertyServices _realEstatePropertyServices;
        private readonly IRealEstatePropertyPhotoServices _realEstatePropertyPhotoServices;
        private readonly ILogger<RealEstatePropertyController> _logger;

        public RealEstatePropertyController(
           IConfiguration configuration,
           IRealEstatePropertyServices realEstatePropertyServices,
           IRealEstatePropertyPhotoServices realEstatePropertyPhotoServices,
            ILogger<RealEstatePropertyController> logger)
        {
            _configuration = configuration;
            _realEstatePropertyServices = realEstatePropertyServices;
            _realEstatePropertyPhotoServices = realEstatePropertyPhotoServices;
            _logger = logger;
        }

        [HttpPost]
        [Route(nameof(Create))]
        public async Task<IActionResult> Create(RealEstatePropertyCreateModel request)
        {
            try
            {
                RealEstatePropertySelectModel Result = await _realEstatePropertyServices.Create(request);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(UploadFiles))]
        public async Task<IActionResult> UploadFiles(UploadFilesModel request)
        {
            try
            {
                await _realEstatePropertyServices.InsertFiles(request);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(Update))]
        public async Task<IActionResult> Update(RealEstatePropertyUpdateModel request)
        {
            try
            {
                RealEstatePropertySelectModel Result = await _realEstatePropertyServices.Update(request);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(UpdatePhotosOrder))]
        public async Task<IActionResult> UpdatePhotosOrder(List<RealEstatePropertyPhotoUpdateModel> request)
        {
            try
            {
                List<RealEstatePropertyPhotoSelectModel> result = await _realEstatePropertyPhotoServices.UpdateOrder(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetMain))]
        public async Task<IActionResult> GetMain(int currentPage, string? filterRequest, string? status, string? typologie, string? location, int? code, int? from, int? to, string? agencyId, string? city)
        {
            try
            {
                ListViewModel<RealEstatePropertySelectModel> res = await _realEstatePropertyServices.Get(currentPage, filterRequest, status, typologie, location,
                 code,
                 from,
                 to,
                 agencyId,
                 city, null);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(Get))]
        public async Task<IActionResult> Get(int currentPage, string? agencyId, string? filterRequest, string? contract, int? priceFrom, int? priceTo, string? category, string? typologie, string? city)
        {
            try
            {
                ListViewModel<RealEstatePropertySelectModel> res = await _realEstatePropertyServices.Get(
                    currentPage, agencyId, filterRequest, contract, priceFrom, priceTo, category, typologie, city);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetList))]
        public async Task<IActionResult> GetList(int currentPage, string? agencyId, string? filterRequest, string? contract, int? priceFrom, int? priceTo, string? category, string? typologie, string? city, bool? sold)
        {
            try
            {
                ListViewModel<RealEstatePropertyListModel> res = await _realEstatePropertyServices.GetList(
                    currentPage, agencyId, filterRequest, contract, priceFrom, priceTo, category, typologie, city, sold);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetPropertyCount))]
        public IActionResult GetPropertyCount()
        {
            try
            {
                int res = _realEstatePropertyServices.GetPropertyCount();

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetToInsert))]
        public async Task<IActionResult> GetToInsert(string? agencyId)
        {
            try
            {
                RealEstatePropertyCreateViewModel res = await _realEstatePropertyServices.GetToInsert(agencyId);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpGet]
        [Route(nameof(GetById))]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                RealEstatePropertySelectModel result = new RealEstatePropertySelectModel();
                result = await _realEstatePropertyServices.GetById(id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(Delete))]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _realEstatePropertyServices.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpDelete]
        [Route(nameof(DeletePhoto))]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            try
            {
                await _realEstatePropertyPhotoServices.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(SetHighlighted))]
        public async Task<IActionResult> SetHighlighted(int realEstatePropertyId)
        {
            try
            {
                await _realEstatePropertyServices.SetHighlighted(realEstatePropertyId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(SetRealEstatePropertyPhotoHighlighted))]
        public async Task<IActionResult> SetRealEstatePropertyPhotoHighlighted([FromForm] int realEstatePropertyPhotoId)
        {
            try
            {
                await _realEstatePropertyPhotoServices.SetHighlighted(realEstatePropertyPhotoId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(SetInHome))]
        public async Task<IActionResult> SetInHome(int realEstatePropertyId)
        {
            try
            {
                await _realEstatePropertyServices.SetInHome(realEstatePropertyId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetSearchItems))]
        public async Task<IActionResult> GetSearchItems(string userId, string? agencyId)
        {
            try
            {
                CalendarSearchModel res = await _realEstatePropertyServices.GetSearchItems(userId, agencyId);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        //[HttpGet]
        //[Route(nameof(ExportExcel))]
        //public async Task<IActionResult> ExportExcel(char? fromName, char? toName)
        //{
        //    try
        //    {
        //        var result = await _realEstatePropertyServices.Get(0, null, null, null, fromName, toName);
        //        DataTable table = Export.ToDataTable<RealEstatePropertySelectModel>(result.Data);
        //        byte[] fileBytes = Export.GenerateExcelContent(table);

        //        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Output.xlsx");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
        //    }
        //}
        //[HttpGet]
        //[Route(nameof(ExportCsv))]
        //public async Task<IActionResult> ExportCsv(char? fromName, char? toName)
        //{
        //    try
        //    {
        //        var result = await _realEstatePropertyServices.Get(0, null, null, null, fromName, toName);
        //        DataTable table = Export.ToDataTable<RealEstatePropertySelectModel>(result.Data);
        //        byte[] fileBytes = Export.GenerateCsvContent(table);

        //        return File(fileBytes, "text/csv", "Output.csv");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
        //    }
        //}
    }
}
