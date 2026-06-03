using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication1.Migrations.PostgresDb
{
    /// <inheritdoc />
    public partial class startertables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StarterDocument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StarterPackageId = table.Column<int>(type: "integer", nullable: false),
                    DocumentId = table.Column<string>(type: "text", nullable: false),
                    DocumentName = table.Column<string>(type: "text", nullable: false),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarterDocument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StarterPackage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParcelId = table.Column<string>(type: "text", nullable: false),
                    CoverageFromYear = table.Column<int>(type: "integer", nullable: false),
                    CoverageToYear = table.Column<int>(type: "integer", nullable: false),
                    StarterType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RiskScore = table.Column<int>(type: "integer", nullable: false),
                    InsuranceReady = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StarterJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarterPackage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StarterIssue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StarterPackageId = table.Column<int>(type: "integer", nullable: false),
                    IssueType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Resolved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarterIssue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StarterIssue_StarterPackage_StarterPackageId",
                        column: x => x.StarterPackageId,
                        principalTable: "StarterPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StarterIssue_StarterPackageId",
                table: "StarterIssue",
                column: "StarterPackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StarterDocument");

            migrationBuilder.DropTable(
                name: "StarterIssue");

            migrationBuilder.DropTable(
                name: "StarterPackage");
        }
    }
}
