using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814
namespace SubastaYa.Infrastructure.Migrations
{
    public partial class RemoveStaticSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BILLETERA",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BILLETERA",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "BILLETERA",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CATEGORIA",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PUJA",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PUJA",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SUBASTA",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SUBASTA",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TRANSACCION_LEDGER",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TRANSACCION_LEDGER",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BILLETERA",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CATEGORIA",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CATEGORIA",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SUBASTA",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "USUARIO",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "USUARIO",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CATEGORIA",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "USUARIO",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "USUARIO",
                keyColumn: "id",
                keyValue: 2);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
