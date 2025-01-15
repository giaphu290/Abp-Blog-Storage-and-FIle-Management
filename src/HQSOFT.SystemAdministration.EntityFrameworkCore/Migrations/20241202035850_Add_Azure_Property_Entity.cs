using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HQSOFT.SystemAdministration.Migrations
{
    /// <inheritdoc />
    public partial class Add_Azure_Property_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AzureConnectionString",
                table: "SystemAdministrationContainers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AzureContainerName",
                table: "SystemAdministrationContainers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AzureConnectionString",
                table: "SystemAdministrationContainers");

            migrationBuilder.DropColumn(
                name: "AzureContainerName",
                table: "SystemAdministrationContainers");
        }
    }
}
