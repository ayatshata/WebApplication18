using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Resident> Residents { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<MaintenanceTask> MaintenanceTasks { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Resident configuration
            builder.Entity<Resident>(entity =>
            {
                entity.HasIndex(r => r.IdentityNumber).IsUnique();
                entity.HasIndex(r => r.RoomNumber);
                entity.HasIndex(r => r.IsActive);

                entity.Property(r => r.MonthlyRent)
                    .HasColumnType("decimal(18,2)");

                entity.HasOne(r => r.ManagedByUser)
                    .WithMany(u => u.ManagedResidents)
                    .HasForeignKey(r => r.ManagedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment configuration
            builder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount)
                    .HasColumnType("decimal(18,2)");

                entity.HasIndex(p => p.PaymentDate);
                entity.HasIndex(p => p.ForMonth);

                entity.HasOne(p => p.Resident)
                    .WithMany(r => r.Payments)
                    .HasForeignKey(p => p.ResidentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.ProcessedByUser)
                    .WithMany(u => u.ProcessedPayments)
                    .HasForeignKey(p => p.ProcessedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification configuration
            builder.Entity<Notification>(entity =>
            {
                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => n.IsRead);
                entity.HasIndex(n => n.CreatedAt);

                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AuditLog configuration
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(a => a.UserId);
                entity.HasIndex(a => a.Timestamp);
                entity.HasIndex(a => a.Action);
                entity.HasIndex(a => a.Entity);

                entity.HasOne(a => a.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MaintenanceTask configuration
            builder.Entity<MaintenanceTask>(entity =>
            {
                entity.HasIndex(m => m.Status);
                entity.HasIndex(m => m.Priority);
                entity.HasIndex(m => m.RoomNumber);
                entity.HasIndex(m => m.ReportedDate);

                entity.Property(m => m.Cost)
                    .HasColumnType("decimal(18,2)");

                entity.HasOne(m => m.ReportedByUser)
                    .WithMany(u => u.CreatedTasks)
                    .HasForeignKey(m => m.ReportedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Expense configuration
            builder.Entity<Expense>(entity =>
            {
                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.ExpenseDate);
                entity.HasIndex(e => e.Category);

                entity.HasOne(e => e.ProcessedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ProcessedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Attendance configuration
            builder.Entity<Attendance>(entity =>
            {
                entity.HasIndex(a => a.ResidentId);
                entity.HasIndex(a => a.Date);
                entity.HasIndex(a => a.IsPresent);

                entity.HasOne(a => a.Resident)
                    .WithMany(r => r.Attendances)
                    .HasForeignKey(a => a.ResidentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.RecordedByUser)
                    .WithMany()
                    .HasForeignKey(a => a.RecordedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Schedule configuration
            builder.Entity<Schedule>(entity =>
            {
                entity.HasIndex(s => s.StartTime);
                entity.HasIndex(s => s.Type);

                entity.HasOne(s => s.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(s => s.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Document configuration
            builder.Entity<Document>(entity =>
            {
                entity.HasIndex(d => d.Category);
                entity.HasIndex(d => d.UploadedAt);
                entity.HasIndex(d => d.IsPublic);

                entity.HasOne(d => d.Resident)
                    .WithMany()
                    .HasForeignKey(d => d.ResidentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.UploadedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Review configuration
            builder.Entity<Review>(entity =>
            {
                entity.HasIndex(r => r.Rating);
                entity.HasIndex(r => r.IsApproved);
                entity.HasIndex(r => r.CreatedAt);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Resident && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    ((Resident)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
                }

                ((Resident)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}