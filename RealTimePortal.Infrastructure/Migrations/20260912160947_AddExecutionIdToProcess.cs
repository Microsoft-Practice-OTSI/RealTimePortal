using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealTimePortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutionIdToProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExecutionId",
                table: "Processes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_ExecutionId",
                table: "Processes",
                column: "ExecutionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Processes_ExecutionId",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "ExecutionId",
                table: "Processes");
        }
    }
}
