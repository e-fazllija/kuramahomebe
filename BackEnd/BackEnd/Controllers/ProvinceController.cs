using BackEnd.Models.ProvinceModels;
using BackEnd.Services.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class ProvinceController : ControllerBase
    {
        private readonly ProvinceServices _provinceServices;

        public ProvinceController(ProvinceServices provinceServices)
        {
            _provinceServices = provinceServices;
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            try
            {
                var province = await _provinceServices.GetById(id);
                return Ok(province);
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
        public async Task<IActionResult> Get([FromQuery] string? filterRequest)
        {
            try
            {
                var provinces = await _provinceServices.Get(filterRequest);
                return Ok(provinces);
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
                var provinces = await _provinceServices.GetAll();
                return Ok(provinces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

    }
} 