using Microsoft.AspNetCore.Mvc;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.CustomerModels;
using BackEnd.Services;
using System.Data;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.OutputModels;
using BackEnd.Services.BusinessServices;
using BackEnd.Models.DocumentsTabModelModels;
using BackEnd.Models.DocumentsTabModels;
using BackEnd.Interfaces;
using BackEnd.Models.InputModels;
using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("/api/[controller]/")]
    public class DocumentsTabsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IDocumentsTabServices _documentsTabServices;
        private readonly IDocumentServices _documentServices;
        private readonly ILogger<DocumentsTabsController> _logger;
        private readonly IMapper _mapper;

        public DocumentsTabsController(
           IConfiguration configuration,
           IDocumentsTabServices documentsTabServices,
            ILogger<DocumentsTabsController> logger,
            IDocumentServices documentServices, IMapper mapper)
        {
            _configuration = configuration;
            _documentsTabServices = documentsTabServices;
            _logger = logger;
            _documentServices = documentServices;
            _mapper = mapper;
        }
        [HttpPost]
        [Route(nameof(UploadIdentificationDocument))]
        public async Task<IActionResult> UploadIdentificationDocument(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.IdentificationDocumentDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadTaxCodeOrHealthCard))]
        public async Task<IActionResult> UploadTaxCodeOrHealthCard(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.TaxCodeOrHealthCardDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadMarriageCertificateSummary))]
        public async Task<IActionResult> UploadMarriageCertificateSummary(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.MarriageCertificateSummaryDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadDeedOfOrigin))]
        public async Task<IActionResult> UploadDeedOfOrigin(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.DeedOfOriginDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadSystemsComplianceDeclaration))]
        public async Task<IActionResult> UploadSystemsComplianceDeclaration(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.SystemsComplianceDeclarationDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadElectricalElectronicSystem))]
        public async Task<IActionResult> UploadElectricalElectronicSystem(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.ElectricalElectronicSystemDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadPlumbingSanitarySystem))]
        public async Task<IActionResult> UploadPlumbingSanitarySystem(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.PlumbingSanitarySystemDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadGasSystem))]
        public async Task<IActionResult> UploadGasSystem(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.GasSystemDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadHeatingAirConditioningSystem))]
        public async Task<IActionResult> UploadHeatingAirConditioningSystem(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.HeatingAirConditioningSystemDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadLiftingSystem))]
        public async Task<IActionResult> UploadLiftingSystem(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.LiftingSystemDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadFireSafetySystem))]
        public async Task<IActionResult> UploadFireSafetySystem(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.FireSafetySystemDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadBoilerMaintenanceLog))]
        public async Task<IActionResult> UploadBoilerMaintenanceLog(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.BoilerMaintenanceLogDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadHabitabilityCertificate))]
        public async Task<IActionResult> UploadHabitabilityCertificate(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.HabitabilityCertificateDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadStructuralIntegrityCertificate))]
        public async Task<IActionResult> UploadStructuralIntegrityCertificate(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.StructuralIntegrityCertificateDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadBuildingCadastralComplianceReport))]
        public async Task<IActionResult> UploadBuildingCadastralComplianceReport(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.BuildingCadastralComplianceReportDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadLandRegistry))]
        public async Task<IActionResult> UploadLandRegistry(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.LandRegistryDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadCadastralSurveyAndFloorPlan))]
        public async Task<IActionResult> UploadCadastralSurveyAndFloorPlan(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.CadastralSurveyAndFloorPlanDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadCadastralMapExtract))]
        public async Task<IActionResult> UploadCadastralMapExtract(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.CadastralMapExtractDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadFloorPlanWithSubsidiaryUnits))]
        public async Task<IActionResult> UploadFloorPlanWithSubsidiaryUnits(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.FloorPlanWithSubsidiaryUnitsDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadEnergyPerformanceCertificate))]
        public async Task<IActionResult> UploadEnergyPerformanceCertificate(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.EnergyPerformanceCertificateDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadMortgageLienRegistrySearch))]
        public async Task<IActionResult> UploadMortgageLienRegistrySearch(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.MortgageLienRegistrySearchDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadCondominium))]
        public async Task<IActionResult> UploadCondominium(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.CondominiumDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadCondominiumBylaws))]
        public async Task<IActionResult> UploadCondominiumBylaws(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.CondominiumBylawsDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadMillesimalTables))]
        public async Task<IActionResult> UploadMillesimalTables(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.MillesimalTablesDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadLatestFinancialStatementAndBudget))]
        public async Task<IActionResult> UploadLatestFinancialStatementAndBudget(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.LatestFinancialStatementAndBudgetDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadLastTwoCondominiumMeetingMinutes))]
        public async Task<IActionResult> UploadLastTwoCondominiumMeetingMinutes(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.LastTwoCondominiumMeetingMinutesDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadSignedStatementFromAdministrator))]
        public async Task<IActionResult> UploadSignedStatementFromAdministrator(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.SignedStatementFromAdministratorDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadChamberOfCommerceBusinessRegistrySearch))]
        public async Task<IActionResult> UploadChamberOfCommerceBusinessRegistrySearch(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.ChamberOfCommerceBusinessRegistrySearchDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadPowerOfAttorney))]
        public async Task<IActionResult> UploadPowerOfAttorney(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.PowerOfAttorneyDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadUrbanPlanningComplianceCertificate))]
        public async Task<IActionResult> UploadUrbanPlanningComplianceCertificate(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.UrbanPlanningComplianceCertificateDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadLeaseAgreement))]
        public async Task<IActionResult> UploadLeaseAgreement(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.LeaseAgreementDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadLastMortgagePaymentReceipt))]
        public async Task<IActionResult> UploadLastMortgagePaymentReceipt(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.LastMortgagePaymentReceiptDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadTaxDeductionDocumentation))]
        public async Task<IActionResult> UploadTaxDeductionDocumentation(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.TaxDeductionDocumentationDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadPurchaseOffer))]
        public async Task<IActionResult> UploadPurchaseOffer(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.PurchaseOfferDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadCommissionAgreement))]
        public async Task<IActionResult> UploadCommissionAgreement(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.CommissionAgreementDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadPreliminarySaleAgreement))]
        public async Task<IActionResult> UploadPreliminarySaleAgreement(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.PreliminarySaleAgreementDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadDeedOfSale))]
        public async Task<IActionResult> UploadDeedOfSale(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.DeedOfSaleDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadMortgageDeed))]
        public async Task<IActionResult> UploadMortgageDeed(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.MortgageDeedDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(UploadMiscellaneousDocuments))]
        public async Task<IActionResult> UploadMiscellaneousDocuments(SendFileModel request, int documentsTabId)
        {
            try
            {
                var Document = await _documentServices.UploadDocument(request);

                DocumentsTabUpdateModel documentToUpdate = new DocumentsTabUpdateModel();
                DocumentsTabSelectModel documentSelected = await _documentsTabServices.GetById(documentsTabId);
                _mapper.Map(documentSelected, documentToUpdate);
                documentToUpdate.MiscellaneousDocumentsDocumentId = Document.Id;

                DocumentsTabSelectModel Result = await _documentsTabServices.Update(documentToUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }


        [HttpDelete]
        [Route(nameof(DeleteIdentificationDocumentDocument))]
        public async Task<IActionResult> DeleteIdentificationDocumentDocument(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.IdentificationDocumentDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.IdentificationDocumentDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    IdentificationDocumentDocumentId = null 
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteTaxCodeOrHealthCard))]
        public async Task<IActionResult> DeleteTaxCodeOrHealthCard(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.TaxCodeOrHealthCardDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.TaxCodeOrHealthCardDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    TaxCodeOrHealthCardDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteMarriageCertificateSummary))]
        public async Task<IActionResult> DeleteMarriageCertificateSummary(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.MarriageCertificateSummaryDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.MarriageCertificateSummaryDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    MarriageCertificateSummaryDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteDeedOfOrigin))]
        public async Task<IActionResult> DeleteDeedOfOrigin(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.DeedOfOriginDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.DeedOfOriginDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    DeedOfOriginDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteSystemsComplianceDeclaration))]
        public async Task<IActionResult> DeleteSystemsComplianceDeclaration(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.SystemsComplianceDeclarationDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.SystemsComplianceDeclarationDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    SystemsComplianceDeclarationDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteElectricalElectronicSystem))]
        public async Task<IActionResult> DeleteElectricalElectronicSystem(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.ElectricalElectronicSystemDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.ElectricalElectronicSystemDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    ElectricalElectronicSystemDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeletePlumbingSanitarySystem))]
        public async Task<IActionResult> DeletePlumbingSanitarySystem(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.PlumbingSanitarySystemDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.PlumbingSanitarySystemDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    PlumbingSanitarySystemDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteGasSystem))]
        public async Task<IActionResult> DeleteGasSystem(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.GasSystemDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.GasSystemDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    GasSystemDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteHeatingAirConditioningSystem))]
        public async Task<IActionResult> DeleteHeatingAirConditioningSystem(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.HeatingAirConditioningSystemDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.HeatingAirConditioningSystemDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    HeatingAirConditioningSystemDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteLiftingSystem))]
        public async Task<IActionResult> DeleteLiftingSystem(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.LiftingSystemDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.LiftingSystemDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    LiftingSystemDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteFireSafetySystem))]
        public async Task<IActionResult> DeleteFireSafetySystem(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.FireSafetySystemDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.FireSafetySystemDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    FireSafetySystemDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteBoilerMaintenanceLog))]
        public async Task<IActionResult> DeleteBoilerMaintenanceLog(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.BoilerMaintenanceLogDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.BoilerMaintenanceLogDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    BoilerMaintenanceLogDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteHabitabilityCertificate))]
        public async Task<IActionResult> DeleteHabitabilityCertificate(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.HabitabilityCertificateDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.HabitabilityCertificateDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    HabitabilityCertificateDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteStructuralIntegrityCertificate))]
        public async Task<IActionResult> DeleteStructuralIntegrityCertificate(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.StructuralIntegrityCertificateDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.StructuralIntegrityCertificateDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    StructuralIntegrityCertificateDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteBuildingCadastralComplianceReport))]
        public async Task<IActionResult> DeleteBuildingCadastralComplianceReport(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.BuildingCadastralComplianceReportDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.BuildingCadastralComplianceReportDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    BuildingCadastralComplianceReportDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteLandRegistry))]
        public async Task<IActionResult> DeleteLandRegistry(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.LandRegistryDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.LandRegistryDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    LandRegistryDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteCadastralSurveyAndFloorPlan))]
        public async Task<IActionResult> DeleteCadastralSurveyAndFloorPlan(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.CadastralSurveyAndFloorPlanDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.CadastralSurveyAndFloorPlanDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    CadastralSurveyAndFloorPlanDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteCadastralMapExtract))]
        public async Task<IActionResult> DeleteCadastralMapExtract(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.CadastralMapExtractDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.CadastralMapExtractDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    CadastralMapExtractDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteFloorPlanWithSubsidiaryUnits))]
        public async Task<IActionResult> DeleteFloorPlanWithSubsidiaryUnits(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.FloorPlanWithSubsidiaryUnitsDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.FloorPlanWithSubsidiaryUnitsDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    FloorPlanWithSubsidiaryUnitsDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteEnergyPerformanceCertificate))]
        public async Task<IActionResult> DeleteEnergyPerformanceCertificate(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.EnergyPerformanceCertificateDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.EnergyPerformanceCertificateDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    EnergyPerformanceCertificateDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteMortgageLienRegistrySearch))]
        public async Task<IActionResult> DeleteMortgageLienRegistrySearch(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.MortgageLienRegistrySearchDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.MortgageLienRegistrySearchDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    MortgageLienRegistrySearchDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteCondominium))]
        public async Task<IActionResult> DeleteCondominium(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.CondominiumDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.CondominiumDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    CondominiumDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteCondominiumBylaws))]
        public async Task<IActionResult> DeleteCondominiumBylaws(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.CondominiumBylawsDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.CondominiumBylawsDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    CondominiumBylawsDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteMillesimalTables))]
        public async Task<IActionResult> DeleteMillesimalTables(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.MillesimalTablesDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.MillesimalTablesDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    MillesimalTablesDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteLatestFinancialStatementAndBudget))]
        public async Task<IActionResult> DeleteLatestFinancialStatementAndBudget(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.LatestFinancialStatementAndBudgetDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.LatestFinancialStatementAndBudgetDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    LatestFinancialStatementAndBudgetDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteLastTwoCondominiumMeetingMinutes))]
        public async Task<IActionResult> DeleteLastTwoCondominiumMeetingMinutes(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.LastTwoCondominiumMeetingMinutesDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.LastTwoCondominiumMeetingMinutesDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    LastTwoCondominiumMeetingMinutesDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteSignedStatementFromAdministrator))]
        public async Task<IActionResult> DeleteSignedStatementFromAdministrator(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.SignedStatementFromAdministratorDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.SignedStatementFromAdministratorDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    SignedStatementFromAdministratorDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteChamberOfCommerceBusinessRegistrySearch))]
        public async Task<IActionResult> DeleteChamberOfCommerceBusinessRegistrySearch(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.ChamberOfCommerceBusinessRegistrySearchDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.ChamberOfCommerceBusinessRegistrySearchDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    ChamberOfCommerceBusinessRegistrySearchDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeletePowerOfAttorney))]
        public async Task<IActionResult> DeletePowerOfAttorney(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.PowerOfAttorneyDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.PowerOfAttorneyDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    PowerOfAttorneyDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteUrbanPlanningComplianceCertificate))]
        public async Task<IActionResult> DeleteUrbanPlanningComplianceCertificate(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.UrbanPlanningComplianceCertificateDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.UrbanPlanningComplianceCertificateDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    UrbanPlanningComplianceCertificateDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteLeaseAgreement))]
        public async Task<IActionResult> DeleteLeaseAgreement(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.LeaseAgreementDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.LeaseAgreementDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    LeaseAgreementDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteLastMortgagePaymentReceipt))]
        public async Task<IActionResult> DeleteLastMortgagePaymentReceipt(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.LastMortgagePaymentReceiptDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.LastMortgagePaymentReceiptDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    LastMortgagePaymentReceiptDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteTaxDeductionDocumentation))]
        public async Task<IActionResult> DeleteTaxDeductionDocumentation(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.TaxDeductionDocumentationDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.TaxDeductionDocumentationDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    TaxDeductionDocumentationDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeletePurchaseOffer))]
        public async Task<IActionResult> DeletePurchaseOffer(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.PurchaseOfferDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.PurchaseOfferDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    PurchaseOfferDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteCommissionAgreement))]
        public async Task<IActionResult> DeleteCommissionAgreement(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.CommissionAgreementDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.CommissionAgreementDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    CommissionAgreementDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeletePreliminarySaleAgreement))]
        public async Task<IActionResult> DeletePreliminarySaleAgreement(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.PreliminarySaleAgreementDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.PreliminarySaleAgreementDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    PreliminarySaleAgreementDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteDeedOfSale))]
        public async Task<IActionResult> DeleteDeedOfSale(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.DeedOfSaleDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.DeedOfSaleDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    DeedOfSaleDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteMortgageDeed))]
        public async Task<IActionResult> DeleteMortgageDeed(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.MortgageDeedDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.MortgageDeedDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    MortgageDeedDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(DeleteMiscellaneousDocuments))]
        public async Task<IActionResult> DeleteMiscellaneousDocuments(int id)
        {
            try
            {
                var documentTab = await _documentsTabServices.GetById(id);
                if (documentTab.MiscellaneousDocumentsDocumentId == null)
                {
                    return BadRequest("Nessun documento di identificazione associato");
                }
                await _documentServices.DeleteDocument(documentTab.MiscellaneousDocumentsDocumentId.Value);
                var updateModel = new DocumentsTabUpdateModel
                {
                    Id = id,
                    MiscellaneousDocumentsDocumentId = null
                };
                await _documentsTabServices.Update(updateModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del documento di identificazione");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message });
            }
        }



        [HttpGet]
        [Route(nameof(GetById))]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                DocumentsTabSelectModel result = new DocumentsTabSelectModel();
                result = await _documentsTabServices.GetById(id);

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
                DocumentsTab result = await _documentsTabServices.Delete(id);
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
