using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.InputModels;
using BackEnd.Models.MailModels;
using BackEnd.Models.OutputModels;
using BackEnd.Models.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("/api/[controller]/")]
    public class GenericController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IGenericService _genericService;
        private readonly IMailService _mailService;
        private readonly IRealEstatePropertyPhotoServices _realEstatePropertyPhotoServices;
        private readonly ILogger<GenericController> _logger;

        public GenericController(
           IConfiguration configuration,
           IGenericService genericService,
           IMailService mailService,
           IRealEstatePropertyPhotoServices realEstatePropertyPhotoServices,
            ILogger<GenericController> logger)
        {
            _configuration = configuration;
            _genericService = genericService;
            _mailService = mailService;
            _realEstatePropertyPhotoServices = realEstatePropertyPhotoServices;
            _logger = logger;
        }

        [HttpGet]
        [Route(nameof(GetHomeDetails))]
        public async Task<IActionResult> GetHomeDetails()
        {
            try
            {
                HomeDetailsModel result = await _genericService.GetHomeDetails();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [Authorize(Policy = "ActiveSubscription")]
        [HttpGet]
        [Route(nameof(GetAdminHomeDetails))]
        public async Task<IActionResult> GetAdminHomeDetails(string? agencyId)
        {
            try
            {
                AdminHomeDetailsModel result = await _genericService.GetAdminHomeDetails(agencyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(SendEvaluationRequest))]
        public async Task<IActionResult> SendEvaluationRequest([FromBody] SendRequestModel request)
        {
            try
            {
                await _mailService.SendEvaluationRequestAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(SendRequest))]
        public async Task<IActionResult> SendRequest([FromBody] SendRequestModel request)
        {
            try
            {
                await _mailService.SendRequestAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(WorkWithUs))]
        public async Task<IActionResult> WorkWithUs([FromBody] SendRequestModel request)
        {
            try
            {
                await _mailService.SendWorkWithUsRequestAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(InformationRequest))]
        public async Task<IActionResult> InformationRequest([FromBody] SendRequestModel request)
        {
            try
            {
                await _mailService.InformationRequestAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetLocations))]
        public async Task<IActionResult> GetLocations()
        {
            try
            {
                var result = await _genericService.GetLocations();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
    }
}
