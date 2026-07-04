using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSafetyPro.Infrastructure.Persistence.Migrations
{
    public partial class AddBusinessWorkflows : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcknowledgedAt",
                table: "StopUnsafeWorkReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcknowledgedByUserId",
                table: "StopUnsafeWorkReports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "StopUnsafeWorkReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrectiveActionNotes",
                table: "StopUnsafeWorkReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "StopUnsafeWorkReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResolvedByUserId",
                table: "StopUnsafeWorkReports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "StopUnsafeWorkReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VerifiedByUserId",
                table: "StopUnsafeWorkReports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkStoppedAt",
                table: "StopUnsafeWorkReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotificationType",
                table: "SafetyNotifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "JobSafetyAssessments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedByUserId",
                table: "JobSafetyAssessments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "JobSafetyAssessments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "Injuries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClosedByUserId",
                table: "Injuries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvestigatedAt",
                table: "Injuries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InvestigatedByUserId",
                table: "Injuries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalOutcome",
                table: "Injuries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnToWorkDate",
                table: "Injuries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "CorrectiveActions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "CorrectiveActions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "CorrectiveActions",
                type: "datetime2",
                nullable: true);

            // Status columns use string enum names (HasConversion<string>), not integers.
            // Existing rows keep valid names: e.g. NearMiss 'Closed', StopUnsafeWork 'Resolved'.
            migrationBuilder.Sql("UPDATE SafetyNotifications SET NotificationType = 99 WHERE NotificationType = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcknowledgedAt",
                table: "StopUnsafeWorkReports");

            migrationBuilder.DropColumn(
                name: "AcknowledgedByUserId",
                table: "StopUnsafeWorkReports");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "StopUnsafeWorkReports");

            migrationBuilder.DropColumn(
                name: "CorrectiveActionNotes",
                table: "StopUnsafeWorkReports");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "StopUnsafeWorkReports");

            migrationBuilder.DropColumn(
                name: "ResolvedByUserId",
                table: "StopUnsafeWorkReports");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "StopUnsafeWorkReports");

            migrationBuilder.DropColumn(
                name: "VerifiedByUserId",
                table: "StopUnsafeWorkReports");

            migrationBuilder.DropColumn(
                name: "WorkStoppedAt",
                table: "StopUnsafeWorkReports");

            migrationBuilder.DropColumn(
                name: "NotificationType",
                table: "SafetyNotifications");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Injuries");

            migrationBuilder.DropColumn(
                name: "ClosedByUserId",
                table: "Injuries");

            migrationBuilder.DropColumn(
                name: "InvestigatedAt",
                table: "Injuries");

            migrationBuilder.DropColumn(
                name: "InvestigatedByUserId",
                table: "Injuries");

            migrationBuilder.DropColumn(
                name: "MedicalOutcome",
                table: "Injuries");

            migrationBuilder.DropColumn(
                name: "ReturnToWorkDate",
                table: "Injuries");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "CorrectiveActions");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "CorrectiveActions");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "CorrectiveActions");
        }
    }
}
