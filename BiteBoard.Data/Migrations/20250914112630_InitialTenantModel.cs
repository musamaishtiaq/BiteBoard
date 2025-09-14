using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiteBoard.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialTenantModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TenantDb");

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "TenantDb",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Identifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConnectionString = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ValidTill = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Identifier",
                schema: "TenantDb",
                table: "Tenants",
                column: "Identifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "TenantDb");
        }
    }
}
