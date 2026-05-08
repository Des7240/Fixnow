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
  
  // Worker Management DbSets
  public DbSet<WorkerProfile> WorkerProfiles => Set<WorkerProfile>();
  public DbSet<WorkerKyc> WorkerKycs => Set<WorkerKyc>();
  public DbSet<WorkerService> WorkerServices => Set<WorkerService>();
  public DbSet<WorkerLocationHistory> WorkerLocationHistories => Set<WorkerLocationHistory>();
  public DbSet<WorkerReview> WorkerReviews => Set<WorkerReview>();

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

      entity.HasOne(u => u.WorkerProfile)
        .WithOne(wp => wp.User)
        .HasForeignKey<WorkerProfile>(wp => wp.UserId)
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

    // ── WorkerProfile ──────────────────────────────────────────────────────────
    modelBuilder.Entity<WorkerProfile>(entity =>
    {
      entity.ToTable("worker_profiles");
      entity.HasKey(wp => wp.UserId);
      entity.Property(wp => wp.AvailabilityStatus).HasConversion<string>().HasMaxLength(50);
    });

    // ── WorkerKyc ──────────────────────────────────────────────────────────────
    modelBuilder.Entity<WorkerKyc>(entity =>
    {
      entity.ToTable("worker_kyc");
      entity.HasKey(k => k.Id);
      entity.Property(k => k.Status).HasConversion<string>().HasMaxLength(50);

      entity.HasOne(k => k.Worker)
        .WithMany(u => u.WorkerKycs)
        .HasForeignKey(k => k.WorkerId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(k => k.Admin)
        .WithMany()
        .HasForeignKey(k => k.VerifiedBy)
        .OnDelete(DeleteBehavior.SetNull);
    });

    // ── WorkerService (Many-to-Many Mapping) ───────────────────────────────────
    modelBuilder.Entity<WorkerService>(entity =>
    {
      entity.ToTable("worker_services");
      entity.HasKey(ws => new { ws.WorkerId, ws.ServiceId });

      entity.HasOne(ws => ws.Worker)
        .WithMany(u => u.WorkerServices)
        .HasForeignKey(ws => ws.WorkerId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(ws => ws.Service)
        .WithMany()
        .HasForeignKey(ws => ws.ServiceId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    // ── WorkerLocationHistory ──────────────────────────────────────────────────
    modelBuilder.Entity<WorkerLocationHistory>(entity =>
    {
      entity.ToTable("worker_location_histories");
      entity.HasKey(lh => lh.Id);
      entity.Property(lh => lh.Location).HasColumnType("geography(Point, 4326)");

      entity.HasOne(lh => lh.Worker)
        .WithMany(u => u.LocationHistories)
        .HasForeignKey(lh => lh.WorkerId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    // ── WorkerReview ───────────────────────────────────────────────────────────
    modelBuilder.Entity<WorkerReview>(entity =>
    {
      entity.ToTable("worker_reviews");
      entity.HasKey(r => r.Id);

      entity.HasOne(r => r.Booking)
        .WithMany()
        .HasForeignKey(r => r.BookingId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(r => r.Customer)
        .WithMany(u => u.ReviewsGiven)
        .HasForeignKey(r => r.CustomerId)
        .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(r => r.Worker)
        .WithMany(u => u.ReviewsReceived)
        .HasForeignKey(r => r.WorkerId)
        .OnDelete(DeleteBehavior.Cascade);
    });
  }
}
