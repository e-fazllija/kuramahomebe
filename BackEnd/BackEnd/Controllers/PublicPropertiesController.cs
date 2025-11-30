using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.RealEstatePropertyModels;
using BackEnd.Models.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BackEnd.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("/api/public/properties")]
    public class PublicPropertiesController : ControllerBase
    {
        private readonly IRealEstatePropertyServices _realEstatePropertyServices;
        private readonly ILogger<PublicPropertiesController> _logger;

        public PublicPropertiesController(
            IRealEstatePropertyServices realEstatePropertyServices,
            ILogger<PublicPropertiesController> logger)
        {
            _realEstatePropertyServices = realEstatePropertyServices;
            _logger = logger;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] RealEstatePropertyPublicSearchRequest request)
        {
            try
            {
                var filters = request ?? new RealEstatePropertyPublicSearchRequest();
                var result = await _realEstatePropertyServices.SearchPublicAsync(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella ricerca pubblica degli immobili");
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel
                {
                    Status = "Error",
                    Message = "Si è verificato un errore durante la ricerca degli immobili"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            try
            {
                var result = await _realEstatePropertyServices.GetPublicDetailByIdAsync(id);
                if (result == null)
                {
                    return NotFound(new AuthResponseModel
                    {
                        Status = "Error",
                        Message = "Immobile non trovato"
                    });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dei dettagli dell'immobile");
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel
                {
                    Status = "Error",
                    Message = "Si è verificato un errore durante il recupero dei dettagli dell'immobile"
                });
            }
        }
    }
}

