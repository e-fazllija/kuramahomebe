using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MobilePhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Referent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ZipCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AdminId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClientSecret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SyncToIdealista = table.Column<bool>(type: "boolean", nullable: true),
                    StorageUsedBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserType = table.Column<int>(type: "integer", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FiscalCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    VATNumber = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    PEC = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    SDICode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_AspNetUsers_AdminId",
                        column: x => x.AdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentsTabs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdentificationDocument = table.Column<bool>(type: "boolean", nullable: false),
                    IdentificationDocumentDocumentId = table.Column<int>(type: "integer", nullable: true),
                    TaxCodeOrHealthCard = table.Column<bool>(type: "boolean", nullable: false),
                    TaxCodeOrHealthCardDocumentId = table.Column<int>(type: "integer", nullable: true),
                    MarriageCertificateSummary = table.Column<bool>(type: "boolean", nullable: false),
                    MarriageCertificateSummaryDocumentId = table.Column<int>(type: "integer", nullable: true),
                    DeedOfOrigin = table.Column<bool>(type: "boolean", nullable: false),
                    DeedOfOriginDocumentId = table.Column<int>(type: "integer", nullable: true),
                    SystemsComplianceDeclaration = table.Column<bool>(type: "boolean", nullable: false),
                    SystemsComplianceDeclarationDocumentId = table.Column<int>(type: "integer", nullable: true),
                    ElectricalElectronicSystem = table.Column<bool>(type: "boolean", nullable: false),
                    ElectricalElectronicSystemDocumentId = table.Column<int>(type: "integer", nullable: true),
                    PlumbingSanitarySystem = table.Column<bool>(type: "boolean", nullable: false),
                    PlumbingSanitarySystemDocumentId = table.Column<int>(type: "integer", nullable: true),
                    GasSystem = table.Column<bool>(type: "boolean", nullable: false),
                    GasSystemDocumentId = table.Column<int>(type: "integer", nullable: true),
                    HeatingAirConditioningSystem = table.Column<bool>(type: "boolean", nullable: false),
                    HeatingAirConditioningSystemDocumentId = table.Column<int>(type: "integer", nullable: true),
                    LiftingSystem = table.Column<bool>(type: "boolean", nullable: false),
                    LiftingSystemDocumentId = table.Column<int>(type: "integer", nullable: true),
                    FireSafetySystem = table.Column<bool>(type: "boolean", nullable: false),
                    FireSafetySystemDocumentId = table.Column<int>(type: "integer", nullable: true),
                    BoilerMaintenanceLog = table.Column<bool>(type: "boolean", nullable: false),
                    BoilerMaintenanceLogDocumentId = table.Column<int>(type: "integer", nullable: true),
                    HabitabilityCertificate = table.Column<bool>(type: "boolean", nullable: false),
                    HabitabilityCertificateDocumentId = table.Column<int>(type: "integer", nullable: true),
                    StructuralIntegrityCertificate = table.Column<bool>(type: "boolean", nullable: false),
                    StructuralIntegrityCertificateDocumentId = table.Column<int>(type: "integer", nullable: true),
                    BuildingCadastralComplianceReport = table.Column<bool>(type: "boolean", nullable: false),
                    BuildingCadastralComplianceReportDocumentId = table.Column<int>(type: "integer", nullable: true),
                    LandRegistry = table.Column<bool>(type: "boolean", nullable: false),
                    LandRegistryDocumentId = table.Column<int>(type: "integer", nullable: true),
                    CadastralSurveyAndFloorPlan = table.Column<bool>(type: "boolean", nullable: false),
                    CadastralSurveyAndFloorPlanDocumentId = table.Column<int>(type: "integer", nullable: true),
                    CadastralMapExtract = table.Column<bool>(type: "boolean", nullable: false),
                    CadastralMapExtractDocumentId = table.Column<int>(type: "integer", nullable: true),
                    FloorPlanWithSubsidiaryUnits = table.Column<bool>(type: "boolean", nullable: false),
                    FloorPlanWithSubsidiaryUnitsDocumentId = table.Column<int>(type: "integer", nullable: true),
                    EnergyPerformanceCertificate = table.Column<bool>(type: "boolean", nullable: false),
                    EnergyPerformanceCertificateDocumentId = table.Column<int>(type: "integer", nullable: true),
                    MortgageLienRegistrySearch = table.Column<bool>(type: "boolean", nullable: false),
                    MortgageLienRegistrySearchDocumentId = table.Column<int>(type: "integer", nullable: true),
                    Condominium = table.Column<bool>(type: "boolean", nullable: false),
                    CondominiumDocumentId = table.Column<int>(type: "integer", nullable: true),
                    CondominiumBylaws = table.Column<bool>(type: "boolean", nullable: false),
                    CondominiumBylawsDocumentId = table.Column<int>(type: "integer", nullable: true),
                    MillesimalTables = table.Column<bool>(type: "boolean", nullable: false),
                    MillesimalTablesDocumentId = table.Column<int>(type: "integer", nullable: true),
                    LatestFinancialStatementAndBudget = table.Column<bool>(type: "boolean", nullable: false),
                    LatestFinancialStatementAndBudgetDocumentId = table.Column<int>(type: "integer", nullable: true),
                    LastTwoCondominiumMeetingMinutes = table.Column<bool>(type: "boolean", nullable: false),
                    LastTwoCondominiumMeetingMinutesDocumentId = table.Column<int>(type: "integer", nullable: true),
                    SignedStatementFromAdministrator = table.Column<bool>(type: "boolean", nullable: false),
                    SignedStatementFromAdministratorDocumentId = table.Column<int>(type: "integer", nullable: true),
                    ChamberOfCommerceBusinessRegistrySearch = table.Column<bool>(type: "boolean", nullable: false),
                    ChamberOfCommerceBusinessRegistrySearchDocumentId = table.Column<int>(type: "integer", nullable: true),
                    PowerOfAttorney = table.Column<bool>(type: "boolean", nullable: false),
                    PowerOfAttorneyDocumentId = table.Column<int>(type: "integer", nullable: true),
                    UrbanPlanningComplianceCertificate = table.Column<bool>(type: "boolean", nullable: false),
                    UrbanPlanningComplianceCertificateDocumentId = table.Column<int>(type: "integer", nullable: true),
                    LeaseAgreement = table.Column<bool>(type: "boolean", nullable: false),
                    LeaseAgreementDocumentId = table.Column<int>(type: "integer", nullable: true),
                    LastMortgagePaymentReceipt = table.Column<bool>(type: "boolean", nullable: false),
                    LastMortgagePaymentReceiptDocumentId = table.Column<int>(type: "integer", nullable: true),
                    TaxDeductionDocumentation = table.Column<bool>(type: "boolean", nullable: false),
                    TaxDeductionDocumentationDocumentId = table.Column<int>(type: "integer", nullable: true),
                    PurchaseOffer = table.Column<bool>(type: "boolean", nullable: false),
                    PurchaseOfferDocumentId = table.Column<int>(type: "integer", nullable: true),
                    CommissionAgreement = table.Column<bool>(type: "boolean", nullable: false),
                    CommissionAgreementDocumentId = table.Column<int>(type: "integer", nullable: true),
                    PreliminarySaleAgreement = table.Column<bool>(type: "boolean", nullable: false),
                    PreliminarySaleAgreementDocumentId = table.Column<int>(type: "integer", nullable: true),
                    DeedOfSale = table.Column<bool>(type: "boolean", nullable: false),
                    DeedOfSaleDocumentId = table.Column<int>(type: "integer", nullable: true),
                    MortgageDeed = table.Column<bool>(type: "boolean", nullable: false),
                    MortgageDeedDocumentId = table.Column<int>(type: "integer", nullable: true),
                    MiscellaneousDocuments = table.Column<bool>(type: "boolean", nullable: false),
                    MiscellaneousDocumentsDocumentId = table.Column<int>(type: "integer", nullable: true),
                    RealEstatePropertyDocumentId = table.Column<int>(type: "integer", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentsTabs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StripeWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Data = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Processed = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripeWebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    BillingPeriod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    StripePriceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Buyer = table.Column<bool>(type: "boolean", nullable: false),
                    Seller = table.Column<bool>(type: "boolean", nullable: false),
                    Builder = table.Column<bool>(type: "boolean", nullable: false),
                    Other = table.Column<bool>(type: "boolean", nullable: false),
                    GoldCustomer = table.Column<bool>(type: "boolean", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AcquisitionDone = table.Column<bool>(type: "boolean", nullable: false),
                    OngoingAssignment = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documentation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AgencyId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsFolder = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    ParentPath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentation_Agency_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documentation_User_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExportHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ExportType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExportHistory_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubscriptionPlanId = table.Column<int>(type: "integer", nullable: false),
                    FeatureName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FeatureValue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionFeatures_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CalendarId = table.Column<int>(type: "integer", nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerNotes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerNotes_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RealEstateProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Typology = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InHome = table.Column<bool>(type: "boolean", nullable: false),
                    Highlighted = table.Column<bool>(type: "boolean", nullable: false),
                    Auction = table.Column<bool>(type: "boolean", nullable: false),
                    Sold = table.Column<bool>(type: "boolean", nullable: false),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    Negotiation = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CommercialSurfaceate = table.Column<int>(type: "integer", nullable: false),
                    Floor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TotalBuildingfloors = table.Column<int>(type: "integer", nullable: false),
                    Elevators = table.Column<int>(type: "integer", nullable: false),
                    MoreDetails = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MoreFeatures = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Bedrooms = table.Column<int>(type: "integer", nullable: false),
                    WarehouseRooms = table.Column<int>(type: "integer", nullable: false),
                    Kitchens = table.Column<int>(type: "integer", nullable: false),
                    Bathrooms = table.Column<int>(type: "integer", nullable: false),
                    Furniture = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OtherFeatures = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ParkingSpaces = table.Column<int>(type: "integer", nullable: false),
                    Heating = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Exposure = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EnergyClass = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TypeOfProperty = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StateOfTheProperty = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    YearOfConstruction = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false),
                    PriceReduced = table.Column<double>(type: "double precision", nullable: false),
                    MQGarden = table.Column<int>(type: "integer", nullable: false),
                    CondominiumExpenses = table.Column<double>(type: "double precision", nullable: false),
                    Availability = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    VideoUrl = table.Column<string>(type: "text", nullable: true),
                    AgreedCommission = table.Column<int>(type: "integer", nullable: false),
                    FlatRateCommission = table.Column<int>(type: "integer", nullable: false),
                    CommissionReversal = table.Column<int>(type: "integer", nullable: false),
                    EffectiveCommission = table.Column<double>(type: "double precision", nullable: false),
                    TypeOfAssignment = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignmentEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IdealistaPropertyId = table.Column<int>(type: "integer", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealEstateProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RealEstateProperties_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RealEstateProperties_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Closed = table.Column<bool>(type: "boolean", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    Contract = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PropertyType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RoomsFrom = table.Column<int>(type: "integer", nullable: false),
                    RoomsTo = table.Column<int>(type: "integer", nullable: false),
                    Bathrooms = table.Column<int>(type: "integer", nullable: false),
                    Floor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MQFrom = table.Column<int>(type: "integer", nullable: false),
                    MQTo = table.Column<int>(type: "integer", nullable: false),
                    PropertyState = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Heating = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Furniture = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EnergyClass = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Auction = table.Column<bool>(type: "boolean", nullable: false),
                    ParkingSpaces = table.Column<int>(type: "integer", nullable: false),
                    PriceTo = table.Column<double>(type: "double precision", nullable: false),
                    PriceFrom = table.Column<double>(type: "double precision", nullable: false),
                    GardenFrom = table.Column<int>(type: "integer", nullable: false),
                    GardenTo = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    MortgageAdviceRequired = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Requests_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RealEstatePropertyPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RealEstatePropertyId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Highlighted = table.Column<bool>(type: "boolean", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealEstatePropertyPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RealEstatePropertyPhotos_RealEstateProperties_RealEstatePro~",
                        column: x => x.RealEstatePropertyId,
                        principalTable: "RealEstateProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Calendars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    EventName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    RealEstatePropertyId = table.Column<int>(type: "integer", nullable: true),
                    RequestId = table.Column<int>(type: "integer", nullable: true),
                    EventDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EventLocation = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EventStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    Cancelled = table.Column<bool>(type: "boolean", nullable: false),
                    Postponed = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calendars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calendars_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Calendars_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Calendars_RealEstateProperties_RealEstatePropertyId",
                        column: x => x.RealEstatePropertyId,
                        principalTable: "RealEstateProperties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Calendars_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RequestNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CalendarId = table.Column<int>(type: "integer", nullable: true),
                    RequestId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestNotes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestNotes_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RealEstatePropertyNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CalendarId = table.Column<int>(type: "integer", nullable: true),
                    RealEstatePropertyId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealEstatePropertyNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RealEstatePropertyNotes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RealEstatePropertyNotes_Calendars_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "Calendars",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RealEstatePropertyNotes_RealEstateProperties_RealEstateProp~",
                        column: x => x.RealEstatePropertyId,
                        principalTable: "RealEstateProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SubscriptionId = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StripePaymentIntentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StripeChargeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SubscriptionPlanId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AutoRenew = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastPaymentId = table.Column<int>(type: "integer", nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StripeCustomerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_Payments_LastPaymentId",
                        column: x => x.LastPaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_CreationDate",
                table: "AspNetUsers",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AdminId",
                table: "AspNetUsers",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calendar_EventStartDate",
                table: "Calendars",
                column: "EventStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Calendar_StartDate_EndDate",
                table: "Calendars",
                columns: new[] { "EventStartDate", "EventEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Calendar_UserId_StartDate",
                table: "Calendars",
                columns: new[] { "UserId", "EventStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_CustomerId",
                table: "Calendars",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_RealEstatePropertyId",
                table: "Calendars",
                column: "RealEstatePropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_RequestId",
                table: "Calendars",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotes_CustomerId",
                table: "CustomerNotes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotes_UserId",
                table: "CustomerNotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_UserId_CreationDate",
                table: "Customers",
                columns: new[] { "UserId", "CreationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Documentation_AgencyId",
                table: "Documentation",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentation_UserId",
                table: "Documentation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportHistory_UserId",
                table: "ExportHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_StripePaymentIntentId",
                table: "Payments",
                column: "StripePaymentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId",
                table: "Payments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RealEstateProperties_CustomerId",
                table: "RealEstateProperties",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RealEstateProperty_AgentId_CreationDate",
                table: "RealEstateProperties",
                columns: new[] { "UserId", "CreationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RealEstateProperty_Archived",
                table: "RealEstateProperties",
                column: "Archived");

            migrationBuilder.CreateIndex(
                name: "IX_RealEstateProperty_Category",
                table: "RealEstateProperties",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_RealEstateProperty_City_Category_Status",
                table: "RealEstateProperties",
                columns: new[] { "City", "Category", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RealEstateProperty_Sold",
                table: "RealEstateProperties",
                column: "Sold");

            migrationBuilder.CreateIndex(
                name: "IX_RealEstateProperty_Status",
                table: "RealEstateProperties",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RealEstatePropertyNotes_CalendarId",
                table: "RealEstatePropertyNotes",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_RealEstatePropertyNotes_RealEstatePropertyId",
                table: "RealEstatePropertyNotes",
                column: "RealEstatePropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_RealEstatePropertyNotes_UserId",
                table: "RealEstatePropertyNotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RealEstatePropertyPhotos_RealEstatePropertyId",
                table: "RealEstatePropertyPhotos",
                column: "RealEstatePropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestNotes_RequestId",
                table: "RequestNotes",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestNotes_UserId",
                table: "RequestNotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_Archived",
                table: "Requests",
                column: "Archived");

            migrationBuilder.CreateIndex(
                name: "IX_Request_Closed",
                table: "Requests",
                column: "Closed");

            migrationBuilder.CreateIndex(
                name: "IX_Request_Province_City",
                table: "Requests",
                columns: new[] { "Province", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CustomerId",
                table: "Requests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_UserId",
                table: "Requests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StripeWebhookEvent_EventId",
                table: "StripeWebhookEvents",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionFeatures_SubscriptionPlanId",
                table: "SubscriptionFeatures",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlan_Active",
                table: "SubscriptionPlans",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscription_StripeSubscriptionId",
                table: "UserSubscriptions",
                column: "StripeSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscription_UserId_Status",
                table: "UserSubscriptions",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_LastPaymentId",
                table: "UserSubscriptions",
                column: "LastPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_SubscriptionPlanId",
                table: "UserSubscriptions",
                column: "SubscriptionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_UserSubscriptions_SubscriptionId",
                table: "Payments",
                column: "SubscriptionId",
                principalTable: "UserSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_AspNetUsers_UserId",
                table: "UserSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_UserSubscriptions_SubscriptionId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CustomerNotes");

            migrationBuilder.DropTable(
                name: "Documentation");

            migrationBuilder.DropTable(
                name: "DocumentsTabs");

            migrationBuilder.DropTable(
                name: "ExportHistory");

            migrationBuilder.DropTable(
                name: "RealEstatePropertyNotes");

            migrationBuilder.DropTable(
                name: "RealEstatePropertyPhotos");

            migrationBuilder.DropTable(
                name: "RequestNotes");

            migrationBuilder.DropTable(
                name: "StripeWebhookEvents");

            migrationBuilder.DropTable(
                name: "SubscriptionFeatures");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Calendars");

            migrationBuilder.DropTable(
                name: "RealEstateProperties");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");
        }
    }
}
