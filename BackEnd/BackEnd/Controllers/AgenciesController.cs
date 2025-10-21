using AutoMapper;
using BackEnd.Entities;
using BackEnd.Models.OutputModels;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.UserModel;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace BackEnd.Controllers
{
    //[Authorize(Policy = "ActiveSubscription")]
    [ApiController]
    [Route("/api/[controller]/")]
    public class AgenciesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AgenciesController> _logger;
        private readonly IMapper _mapper;

        public AgenciesController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
           IConfiguration configuration,
            ILogger<AgenciesController> logger,
            IMapper mapper)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost]
        [Route(nameof(Update))]
        public async Task<IActionResult> Update(UserUpdateModel request)
        {
            try
            {
                ApplicationUser user = await userManager.FindByIdAsync(request.Id) ?? throw new NullReferenceException("Agente non trovato");
                _mapper.Map(request, user);

                IdentityResult Result = await userManager.UpdateAsync(user);

                if (Result.Succeeded)
                    return Ok();
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = Result.Errors.ToString() ?? "Si è verificato un errore" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(Get))]
        public async Task<IActionResult> Get(int currentPage, string? filterRequest)
        {
            try
            {
                //currentPage = currentPage > 0 ? currentPage : 1;
                var usersList = await userManager.GetUsersInRoleAsync("Agency");


                if (!string.IsNullOrEmpty(filterRequest))
                    usersList = usersList.Where(x => x.Email.Contains(filterRequest)).ToList();

                List<ApplicationUser> users = usersList.ToList();
                ListViewModel<UserSelectModel> result = new ListViewModel<UserSelectModel>();

                result.Total = users.Count();
                result.Data = _mapper.Map<List<UserSelectModel>>(users);

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
        public async Task<IActionResult> GetMain()
        {
            try
            {
                //currentPage = currentPage > 0 ? currentPage : 1;
                var usersList = await userManager.GetUsersInRoleAsync("Agency");

                List<ApplicationUser> users = usersList.Where(x => x.EmailConfirmed).ToList();
                ListViewModel<UserSelectModel> result = new ListViewModel<UserSelectModel>();

                result.Total = users.Count();
                result.Data = _mapper.Map<List<UserSelectModel>>(users);

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
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var user = await userManager.FindByIdAsync(id);
                UserSelectModel result = _mapper.Map<UserSelectModel>(user);

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
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                ApplicationUser? user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    await userManager.DeleteAsync(user);
                    return Ok();
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Utente non trovato" });
                }

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
        //        var result = await _agentServices.Get(0, null, fromName, toName);
        //        DataTable table = Export.ToDataTable<AgentSelectModel>(result.Data);
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
        //        var result = await _agentServices.Get(0, null, fromName, toName);
        //        DataTable table = Export.ToDataTable<AgentSelectModel>(result.Data);
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
