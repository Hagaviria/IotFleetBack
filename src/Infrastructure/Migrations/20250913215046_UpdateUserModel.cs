using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Primero agregar las nuevas columnas
            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id_perfil",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Identificacion",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombre_completo",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombre_perfil",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TelefonoCelular",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoFijo",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoIdentificacion",
                table: "Users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            // Poblar datos para usuarios existentes ANTES de eliminar las columnas
            migrationBuilder.Sql(@"
                UPDATE ""Users"" 
                SET 
                    ""Identificacion"" = COALESCE(""Id""::text, 'USER_' || EXTRACT(EPOCH FROM ""CreatedAt"")::bigint::text),
                    ""Nombre_completo"" = COALESCE(""FirstName"", 'Usuario') || ' ' || COALESCE(""LastName"", 'Sistema'),
                    ""Id_perfil"" = CASE WHEN ""Role"" = 'Admin' THEN 1 ELSE 2 END,
                    ""Nombre_perfil"" = CASE WHEN ""Role"" = 'Admin' THEN 'Admin' ELSE 'User' END,
                    ""TipoIdentificacion"" = 'CC'
                WHERE ""Identificacion"" = '' OR ""Identificacion"" IS NULL;
            ");

            // Ahora renombrar las columnas existentes
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Users",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "Correo");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Users",
                newName: "Creado_en");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "Users",
                newName: "IX_Users_Correo");

            // Eliminar las columnas que ya no necesitamos
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            // Crear el índice único
            migrationBuilder.CreateIndex(
                name: "IX_Users_Identificacion",
                table: "Users",
                column: "Identificacion",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Identificacion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Id_perfil",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Identificacion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Nombre_completo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Nombre_perfil",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TelefonoCelular",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TelefonoFijo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TipoIdentificacion",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "Users",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "Creado_en",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Correo",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Correo",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: true);
        }
    }
}
