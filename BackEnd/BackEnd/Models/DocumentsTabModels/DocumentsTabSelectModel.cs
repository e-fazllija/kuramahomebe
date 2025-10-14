namespace BackEnd.Models.DocumentsTabModels
{
    public class DocumentsTabSelectModel
    {
        public int Id { get; set; }
        public bool IdentificationDocument { get; set; }
        public int? IdentificationDocumentDocumentId { get; set; }

        public bool TaxCodeOrHealthCard { get; set; }
        public int? TaxCodeOrHealthCardDocumentId { get; set; }

        public bool MarriageCertificateSummary { get; set; }
        public int? MarriageCertificateSummaryDocumentId { get; set; }

        public bool DeedOfOrigin { get; set; }
        public int? DeedOfOriginDocumentId { get; set; }

        public bool SystemsComplianceDeclaration { get; set; }
        public int? SystemsComplianceDeclarationDocumentId { get; set; }

        public bool ElectricalElectronicSystem { get; set; }
        public int? ElectricalElectronicSystemDocumentId { get; set; }

        public bool PlumbingSanitarySystem { get; set; }
        public int? PlumbingSanitarySystemDocumentId { get; set; }

        public bool GasSystem { get; set; }
        public int? GasSystemDocumentId { get; set; }

        public bool HeatingAirConditioningSystem { get; set; }
        public int? HeatingAirConditioningSystemDocumentId { get; set; }

        public bool LiftingSystem { get; set; }
        public int? LiftingSystemDocumentId { get; set; }

        public bool FireSafetySystem { get; set; }
        public int? FireSafetySystemDocumentId { get; set; }

        public bool BoilerMaintenanceLog { get; set; }
        public int? BoilerMaintenanceLogDocumentId { get; set; }

        public bool HabitabilityCertificate { get; set; }
        public int? HabitabilityCertificateDocumentId { get; set; }

        public bool StructuralIntegrityCertificate { get; set; }
        public int? StructuralIntegrityCertificateDocumentId { get; set; }

        public bool BuildingCadastralComplianceReport { get; set; }
        public int? BuildingCadastralComplianceReportDocumentId { get; set; }

        public bool LandRegistry { get; set; }
        public int? LandRegistryDocumentId { get; set; }

        public bool CadastralSurveyAndFloorPlan { get; set; }
        public int? CadastralSurveyAndFloorPlanDocumentId { get; set; }

        public bool CadastralMapExtract { get; set; }
        public int? CadastralMapExtractDocumentId { get; set; }

        public bool FloorPlanWithSubsidiaryUnits { get; set; }
        public int? FloorPlanWithSubsidiaryUnitsDocumentId { get; set; }

        public bool EnergyPerformanceCertificate { get; set; }
        public int? EnergyPerformanceCertificateDocumentId { get; set; }

        public bool MortgageLienRegistrySearch { get; set; }
        public int? MortgageLienRegistrySearchDocumentId { get; set; }

        public bool Condominium { get; set; }
        public int? CondominiumDocumentId { get; set; }

        public bool CondominiumBylaws { get; set; }
        public int? CondominiumBylawsDocumentId { get; set; }

        public bool MillesimalTables { get; set; }
        public int? MillesimalTablesDocumentId { get; set; }

        public bool LatestFinancialStatementAndBudget { get; set; }
        public int? LatestFinancialStatementAndBudgetDocumentId { get; set; }

        public bool LastTwoCondominiumMeetingMinutes { get; set; }
        public int? LastTwoCondominiumMeetingMinutesDocumentId { get; set; }

        public bool SignedStatementFromAdministrator { get; set; }
        public int? SignedStatementFromAdministratorDocumentId { get; set; }

        public bool ChamberOfCommerceBusinessRegistrySearch { get; set; }
        public int? ChamberOfCommerceBusinessRegistrySearchDocumentId { get; set; }

        public bool PowerOfAttorney { get; set; }
        public int? PowerOfAttorneyDocumentId { get; set; }

        public bool UrbanPlanningComplianceCertificate { get; set; }
        public int? UrbanPlanningComplianceCertificateDocumentId { get; set; }

        public bool LeaseAgreement { get; set; }
        public int? LeaseAgreementDocumentId { get; set; }

        public bool LastMortgagePaymentReceipt { get; set; }
        public int? LastMortgagePaymentReceiptDocumentId { get; set; }

        public bool TaxDeductionDocumentation { get; set; }
        public int? TaxDeductionDocumentationDocumentId { get; set; }

        public bool PurchaseOffer { get; set; }
        public int? PurchaseOfferDocumentId { get; set; }

        public bool CommissionAgreement { get; set; }
        public int? CommissionAgreementDocumentId { get; set; }

        public bool PreliminarySaleAgreement { get; set; }
        public int? PreliminarySaleAgreementDocumentId { get; set; }

        public bool DeedOfSale { get; set; }
        public int? DeedOfSaleDocumentId { get; set; }

        public bool MortgageDeed { get; set; }
        public int? MortgageDeedDocumentId { get; set; }

        public bool MiscellaneousDocuments { get; set; }
        public int? MiscellaneousDocumentsDocumentId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public int? RealEstatePropertyId { get; set; }
    }
}
