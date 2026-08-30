using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Constants;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Infrastructure.Persistence.Seeders;

/// <summary>
/// Clase responsable de sembradura de datos iniciales en la base de datos.
/// Responsabilidad única: inyectar datos de prueba de forma organizada.
/// </summary>
public static class DatabaseSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedUsuarios(modelBuilder);
        SeedBilleteras(modelBuilder);
        SeedCategorias(modelBuilder);
        SeedSubastas(modelBuilder);
        SeedPujas(modelBuilder);
        SeedTransacciones(modelBuilder);
    }

    private static void SeedUsuarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                id = 1,
                email = "vendedor@test.com",
                nombre = "Vendedor Test",
                password_hash = "hash123",
                fecha_registro = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Usuario
            {
                id = 2,
                email = "comprador1@test.com",
                nombre = "Comprador 1",
                password_hash = "hash123",
                fecha_registro = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Usuario
            {
                id = 3,
                email = "comprador2@test.com",
                nombre = "Comprador 2",
                password_hash = "hash123",
                fecha_registro = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Usuario
            {
                id = 4,
                email = "sinfondos@test.com",
                nombre = "Sin Fondos",
                password_hash = "hash123",
                fecha_registro = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }

    private static void SeedBilleteras(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Billetera>().HasData(
            new { id = 1, usuario_id = 1, saldo_total = 0m, saldo_retenido = 0m },
            new { id = 2, usuario_id = 2, saldo_total = 150000m, saldo_retenido = 45000m },
            new { id = 3, usuario_id = 3, saldo_total = 200000m, saldo_retenido = 0m },
            new { id = 4, usuario_id = 4, saldo_total = 500m, saldo_retenido = 0m }
        );
    }

    private static void SeedCategorias(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { id = 1, nombre = "Tecnología", url_icono = "/icons/tech.png" },
            new Categoria { id = 2, nombre = "Coleccionables", url_icono = "/icons/collectibles.png" },
            new Categoria { id = 3, nombre = "Indumentaria", url_icono = "/icons/fashion.png" },
            new Categoria { id = 4, nombre = "Vehículos", url_icono = "/icons/cars.png" }
        );
    }

    private static void SeedSubastas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subasta>().HasData(
            new Subasta
            {
                id = 1,
                vendedor_id = 1,
                categoria_id = 1,
                titulo = "iPhone 13 Pro",
                descripcion = "Excelente estado",
                url_imagen = "/img/iphone.jpg",
                precio_base = 30000m,
                incremento_minimo = 5000m,
                fecha_inicio = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                fecha_fin = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                estado = AuctionConstants.ESTADO_ACTIVA
            },
            new Subasta
            {
                id = 2,
                vendedor_id = 1,
                categoria_id = 2,
                titulo = "Figura Coleccionable",
                descripcion = "Edición limitada",
                url_imagen = "/img/figura.jpg",
                precio_base = 10000m,
                incremento_minimo = 1000m,
                fecha_inicio = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                fecha_fin = new DateTime(2025, 1, 1, 10, 2, 0, DateTimeKind.Utc),
                estado = AuctionConstants.ESTADO_ACTIVA
            },
            new Subasta
            {
                id = 3,
                vendedor_id = 1,
                categoria_id = 3,
                titulo = "Campera de Cuero",
                descripcion = "Talle L",
                url_imagen = "/img/campera.jpg",
                precio_base = 20000m,
                incremento_minimo = 2000m,
                fecha_inicio = new DateTime(2025, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                fecha_fin = new DateTime(2025, 1, 3, 10, 0, 0, DateTimeKind.Utc),
                estado = AuctionConstants.ESTADO_PROGRAMADA
            }
        );
    }

    private static void SeedPujas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Puja>().HasData(
            new Puja
            {
                id = 1,
                subasta_id = 1,
                comprador_id = 2,
                monto = 40000m,
                fecha_puja = new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Utc)
            },
            new Puja
            {
                id = 2,
                subasta_id = 1,
                comprador_id = 2,
                monto = 45000m,
                fecha_puja = new DateTime(2025, 1, 1, 11, 0, 0, DateTimeKind.Utc)
            }
        );
    }

    private static void SeedTransacciones(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransaccionLedger>().HasData(
            new TransaccionLedger
            {
                id = 1,
                billetera_id = 2,
                tipo = TransactionConstants.TIPO_DEPOSITO,
                monto = 150000m,
                fecha = new DateTime(2025, 1, 1, 9, 0, 0, DateTimeKind.Utc)
            },
            new TransaccionLedger
            {
                id = 2,
                billetera_id = 2,
                tipo = TransactionConstants.TIPO_RETENCION,
                monto = 45000m,
                fecha = new DateTime(2025, 1, 1, 11, 0, 0, DateTimeKind.Utc),
                subasta_id = 1
            }
        );
    }
}
