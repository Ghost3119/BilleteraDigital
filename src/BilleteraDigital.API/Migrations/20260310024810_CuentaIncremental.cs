using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilleteraDigital.API.Migrations
{
    /// <inheritdoc />
    public partial class CuentaIncremental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "CuentaNumbers",
                startValue: 1000000000L);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroCuenta",
                table: "Cuentas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "CAST(NEXT VALUE FOR [CuentaNumbers] AS NVARCHAR(20))",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "CuentaNumbers");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroCuenta",
                table: "Cuentas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValueSql: "CAST(NEXT VALUE FOR [CuentaNumbers] AS NVARCHAR(20))");
        }
    }
}
