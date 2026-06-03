using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class correctnames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientCompanyBranchesAccess_Branches_BranchId",
                table: "ClientCompanyBranchesAccess");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientCompanyBranchesAccess_Clients_ClientId",
                table: "ClientCompanyBranchesAccess");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientCompanyBranchesAccess_Companies_CompanyId",
                table: "ClientCompanyBranchesAccess");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientCompanyBranchesAccess",
                table: "ClientCompanyBranchesAccess");

            migrationBuilder.RenameTable(
                name: "ClientCompanyBranchesAccess",
                newName: "ClientCompanyBranchAccesses");

            migrationBuilder.RenameIndex(
                name: "IX_ClientCompanyBranchesAccess_CompanyId",
                table: "ClientCompanyBranchAccesses",
                newName: "IX_ClientCompanyBranchAccesses_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientCompanyBranchesAccess_ClientId",
                table: "ClientCompanyBranchAccesses",
                newName: "IX_ClientCompanyBranchAccesses_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientCompanyBranchesAccess_BranchId",
                table: "ClientCompanyBranchAccesses",
                newName: "IX_ClientCompanyBranchAccesses_BranchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientCompanyBranchAccesses",
                table: "ClientCompanyBranchAccesses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientCompanyBranchAccesses_Branches_BranchId",
                table: "ClientCompanyBranchAccesses",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientCompanyBranchAccesses_Clients_ClientId",
                table: "ClientCompanyBranchAccesses",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientCompanyBranchAccesses_Companies_CompanyId",
                table: "ClientCompanyBranchAccesses",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientCompanyBranchAccesses_Branches_BranchId",
                table: "ClientCompanyBranchAccesses");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientCompanyBranchAccesses_Clients_ClientId",
                table: "ClientCompanyBranchAccesses");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientCompanyBranchAccesses_Companies_CompanyId",
                table: "ClientCompanyBranchAccesses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientCompanyBranchAccesses",
                table: "ClientCompanyBranchAccesses");

            migrationBuilder.RenameTable(
                name: "ClientCompanyBranchAccesses",
                newName: "ClientCompanyBranchesAccess");

            migrationBuilder.RenameIndex(
                name: "IX_ClientCompanyBranchAccesses_CompanyId",
                table: "ClientCompanyBranchesAccess",
                newName: "IX_ClientCompanyBranchesAccess_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientCompanyBranchAccesses_ClientId",
                table: "ClientCompanyBranchesAccess",
                newName: "IX_ClientCompanyBranchesAccess_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientCompanyBranchAccesses_BranchId",
                table: "ClientCompanyBranchesAccess",
                newName: "IX_ClientCompanyBranchesAccess_BranchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientCompanyBranchesAccess",
                table: "ClientCompanyBranchesAccess",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientCompanyBranchesAccess_Branches_BranchId",
                table: "ClientCompanyBranchesAccess",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientCompanyBranchesAccess_Clients_ClientId",
                table: "ClientCompanyBranchesAccess",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientCompanyBranchesAccess_Companies_CompanyId",
                table: "ClientCompanyBranchesAccess",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }
    }
}
