using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubastaYa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentitySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IDENTITY_ROLES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IDENTITY_ROLES", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IDENTITY_USUARIOS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IDENTITY_USUARIOS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IDENTITY_ROL_CLAIMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IDENTITY_ROL_CLAIMS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IDENTITY_ROL_CLAIMS_IDENTITY_ROLES_RoleId",
                        column: x => x.RoleId,
                        principalTable: "IDENTITY_ROLES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IDENTITY_USUARIO_CLAIMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IDENTITY_USUARIO_CLAIMS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IDENTITY_USUARIO_CLAIMS_IDENTITY_USUARIOS_UserId",
                        column: x => x.UserId,
                        principalTable: "IDENTITY_USUARIOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IDENTITY_USUARIO_LOGINS",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IDENTITY_USUARIO_LOGINS", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_IDENTITY_USUARIO_LOGINS_IDENTITY_USUARIOS_UserId",
                        column: x => x.UserId,
                        principalTable: "IDENTITY_USUARIOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IDENTITY_USUARIO_ROLES",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IDENTITY_USUARIO_ROLES", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_IDENTITY_USUARIO_ROLES_IDENTITY_ROLES_RoleId",
                        column: x => x.RoleId,
                        principalTable: "IDENTITY_ROLES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IDENTITY_USUARIO_ROLES_IDENTITY_USUARIOS_UserId",
                        column: x => x.UserId,
                        principalTable: "IDENTITY_USUARIOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IDENTITY_USUARIO_TOKENS",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IDENTITY_USUARIO_TOKENS", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_IDENTITY_USUARIO_TOKENS_IDENTITY_USUARIOS_UserId",
                        column: x => x.UserId,
                        principalTable: "IDENTITY_USUARIOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IDENTITY_ROL_CLAIMS_RoleId",
                table: "IDENTITY_ROL_CLAIMS",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "IDENTITY_ROLES",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IDENTITY_USUARIO_CLAIMS_UserId",
                table: "IDENTITY_USUARIO_CLAIMS",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IDENTITY_USUARIO_LOGINS_UserId",
                table: "IDENTITY_USUARIO_LOGINS",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IDENTITY_USUARIO_ROLES_RoleId",
                table: "IDENTITY_USUARIO_ROLES",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "IDENTITY_USUARIOS",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "IDENTITY_USUARIOS",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IDENTITY_ROL_CLAIMS");

            migrationBuilder.DropTable(
                name: "IDENTITY_USUARIO_CLAIMS");

            migrationBuilder.DropTable(
                name: "IDENTITY_USUARIO_LOGINS");

            migrationBuilder.DropTable(
                name: "IDENTITY_USUARIO_ROLES");

            migrationBuilder.DropTable(
                name: "IDENTITY_USUARIO_TOKENS");

            migrationBuilder.DropTable(
                name: "IDENTITY_ROLES");

            migrationBuilder.DropTable(
                name: "IDENTITY_USUARIOS");
        }
    }
}
