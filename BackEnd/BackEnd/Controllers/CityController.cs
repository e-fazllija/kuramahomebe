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

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CityCreateModel model)
        {
            try
            {
                var city = await _cityServices.Create(model);
                return Ok(city);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] CityUpdateModel model)
        {
            try
            {
                var city = await _cityServices.Update(model);
                return Ok(city);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno del server" });
            }
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

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            try
            {
                await _cityServices.Delete(id);
                return Ok(new { message = "Città eliminata con successo" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }
    }
} 