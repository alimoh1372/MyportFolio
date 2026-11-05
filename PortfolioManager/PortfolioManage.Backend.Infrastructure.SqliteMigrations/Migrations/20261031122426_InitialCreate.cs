using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioManage.Backend.Infrastructure.SqliteMigrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartnerShares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PartnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValueRial = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerShares", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioOperations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    PartnerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AmountRial = table.Column<long>(type: "INTEGER", nullable: false),
                    PortfolioBeforeRial = table.Column<long>(type: "INTEGER", nullable: false),
                    PortfolioAfterRial = table.Column<long>(type: "INTEGER", nullable: false),
                    CashBeforeRial = table.Column<long>(type: "INTEGER", nullable: false),
                    CashAfterRial = table.Column<long>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioOperations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CashValueRial = table.Column<long>(type: "INTEGER", nullable: false),
                    StockValueRial = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShareSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OperationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PartnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValueBeforeRial = table.Column<long>(type: "INTEGER", nullable: false),
                    ValueAfterRial = table.Column<long>(type: "INTEGER", nullable: false),
                    RatioBefore = table.Column<decimal>(type: "TEXT", precision: 28, scale: 12, nullable: false),
                    RatioAfter = table.Column<decimal>(type: "TEXT", precision: 28, scale: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_Code",
                table: "Partners",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartnerShares_PartnerId",
                table: "PartnerShares",
                column: "PartnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioOperations_CreatedAtUtc",
                table: "PortfolioOperations",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ShareSnapshots_OperationId_PartnerId",
                table: "ShareSnapshots",
                columns: new[] { "OperationId", "PartnerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.DropTable(
                name: "PartnerShares");

            migrationBuilder.DropTable(
                name: "PortfolioOperations");

            migrationBuilder.DropTable(
                name: "Portfolios");

            migrationBuilder.DropTable(
                name: "ShareSnapshots");
        }
    }
}
