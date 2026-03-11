using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataWarehouse.Domain.Migrations
{
    /// <inheritdoc />
    public partial class generalTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "ProcessItemIsProgresses",
                columns: table => new
                {
                    ProcessItemIsProgressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessType = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrentStepOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessItemIsProgresses", x => x.ProcessItemIsProgressId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "SapSyncStatusFronts",
                columns: table => new
                {
                    SapSyncStatusFrontId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SapSyncStatusFronts", x => x.SapSyncStatusFrontId);
                    table.ForeignKey(
                        name: "FK_SapSyncStatusFronts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoles_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId");
                });

            migrationBuilder.CreateTable(
                name: "CompanyUsers",
                columns: table => new
                {
                    CompanyUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyUsers", x => x.CompanyUserId);
                    table.ForeignKey(
                        name: "FK_CompanyUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyUsers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId");
                });

            migrationBuilder.CreateTable(
                name: "ProcessesTypes",
                columns: table => new
                {
                    ProcessesTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessesName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessesTypes", x => x.ProcessesTypeId);
                    table.ForeignKey(
                        name: "FK_ProcessesTypes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId");
                });

            migrationBuilder.CreateTable(
                name: "ProcessSettingApprovals",
                columns: table => new
                {
                    ProcessSettingApprovalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessType = table.Column<int>(type: "int", nullable: false),
                    IgnoreSteps = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessSettingApprovals", x => x.ProcessSettingApprovalId);
                    table.ForeignKey(
                        name: "FK_ProcessSettingApprovals_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId");
                });

            migrationBuilder.CreateTable(
                name: "Saps",
                columns: table => new
                {
                    SapId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SapUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyDB = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Saps", x => x.SapId);
                    table.ForeignKey(
                        name: "FK_Saps_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId");
                });

            migrationBuilder.CreateTable(
                name: "ProcessesTypesDates",
                columns: table => new
                {
                    ProcessesTypesDateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ProcessesTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessesTypesDates", x => x.ProcessesTypesDateId);
                    table.ForeignKey(
                        name: "FK_ProcessesTypesDates_ProcessesTypes_ProcessesTypeId",
                        column: x => x.ProcessesTypeId,
                        principalTable: "ProcessesTypes",
                        principalColumn: "ProcessesTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalSteps",
                columns: table => new
                {
                    ApprovalStepId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StepName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFinalStep = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ProcessSettingApprovalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalSteps", x => x.ApprovalStepId);
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId");
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_ProcessSettingApprovals_ProcessSettingApprovalId",
                        column: x => x.ProcessSettingApprovalId,
                        principalTable: "ProcessSettingApprovals",
                        principalColumn: "ProcessSettingApprovalId");
                });

            migrationBuilder.CreateTable(
                name: "BarCodeSettings",
                columns: table => new
                {
                    BarCodeSettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalLength = table.Column<int>(type: "int", nullable: false),
                    StartsWith = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SapStartPosition = table.Column<int>(type: "int", nullable: false),
                    SapLength = table.Column<int>(type: "int", nullable: false),
                    QuantityStartPosition = table.Column<int>(type: "int", nullable: false),
                    QuantityLength = table.Column<int>(type: "int", nullable: false),
                    IgnoreLastDigit = table.Column<bool>(type: "bit", nullable: false),
                    DefaultUom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarCodeSettings", x => x.BarCodeSettingId);
                    table.ForeignKey(
                        name: "FK_BarCodeSettings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId");
                    table.ForeignKey(
                        name: "FK_BarCodeSettings_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId");
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_Customers_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId");
                });

            migrationBuilder.CreateTable(
                name: "DocumentAttachments",
                columns: table => new
                {
                    DocumentAttachmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentAttachments", x => x.DocumentAttachmentId);
                    table.ForeignKey(
                        name: "FK_DocumentAttachments_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ItemGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UoM = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalesPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatchNumbers = table.Column<bool>(type: "bit", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false),
                    ProcurementType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PurchaseItem = table.Column<bool>(type: "bit", nullable: false),
                    SalesItem = table.Column<bool>(type: "bit", nullable: false),
                    InventoryItem = table.Column<bool>(type: "bit", nullable: false),
                    Valid = table.Column<bool>(type: "bit", nullable: false),
                    Frozen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Items_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId");
                });

            migrationBuilder.CreateTable(
                name: "SapEmployee",
                columns: table => new
                {
                    SapEmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SapEmployee", x => x.SapEmployeeId);
                    table.ForeignKey(
                        name: "FK_SapEmployee_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SapEmployee_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SapSyncPaginations",
                columns: table => new
                {
                    SapSyncPaginationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Skip = table.Column<int>(type: "int", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SapSyncPaginations", x => x.SapSyncPaginationId);
                    table.ForeignKey(
                        name: "FK_SapSyncPaginations_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SapSyncStatuses",
                columns: table => new
                {
                    SapSyncStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SapSyncStatuses", x => x.SapSyncStatusId);
                    table.ForeignKey(
                        name: "FK_SapSyncStatuses_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SapUsers",
                columns: table => new
                {
                    SapUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SapUsers", x => x.SapUserId);
                    table.ForeignKey(
                        name: "FK_SapUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SapUsers_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId");
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    SupplierId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierId);
                    table.ForeignKey(
                        name: "FK_Suppliers_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId");
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.WarehouseId);
                    table.ForeignKey(
                        name: "FK_Warehouses_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId");
                });

            migrationBuilder.CreateTable(
                name: "WmsSyncStatuses",
                columns: table => new
                {
                    WmsSyncStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WmsSyncStatuses", x => x.WmsSyncStatusId);
                    table.ForeignKey(
                        name: "FK_WmsSyncStatuses_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BinLocations",
                columns: table => new
                {
                    BinLocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinLocations", x => x.BinLocationId);
                    table.ForeignKey(
                        name: "FK_BinLocations_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "ItemBarCodes",
                columns: table => new
                {
                    ItemBarCodeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UoMEntry = table.Column<int>(type: "int", nullable: false),
                    AbsEntry = table.Column<int>(type: "int", nullable: false),
                    FreeText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SapFlag = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemBarCodes", x => x.ItemBarCodeId);
                    table.ForeignKey(
                        name: "FK_ItemBarCodes_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemBarCodes_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId");
                });

            migrationBuilder.CreateTable(
                name: "ItemUomGroups",
                columns: table => new
                {
                    ItemUomGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseQty = table.Column<float>(type: "real", nullable: false),
                    UomCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UomEntry = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemUomGroups", x => x.ItemUomGroupId);
                    table.ForeignKey(
                        name: "FK_ItemUomGroups_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemUomGroups_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId");
                });

            migrationBuilder.CreateTable(
                name: "SupplierItems",
                columns: table => new
                {
                    SupplierItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: false),
                    IsPreferred = table.Column<bool>(type: "bit", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierItems", x => x.SupplierItemId);
                    table.ForeignKey(
                        name: "FK_SupplierItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_SupplierItems_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId");
                });

            migrationBuilder.CreateTable(
                name: "CountStocks",
                columns: table => new
                {
                    CountStockId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DocEntry = table.Column<int>(type: "int", nullable: true),
                    DocNum = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountStocks", x => x.CountStockId);
                    table.ForeignKey(
                        name: "FK_CountStocks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CountStocks_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "ProcessApprovals",
                columns: table => new
                {
                    ProcessApprovalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovalStepId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ProcessItemIsProgressId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessApprovals", x => x.ProcessApprovalId);
                    table.ForeignKey(
                        name: "FK_ProcessApprovals_ApprovalSteps_ApprovalStepId",
                        column: x => x.ApprovalStepId,
                        principalTable: "ApprovalSteps",
                        principalColumn: "ApprovalStepId");
                    table.ForeignKey(
                        name: "FK_ProcessApprovals_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProcessApprovals_ProcessItemIsProgresses_ProcessItemIsProgressId",
                        column: x => x.ProcessItemIsProgressId,
                        principalTable: "ProcessItemIsProgresses",
                        principalColumn: "ProcessItemIsProgressId");
                    table.ForeignKey(
                        name: "FK_ProcessApprovals_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrders",
                columns: table => new
                {
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrders", x => x.ProductionOrderId);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionOrders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocEntry = table.Column<int>(type: "int", nullable: true),
                    DocNum = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.PurchaseOrderId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "QuantityAdjustmentStocks",
                columns: table => new
                {
                    QuantityAdjustmentStockId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocEntry = table.Column<int>(type: "int", nullable: true),
                    DocNum = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuantityAdjustmentStocks", x => x.QuantityAdjustmentStockId);
                    table.ForeignKey(
                        name: "FK_QuantityAdjustmentStocks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuantityAdjustmentStocks_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrders",
                columns: table => new
                {
                    SalesOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocEntry = table.Column<int>(type: "int", nullable: true),
                    DocNum = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrders", x => x.SalesOrderId);
                    table.ForeignKey(
                        name: "FK_SalesOrders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_SalesOrders_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId");
                    table.ForeignKey(
                        name: "FK_SalesOrders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "TransferredRequests",
                columns: table => new
                {
                    TransferredRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocEntry = table.Column<int>(type: "int", nullable: true),
                    DocNum = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    DistinationWarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferredRequests", x => x.TransferredRequestId);
                    table.ForeignKey(
                        name: "FK_TransferredRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransferredRequests_Warehouses_DistinationWarehouseId",
                        column: x => x.DistinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                    table.ForeignKey(
                        name: "FK_TransferredRequests_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "UserWarehouses",
                columns: table => new
                {
                    UserWarehousesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWarehouses", x => x.UserWarehousesId);
                    table.ForeignKey(
                        name: "FK_UserWarehouses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserWarehouses_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseItems",
                columns: table => new
                {
                    WarehouseItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WarehouseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InStock = table.Column<double>(type: "float", nullable: true),
                    MinStock = table.Column<double>(type: "float", nullable: true),
                    FinishedGood = table.Column<bool>(type: "bit", nullable: false),
                    HasActiveBOM = table.Column<bool>(type: "bit", nullable: false),
                    IsBatchManaged = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseItems", x => x.WarehouseItemId);
                    table.ForeignKey(
                        name: "FK_WarehouseItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_WarehouseItems_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "DynamicBarCodes",
                columns: table => new
                {
                    DynamicBarCodeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AbsEntry = table.Column<int>(type: "int", nullable: false),
                    SapFlag = table.Column<bool>(type: "bit", nullable: false),
                    ItemBarCodeId = table.Column<int>(type: "int", nullable: false),
                    SapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicBarCodes", x => x.DynamicBarCodeId);
                    table.ForeignKey(
                        name: "FK_DynamicBarCodes_ItemBarCodes_ItemBarCodeId",
                        column: x => x.ItemBarCodeId,
                        principalTable: "ItemBarCodes",
                        principalColumn: "ItemBarCodeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicBarCodes_Saps_SapId",
                        column: x => x.SapId,
                        principalTable: "Saps",
                        principalColumn: "SapId");
                });

            migrationBuilder.CreateTable(
                name: "CountStockItems",
                columns: table => new
                {
                    CountStockItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UoMEntry = table.Column<int>(type: "int", nullable: false),
                    BarCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNum = table.Column<int>(type: "int", nullable: true),
                    CountStockId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountStockItems", x => x.CountStockItemId);
                    table.ForeignKey(
                        name: "FK_CountStockItems_CountStocks_CountStockId",
                        column: x => x.CountStockId,
                        principalTable: "CountStocks",
                        principalColumn: "CountStockId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CountStockItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "ProductionHeaderBatches",
                columns: table => new
                {
                    ProductionHeaderBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionHeaderBatches", x => x.ProductionHeaderBatchId);
                    table.ForeignKey(
                        name: "FK_ProductionHeaderBatches_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "ProductionOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrderItems",
                columns: table => new
                {
                    ProductionOrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProducedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AbsoluteEntry = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderItems", x => x.ProductionOrderItemId);
                    table.ForeignKey(
                        name: "FK_ProductionOrderItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ProductionOrderItems_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "ProductionOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderItems",
                columns: table => new
                {
                    PurchaseOrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UoMEntry = table.Column<int>(type: "int", nullable: false),
                    BarCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNum = table.Column<int>(type: "int", nullable: true),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderItems", x => x.PurchaseOrderItemId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "PurchaseOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptPurchaseOrders",
                columns: table => new
                {
                    ReceiptPurchaseOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocEntry = table.Column<int>(type: "int", nullable: true),
                    DocNum = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptPurchaseOrders", x => x.ReceiptPurchaseOrderId);
                    table.ForeignKey(
                        name: "FK_ReceiptPurchaseOrders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptPurchaseOrders_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "PurchaseOrderId");
                    table.ForeignKey(
                        name: "FK_ReceiptPurchaseOrders_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_ReceiptPurchaseOrders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuantityAdjustmentStockItems",
                columns: table => new
                {
                    QuantityAdjustmentStockItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UoMEntry = table.Column<int>(type: "int", nullable: false),
                    BarCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNum = table.Column<int>(type: "int", nullable: true),
                    QuantityAdjustmentStockId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuantityAdjustmentStockItems", x => x.QuantityAdjustmentStockItemId);
                    table.ForeignKey(
                        name: "FK_QuantityAdjustmentStockItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_QuantityAdjustmentStockItems_QuantityAdjustmentStocks_QuantityAdjustmentStockId",
                        column: x => x.QuantityAdjustmentStockId,
                        principalTable: "QuantityAdjustmentStocks",
                        principalColumn: "QuantityAdjustmentStockId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryNoteOrders",
                columns: table => new
                {
                    DeliveryNoteOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocEntry = table.Column<int>(type: "int", nullable: true),
                    DocNum = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNoteOrders", x => x.DeliveryNoteOrderId);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteOrders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteOrders_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "SalesOrderId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteOrders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderItems",
                columns: table => new
                {
                    SalesOrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UoMEntry = table.Column<int>(type: "int", nullable: false),
                    BarCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNum = table.Column<int>(type: "int", nullable: true),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderItems", x => x.SalesOrderItemId);
                    table.ForeignKey(
                        name: "FK_SalesOrderItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_SalesOrderItems_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "SalesOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferredRequestItems",
                columns: table => new
                {
                    TransferredRequestItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UoMEntry = table.Column<int>(type: "int", nullable: false),
                    BarCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNum = table.Column<int>(type: "int", nullable: true),
                    TransferredRequestId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferredRequestItems", x => x.TransferredRequestItemId);
                    table.ForeignKey(
                        name: "FK_TransferredRequestItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_TransferredRequestItems_TransferredRequests_TransferredRequestId",
                        column: x => x.TransferredRequestId,
                        principalTable: "TransferredRequests",
                        principalColumn: "TransferredRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferredStocks",
                columns: table => new
                {
                    TransferredStockId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocEntry = table.Column<int>(type: "int", nullable: true),
                    DocNum = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransferredRequestId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    DistinationWarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferredStocks", x => x.TransferredStockId);
                    table.ForeignKey(
                        name: "FK_TransferredStocks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransferredStocks_TransferredRequests_TransferredRequestId",
                        column: x => x.TransferredRequestId,
                        principalTable: "TransferredRequests",
                        principalColumn: "TransferredRequestId");
                    table.ForeignKey(
                        name: "FK_TransferredStocks_Warehouses_DistinationWarehouseId",
                        column: x => x.DistinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                    table.ForeignKey(
                        name: "FK_TransferredStocks_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "CountStockBatches",
                columns: table => new
                {
                    CountStockBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CountStockItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountStockBatches", x => x.CountStockBatchId);
                    table.ForeignKey(
                        name: "FK_CountStockBatches_CountStockItems_CountStockItemId",
                        column: x => x.CountStockItemId,
                        principalTable: "CountStockItems",
                        principalColumn: "CountStockItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReceipts",
                columns: table => new
                {
                    ProductionReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionOrderItemId = table.Column<int>(type: "int", nullable: false),
                    ProducedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReceipts", x => x.ProductionReceiptId);
                    table.ForeignKey(
                        name: "FK_ProductionReceipts_ProductionOrderItems_ProductionOrderItemId",
                        column: x => x.ProductionOrderItemId,
                        principalTable: "ProductionOrderItems",
                        principalColumn: "ProductionOrderItemId");
                });

            migrationBuilder.CreateTable(
                name: "GoodsReturnOrders",
                columns: table => new
                {
                    GoodsReturnOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocEntry = table.Column<int>(type: "int", nullable: true),
                    DocNum = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    ReceiptPurchaseOrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReturnOrders", x => x.GoodsReturnOrderId);
                    table.ForeignKey(
                        name: "FK_GoodsReturnOrders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GoodsReturnOrders_ReceiptPurchaseOrders_ReceiptPurchaseOrderId",
                        column: x => x.ReceiptPurchaseOrderId,
                        principalTable: "ReceiptPurchaseOrders",
                        principalColumn: "ReceiptPurchaseOrderId");
                    table.ForeignKey(
                        name: "FK_GoodsReturnOrders_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_GoodsReturnOrders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptPurchaseOrderItems",
                columns: table => new
                {
                    ReceiptPurchaseOrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UoMEntry = table.Column<int>(type: "int", nullable: false),
                    BarCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNum = table.Column<int>(type: "int", nullable: true),
                    ReceiptPurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderItemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptPurchaseOrderItems", x => x.ReceiptPurchaseOrderItemId);
                    table.ForeignKey(
                        name: "FK_ReceiptPurchaseOrderItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ReceiptPurchaseOrderItems_PurchaseOrderItems_PurchaseOrderItemId",
                        column: x => x.PurchaseOrderItemId,
                        principalTable: "PurchaseOrderItems",
                        principalColumn: "PurchaseOrderItemId");
                    table.ForeignKey(
                        name: "FK_ReceiptPurchaseOrderItems_ReceiptPurchaseOrders_ReceiptPurchaseOrderId",
                        column: x => x.ReceiptPurchaseOrderId,
                        principalTable: "ReceiptPurchaseOrders",
                        principalColumn: "ReceiptPurchaseOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuantityAdjustmentStockBatches",
                columns: table => new
                {
                    QuantityAdjustmentStockBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantityAdjustmentStockItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuantityAdjustmentStockBatches", x => x.QuantityAdjustmentStockBatchId);
                    table.ForeignKey(
                        name: "FK_QuantityAdjustmentStockBatches_QuantityAdjustmentStockItems_QuantityAdjustmentStockItemId",
                        column: x => x.QuantityAdjustmentStockItemId,
                        principalTable: "QuantityAdjustmentStockItems",
                        principalColumn: "QuantityAdjustmentStockItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnOrders",
                columns: table => new
                {
                    SalesReturnOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocEntry = table.Column<int>(type: "int", nullable: true),
                    DocNum = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    DeliveryNoteOrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnOrders", x => x.SalesReturnOrderId);
                    table.ForeignKey(
                        name: "FK_SalesReturnOrders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesReturnOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_SalesReturnOrders_DeliveryNoteOrders_DeliveryNoteOrderId",
                        column: x => x.DeliveryNoteOrderId,
                        principalTable: "DeliveryNoteOrders",
                        principalColumn: "DeliveryNoteOrderId");
                    table.ForeignKey(
                        name: "FK_SalesReturnOrders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryNoteItems",
                columns: table => new
                {
                    DeliveryNoteItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UoMEntry = table.Column<int>(type: "int", nullable: false),
                    BarCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNum = table.Column<int>(type: "int", nullable: true),
                    DeliveryNoteOrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderItemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNoteItems", x => x.DeliveryNoteItemId);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteItems_DeliveryNoteOrders_DeliveryNoteOrderId",
                        column: x => x.DeliveryNoteOrderId,
                        principalTable: "DeliveryNoteOrders",
                        principalColumn: "DeliveryNoteOrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteItems_SalesOrderItems_SalesOrderItemId",
                        column: x => x.SalesOrderItemId,
                        principalTable: "SalesOrderItems",
                        principalColumn: "SalesOrderItemId");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderBatches",
                columns: table => new
                {
                    SalesOrderBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesOrderItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderBatches", x => x.SalesOrderBatchId);
                    table.ForeignKey(
                        name: "FK_SalesOrderBatches_SalesOrderItems_SalesOrderItemId",
                        column: x => x.SalesOrderItemId,
                        principalTable: "SalesOrderItems",
                        principalColumn: "SalesOrderItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferredRequestBatches",
                columns: table => new
                {
                    TransferredRequestBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferredRequestItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferredRequestBatches", x => x.TransferredRequestBatchId);
                    table.ForeignKey(
                        name: "FK_TransferredRequestBatches_TransferredRequestItems_TransferredRequestItemId",
                        column: x => x.TransferredRequestItemId,
                        principalTable: "TransferredRequestItems",
                        principalColumn: "TransferredRequestItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceivedStocks",
                columns: table => new
                {
                    ReceivedStockId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DocEntry = table.Column<int>(type: "int", nullable: true),
                    DocNum = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransferredStockId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    SourceWarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivedStocks", x => x.ReceivedStockId);
                    table.ForeignKey(
                        name: "FK_ReceivedStocks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceivedStocks_TransferredStocks_TransferredStockId",
                        column: x => x.TransferredStockId,
                        principalTable: "TransferredStocks",
                        principalColumn: "TransferredStockId");
                    table.ForeignKey(
                        name: "FK_ReceivedStocks_Warehouses_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                    table.ForeignKey(
                        name: "FK_ReceivedStocks_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "TransferredItems",
                columns: table => new
                {
                    TransferredItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UoMEntry = table.Column<int>(type: "int", nullable: false),
                    BarCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNum = table.Column<int>(type: "int", nullable: true),
                    TransferredRequestItemId = table.Column<int>(type: "int", nullable: true),
                    TransferredStockId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferredItems", x => x.TransferredItemId);
                    table.ForeignKey(
                        name: "FK_TransferredItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_TransferredItems_TransferredRequestItems_TransferredRequestItemId",
                        column: x => x.TransferredRequestItemId,
                        principalTable: "TransferredRequestItems",
                        principalColumn: "TransferredRequestItemId");
                    table.ForeignKey(
                        name: "FK_TransferredItems_TransferredStocks_TransferredStockId",
                        column: x => x.TransferredStockId,
                        principalTable: "TransferredStocks",
                        principalColumn: "TransferredStockId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReturnOrderItems",
                columns: table => new
                {
                    GoodsReturnOrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UoMEntry = table.Column<int>(type: "int", nullable: false),
                    BarCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNum = table.Column<int>(type: "int", nullable: true),
                    GoodsReturnOrderId = table.Column<int>(type: "int", nullable: false),
                    ReceiptPurchaseOrderItemId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReturnOrderItems", x => x.GoodsReturnOrderItemId);
                    table.ForeignKey(
                        name: "FK_GoodsReturnOrderItems_GoodsReturnOrders_GoodsReturnOrderId",
                        column: x => x.GoodsReturnOrderId,
                        principalTable: "GoodsReturnOrders",
                        principalColumn: "GoodsReturnOrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodsReturnOrderItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_GoodsReturnOrderItems_ReceiptPurchaseOrderItems_ReceiptPurchaseOrderItemId",
                        column: x => x.ReceiptPurchaseOrderItemId,
                        principalTable: "ReceiptPurchaseOrderItems",
                        principalColumn: "ReceiptPurchaseOrderItemId");
                });

            migrationBuilder.CreateTable(
                name: "ReceiptPurchaseOrderBatches",
                columns: table => new
                {
                    ReceiptPurchaseOrderBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptPurchaseOrderItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptPurchaseOrderBatches", x => x.ReceiptPurchaseOrderBatchId);
                    table.ForeignKey(
                        name: "FK_ReceiptPurchaseOrderBatches_ReceiptPurchaseOrderItems_ReceiptPurchaseOrderItemId",
                        column: x => x.ReceiptPurchaseOrderItemId,
                        principalTable: "ReceiptPurchaseOrderItems",
                        principalColumn: "ReceiptPurchaseOrderItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnOrderItems",
                columns: table => new
                {
                    SalesReturnOrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UoMEntry = table.Column<int>(type: "int", nullable: false),
                    BarCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNum = table.Column<int>(type: "int", nullable: true),
                    SalesReturnOrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    DeliveryNoteItemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnOrderItems", x => x.SalesReturnOrderItemId);
                    table.ForeignKey(
                        name: "FK_SalesReturnOrderItems_DeliveryNoteItems_DeliveryNoteItemId",
                        column: x => x.DeliveryNoteItemId,
                        principalTable: "DeliveryNoteItems",
                        principalColumn: "DeliveryNoteItemId");
                    table.ForeignKey(
                        name: "FK_SalesReturnOrderItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_SalesReturnOrderItems_SalesReturnOrders_SalesReturnOrderId",
                        column: x => x.SalesReturnOrderId,
                        principalTable: "SalesReturnOrders",
                        principalColumn: "SalesReturnOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryNoteBatches",
                columns: table => new
                {
                    DeliveryNoteBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesOrderBatchId = table.Column<int>(type: "int", nullable: true),
                    DeliveryNoteItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNoteBatches", x => x.DeliveryNoteBatchId);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteBatches_DeliveryNoteItems_DeliveryNoteItemId",
                        column: x => x.DeliveryNoteItemId,
                        principalTable: "DeliveryNoteItems",
                        principalColumn: "DeliveryNoteItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteBatches_SalesOrderBatches_SalesOrderBatchId",
                        column: x => x.SalesOrderBatchId,
                        principalTable: "SalesOrderBatches",
                        principalColumn: "SalesOrderBatchId");
                });

            migrationBuilder.CreateTable(
                name: "ReceivedItems",
                columns: table => new
                {
                    ReceivedItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UoMEntry = table.Column<int>(type: "int", nullable: false),
                    BarCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNum = table.Column<int>(type: "int", nullable: true),
                    TransferredItemId = table.Column<int>(type: "int", nullable: true),
                    ReceivedStockId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivedItems", x => x.ReceivedItemId);
                    table.ForeignKey(
                        name: "FK_ReceivedItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ReceivedItems_ReceivedStocks_ReceivedStockId",
                        column: x => x.ReceivedStockId,
                        principalTable: "ReceivedStocks",
                        principalColumn: "ReceivedStockId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceivedItems_TransferredItems_TransferredItemId",
                        column: x => x.TransferredItemId,
                        principalTable: "TransferredItems",
                        principalColumn: "TransferredItemId");
                });

            migrationBuilder.CreateTable(
                name: "TransferredStockBatches",
                columns: table => new
                {
                    TransferredStockBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferredRequestBatchId = table.Column<int>(type: "int", nullable: true),
                    TransferredItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferredStockBatches", x => x.TransferredStockBatchId);
                    table.ForeignKey(
                        name: "FK_TransferredStockBatches_TransferredItems_TransferredItemId",
                        column: x => x.TransferredItemId,
                        principalTable: "TransferredItems",
                        principalColumn: "TransferredItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransferredStockBatches_TransferredRequestBatches_TransferredRequestBatchId",
                        column: x => x.TransferredRequestBatchId,
                        principalTable: "TransferredRequestBatches",
                        principalColumn: "TransferredRequestBatchId");
                });

            migrationBuilder.CreateTable(
                name: "GoodsReturnOrderBatches",
                columns: table => new
                {
                    GoodsReturnOrderBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptPurchaseOrderBatchId = table.Column<int>(type: "int", nullable: true),
                    GoodsReturnOrderItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReturnOrderBatches", x => x.GoodsReturnOrderBatchId);
                    table.ForeignKey(
                        name: "FK_GoodsReturnOrderBatches_GoodsReturnOrderItems_GoodsReturnOrderItemId",
                        column: x => x.GoodsReturnOrderItemId,
                        principalTable: "GoodsReturnOrderItems",
                        principalColumn: "GoodsReturnOrderItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodsReturnOrderBatches_ReceiptPurchaseOrderBatches_ReceiptPurchaseOrderBatchId",
                        column: x => x.ReceiptPurchaseOrderBatchId,
                        principalTable: "ReceiptPurchaseOrderBatches",
                        principalColumn: "ReceiptPurchaseOrderBatchId");
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnOrderBatches",
                columns: table => new
                {
                    SalesReturnOrderBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryNoteBatchId = table.Column<int>(type: "int", nullable: true),
                    SalesReturnOrderItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnOrderBatches", x => x.SalesReturnOrderBatchId);
                    table.ForeignKey(
                        name: "FK_SalesReturnOrderBatches_DeliveryNoteBatches_DeliveryNoteBatchId",
                        column: x => x.DeliveryNoteBatchId,
                        principalTable: "DeliveryNoteBatches",
                        principalColumn: "DeliveryNoteBatchId");
                    table.ForeignKey(
                        name: "FK_SalesReturnOrderBatches_SalesReturnOrderItems_SalesReturnOrderItemId",
                        column: x => x.SalesReturnOrderItemId,
                        principalTable: "SalesReturnOrderItems",
                        principalColumn: "SalesReturnOrderItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceivedStockBatches",
                columns: table => new
                {
                    ReceivedStockBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferredStockBatchId = table.Column<int>(type: "int", nullable: true),
                    ReceivedItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivedStockBatches", x => x.ReceivedStockBatchId);
                    table.ForeignKey(
                        name: "FK_ReceivedStockBatches_ReceivedItems_ReceivedItemId",
                        column: x => x.ReceivedItemId,
                        principalTable: "ReceivedItems",
                        principalColumn: "ReceivedItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceivedStockBatches_TransferredStockBatches_TransferredStockBatchId",
                        column: x => x.TransferredStockBatchId,
                        principalTable: "TransferredStockBatches",
                        principalColumn: "TransferredStockBatchId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_CompanyId",
                table: "ApprovalSteps",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_ProcessSettingApprovalId",
                table: "ApprovalSteps",
                column: "ProcessSettingApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_CompanyId_NormalizedName",
                table: "AspNetRoles",
                columns: new[] { "CompanyId", "NormalizedName" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL AND [NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[CompanyId] IS NULL AND [NormalizedName] IS NOT NULL");

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
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BarCodeSettings_CompanyId",
                table: "BarCodeSettings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BarCodeSettings_SapId",
                table: "BarCodeSettings",
                column: "SapId");

            migrationBuilder.CreateIndex(
                name: "IX_BinLocations_ItemId",
                table: "BinLocations",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_CompanyId",
                table: "CompanyUsers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_UserId",
                table: "CompanyUsers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CountStockBatches_CountStockItemId",
                table: "CountStockBatches",
                column: "CountStockItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CountStockItems_CountStockId",
                table: "CountStockItems",
                column: "CountStockId");

            migrationBuilder.CreateIndex(
                name: "IX_CountStockItems_ItemId",
                table: "CountStockItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CountStocks_UserId",
                table: "CountStocks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CountStocks_WarehouseId",
                table: "CountStocks",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerCode",
                table: "Customers",
                column: "CustomerCode");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerCode_SapId",
                table: "Customers",
                columns: new[] { "CustomerCode", "SapId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SapId",
                table: "Customers",
                column: "SapId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteBatches_DeliveryNoteItemId",
                table: "DeliveryNoteBatches",
                column: "DeliveryNoteItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteBatches_SalesOrderBatchId",
                table: "DeliveryNoteBatches",
                column: "SalesOrderBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteItems_DeliveryNoteOrderId",
                table: "DeliveryNoteItems",
                column: "DeliveryNoteOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteItems_ItemId",
                table: "DeliveryNoteItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteItems_SalesOrderItemId",
                table: "DeliveryNoteItems",
                column: "SalesOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteOrders_CustomerId",
                table: "DeliveryNoteOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteOrders_SalesOrderId",
                table: "DeliveryNoteOrders",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteOrders_UserId",
                table: "DeliveryNoteOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteOrders_WarehouseId",
                table: "DeliveryNoteOrders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAttachments_SapId",
                table: "DocumentAttachments",
                column: "SapId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicBarCodes_ItemBarCodeId",
                table: "DynamicBarCodes",
                column: "ItemBarCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicBarCodes_SapId",
                table: "DynamicBarCodes",
                column: "SapId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrderBatches_GoodsReturnOrderItemId",
                table: "GoodsReturnOrderBatches",
                column: "GoodsReturnOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrderBatches_ReceiptPurchaseOrderBatchId",
                table: "GoodsReturnOrderBatches",
                column: "ReceiptPurchaseOrderBatchId",
                unique: true,
                filter: "[ReceiptPurchaseOrderBatchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrderItems_GoodsReturnOrderId",
                table: "GoodsReturnOrderItems",
                column: "GoodsReturnOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrderItems_ItemId",
                table: "GoodsReturnOrderItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrderItems_ReceiptPurchaseOrderItemId",
                table: "GoodsReturnOrderItems",
                column: "ReceiptPurchaseOrderItemId",
                unique: true,
                filter: "[ReceiptPurchaseOrderItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrders_ReceiptPurchaseOrderId",
                table: "GoodsReturnOrders",
                column: "ReceiptPurchaseOrderId",
                unique: true,
                filter: "[ReceiptPurchaseOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrders_SupplierId",
                table: "GoodsReturnOrders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrders_UserId",
                table: "GoodsReturnOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrders_WarehouseId",
                table: "GoodsReturnOrders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBarcode_SapId_Barcode",
                table: "ItemBarCodes",
                columns: new[] { "SapId", "BarCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemBarCodes_ItemId",
                table: "ItemBarCodes",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemCode",
                table: "Items",
                column: "ItemCode");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemName",
                table: "Items",
                column: "ItemName");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SapId_ItemCode",
                table: "Items",
                columns: new[] { "SapId", "ItemCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemUomGroups_ItemId",
                table: "ItemUomGroups",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUomGroups_SapId",
                table: "ItemUomGroups",
                column: "SapId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessApprovals_ApprovalStepId",
                table: "ProcessApprovals",
                column: "ApprovalStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessApprovals_ProcessItemIsProgressId",
                table: "ProcessApprovals",
                column: "ProcessItemIsProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessApprovals_UserId",
                table: "ProcessApprovals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessApprovals_WarehouseId",
                table: "ProcessApprovals",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessesType_ProcessesName",
                table: "ProcessesTypes",
                column: "ProcessesName");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessesTypes_CompanyId",
                table: "ProcessesTypes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessesTypesDates_ProcessesTypeId",
                table: "ProcessesTypesDates",
                column: "ProcessesTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessItemIsProgresses_ProcessType",
                table: "ProcessItemIsProgresses",
                column: "ProcessType");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessItemIsProgresses_ReferenceId",
                table: "ProcessItemIsProgresses",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessItemIsProgresses_ReferenceId_ProcessType_Status",
                table: "ProcessItemIsProgresses",
                columns: new[] { "ReferenceId", "ProcessType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessSettingApprovals_CompanyId",
                table: "ProcessSettingApprovals",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionHeaderBatches_ProductionOrderId",
                table: "ProductionHeaderBatches",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderItems_ItemId",
                table: "ProductionOrderItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderItems_ProductionOrderId",
                table: "ProductionOrderItems",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderItems_Status_AbsoluteEntry",
                table: "ProductionOrderItems",
                columns: new[] { "Status", "AbsoluteEntry" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_UserId",
                table: "ProductionOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_WarehouseId",
                table: "ProductionOrders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceipts_ProductionOrderItemId",
                table: "ProductionReceipts",
                column: "ProductionOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_ItemId",
                table: "PurchaseOrderItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId",
                table: "PurchaseOrderItems",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_UserId",
                table: "PurchaseOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_WarehouseId",
                table: "PurchaseOrders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_QuantityAdjustmentStockBatches_QuantityAdjustmentStockItemId",
                table: "QuantityAdjustmentStockBatches",
                column: "QuantityAdjustmentStockItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuantityAdjustmentStockItems_ItemId",
                table: "QuantityAdjustmentStockItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuantityAdjustmentStockItems_QuantityAdjustmentStockId",
                table: "QuantityAdjustmentStockItems",
                column: "QuantityAdjustmentStockId");

            migrationBuilder.CreateIndex(
                name: "IX_QuantityAdjustmentStocks_UserId",
                table: "QuantityAdjustmentStocks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuantityAdjustmentStocks_WarehouseId",
                table: "QuantityAdjustmentStocks",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPurchaseOrderBatches_ReceiptPurchaseOrderItemId",
                table: "ReceiptPurchaseOrderBatches",
                column: "ReceiptPurchaseOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPurchaseOrderItems_ItemId",
                table: "ReceiptPurchaseOrderItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPurchaseOrderItems_PurchaseOrderItemId",
                table: "ReceiptPurchaseOrderItems",
                column: "PurchaseOrderItemId",
                unique: true,
                filter: "[PurchaseOrderItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPurchaseOrderItems_ReceiptPurchaseOrderId",
                table: "ReceiptPurchaseOrderItems",
                column: "ReceiptPurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPurchaseOrders_PurchaseOrderId",
                table: "ReceiptPurchaseOrders",
                column: "PurchaseOrderId",
                unique: true,
                filter: "[PurchaseOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPurchaseOrders_SupplierId",
                table: "ReceiptPurchaseOrders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPurchaseOrders_UserId",
                table: "ReceiptPurchaseOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPurchaseOrders_WarehouseId",
                table: "ReceiptPurchaseOrders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedItems_ItemId",
                table: "ReceivedItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedItems_ReceivedStockId",
                table: "ReceivedItems",
                column: "ReceivedStockId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedItems_TransferredItemId",
                table: "ReceivedItems",
                column: "TransferredItemId",
                unique: true,
                filter: "[TransferredItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedStockBatches_ReceivedItemId",
                table: "ReceivedStockBatches",
                column: "ReceivedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedStockBatches_TransferredStockBatchId",
                table: "ReceivedStockBatches",
                column: "TransferredStockBatchId",
                unique: true,
                filter: "[TransferredStockBatchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedStocks_SourceWarehouseId",
                table: "ReceivedStocks",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedStocks_TransferredStockId",
                table: "ReceivedStocks",
                column: "TransferredStockId",
                unique: true,
                filter: "[TransferredStockId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedStocks_UserId",
                table: "ReceivedStocks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedStocks_WarehouseId",
                table: "ReceivedStocks",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderBatches_SalesOrderItemId",
                table: "SalesOrderBatches",
                column: "SalesOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderItems_ItemId",
                table: "SalesOrderItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderItems_SalesOrderId",
                table: "SalesOrderItems",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CustomerId",
                table: "SalesOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_SapId",
                table: "SalesOrders",
                column: "SapId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_UserId",
                table: "SalesOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_WarehouseId",
                table: "SalesOrders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnOrderBatches_DeliveryNoteBatchId",
                table: "SalesReturnOrderBatches",
                column: "DeliveryNoteBatchId",
                unique: true,
                filter: "[DeliveryNoteBatchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnOrderBatches_SalesReturnOrderItemId",
                table: "SalesReturnOrderBatches",
                column: "SalesReturnOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnOrderItems_DeliveryNoteItemId",
                table: "SalesReturnOrderItems",
                column: "DeliveryNoteItemId",
                unique: true,
                filter: "[DeliveryNoteItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnOrderItems_ItemId",
                table: "SalesReturnOrderItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnOrderItems_SalesReturnOrderId",
                table: "SalesReturnOrderItems",
                column: "SalesReturnOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnOrders_CustomerId",
                table: "SalesReturnOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnOrders_DeliveryNoteOrderId",
                table: "SalesReturnOrders",
                column: "DeliveryNoteOrderId",
                unique: true,
                filter: "[DeliveryNoteOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnOrders_UserId",
                table: "SalesReturnOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnOrders_WarehouseId",
                table: "SalesReturnOrders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_SapEmployee_SapId",
                table: "SapEmployee",
                column: "SapId");

            migrationBuilder.CreateIndex(
                name: "IX_SapEmployee_UserId",
                table: "SapEmployee",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Saps_CompanyId",
                table: "Saps",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SapSyncPaginations_SapId",
                table: "SapSyncPaginations",
                column: "SapId");

            migrationBuilder.CreateIndex(
                name: "IX_SapSyncStatuses_SapId",
                table: "SapSyncStatuses",
                column: "SapId");

            migrationBuilder.CreateIndex(
                name: "IX_SapSyncStatusFronts_UserId",
                table: "SapSyncStatusFronts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SapUsers_SapId",
                table: "SapUsers",
                column: "SapId");

            migrationBuilder.CreateIndex(
                name: "IX_SapUsers_UserId",
                table: "SapUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierItems_ItemId",
                table: "SupplierItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierItems_SupplierId",
                table: "SupplierItems",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SapId",
                table: "Suppliers",
                column: "SapId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SupplierCode",
                table: "Suppliers",
                column: "SupplierCode");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SupplierCode_SapId",
                table: "Suppliers",
                columns: new[] { "SupplierCode", "SapId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransferredItems_ItemId",
                table: "TransferredItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredItems_TransferredRequestItemId",
                table: "TransferredItems",
                column: "TransferredRequestItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredItems_TransferredStockId",
                table: "TransferredItems",
                column: "TransferredStockId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredRequestBatches_TransferredRequestItemId",
                table: "TransferredRequestBatches",
                column: "TransferredRequestItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredRequestItems_ItemId",
                table: "TransferredRequestItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredRequestItems_TransferredRequestId",
                table: "TransferredRequestItems",
                column: "TransferredRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredRequests_DistinationWarehouseId",
                table: "TransferredRequests",
                column: "DistinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredRequests_UserId",
                table: "TransferredRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredRequests_WarehouseId",
                table: "TransferredRequests",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredStockBatches_TransferredItemId",
                table: "TransferredStockBatches",
                column: "TransferredItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredStockBatches_TransferredRequestBatchId",
                table: "TransferredStockBatches",
                column: "TransferredRequestBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredStocks_DistinationWarehouseId",
                table: "TransferredStocks",
                column: "DistinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredStocks_TransferredRequestId",
                table: "TransferredStocks",
                column: "TransferredRequestId",
                unique: true,
                filter: "[TransferredRequestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredStocks_UserId",
                table: "TransferredStocks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferredStocks_WarehouseId",
                table: "TransferredStocks",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWarehouses_UserId",
                table: "UserWarehouses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWarehouses_WarehouseId",
                table: "UserWarehouses",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseItems_ItemId_WarehouseId",
                table: "WarehouseItems",
                columns: new[] { "ItemId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseItems_WarehouseId",
                table: "WarehouseItems",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseItems_WarehouseId_FinishedGood_HasActiveBOM",
                table: "WarehouseItems",
                columns: new[] { "WarehouseId", "FinishedGood", "HasActiveBOM" });

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseItems_WarehouseId_ItemId",
                table: "WarehouseItems",
                columns: new[] { "WarehouseId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_SapId",
                table: "Warehouses",
                column: "SapId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_WarehouseCode",
                table: "Warehouses",
                column: "WarehouseCode");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_WarehouseCode_SapId",
                table: "Warehouses",
                columns: new[] { "WarehouseCode", "SapId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WmsSyncStatuses_SapId",
                table: "WmsSyncStatuses",
                column: "SapId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "BarCodeSettings");

            migrationBuilder.DropTable(
                name: "BinLocations");

            migrationBuilder.DropTable(
                name: "CompanyUsers");

            migrationBuilder.DropTable(
                name: "CountStockBatches");

            migrationBuilder.DropTable(
                name: "DocumentAttachments");

            migrationBuilder.DropTable(
                name: "DynamicBarCodes");

            migrationBuilder.DropTable(
                name: "GoodsReturnOrderBatches");

            migrationBuilder.DropTable(
                name: "ItemUomGroups");

            migrationBuilder.DropTable(
                name: "ProcessApprovals");

            migrationBuilder.DropTable(
                name: "ProcessesTypesDates");

            migrationBuilder.DropTable(
                name: "ProductionHeaderBatches");

            migrationBuilder.DropTable(
                name: "ProductionReceipts");

            migrationBuilder.DropTable(
                name: "QuantityAdjustmentStockBatches");

            migrationBuilder.DropTable(
                name: "ReceivedStockBatches");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SalesReturnOrderBatches");

            migrationBuilder.DropTable(
                name: "SapEmployee");

            migrationBuilder.DropTable(
                name: "SapSyncPaginations");

            migrationBuilder.DropTable(
                name: "SapSyncStatuses");

            migrationBuilder.DropTable(
                name: "SapSyncStatusFronts");

            migrationBuilder.DropTable(
                name: "SapUsers");

            migrationBuilder.DropTable(
                name: "SupplierItems");

            migrationBuilder.DropTable(
                name: "UserWarehouses");

            migrationBuilder.DropTable(
                name: "WarehouseItems");

            migrationBuilder.DropTable(
                name: "WmsSyncStatuses");

            migrationBuilder.DropTable(
                name: "CountStockItems");

            migrationBuilder.DropTable(
                name: "ItemBarCodes");

            migrationBuilder.DropTable(
                name: "GoodsReturnOrderItems");

            migrationBuilder.DropTable(
                name: "ReceiptPurchaseOrderBatches");

            migrationBuilder.DropTable(
                name: "ApprovalSteps");

            migrationBuilder.DropTable(
                name: "ProcessItemIsProgresses");

            migrationBuilder.DropTable(
                name: "ProcessesTypes");

            migrationBuilder.DropTable(
                name: "ProductionOrderItems");

            migrationBuilder.DropTable(
                name: "QuantityAdjustmentStockItems");

            migrationBuilder.DropTable(
                name: "ReceivedItems");

            migrationBuilder.DropTable(
                name: "TransferredStockBatches");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "DeliveryNoteBatches");

            migrationBuilder.DropTable(
                name: "SalesReturnOrderItems");

            migrationBuilder.DropTable(
                name: "CountStocks");

            migrationBuilder.DropTable(
                name: "GoodsReturnOrders");

            migrationBuilder.DropTable(
                name: "ReceiptPurchaseOrderItems");

            migrationBuilder.DropTable(
                name: "ProcessSettingApprovals");

            migrationBuilder.DropTable(
                name: "ProductionOrders");

            migrationBuilder.DropTable(
                name: "QuantityAdjustmentStocks");

            migrationBuilder.DropTable(
                name: "ReceivedStocks");

            migrationBuilder.DropTable(
                name: "TransferredItems");

            migrationBuilder.DropTable(
                name: "TransferredRequestBatches");

            migrationBuilder.DropTable(
                name: "SalesOrderBatches");

            migrationBuilder.DropTable(
                name: "DeliveryNoteItems");

            migrationBuilder.DropTable(
                name: "SalesReturnOrders");

            migrationBuilder.DropTable(
                name: "PurchaseOrderItems");

            migrationBuilder.DropTable(
                name: "ReceiptPurchaseOrders");

            migrationBuilder.DropTable(
                name: "TransferredStocks");

            migrationBuilder.DropTable(
                name: "TransferredRequestItems");

            migrationBuilder.DropTable(
                name: "SalesOrderItems");

            migrationBuilder.DropTable(
                name: "DeliveryNoteOrders");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "TransferredRequests");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "SalesOrders");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "Saps");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
