using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSafetyPro.Infrastructure.Persistence.Migrations
{
    public partial class AddPpeManagement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PpeCatalogueItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuantityInStock = table.Column<int>(type: "int", nullable: false),
                    MinimumStockLevel = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpeCatalogueItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PpeRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WorkDepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Section = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PpeCatalogueItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequiredByDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DispatchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectedByEmployee = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmployeeSignature = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SafetyOfficerSignature = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DispatchNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CollectedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PpeRequests_PpeCatalogueItems_PpeCatalogueItemId",
                        column: x => x.PpeCatalogueItemId,
                        principalTable: "PpeCatalogueItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PpeRequests_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PpeRequests_Users_EmployeeUserId",
                        column: x => x.EmployeeUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PpeRequests_Users_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PpeRequests_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PpeRequests_WorkDepartments_WorkDepartmentId",
                        column: x => x.WorkDepartmentId,
                        principalTable: "WorkDepartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PpeRequestStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PpeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActionByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionByUserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpeRequestStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PpeRequestStatusHistories_PpeRequests_PpeRequestId",
                        column: x => x.PpeRequestId,
                        principalTable: "PpeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PpeRequestStatusHistories_Users_ActionByUserId",
                        column: x => x.ActionByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PpeCatalogueItems_ItemName",
                table: "PpeCatalogueItems",
                column: "ItemName");

            migrationBuilder.CreateIndex(
                name: "IX_PpeRequests_ApprovedByUserId",
                table: "PpeRequests",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PpeRequests_EmployeeUserId",
                table: "PpeRequests",
                column: "EmployeeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PpeRequests_IssuedByUserId",
                table: "PpeRequests",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PpeRequests_PpeCatalogueItemId",
                table: "PpeRequests",
                column: "PpeCatalogueItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PpeRequests_RequestedByUserId",
                table: "PpeRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PpeRequests_RequestNumber",
                table: "PpeRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PpeRequests_RequiredByDate",
                table: "PpeRequests",
                column: "RequiredByDate");

            migrationBuilder.CreateIndex(
                name: "IX_PpeRequests_Status",
                table: "PpeRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PpeRequests_WorkDepartmentId",
                table: "PpeRequests",
                column: "WorkDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PpeRequestStatusHistories_ActionByUserId",
                table: "PpeRequestStatusHistories",
                column: "ActionByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PpeRequestStatusHistories_PpeRequestId",
                table: "PpeRequestStatusHistories",
                column: "PpeRequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PpeRequestStatusHistories");

            migrationBuilder.DropTable(
                name: "PpeRequests");

            migrationBuilder.DropTable(
                name: "PpeCatalogueItems");
        }
    }
}
