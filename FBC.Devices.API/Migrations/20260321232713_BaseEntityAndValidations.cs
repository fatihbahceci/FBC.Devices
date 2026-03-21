using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBC.Devices.API.Migrations
{
    /// <inheritdoc />
    public partial class BaseEntityAndValidations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateUTC",
                table: "SysUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateUTC",
                table: "DeviceTypes",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateUTC",
                table: "Devices",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Devices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateUTC",
                table: "DeviceGroups",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateUTC",
                table: "DeviceAddresses",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateUTC",
                table: "AddrTypes",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDateUTC",
                table: "SysUsers");

            migrationBuilder.DropColumn(
                name: "CreatedDateUTC",
                table: "DeviceTypes");

            migrationBuilder.DropColumn(
                name: "CreatedDateUTC",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "CreatedDateUTC",
                table: "DeviceGroups");

            migrationBuilder.DropColumn(
                name: "CreatedDateUTC",
                table: "DeviceAddresses");

            migrationBuilder.DropColumn(
                name: "CreatedDateUTC",
                table: "AddrTypes");
        }
    }
}
