﻿// <auto-generated />
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snap.Hutao.Server.Migrations
{
    /// <inheritdoc />
    public partial class ImplGachaLogStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Licenses_AspNetUsers_UserId",
                table: "Licenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Licenses",
                table: "Licenses");

            migrationBuilder.RenameTable(
                name: "Licenses",
                newName: "license_application_records");

            migrationBuilder.RenameIndex(
                name: "IX_Licenses_UserId",
                table: "license_application_records",
                newName: "IX_license_application_records_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_license_application_records",
                table: "license_application_records",
                column: "PrimaryId");

            migrationBuilder.CreateTable(
                name: "gacha_statistics",
                columns: table => new
                {
                    PrimaryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Data = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gacha_statistics", x => x.PrimaryId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_license_application_records_AspNetUsers_UserId",
                table: "license_application_records",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_license_application_records_AspNetUsers_UserId",
                table: "license_application_records");

            migrationBuilder.DropTable(
                name: "gacha_statistics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_license_application_records",
                table: "license_application_records");

            migrationBuilder.RenameTable(
                name: "license_application_records",
                newName: "Licenses");

            migrationBuilder.RenameIndex(
                name: "IX_license_application_records_UserId",
                table: "Licenses",
                newName: "IX_Licenses_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Licenses",
                table: "Licenses",
                column: "PrimaryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Licenses_AspNetUsers_UserId",
                table: "Licenses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}