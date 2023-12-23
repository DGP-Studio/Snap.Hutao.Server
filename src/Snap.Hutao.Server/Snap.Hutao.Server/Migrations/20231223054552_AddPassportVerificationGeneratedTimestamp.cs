﻿// <auto-generated />
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snap.Hutao.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddPassportVerificationGeneratedTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GeneratedTimestamp",
                table: "passport_verifications",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedTimestamp",
                table: "passport_verifications");
        }
    }
}
