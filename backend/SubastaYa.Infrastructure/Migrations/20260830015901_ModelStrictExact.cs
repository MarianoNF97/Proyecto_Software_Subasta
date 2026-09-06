using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SubastaYa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModelStrictExact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CATEGORIA",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    url_icono = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORIA", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AUDITORIA_LOG",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    entidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    entidad_id = table.Column<int>(type: "int", nullable: false),
                    accion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: true),
                    detalle_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUDITORIA_LOG", x => x.id);
                    table.ForeignKey(
                        name: "FK_AUDITORIA_LOG_USUARIO_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "USUARIO",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BILLETERA",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    saldo_total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_retenido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_disponible = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, computedColumnSql: "[saldo_total] - [saldo_retenido]", stored: true),
                    version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BILLETERA", x => x.id);
                    table.ForeignKey(
                        name: "FK_BILLETERA_USUARIO_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "USUARIO",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SUBASTA",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vendedor_id = table.Column<int>(type: "int", nullable: false),
                    categoria_id = table.Column<int>(type: "int", nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    url_imagen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    precio_base = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    incremento_minimo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUBASTA", x => x.id);
                    table.ForeignKey(
                        name: "FK_SUBASTA_CATEGORIA_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "CATEGORIA",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SUBASTA_USUARIO_vendedor_id",
                        column: x => x.vendedor_id,
                        principalTable: "USUARIO",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PUJA",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    subasta_id = table.Column<int>(type: "int", nullable: false),
                    comprador_id = table.Column<int>(type: "int", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    fecha_puja = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PUJA", x => x.id);
                    table.ForeignKey(
                        name: "FK_PUJA_SUBASTA_subasta_id",
                        column: x => x.subasta_id,
                        principalTable: "SUBASTA",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PUJA_USUARIO_comprador_id",
                        column: x => x.comprador_id,
                        principalTable: "USUARIO",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRANSACCION_LEDGER",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    billetera_id = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    subasta_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSACCION_LEDGER", x => x.id);
                    table.ForeignKey(
                        name: "FK_TRANSACCION_LEDGER_BILLETERA_billetera_id",
                        column: x => x.billetera_id,
                        principalTable: "BILLETERA",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRANSACCION_LEDGER_SUBASTA_subasta_id",
                        column: x => x.subasta_id,
                        principalTable: "SUBASTA",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CATEGORIA",
                columns: new[] { "id", "nombre", "url_icono" },
                values: new object[,]
                {
                    { 1, "Tecnología", "/icons/tech.png" },
                    { 2, "Coleccionables", "/icons/collectibles.png" },
                    { 3, "Indumentaria", "/icons/fashion.png" },
                    { 4, "Vehículos", "/icons/cars.png" }
                });

            migrationBuilder.InsertData(
                table: "USUARIO",
                columns: new[] { "id", "email", "fecha_registro", "nombre", "password_hash" },
                values: new object[,]
                {
                    { 1, "vendedor@test.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vendedor Test", "hash123" },
                    { 2, "comprador1@test.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Comprador 1", "hash123" },
                    { 3, "comprador2@test.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Comprador 2", "hash123" },
                    { 4, "sinfondos@test.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sin Fondos", "hash123" }
                });

            migrationBuilder.InsertData(
                table: "BILLETERA",
                columns: new[] { "id", "saldo_retenido", "saldo_total", "usuario_id" },
                values: new object[,]
                {
                    { 1, 0m, 0m, 1 },
                    { 2, 45000m, 150000m, 2 },
                    { 3, 0m, 200000m, 3 },
                    { 4, 0m, 500m, 4 }
                });

            migrationBuilder.InsertData(
                table: "SUBASTA",
                columns: new[] { "id", "categoria_id", "descripcion", "estado", "fecha_fin", "fecha_inicio", "incremento_minimo", "precio_base", "titulo", "url_imagen", "vendedor_id" },
                values: new object[,]
                {
                    { 1, 1, "Excelente estado", "ACTIVA", new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), 5000m, 30000m, "iPhone 13 Pro", "/img/iphone.jpg", 1 },
                    { 2, 2, "Edición limitada", "ACTIVA", new DateTime(2025, 1, 1, 10, 2, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), 1000m, 10000m, "Figura Coleccionable", "/img/figura.jpg", 1 },
                    { 3, 3, "Talle L", "PROGRAMADA", new DateTime(2025, 1, 3, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc), 2000m, 20000m, "Campera de Cuero", "/img/campera.jpg", 1 }
                });

            migrationBuilder.InsertData(
                table: "PUJA",
                columns: new[] { "id", "comprador_id", "fecha_puja", "monto", "subasta_id" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2025, 1, 1, 10, 30, 0, 0, DateTimeKind.Utc), 40000m, 1 },
                    { 2, 2, new DateTime(2025, 1, 1, 11, 0, 0, 0, DateTimeKind.Utc), 45000m, 1 }
                });

            migrationBuilder.InsertData(
                table: "TRANSACCION_LEDGER",
                columns: new[] { "id", "billetera_id", "fecha", "monto", "subasta_id", "tipo" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2025, 1, 1, 9, 0, 0, 0, DateTimeKind.Utc), 150000m, null, "DEPOSITO" },
                    { 2, 2, new DateTime(2025, 1, 1, 11, 0, 0, 0, DateTimeKind.Utc), 45000m, 1, "RETENCION" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AUDITORIA_LOG_usuario_id",
                table: "AUDITORIA_LOG",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_BILLETERA_usuario_id",
                table: "BILLETERA",
                column: "usuario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PUJA_comprador_id",
                table: "PUJA",
                column: "comprador_id");

            migrationBuilder.CreateIndex(
                name: "IX_PUJA_subasta_id",
                table: "PUJA",
                column: "subasta_id");

            migrationBuilder.CreateIndex(
                name: "IX_SUBASTA_categoria_id",
                table: "SUBASTA",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_SUBASTA_vendedor_id",
                table: "SUBASTA",
                column: "vendedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_TRANSACCION_LEDGER_billetera_id",
                table: "TRANSACCION_LEDGER",
                column: "billetera_id");

            migrationBuilder.CreateIndex(
                name: "IX_TRANSACCION_LEDGER_subasta_id",
                table: "TRANSACCION_LEDGER",
                column: "subasta_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AUDITORIA_LOG");

            migrationBuilder.DropTable(
                name: "PUJA");

            migrationBuilder.DropTable(
                name: "TRANSACCION_LEDGER");

            migrationBuilder.DropTable(
                name: "BILLETERA");

            migrationBuilder.DropTable(
                name: "SUBASTA");

            migrationBuilder.DropTable(
                name: "CATEGORIA");

            migrationBuilder.DropTable(
                name: "USUARIO");
        }
    }
}
