using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations.PostgresDb
{
    /// <inheritdoc />
    public partial class docjobstablesandIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentProcessingJob",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParcelId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TotalDocuments = table.Column<int>(type: "integer", nullable: false),
                    ProcessedDocuments = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentProcessingJob", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentProcessingItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentProcessingJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentName = table.Column<string>(type: "text", nullable: false),
                    S3Key = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentProcessingItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentProcessingItem_DocumentProcessingJob_DocumentProces~",
                        column: x => x.DocumentProcessingJobId,
                        principalTable: "DocumentProcessingJob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentProcessingItem_DocumentProcessingJobId",
                table: "DocumentProcessingItem",
                column: "DocumentProcessingJobId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentProcessingJob_ParcelId",
                table: "DocumentProcessingJob",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentProcessingJob_Status",
                table: "DocumentProcessingJob",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentProcessingItem");

            migrationBuilder.DropTable(
                name: "DocumentProcessingJob");
        }
    }
}
