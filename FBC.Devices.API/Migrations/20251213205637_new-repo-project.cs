using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBC.Devices.API.Migrations
{
    /// <inheritdoc />
    public partial class newrepoproject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "SysUsers",
                newName: "UpdatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "DeletedDate",
                table: "SysUsers",
                newName: "DeletedDateUTC");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "SysUsers",
                newName: "CreatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "DeviceTypes",
                newName: "UpdatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "DeletedDate",
                table: "DeviceTypes",
                newName: "DeletedDateUTC");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "DeviceTypes",
                newName: "CreatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "DeviceSearchMetas",
                newName: "UpdatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "DeletedDate",
                table: "DeviceSearchMetas",
                newName: "DeletedDateUTC");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "DeviceSearchMetas",
                newName: "CreatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "Devices",
                newName: "UpdatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "DeletedDate",
                table: "Devices",
                newName: "DeletedDateUTC");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Devices",
                newName: "CreatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "DeviceGroups",
                newName: "UpdatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "DeletedDate",
                table: "DeviceGroups",
                newName: "DeletedDateUTC");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "DeviceGroups",
                newName: "CreatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "DeviceAddresses",
                newName: "UpdatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "DeletedDate",
                table: "DeviceAddresses",
                newName: "DeletedDateUTC");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "DeviceAddresses",
                newName: "CreatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "AddrTypes",
                newName: "UpdatedDateUTC");

            migrationBuilder.RenameColumn(
                name: "DeletedDate",
                table: "AddrTypes",
                newName: "DeletedDateUTC");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "AddrTypes",
                newName: "CreatedDateUTC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedDateUTC",
                table: "SysUsers",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "DeletedDateUTC",
                table: "SysUsers",
                newName: "DeletedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedDateUTC",
                table: "SysUsers",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedDateUTC",
                table: "DeviceTypes",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "DeletedDateUTC",
                table: "DeviceTypes",
                newName: "DeletedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedDateUTC",
                table: "DeviceTypes",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedDateUTC",
                table: "DeviceSearchMetas",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "DeletedDateUTC",
                table: "DeviceSearchMetas",
                newName: "DeletedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedDateUTC",
                table: "DeviceSearchMetas",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedDateUTC",
                table: "Devices",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "DeletedDateUTC",
                table: "Devices",
                newName: "DeletedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedDateUTC",
                table: "Devices",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedDateUTC",
                table: "DeviceGroups",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "DeletedDateUTC",
                table: "DeviceGroups",
                newName: "DeletedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedDateUTC",
                table: "DeviceGroups",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedDateUTC",
                table: "DeviceAddresses",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "DeletedDateUTC",
                table: "DeviceAddresses",
                newName: "DeletedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedDateUTC",
                table: "DeviceAddresses",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedDateUTC",
                table: "AddrTypes",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "DeletedDateUTC",
                table: "AddrTypes",
                newName: "DeletedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedDateUTC",
                table: "AddrTypes",
                newName: "CreatedDate");
        }
    }
}
