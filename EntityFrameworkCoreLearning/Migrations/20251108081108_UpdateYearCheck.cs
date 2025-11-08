using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFrameworkCoreLearning.Migrations
{
    /// <inheritdoc />
    public partial class UpdateYearCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Book_Year_ValidRange",
                table: "books");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Book_Year_ValidRange",
                table: "books",
                sql: "year >= 1600 AND year <= EXTRACT(YEAR FROM NOW())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Book_Year_ValidRange",
                table: "books");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Book_Year_ValidRange",
                table: "books",
                sql: "year >= 1700 AND year <= EXTRACT(YEAR FROM NOW())");
        }
    }
}
