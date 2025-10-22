using Microsoft.AspNetCore.Mvc;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.CalendarModels;
using BackEnd.Services;
using System.Data;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.OutputModels;

namespace BackEnd.Controllers
{
    [Authorize(Policy = "ActiveSubscription")]
    [ApiController]
    [Route("/api/[controller]/")]
    public class CalendarController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ICalendarServices _calendarServices;
        private readonly ILogger<CalendarController> _logger;

        public CalendarController(
           IConfiguration configuration,
           ICalendarServices calendarServices,
            ILogger<CalendarController> logger)
        {
            _configuration = configuration;
            _calendarServices = calendarServices;
            _logger = logger;
        }
        [HttpPost]
        [Route(nameof(Create))]
        public async Task<IActionResult> Create(CalendarCreateModel request)
        {
            try
            {
                CalendarSelectModel Result = await _calendarServices.Create(request);
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
        public async Task<IActionResult> Update(CalendarUpdateModel request)
        {
            try
            {
               CalendarSelectModel Result = await _calendarServices.Update(request);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(Get))]
        public async Task<IActionResult> Get(string? agencyId, string? agentId)
        {
            try
            {
                ListViewModel<CalendarSelectModel> res = await _calendarServices.Get(agencyId, agentId, null, null);

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
        public async Task<IActionResult> GetToInsert(string agencyId)
        {
            try
            {
                CalendarCreateViewModel res = await _calendarServices.GetToInsert(agencyId);

                return Ok(res);
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
                CalendarSearchModel res = await _calendarServices.GetSearchItems(userId, agencyId);

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
                CalendarSelectModel result = new CalendarSelectModel();
                result = await _calendarServices.GetById(id);

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
                Calendar result = await _calendarServices.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }        
    }
}
