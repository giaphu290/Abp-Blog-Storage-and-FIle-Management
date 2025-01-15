using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HQSOFT.SystemAdministration.Migrations
{
    /// <inheritdoc />
    public partial class Add_Container_Property_AWS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessKeyId",
                table: "SystemAdministrationContainers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BucketName",
                table: "SystemAdministrationContainers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "SystemAdministrationContainers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecretAccessKey",
                table: "SystemAdministrationContainers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessKeyId",
                table: "SystemAdministrationContainers");

            migrationBuilder.DropColumn(
                name: "BucketName",
                table: "SystemAdministrationContainers");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "SystemAdministrationContainers");

            migrationBuilder.DropColumn(
                name: "SecretAccessKey",
                table: "SystemAdministrationContainers");
        }
    }
}
