using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilleteraDigital.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacciones_Cuentas_CuentaOrigenId",
                table: "Transacciones");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Usuarios",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaId",
                table: "Transacciones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_CuentaId",
                table: "Transacciones",
                column: "CuentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacciones_Cuentas_CuentaId",
                table: "Transacciones",
                column: "CuentaId",
                principalTable: "Cuentas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacciones_Cuentas_CuentaId",
                table: "Transacciones");

            migrationBuilder.DropIndex(
                name: "IX_Transacciones_CuentaId",
                table: "Transacciones");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CuentaId",
                table: "Transacciones");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacciones_Cuentas_CuentaOrigenId",
                table: "Transacciones",
                column: "CuentaOrigenId",
                principalTable: "Cuentas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
