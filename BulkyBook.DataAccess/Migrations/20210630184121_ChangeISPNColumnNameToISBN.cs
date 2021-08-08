using Microsoft.EntityFrameworkCore.Migrations;

namespace BulkyBookApp.DataAccess.Migrations
{
    public partial class ChangeISPNColumnNameToISBN : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ISPN",
                table: "Products",
                newName: "ISBN");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ISBN",
                table: "Products",
                newName: "ISPN");
        }
    }
}
