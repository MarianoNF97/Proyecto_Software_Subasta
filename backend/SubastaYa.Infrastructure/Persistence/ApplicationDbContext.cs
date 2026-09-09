using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Identity;
using SubastaYa.Infrastructure.Persistence.Seeders;

namespace SubastaYa.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Billetera> Billeteras => Set<Billetera>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Subasta> Subastas => Set<Subasta>();
    public DbSet<Puja> Pujas => Set<Puja>();
    public DbSet<TransaccionLedger> TransaccionesLedger => Set<TransaccionLedger>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Configuración interna base de Identity
        base.OnModelCreating(modelBuilder);

        // 2. Personalización de tablas de Identity 
        modelBuilder.Entity<ApplicationUser>().ToTable("IDENTITY_USUARIOS");
        modelBuilder.Entity<IdentityRole<int>>().ToTable("IDENTITY_ROLES");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("IDENTITY_USUARIO_ROLES");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("IDENTITY_USUARIO_CLAIMS");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("IDENTITY_USUARIO_LOGINS");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("IDENTITY_ROL_CLAIMS");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("IDENTITY_USUARIO_TOKENS");

        // 3. Mapeo de tablas de dominio
        modelBuilder.Entity<Usuario>().ToTable("USUARIO");
        modelBuilder.Entity<Billetera>().ToTable("BILLETERA");
        modelBuilder.Entity<Categoria>().ToTable("CATEGORIA");
        modelBuilder.Entity<Subasta>().ToTable("SUBASTA");
        modelBuilder.Entity<Puja>().ToTable("PUJA");
        modelBuilder.Entity<TransaccionLedger>().ToTable("TRANSACCION_LEDGER");
        modelBuilder.Entity<AuditoriaLog>().ToTable("AUDITORIA_LOG");

        // Precisiones numéricas monetarias
        modelBuilder.Entity<Billetera>().Property(b => b.saldo_total).HasPrecision(18, 2);
        modelBuilder.Entity<Billetera>().Property(b => b.saldo_retenido).HasPrecision(18, 2);
        modelBuilder.Entity<Subasta>().Property(s => s.precio_base).HasPrecision(18, 2);
        modelBuilder.Entity<Subasta>().Property(s => s.incremento_minimo).HasPrecision(18, 2);
        modelBuilder.Entity<Puja>().Property(p => p.monto).HasPrecision(18, 2);
        modelBuilder.Entity<TransaccionLedger>().Property(t => t.monto).HasPrecision(18, 2);

        // Columna calculada: disponible = total - retenido
        modelBuilder.Entity<Billetera>()
            .Property(b => b.saldo_disponible)
            .HasPrecision(18, 2)
            .HasComputedColumnSql("[saldo_total] - [saldo_retenido]", stored: true);

        // Control de concurrencia optimista (RowVersion)
        modelBuilder.Entity<Billetera>().Property(b => b.version).IsRowVersion();
        modelBuilder.Entity<Subasta>().Property(s => s.version).IsRowVersion();

        // Configuración de Relaciones de Dominio
        modelBuilder.Entity<Usuario>().HasOne(u => u.Billetera).WithOne(b => b.Usuario).HasForeignKey<Billetera>(b => b.usuario_id).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Subasta>().HasOne(s => s.Vendedor).WithMany(u => u.SubastasCreadas).HasForeignKey(s => s.vendedor_id).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Subasta>().HasOne(s => s.Categoria).WithMany(c => c.Subastas).HasForeignKey(s => s.categoria_id).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Puja>().HasOne(p => p.Subasta).WithMany(s => s.Pujas).HasForeignKey(p => p.subasta_id).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Puja>().HasOne(p => p.Comprador).WithMany(u => u.Pujas).HasForeignKey(p => p.comprador_id).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TransaccionLedger>().HasOne(t => t.Billetera).WithMany(b => b.Transacciones).HasForeignKey(t => t.billetera_id).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TransaccionLedger>().HasOne(t => t.Subasta).WithMany(s => s.TransaccionesLedger).HasForeignKey(t => t.subasta_id).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AuditoriaLog>().HasOne(a => a.Usuario).WithMany(u => u.AuditoriaLogs).HasForeignKey(a => a.usuario_id).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        // Sembradura de datos iniciales
        DatabaseSeeder.Seed(modelBuilder);
    }
}