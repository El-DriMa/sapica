using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sapica_backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangedAnimalImageToByteAndAddedCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdoptionPost_AnimalId",
                table: "AdoptionPost");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "AnimalImage");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "AnimalImage",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionPost_AnimalId",
                table: "AdoptionPost",
                column: "AnimalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdoptionPost_AnimalId",
                table: "AdoptionPost");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "AnimalImage");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "AnimalImage",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionPost_AnimalId",
                table: "AdoptionPost",
                column: "AnimalId");
        }
    }
}
