using Microsoft.AspNetCore.Mvc;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.LocationModels;
using BackEnd.Models.OutputModels;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("/api/[controller]/")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationServices _locationServices;
        private readonly ILogger<LocationController> _logger;

        public LocationController(
            ILocationServices locationServices,
            ILogger<LocationController> logger)
        {
            _locationServices = locationServices;
            _logger = logger;
        }

        [HttpPost]
        [Route(nameof(Create))]
        public async Task<IActionResult> Create(LocationCreateModel request)
        {
            try
            {
                LocationSelectModel result = await _locationServices.Create(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(Update))]
        public async Task<IActionResult> Update(LocationUpdateModel request)
        {
            try
            {
                LocationSelectModel result = await _locationServices.Update(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(Get))]
        public async Task<IActionResult> Get(int currentPage = 1, string? filterRequest = null, string? city = null)
        {
            try
            {
                ListViewModel<LocationSelectModel> result = await _locationServices.Get(currentPage, filterRequest, city);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetAll))]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<LocationSelectModel> result = await _locationServices.GetAll();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetGroupedByCity))]
        public async Task<IActionResult> GetGroupedByCity()
        {
            try
            {
                List<LocationGroupedModel> result = await _locationServices.GetGroupedByCity();
                return Ok(result);
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
                LocationSelectModel result = await _locationServices.GetById(id);
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
                await _locationServices.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(SeedLocations))]
        public async Task<IActionResult> SeedLocations()
        {
            try
            {
                bool result = await _locationServices.SeedLocations();
                return Ok(new { Status = "Success", Message = "Locations seeded successfully", Result = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
    }
} 