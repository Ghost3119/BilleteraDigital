using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilleteraDigital.API.Migrations
{
    /// <inheritdoc />
    public partial class FixNumeroCuentaIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server no permite ALTER COLUMN para agregar IDENTITY a una columna existente.
            // Pasos:
            //  1. Eliminar el DEFAULT constraint que enlaza NumeroCuenta con la secuencia.
            //  2. Eliminar la secuencia (ya sin referencias).
            //  3. Eliminar el índice único sobre NumeroCuenta.
            //  4. Eliminar la columna nvarchar antigua.
            //  5. Recrear la columna como bigint IDENTITY(1000000000, 1).
            //  6. Recrear el índice único.

            // 1. Eliminar el DEFAULT constraint generado automáticamente por SQL Server
            migrationBuilder.Sql(@"
                DECLARE @constraintName NVARCHAR(256);
                SELECT @constraintName = dc.name
                FROM   sys.default_constraints dc
                JOIN   sys.columns             c  ON dc.parent_object_id = c.object_id
                                                 AND dc.parent_column_id = c.column_id
                JOIN   sys.tables              t  ON c.object_id = t.object_id
                WHERE  t.name = 'Cuentas'
                  AND  c.name = 'NumeroCuenta';
                IF @constraintName IS NOT NULL
                    EXEC('ALTER TABLE [Cuentas] DROP CONSTRAINT [' + @constraintName + ']');
            ");

            // 2. Eliminar la secuencia (ahora sin referencias)
            migrationBuilder.DropSequence(
                name: "CuentaNumbers");

            // 3. Eliminar el índice único (depende de la columna)
            migrationBuilder.DropIndex(
                name: "UX_Cuentas_NumeroCuenta",
                table: "Cuentas");

            // 4. Eliminar la columna nvarchar antigua
            migrationBuilder.DropColumn(
                name: "NumeroCuenta",
                table: "Cuentas");

            // 5. Recrear como bigint IDENTITY(1000000000, 1)
            migrationBuilder.AddColumn<long>(
                name: "NumeroCuenta",
                table: "Cuentas",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1000000000, 1");

            // 6. Recrear el índice único
            migrationBuilder.CreateIndex(
                name: "UX_Cuentas_NumeroCuenta",
                table: "Cuentas",
                column: "NumeroCuenta",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("SqlServer:Identity", "1000000000, 1");
        }
    }
}
