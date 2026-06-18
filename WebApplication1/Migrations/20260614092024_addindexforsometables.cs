using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class addindexforsometables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.AlterColumn<string>(
                name: "StateCode",
                table: "States",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CountyCode",
                table: "Counties",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Companies",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_States_StateCode",
                table: "States",
                column: "StateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Counties_CountyCode",
                table: "Counties",
                column: "CountyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyStateCountyAccesses_CompanyId_CountyId",
                table: "CompanyStateCountyAccesses",
                columns: new[] { "CompanyId", "CountyId" });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_States_StateCode",
                table: "States");

            migrationBuilder.DropIndex(
                name: "IX_Counties_CountyCode",
                table: "Counties");

            migrationBuilder.DropIndex(
                name: "IX_CompanyStateCountyAccesses_CompanyId_CountyId",
                table: "CompanyStateCountyAccesses");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Name",
                table: "Companies");

            migrationBuilder.AlterColumn<string>(
                name: "StateCode",
                table: "States",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CountyCode",
                table: "Counties",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

        
        }
    }
}
