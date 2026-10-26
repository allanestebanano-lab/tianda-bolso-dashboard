using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TiendaBolsos.Core.Modules.Products.Models;
using TiendaBolsos.Core.Modules.Auth.Models;
using TiendaBolsos.Core.Modules.Sales.Models;
using TiendaBolsos.Core.Modules.Inventory.Models;

namespace TiendaBolsos.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public string? CurrentUser { get; set; }

        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // =========================
        // DBSets
        // =========================

        public DbSet<Product> Productos => Set<Product>();

        public DbSet<Categoria> Categorias => Set<Categoria>();

        public DbSet<Marca> Marcas => Set<Marca>();

        public DbSet<User> Usuarios => Set<User>();

        public DbSet<Venta> Ventas => Set<Venta>();

        public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

        public DbSet<Inventory> Inventories => Set<Inventory>();

        public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();

        public DbSet<Auditoria> Auditorias => Set<Auditoria>();

        // =========================
        // MODEL CONFIGURATION
        // =========================

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // PRODUCTO
            // =========================

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable(
                    "Producto",
                    "Catalogos");

                entity.HasKey(e => e.IdProducto);

                entity.Property(e => e.CodigoProducto)
                    .HasMaxLength(50);

                entity.Property(e => e.Nombre)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500);

                entity.Property(e => e.Precio)
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.Stock)
                    .HasDefaultValue(0);

                entity.Property(e => e.ImagenUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.Property(e => e.FechaRegistro)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Categoria)
                    .WithMany(c => c.Productos)
                    .HasForeignKey(e => e.IdCategoria);

                entity.HasOne(e => e.Marca)
                    .WithMany(m => m.Productos)
                    .HasForeignKey(e => e.IdMarca);
            });

            // =========================
            // CATEGORIA
            // =========================

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable(
                    "Categoria",
                    "Catalogos");

                entity.HasKey(e => e.IdCategoria);

                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(e => e.Nombre)
                    .IsUnique();

                entity.Property(e => e.FechaRegistro)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");
            });

            // =========================
            // MARCA
            // =========================

            modelBuilder.Entity<Marca>(entity =>
            {
                entity.ToTable(
                    "Marca",
                    "Catalogos");

                entity.HasKey(e => e.IdMarca);

                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(e => e.Nombre)
                    .IsUnique();

                entity.Property(e => e.FechaRegistro)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");
            });

            // =========================
            // USUARIO
            // =========================

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable(
                    "Usuario",
                    "Seguridad");

                entity.HasKey(e => e.IdUsuario);

                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Correo)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.HasIndex(e => e.Correo)
                    .IsUnique();

                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Rol)
                    .HasMaxLength(50)
                    .IsRequired()
                    .HasDefaultValue("Usuario");

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.Property(e => e.FechaRegistro)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");
            });

            // =========================
            // VENTA
            // =========================

            modelBuilder.Entity<Venta>(entity =>
            {
                entity.ToTable(
                    "Venta",
                    "Ventas");

                entity.HasKey(e => e.IdVenta);

                entity.Property(e => e.Fecha)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Total)
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.MetodoPago)
                    .HasMaxLength(50);

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // DETALLE VENTA
            // =========================

            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.ToTable(
                    "DetalleVenta",
                    "Ventas");

                entity.HasKey(e => e.IdDetalleVenta);

                entity.Property(e => e.Cantidad)
                    .IsRequired();

                entity.Property(e => e.PrecioUnitario)
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.Subtotal)
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.HasOne(e => e.Venta)
                    .WithMany(v => v.Detalles)
                    .HasForeignKey(e => e.IdVenta)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Producto)
                    .WithMany()
                    .HasForeignKey(e => e.IdProducto)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Metadata.AddTrigger("TR_DescontarStock");
            });

            // =========================
            // INVENTORY
            // =========================

            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("Inventory", "Catalogos");

                entity.HasKey(e => e.IdInventory);

                entity.Property(e => e.Stock)
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.MinStock)
                    .HasDefaultValue(5)
                    .IsRequired();

                entity.Property(e => e.LastUpdate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.IdProducto)
                    .IsUnique();

                entity.HasOne(e => e.Producto)
                    .WithMany()
                    .HasForeignKey(e => e.IdProducto)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // INVENTORY MOVEMENT
            // =========================

            modelBuilder.Entity<InventoryMovement>(entity =>
            {
                entity.ToTable("InventoryMovement", "Catalogos");

                entity.HasKey(e => e.IdMovement);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Quantity)
                    .IsRequired();

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne<Inventory>()
                    .WithMany()
                    .HasForeignKey(e => e.IdInventory)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // AUDITORIA
            // =========================

            modelBuilder.Entity<Auditoria>(entity =>
            {
                entity.ToTable(
                    "Auditoria",
                    "Seguridad");

                entity.HasKey(e => e.IdAuditoria);

                entity.Property(e => e.Tabla)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Accion)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.UsuarioAccion)
                    .HasMaxLength(100);

                entity.Property(e => e.Fecha)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Datos)
                    .HasColumnType("nvarchar(max)");
            });
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await AuditChangesAsync();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private async Task AuditChangesAsync()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Product && (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            foreach (var entry in entries)
            {
                var tableName = "Producto";
                var action = entry.State switch
                {
                    EntityState.Added => "INSERT",
                    EntityState.Modified => "UPDATE",
                    EntityState.Deleted => "DELETE",
                    _ => "UNKNOWN"
                };

                var data = SerializeEntity(entry);

                var audit = new Auditoria
                {
                    Tabla = tableName,
                    Accion = action,
                    UsuarioAccion = CurrentUser,
                    Fecha = DateTime.UtcNow,
                    Datos = data
                };

                Auditorias.Add(audit);
            }
        }

        private static string? SerializeEntity(EntityEntry entry)
        {
            var entity = entry.Entity;
            var properties = entry.Metadata.GetProperties()
                .Where(p => !p.IsKey() || entry.State == EntityState.Added)
                .Select(p => $"{p.Name}: {entry.Property(p.Name).CurrentValue}")
                .ToList();

            return string.Join(", ", properties);
        }
    }
}
