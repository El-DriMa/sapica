using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sapica_backend.Migrations
{
    /// <inheritdoc />
    public partial class cascadeDeleteAnimalAdoptionPostImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdoptionPost_Animal_AnimalId",
                table: "AdoptionPost");

            migrationBuilder.DropForeignKey(
                name: "FK_AnimalImage_Animal_AnimalId",
                table: "AnimalImage");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "AnimalImage",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_AdoptionPost_Animal_AnimalId",
                table: "AdoptionPost",
                column: "AnimalId",
                principalTable: "Animal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalImage_Animal_AnimalId",
                table: "AnimalImage",
                column: "AnimalId",
                principalTable: "Animal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdoptionPost_Animal_AnimalId",
                table: "AdoptionPost");

            migrationBuilder.DropForeignKey(
                name: "FK_AnimalImage_Animal_AnimalId",
                table: "AnimalImage");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "AnimalImage",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddForeignKey(
                name: "FK_AdoptionPost_Animal_AnimalId",
                table: "AdoptionPost",
                column: "AnimalId",
                principalTable: "Animal",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalImage_Animal_AnimalId",
                table: "AnimalImage",
                column: "AnimalId",
                principalTable: "Animal",
                principalColumn: "Id");
        }
    }
}
