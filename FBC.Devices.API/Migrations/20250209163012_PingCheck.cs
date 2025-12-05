using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBC.Devices.Migrations
{
    /// <inheritdoc />
    public partial class PingCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PeriodicPingCheck",
                table: "DeviceAddresses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PeriodicPingCheck",
                table: "DeviceAddresses");
        }
    }
}
