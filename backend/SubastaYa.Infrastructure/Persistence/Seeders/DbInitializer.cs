using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Constants;
using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Identity;

namespace SubastaYa.Infrastructure.Persistence.Seeders;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        if (await context.Subastas.AnyAsync())
        {
            return;
        }

        var ahoraUtc = DateTime.UtcNow;

        // 1. Usuarios en ASP.NET Identity (Consigna: 4 usuarios obligatorios)
        var testUsers = new[]
        {
            new { Id = 1, Email = "vendedor@test.com", Nombre = "Vendedor Test" },
            new { Id = 2, Email = "comprador1@test.com", Nombre = "Comprador 1" },
            new { Id = 3, Email = "comprador2@test.com", Nombre = "Comprador 2" },
            new { Id = 4, Email = "sinfondos@test.com", Nombre = "Sin Fondos" }
        };

        foreach (var tu in testUsers)
        {
            var identityUser = await userManager.FindByEmailAsync(tu.Email);
            if (identityUser == null)
            {
                identityUser = new ApplicationUser
                {
                    UserName = tu.Email,
                    Email = tu.Email,
                    NombreCompleto = tu.Nombre,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(identityUser, "hash123");
            }
        }

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // Entidades de Dominio vinculadas
                var usuarios = testUsers.Select(tu => new Usuario
                {
                    id = tu.Id,
                    email = tu.Email,
                    nombre = tu.Nombre,
                    password_hash = "IDENTITY_MANAGED",
                    fecha_registro = ahoraUtc.AddDays(-30)
                }).ToList();

                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT USUARIO ON");
                await context.Usuarios.AddRangeAsync(usuarios);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT USUARIO OFF");

                // 2. Categorías obligatorias
                var categorias = new List<Categoria>
                {
                    new() { id = 1, nombre = "Tecnología", url_icono = "/icons/tech.png" },
                    new() { id = 2, nombre = "Coleccionables", url_icono = "/icons/collectibles.png" },
                    new() { id = 3, nombre = "Indumentaria", url_icono = "/icons/fashion.png" },
                    new() { id = 4, nombre = "Vehículos", url_icono = "/icons/cars.png" }
                };
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT CATEGORIA ON");
                await context.Categorias.AddRangeAsync(categorias);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT CATEGORIA OFF");

                // 3. Billeteras con saldos iniciales exactos
                var billeteras = new List<Billetera>
                {
                    new() { id = 1, usuario_id = 1, saldo_total = 0m, saldo_retenido = 0m },
                    new() { id = 2, usuario_id = 2, saldo_total = 150000m, saldo_retenido = 45000m },
                    new() { id = 3, usuario_id = 3, saldo_total = 200000m, saldo_retenido = 0m },
                    new() { id = 4, usuario_id = 4, saldo_total = 500m, saldo_retenido = 0m }
                };
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT BILLETERA ON");
                await context.Billeteras.AddRangeAsync(billeteras);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT BILLETERA OFF");

                // 4. Las 5 Subastas obligatorias (Casos de prueba de la cátedra)
                var subastas = new List<Subasta>
                {
                    // Caso 1: Activa estándar (Cierra en 20-30 min con 2 ofertas previas)
                    new()
                    {
                        id = 1,
                        vendedor_id = 1,
                        categoria_id = 1,
                        titulo = "iPhone 13 Pro",
                        descripcion = "iPhone 13 Pro 128GB Grafito. Batería 88%. Impecable estado.",
                        url_imagen = "https://images.unsplash.com/photo-1632661674596-df8be070a5c5?auto=format&fit=crop&w=800&q=80",
                        precio_base = 30000m,
                        incremento_minimo = 5000m,
                        fecha_inicio = ahoraUtc.AddHours(-2),
                        fecha_fin = ahoraUtc.AddMinutes(25), 
                        estado = AuctionConstants.ESTADO_ACTIVA
                    },
                    // Caso 2: Activa crítica 
                    new()
                    {
                        id = 2,
                        vendedor_id = 1,
                        categoria_id = 2,
                        titulo = "Figura Coleccionable",
                        descripcion = "Figura de colección edición limitada con caja original.",
                        url_imagen = "https://s.pacn.ws/1/p/1g8/mushoku-tensei-jobless-reincarnation-17--scale-prepainted-figure-940091.11.jpg?v=thxw9m&width=3500",
                        precio_base = 10000m,
                        incremento_minimo = 1000m,
                        fecha_inicio = ahoraUtc.AddHours(-1),
                        fecha_fin = ahoraUtc.AddMinutes(1).AddSeconds(50), 
                        estado = AuctionConstants.ESTADO_ACTIVA
                    },
                    // Caso 3: Próxima 
                    new()
                    {
                        id = 3,
                        vendedor_id = 1,
                        categoria_id = 4,
                        titulo = "Moto Scooter 125cc",
                        descripcion = "Scooter automática 125cc, único dueño. Lista para transferir.",
                        url_imagen = "https://corrientesmotos.com.ar/wp-content/uploads/2020/11/1.png",
                        precio_base = 100000m,
                        incremento_minimo = 1000m,
                        fecha_inicio = ahoraUtc.AddHours(24), 
                        fecha_fin = ahoraUtc.AddDays(3),
                        estado = AuctionConstants.ESTADO_PROGRAMADA
                    },
                    // Caso 4: Vencida con ganador 
                    new()
                    {
                        id = 4,
                        vendedor_id = 1,
                        categoria_id = 3,
                        titulo = "Campera de Cuero Vintage",
                        descripcion = "Campera de cuero vintage auténtica, talle L. Excelente estado.",
                        url_imagen = "https://images.unsplash.com/photo-1521223890158-f9f7c3d5d504?auto=format&fit=crop&w=800&q=80",
                        precio_base = 20000m,
                        incremento_minimo = 2000m,
                        fecha_inicio = ahoraUtc.AddDays(-2),
                        fecha_fin = ahoraUtc.AddHours(-1), 
                        estado = "FINALIZADA"
                    },
                    // Caso 5: Vencida desierta 
                    new()
                    {
                        id = 5,
                        vendedor_id = 1,
                        categoria_id = 1,
                        titulo = "Monitor Gamer 144Hz",
                        descripcion = "Monitor IPS 24 pulgadas, 144Hz 1ms Freesync.",
                        url_imagen = "https://fullh4rd.com.ar/img/productos/18/monitor-gamer-24-benq-led-zowie-xl2411k-r-dark-grey-144hz-0.jpg",
                        precio_base = 50000m,
                        incremento_minimo = 5000m,
                        fecha_inicio = ahoraUtc.AddDays(-2),
                        fecha_fin = ahoraUtc.AddHours(-1), 
                        estado = "DESIERTA"
                    }
                };
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT SUBASTA ON");
                await context.Subastas.AddRangeAsync(subastas);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT SUBASTA OFF");

                // 5. Historial de las 2 ofertas previas en la subasta activa
                var pujas = new List<Puja>
                {
                    new() { id = 1, subasta_id = 1, comprador_id = 3, monto = 35000m, fecha_puja = ahoraUtc.AddMinutes(-40) },
                    new() { id = 2, subasta_id = 1, comprador_id = 2, monto = 45000m, fecha_puja = ahoraUtc.AddMinutes(-20) }
                };
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT PUJA ON");
                await context.Pujas.AddRangeAsync(pujas);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT PUJA OFF");

                // 6. Transacciones 
                var transacciones = new List<TransaccionLedger>
                {
                    new() { id = 1, billetera_id = 2, tipo = TransactionConstants.TIPO_DEPOSITO, monto = 150000m, fecha = ahoraUtc.AddDays(-1) },
                    new() { id = 2, billetera_id = 2, tipo = TransactionConstants.TIPO_RETENCION, monto = 45000m, fecha = ahoraUtc.AddMinutes(-20), subasta_id = 1 },
                    new() { id = 3, billetera_id = 3, tipo = TransactionConstants.TIPO_DEPOSITO, monto = 200000m, fecha = ahoraUtc.AddDays(-1) },
                    new() { id = 4, billetera_id = 4, tipo = TransactionConstants.TIPO_DEPOSITO, monto = 500m, fecha = ahoraUtc.AddDays(-1) }
                };
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT TRANSACCION_LEDGER ON");
                await context.TransaccionesLedger.AddRangeAsync(transacciones);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT TRANSACCION_LEDGER OFF");

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}