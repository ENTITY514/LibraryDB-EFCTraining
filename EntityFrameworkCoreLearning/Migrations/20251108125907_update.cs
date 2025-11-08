using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFrameworkCoreLearning.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_borrow_entity_books_book_id",
                table: "borrow_entity");

            migrationBuilder.DropForeignKey(
                name: "fk_borrow_entity_readers_reader_id",
                table: "borrow_entity");

            migrationBuilder.DropPrimaryKey(
                name: "pk_borrow_entity",
                table: "borrow_entity");

            migrationBuilder.RenameTable(
                name: "borrow_entity",
                newName: "borrows");

            migrationBuilder.RenameIndex(
                name: "ix_borrow_entity_reader_id",
                table: "borrows",
                newName: "ix_borrows_reader_id");

            migrationBuilder.RenameIndex(
                name: "ix_borrow_entity_book_id",
                table: "borrows",
                newName: "ix_borrows_book_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_borrows",
                table: "borrows",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_borrows_books_book_id",
                table: "borrows",
                column: "book_id",
                principalTable: "books",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_borrows_readers_reader_id",
                table: "borrows",
                column: "reader_id",
                principalTable: "readers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_borrows_books_book_id",
                table: "borrows");

            migrationBuilder.DropForeignKey(
                name: "fk_borrows_readers_reader_id",
                table: "borrows");

            migrationBuilder.DropPrimaryKey(
                name: "pk_borrows",
                table: "borrows");

            migrationBuilder.RenameTable(
                name: "borrows",
                newName: "borrow_entity");

            migrationBuilder.RenameIndex(
                name: "ix_borrows_reader_id",
                table: "borrow_entity",
                newName: "ix_borrow_entity_reader_id");

            migrationBuilder.RenameIndex(
                name: "ix_borrows_book_id",
                table: "borrow_entity",
                newName: "ix_borrow_entity_book_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_borrow_entity",
                table: "borrow_entity",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_borrow_entity_books_book_id",
                table: "borrow_entity",
                column: "book_id",
                principalTable: "books",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_borrow_entity_readers_reader_id",
                table: "borrow_entity",
                column: "reader_id",
                principalTable: "readers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
