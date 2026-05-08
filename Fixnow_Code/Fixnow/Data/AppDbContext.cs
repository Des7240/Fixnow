using Fixnow.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Fixnow.Data;

/// <summary>
/// EF Core database context for FixNow application.
/// Supports PostGIS geography via NetTopologySuite.
/// </summary>
public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  public DbSet<User> Users => Set<User>();
  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
  public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
  public DbSet<Booking> Bookings => Set<Booking>();
  public DbSet<BookingStatusHistory> BookingStatusHistories => Set<BookingStatusHistory>();
  public DbSet<WorkerLocation> WorkerLocations => Set<WorkerLocation>();
  public DbSet<BookingMatchingLog> BookingMatchingLogs => Set<BookingMatchingLog>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Enable PostGIS extension
    modelBuilder.HasPostgresExtension("postgis");

    // ── User ──────────────────────────────────────────────────────────────────
    modelBuilder.Entity<User>(entity =>
    {
      entity.ToTable("users");
      entity.HasKey(u => u.Id);
      entity.HasIndex(u => u.Email).IsUnique();
      entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
      entity.Property(u => u.PasswordHash).IsRequired();
      entity.Property(u => u.FullName).IsRequired().HasMaxLength(200);
      entity.Property(u => u.Role).HasConversion<string>();
      entity.Property(u => u.Status).HasMaxLength(50).HasDefaultValue("ACTIVE");

      entity.HasMany(u => u.CustomerBookings)
        .WithOne(b => b.Customer)
        .HasForeignKey(b => b.CustomerId)
        .OnDelete(DeleteBehavior.Restrict);

      entity.HasMany(u => u.WorkerBookings)
        .WithOne(b => b.Worker)
        .HasForeignKey(b => b.WorkerId)
        .OnDelete(DeleteBehavior.SetNull);

      entity.HasOne(u => u.CurrentLocation)
        .WithOne(wl => wl.Worker)
        .HasForeignKey<WorkerLocation>(wl => wl.WorkerId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    // ── RefreshToken ───────────────────────────────────────────────────────────
    modelBuilder.Entity<RefreshToken>(entity =>
    {
      entity.ToTable("refresh_tokens");
      entity.HasKey(r => r.Id);
      entity.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ── ServiceCategory ────────────────────────────────────────────────────────
    modelBuilder.Entity<ServiceCategory>(entity =>
    {
      entity.ToTable("services");
      entity.HasKey(s => s.Id);
      entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
    });

    // ── Booking ────────────────────────────────────────────────────────────────
    modelBuilder.Entity<Booking>(entity =>
    {
      entity.ToTable("bookings");
      entity.HasKey(b => b.Id);
      entity.Property(b => b.Status).HasConversion<string>().HasMaxLength(50);
      entity.Property(b => b.Address).IsRequired();
      entity.Property(b => b.Location)
        .HasColumnType("geography(Point, 4326)");

      entity.HasOne(b => b.Service)
        .WithMany(s => s.Bookings)
        .HasForeignKey(b => b.ServiceId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    // ── BookingStatusHistory ───────────────────────────────────────────────────
    modelBuilder.Entity<BookingStatusHistory>(entity =>
    {
      entity.ToTable("booking_status_histories");
      entity.HasKey(h => h.Id);
      entity.Property(h => h.OldStatus).HasConversion<string>().HasMaxLength(50);
      entity.Property(h => h.NewStatus).HasConversion<string>().HasMaxLength(50);

      entity.HasOne(h => h.Booking)
        .WithMany(b => b.StatusHistories)
        .HasForeignKey(h => h.BookingId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    // ── WorkerLocation ─────────────────────────────────────────────────────────
    modelBuilder.Entity<WorkerLocation>(entity =>
    {
      entity.ToTable("worker_locations");
      entity.HasKey(wl => wl.WorkerId);
      entity.Property(wl => wl.Location)
        .HasColumnType("geography(Point, 4326)");
    });

    // ── BookingMatchingLog ─────────────────────────────────────────────────────
    modelBuilder.Entity<BookingMatchingLog>(entity =>
    {
      entity.ToTable("booking_matching_logs");
      entity.HasKey(l => l.Id);
      entity.Property(l => l.Status).HasConversion<string>().HasMaxLength(50);

      entity.HasOne(l => l.Booking)
        .WithMany(b => b.MatchingLogs)
        .HasForeignKey(l => l.BookingId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(l => l.Worker)
        .WithMany()
        .HasForeignKey(l => l.WorkerId)
        .OnDelete(DeleteBehavior.Cascade);
    });
  }
}
