using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBC.Devices.Migrations
{
    /// <inheritdoc />
    public partial class SearchMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceSearchMetas",
                columns: table => new
                {
                    DeviceSearchDataId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FieldTable = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    DeviceGroupId = table.Column<int>(type: "INTEGER", nullable: true),
                    DeviceAddrId = table.Column<int>(type: "INTEGER", nullable: true),
                    DeviceAddrTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    FieldName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FieldValue = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceSearchMetas", x => x.DeviceSearchDataId);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_DeviceSearchData_Freq_Fields",
                table: "DeviceSearchMetas",
                columns: new[] { "FieldTable", "DeviceId", "FieldName" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSearchData_DeviceId",
                table: "DeviceSearchMetas",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "UX_DeviceSearchData_Key",
                table: "DeviceSearchMetas",
                columns: new[] { "FieldTable", "DeviceId", "DeviceTypeId", "DeviceGroupId", "DeviceAddrId", "DeviceAddrTypeId", "FieldName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceSearchMetas");
        }
    }
}
