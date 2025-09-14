using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeofenceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Geofences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Center_Latitude = table.Column<double>(type: "double precision", precision: 18, scale: 15, nullable: true),
                    Center_Longitude = table.Column<double>(type: "double precision", precision: 18, scale: 15, nullable: true),
                    Radius = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AlertOnEnter = table.Column<bool>(type: "boolean", nullable: false),
                    AlertOnExit = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Geofences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Geofences_IsActive",
                table: "Geofences",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Geofences_Name",
                table: "Geofences",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Geofences");
        }
    }
}
