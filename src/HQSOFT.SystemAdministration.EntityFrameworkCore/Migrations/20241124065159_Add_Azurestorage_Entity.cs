using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HQSOFT.SystemAdministration.Migrations
{
    /// <inheritdoc />
    public partial class Add_Azurestorage_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemAdministrationAzurestorages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerName = table.Column<string>(type: "text", nullable: false),
                    ConnectionString = table.Column<string>(type: "text", nullable: false),
                    CreateContainerIfNotExists = table.Column<bool>(type: "boolean", nullable: false),
                    ContainerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAdministrationAzurestorages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemAdministrationAzurestorages_SystemAdministrationConta~",
                        column: x => x.ContainerId,
                        principalTable: "SystemAdministrationContainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemAdministrationAzurestorages_ContainerId",
                table: "SystemAdministrationAzurestorages",
                column: "ContainerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemAdministrationAzurestorages_ContainerName",
                table: "SystemAdministrationAzurestorages",
                column: "ContainerName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemAdministrationAzurestorages");
        }
    }
}
