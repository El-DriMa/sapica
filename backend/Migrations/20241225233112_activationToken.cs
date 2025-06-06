﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sapica_backend.Migrations
{
    /// <inheritdoc />
    public partial class activationToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActivationToken",
                table: "Shelter",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActivated",
                table: "Shelter",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivationToken",
                table: "Shelter");

            migrationBuilder.DropColumn(
                name: "IsActivated",
                table: "Shelter");
        }
    }
}
