using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFrameworkCoreLearning.Migrations
{
    /// <inheritdoc />
    public partial class updateReader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_reader_entity",
                table: "reader_entity");

            migrationBuilder.RenameTable(
                name: "reader_entity",
                newName: "readers");

            migrationBuilder.AddPrimaryKey(
                name: "pk_readers",
                table: "readers",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_readers",
                table: "readers");

            migrationBuilder.RenameTable(
                name: "readers",
                newName: "reader_entity");

            migrationBuilder.AddPrimaryKey(
                name: "pk_reader_entity",
                table: "reader_entity",
                column: "id");
        }
    }
}
