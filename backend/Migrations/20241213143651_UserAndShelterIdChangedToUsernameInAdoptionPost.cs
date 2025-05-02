using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sapica_backend.Migrations
{
    /// <inheritdoc />
    public partial class UserAndShelterIdChangedToUsernameInAdoptionPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdoptionPost_Shelter_ShelterId",
                table: "AdoptionPost");

            migrationBuilder.DropForeignKey(
                name: "FK_AdoptionPost_User_UserId",
                table: "AdoptionPost");

            migrationBuilder.DropIndex(
                name: "IX_AdoptionPost_ShelterId",
                table: "AdoptionPost");

            migrationBuilder.DropIndex(
                name: "IX_AdoptionPost_UserId",
                table: "AdoptionPost");

            migrationBuilder.DropColumn(
                name: "ShelterId",
                table: "AdoptionPost");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AdoptionPost");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "AdoptionPost",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "AdoptionPost");

            migrationBuilder.AddColumn<int>(
                name: "ShelterId",
                table: "AdoptionPost",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "AdoptionPost",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionPost_ShelterId",
                table: "AdoptionPost",
                column: "ShelterId");

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionPost_UserId",
                table: "AdoptionPost",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdoptionPost_Shelter_ShelterId",
                table: "AdoptionPost",
                column: "ShelterId",
                principalTable: "Shelter",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdoptionPost_User_UserId",
                table: "AdoptionPost",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
