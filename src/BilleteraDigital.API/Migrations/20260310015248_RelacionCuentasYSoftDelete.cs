using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilleteraDigital.API.Migrations
{
    /// <inheritdoc />
    public partial class RelacionCuentasYSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EstaEliminado",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstaEliminado",
                table: "Cuentas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "Cuentas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId",
                table: "Cuentas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cuentas_UsuarioId",
                table: "Cuentas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cuentas_Usuarios_UsuarioId",
                table: "Cuentas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cuentas_Usuarios_UsuarioId",
                table: "Cuentas");

            migrationBuilder.DropIndex(
                name: "IX_Cuentas_UsuarioId",
                table: "Cuentas");

            migrationBuilder.DropColumn(
                name: "EstaEliminado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EstaEliminado",
                table: "Cuentas");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Cuentas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Cuentas");
        }
    }
}
