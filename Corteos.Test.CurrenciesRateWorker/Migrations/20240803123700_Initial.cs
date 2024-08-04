using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corteos.Test.CurrenciesRateWorker.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    ISONumCodeId = table.Column<int>(type: "integer", nullable: false),
                    ISOCharCode = table.Column<string>(type: "text", nullable: false),
                    CurrencyName = table.Column<string>(type: "text", nullable: false),
                    CurrencyEngName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.ISONumCodeId);
                });

            migrationBuilder.CreateTable(
                name: "CurrenciesRate",
                columns: table => new
                {
                    CurrencyRateDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NumCodeId = table.Column<int>(type: "integer", nullable: false),
                    Nominal = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrenciesRate", x => new { x.NumCodeId, x.CurrencyRateDate });
                    table.ForeignKey(
                        name: "FK_CurrenciesRate_Currencies_NumCodeId",
                        column: x => x.NumCodeId,
                        principalTable: "Currencies",
                        principalColumn: "ISONumCodeId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrenciesRate");

            migrationBuilder.DropTable(
                name: "Currencies");
        }
    }
}
