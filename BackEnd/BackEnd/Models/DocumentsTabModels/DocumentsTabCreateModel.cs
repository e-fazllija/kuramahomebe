using BackEnd.Entities;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.DocumentsTabModelModels
{
    public class DocumentsTabCreateModel
    {
        public bool IdentificationDocument { get; set; }
        public IFormFile? IdentifacationDocumentFile { get; set; }
        public bool TaxCodeOrHealthCard { get; set; }
        public IFormFile? TaxCodeOrHealthCardFile { get; set; }
        public bool MarriageCertificateSummary { get; set; }
        public IFormFile? MarriageCertificateSummaryFile { get; set; }
        public bool DeedOfOrigin { get; set; }
        public IFormFile? DeedOfOriginFile { get; set; }
        public bool SystemsComplianceDeclaration { get; set; }
        public IFormFile? SystemsComplianceDeclarationFile { get; set; }
        public bool ElectricalElectronicSystem { get; set; }
        public IFormFile? ElectricalElectronicSystemFile { get; set; }
        public bool PlumbingSanitarySystem { get; set; }
        public IFormFile? PlumbingSanitarySystemFile { get; set; }
        public bool GasSystem { get; set; }
        public IFormFile? GasSystemFile { get; set; }
        public bool HeatingAirConditioningSystem { get; set; }
        public IFormFile? HeatingAirConditioningSystemFile { get; set; }
        public bool LiftingSystem { get; set; }
        public IFormFile? LiftingSystemFile { get; set; }
        public bool FireSafetySystem { get; set; }
        public IFormFile? FireSafetySystemFile { get; set; }
        public bool BoilerMaintenanceLog { get; set; }
        public IFormFile? BoilerMaintenanceLogFile { get; set; }
        public bool HabitabilityCertificate { get; set; }
        public IFormFile? HabitabilityCertificateFile { get; set; }
        public bool StructuralIntegrityCertificate { get; set; }
        public IFormFile? StructuralIntegrityCertificateFile { get; set; }
        public bool BuildingCadastralComplianceReport { get; set; }
        public IFormFile? BuildingCadastralComplianceReportFile { get; set; }
        public bool LandRegistry { get; set; }
        public IFormFile? LandRegistryFile { get; set; }
        public bool CadastralSurveyAndFloorPlan { get; set; }
        public IFormFile? CadastralSurveyAndFloorPlanFile { get; set; }
        public bool CadastralMapExtract { get; set; }
        public IFormFile? CadastralMapExtractFile { get; set; }
        public bool FloorPlanWithSubsidiaryUnits { get; set; }
        public IFormFile? FloorPlanWithSubsidiaryUnitsFile { get; set; }
        public bool EnergyPerformanceCertificate { get; set; }
        public IFormFile? EnergyPerformanceCertificateFile { get; set; }
        public bool MortgageLienRegistrySearch { get; set; }
        public IFormFile? MortgageLienRegistrySearchFile { get; set; }
        public bool Condominium { get; set; }
        public IFormFile? CondominiumFile { get; set; }
        public bool CondominiumBylaws { get; set; }
        public IFormFile? CondominiumBylawsFile { get; set; }
        public bool MillesimalTables { get; set; }
        public IFormFile? MillesimalTablesFile { get; set; }
        public bool LatestFinancialStatementAndBudget { get; set; }
        public IFormFile? LatestFinancialStatementAndBudgetFile { get; set; }
        public bool LastTwoCondominiumMeetingMinutes { get; set; }
        public IFormFile? LastTwoCondominiumMeetingMinutesFile { get; set; }
        public bool SignedStatementFromAdministrator { get; set; }
        public IFormFile? SignedStatementFromAdministratorFile { get; set; }
        public bool ChamberOfCommerceBusinessRegistrySearch { get; set; }
        public IFormFile? ChamberOfCommerceBusinessRegistrySearchFile { get; set; }
        public bool PowerOfAttorney { get; set; }
        public IFormFile? PowerOfAttorneyFile { get; set; }
        public bool UrbanPlanningComplianceCertificate { get; set; }
        public IFormFile? UrbanPlanningComplianceCertificateFile { get; set; }
        public bool LeaseAgreement { get; set; }
        public IFormFile? LeaseAgreementFile { get; set; }
        public bool LastMortgagePaymentReceipt { get; set; }
        public IFormFile? LastMortgagePaymentReceiptFile { get; set; }
        public bool TaxDeductionDocumentation { get; set; }
        public IFormFile? TaxDeductionDocumentationFile { get; set; }
        public bool PurchaseOffer { get; set; }
        public IFormFile? PurchaseOfferFile { get; set; }
        public bool CommissionAgreement { get; set; }
        public IFormFile? CommissionAgreementFile { get; set; }
        public bool PreliminarySaleAgreement { get; set; }
        public IFormFile? PreliminarySaleAgreementFile { get; set; }
        public bool DeedOfSale { get; set; }
        public IFormFile? DeedOfSaleFile { get; set; }
        public bool MortgageDeed { get; set; }
        public IFormFile? MortgageDeedFile { get; set; }
        public bool MiscellaneousDocuments { get; set; }
        public IFormFile? MiscellaneousDocumentsFile { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public int? RealEstatePropertyId { get; set; }
    }
}
