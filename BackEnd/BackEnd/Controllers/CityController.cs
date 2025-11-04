using BackEnd.Models.CityModels;
using BackEnd.Services.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class CityController : ControllerBase
    {
        private readonly CityServices _cityServices;

        public CityController(CityServices cityServices)
        {
            _cityServices = cityServices;
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            try
            {
                var city = await _cityServices.GetById(id);
                return Ok(city);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("Get")]
        public async Task<IActionResult> Get([FromQuery] string? filterRequest, [FromQuery] int? provinceId)
        {
            try
            {
                var cities = await _cityServices.Get(filterRequest, provinceId);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var cities = await _cityServices.GetAll();
                return Ok(cities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("GetByProvince")]
        public async Task<IActionResult> GetByProvince([FromQuery] int provinceId)
        {
            try
            {
                var cities = await _cityServices.GetByProvince(provinceId);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("GetList")]
        public async Task<IActionResult> GetList([FromQuery] string? filterRequest, [FromQuery] int? provinceId)
        {
            try
            {
                var cities = await _cityServices.GetList(filterRequest, provinceId);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("GetGroupedByProvince")]
        public async Task<IActionResult> GetGroupedByProvince()
        {
            try
            {
                var cities = await _cityServices.GetGroupedByProvince();
                return Ok(cities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("Exists")]
        public async Task<IActionResult> Exists([FromQuery] string name, [FromQuery] int provinceId, [FromQuery] int? excludeId)
        {
            try
            {
                var exists = await _cityServices.Exists(name, provinceId, excludeId);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }
    }
}