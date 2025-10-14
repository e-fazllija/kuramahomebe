using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class LocationInProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgreedCommission",
                table: "RealEstateProperties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FlatRateCommission",
                table: "RealEstateProperties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "RealEstateProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfAssignment",
                table: "RealEstateProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentsTabs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentificationDocument = table.Column<bool>(type: "bit", nullable: false),
                    IdentificationDocumentDocumentId = table.Column<int>(type: "int", nullable: true),
                    TaxCodeOrHealthCard = table.Column<bool>(type: "bit", nullable: false),
                    TaxCodeOrHealthCardDocumentId = table.Column<int>(type: "int", nullable: true),
                    MarriageCertificateSummary = table.Column<bool>(type: "bit", nullable: false),
                    MarriageCertificateSummaryDocumentId = table.Column<int>(type: "int", nullable: true),
                    DeedOfOrigin = table.Column<bool>(type: "bit", nullable: false),
                    DeedOfOriginDocumentId = table.Column<int>(type: "int", nullable: true),
                    SystemsComplianceDeclaration = table.Column<bool>(type: "bit", nullable: false),
                    SystemsComplianceDeclarationDocumentId = table.Column<int>(type: "int", nullable: true),
                    ElectricalElectronicSystem = table.Column<bool>(type: "bit", nullable: false),
                    ElectricalElectronicSystemDocumentId = table.Column<int>(type: "int", nullable: true),
                    PlumbingSanitarySystem = table.Column<bool>(type: "bit", nullable: false),
                    PlumbingSanitarySystemDocumentId = table.Column<int>(type: "int", nullable: true),
                    GasSystem = table.Column<bool>(type: "bit", nullable: false),
                    GasSystemDocumentId = table.Column<int>(type: "int", nullable: true),
                    HeatingAirConditioningSystem = table.Column<bool>(type: "bit", nullable: false),
                    HeatingAirConditioningSystemDocumentId = table.Column<int>(type: "int", nullable: true),
                    LiftingSystem = table.Column<bool>(type: "bit", nullable: false),
                    LiftingSystemDocumentId = table.Column<int>(type: "int", nullable: true),
                    FireSafetySystem = table.Column<bool>(type: "bit", nullable: false),
                    FireSafetySystemDocumentId = table.Column<int>(type: "int", nullable: true),
                    BoilerMaintenanceLog = table.Column<bool>(type: "bit", nullable: false),
                    BoilerMaintenanceLogDocumentId = table.Column<int>(type: "int", nullable: true),
                    HabitabilityCertificate = table.Column<bool>(type: "bit", nullable: false),
                    HabitabilityCertificateDocumentId = table.Column<int>(type: "int", nullable: true),
                    StructuralIntegrityCertificate = table.Column<bool>(type: "bit", nullable: false),
                    StructuralIntegrityCertificateDocumentId = table.Column<int>(type: "int", nullable: true),
                    BuildingCadastralComplianceReport = table.Column<bool>(type: "bit", nullable: false),
                    BuildingCadastralComplianceReportDocumentId = table.Column<int>(type: "int", nullable: true),
                    LandRegistry = table.Column<bool>(type: "bit", nullable: false),
                    LandRegistryDocumentId = table.Column<int>(type: "int", nullable: true),
                    CadastralSurveyAndFloorPlan = table.Column<bool>(type: "bit", nullable: false),
                    CadastralSurveyAndFloorPlanDocumentId = table.Column<int>(type: "int", nullable: true),
                    CadastralMapExtract = table.Column<bool>(type: "bit", nullable: false),
                    CadastralMapExtractDocumentId = table.Column<int>(type: "int", nullable: true),
                    FloorPlanWithSubsidiaryUnits = table.Column<bool>(type: "bit", nullable: false),
                    FloorPlanWithSubsidiaryUnitsDocumentId = table.Column<int>(type: "int", nullable: true),
                    EnergyPerformanceCertificate = table.Column<bool>(type: "bit", nullable: false),
                    EnergyPerformanceCertificateDocumentId = table.Column<int>(type: "int", nullable: true),
                    MortgageLienRegistrySearch = table.Column<bool>(type: "bit", nullable: false),
                    MortgageLienRegistrySearchDocumentId = table.Column<int>(type: "int", nullable: true),
                    Condominium = table.Column<bool>(type: "bit", nullable: false),
                    CondominiumDocumentId = table.Column<int>(type: "int", nullable: true),
                    CondominiumBylaws = table.Column<bool>(type: "bit", nullable: false),
                    CondominiumBylawsDocumentId = table.Column<int>(type: "int", nullable: true),
                    MillesimalTables = table.Column<bool>(type: "bit", nullable: false),
                    MillesimalTablesDocumentId = table.Column<int>(type: "int", nullable: true),
                    LatestFinancialStatementAndBudget = table.Column<bool>(type: "bit", nullable: false),
                    LatestFinancialStatementAndBudgetDocumentId = table.Column<int>(type: "int", nullable: true),
                    LastTwoCondominiumMeetingMinutes = table.Column<bool>(type: "bit", nullable: false),
                    LastTwoCondominiumMeetingMinutesDocumentId = table.Column<int>(type: "int", nullable: true),
                    SignedStatementFromAdministrator = table.Column<bool>(type: "bit", nullable: false),
                    SignedStatementFromAdministratorDocumentId = table.Column<int>(type: "int", nullable: true),
                    ChamberOfCommerceBusinessRegistrySearch = table.Column<bool>(type: "bit", nullable: false),
                    ChamberOfCommerceBusinessRegistrySearchDocumentId = table.Column<int>(type: "int", nullable: true),
                    PowerOfAttorney = table.Column<bool>(type: "bit", nullable: false),
                    PowerOfAttorneyDocumentId = table.Column<int>(type: "int", nullable: true),
                    UrbanPlanningComplianceCertificate = table.Column<bool>(type: "bit", nullable: false),
                    UrbanPlanningComplianceCertificateDocumentId = table.Column<int>(type: "int", nullable: true),
                    LeaseAgreement = table.Column<bool>(type: "bit", nullable: false),
                    LeaseAgreementDocumentId = table.Column<int>(type: "int", nullable: true),
                    LastMortgagePaymentReceipt = table.Column<bool>(type: "bit", nullable: false),
                    LastMortgagePaymentReceiptDocumentId = table.Column<int>(type: "int", nullable: true),
                    TaxDeductionDocumentation = table.Column<bool>(type: "bit", nullable: false),
                    TaxDeductionDocumentationDocumentId = table.Column<int>(type: "int", nullable: true),
                    PurchaseOffer = table.Column<bool>(type: "bit", nullable: false),
                    PurchaseOfferDocumentId = table.Column<int>(type: "int", nullable: true),
                    CommissionAgreement = table.Column<bool>(type: "bit", nullable: false),
                    CommissionAgreementDocumentId = table.Column<int>(type: "int", nullable: true),
                    PreliminarySaleAgreement = table.Column<bool>(type: "bit", nullable: false),
                    PreliminarySaleAgreementDocumentId = table.Column<int>(type: "int", nullable: true),
                    DeedOfSale = table.Column<bool>(type: "bit", nullable: false),
                    DeedOfSaleDocumentId = table.Column<int>(type: "int", nullable: true),
                    MortgageDeed = table.Column<bool>(type: "bit", nullable: false),
                    MortgageDeedDocumentId = table.Column<int>(type: "int", nullable: true),
                    MiscellaneousDocuments = table.Column<bool>(type: "bit", nullable: false),
                    MiscellaneousDocumentsDocumentId = table.Column<int>(type: "int", nullable: true),
                    RealEstatePropertyDocumentId = table.Column<int>(type: "int", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentsTabs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentsTabs");

            migrationBuilder.DropColumn(
                name: "AgreedCommission",
                table: "RealEstateProperties");

            migrationBuilder.DropColumn(
                name: "FlatRateCommission",
                table: "RealEstateProperties");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "RealEstateProperties");

            migrationBuilder.DropColumn(
                name: "TypeOfAssignment",
                table: "RealEstateProperties");
        }
    }
}
