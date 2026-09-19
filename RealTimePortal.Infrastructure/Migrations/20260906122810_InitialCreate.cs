using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealTimePortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Processes_ReferenceNumber",
                table: "Processes");

            migrationBuilder.AddColumn<string>(
                name: "InputData",
                table: "ProcessSteps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "ProcessSteps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutputData",
                table: "ProcessSteps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "ProcessSteps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationId",
                table: "Processes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "Processes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProcessType",
                table: "Processes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequestData",
                table: "Processes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Processes_ClientId_ApplicationId_ProcessType",
                table: "Processes",
                columns: new[] { "ClientId", "ApplicationId", "ProcessType" });

            migrationBuilder.CreateIndex(
                name: "IX_Processes_Status_CreatedAt",
                table: "Processes",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Processes_ClientId_ApplicationId_ProcessType",
                table: "Processes");

            migrationBuilder.DropIndex(
                name: "IX_Processes_Status_CreatedAt",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "InputData",
                table: "ProcessSteps");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "ProcessSteps");

            migrationBuilder.DropColumn(
                name: "OutputData",
                table: "ProcessSteps");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "ProcessSteps");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "ProcessType",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "RequestData",
                table: "Processes");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_ReferenceNumber",
                table: "Processes",
                column: "ReferenceNumber");
        }
    }
}
