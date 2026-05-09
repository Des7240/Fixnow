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
  
  // Notification & Rating DbSets
  public DbSet<Notification> Notifications => Set<Notification>();
  public DbSet<WorkerRatingSummary> WorkerRatingSummaries => Set<WorkerRatingSummary>();

  // P2 DbSets
  public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();
  public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

  // P3 Payment DbSets
  public DbSet<Payment> Payments => Set<Payment>();
  public DbSet<Transaction> Transactions => Set<Transaction>();
  public DbSet<PaymentCallback> PaymentCallbacks => Set<PaymentCallback>();
  public DbSet<BookingFinancial> BookingFinancials => Set<BookingFinancial>();

  // P3 Chat DbSets
  public DbSet<Conversation> Conversations => Set<Conversation>();
  public DbSet<Message> Messages => Set<Message>();
  public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();

  // P3 Quotation DbSets
  public DbSet<Quotation> Quotations => Set<Quotation>();
  public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();

  // P4 Wallet DbSets
  public DbSet<Wallet> Wallets => Set<Wallet>();
  public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
  public DbSet<Withdrawal> Withdrawals => Set<Withdrawal>();

  // P4 Dispute DbSets
  public DbSet<Dispute> Disputes => Set<Dispute>();
  public DbSet<DisputeEvidence> DisputeEvidences => Set<DisputeEvidence>();
  public DbSet<Refund> Refunds => Set<Refund>();

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

    // ── Notification ────────────────────────────────────────────────────────────
    modelBuilder.Entity<Notification>(entity =>
    {
      entity.ToTable("notifications");
      entity.HasKey(n => n.Id);
      entity.HasIndex(n => n.UserId);
      entity.HasIndex(n => n.IsRead);
      entity.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ── WorkerRatingSummary ──────────────────────────────────────────────────────
    modelBuilder.Entity<WorkerRatingSummary>(entity =>
    {
      entity.ToTable("worker_rating_summaries");
      entity.HasKey(w => w.WorkerId);
      entity.HasOne(w => w.Worker)
            .WithOne()
            .HasForeignKey<WorkerRatingSummary>(w => w.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);
    });
    // ── UploadedFile ─────────────────────────────────────────────────────────────
    modelBuilder.Entity<UploadedFile>(entity =>
    {
      entity.ToTable("uploaded_files");
      entity.HasKey(f => f.Id);
      entity.HasOne(f => f.Uploader)
            .WithMany()
            .HasForeignKey(f => f.UploadedBy)
            .OnDelete(DeleteBehavior.SetNull);
    });

    // ── AuditLog ─────────────────────────────────────────────────────────────────
    modelBuilder.Entity<AuditLog>(entity =>
    {
      entity.ToTable("audit_logs");
      entity.HasKey(a => a.Id);
      entity.HasIndex(a => a.CreatedAt);
    });

    // ── Payment ──────────────────────────────────────────────────────────────────
    modelBuilder.Entity<Payment>(entity =>
    {
      entity.ToTable("payments");
      entity.HasKey(p => p.Id);
      entity.Property(p => p.Provider).HasConversion<string>();
      entity.Property(p => p.Status).HasConversion<string>();
      entity.Property(p => p.Type).HasConversion<string>();
      entity.HasOne(p => p.Booking)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
      entity.HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // ── Transaction ──────────────────────────────────────────────────────────────
    modelBuilder.Entity<Transaction>(entity =>
    {
      entity.ToTable("transactions");
      entity.HasKey(t => t.Id);
      entity.Property(t => t.Status).HasConversion<string>();
      entity.HasOne(t => t.Payment)
            .WithMany(p => p.Transactions)
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ── PaymentCallback ──────────────────────────────────────────────────────────
    modelBuilder.Entity<PaymentCallback>(entity =>
    {
      entity.ToTable("payment_callbacks");
      entity.HasKey(pc => pc.Id);
      entity.Property(pc => pc.Provider).HasConversion<string>();
    });

    // ── BookingFinancial ─────────────────────────────────────────────────────────
    modelBuilder.Entity<BookingFinancial>(entity =>
    {
      entity.ToTable("booking_financials");
      entity.HasKey(bf => bf.BookingId);
      entity.HasOne(bf => bf.Booking)
            .WithOne(b => b.Financial)
            .HasForeignKey<BookingFinancial>(bf => bf.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ── Chat: Conversation ───────────────────────────────────────────────────────
    modelBuilder.Entity<Conversation>(entity =>
    {
      entity.ToTable("conversations");
      entity.HasKey(c => c.Id);
      entity.HasOne(c => c.Booking)
            .WithMany()
            .HasForeignKey(c => c.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
      entity.HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
      entity.HasOne(c => c.Worker)
            .WithMany()
            .HasForeignKey(c => c.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // ── Chat: Message ────────────────────────────────────────────────────────────
    modelBuilder.Entity<Message>(entity =>
    {
      entity.ToTable("messages");
      entity.HasKey(m => m.Id);
      entity.Property(m => m.MessageType).HasConversion<string>();
      entity.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
      entity.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // ── Chat: MessageAttachment ──────────────────────────────────────────────────
    modelBuilder.Entity<MessageAttachment>(entity =>
    {
      entity.ToTable("message_attachments");
      entity.HasKey(ma => ma.Id);
      entity.HasOne(ma => ma.Message)
            .WithMany(m => m.Attachments)
            .HasForeignKey(ma => ma.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
      entity.HasOne(ma => ma.File)
            .WithMany()
            .HasForeignKey(ma => ma.FileId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // ── Quotation ────────────────────────────────────────────────────────────────
    modelBuilder.Entity<Quotation>(entity =>
    {
      entity.ToTable("quotations");
      entity.HasKey(q => q.Id);
      entity.Property(q => q.Status).HasConversion<string>();
      entity.HasOne(q => q.Booking)
            .WithMany(b => b.Quotations)
            .HasForeignKey(q => q.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
      entity.HasOne(q => q.Worker)
            .WithMany()
            .HasForeignKey(q => q.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);
      entity.HasOne(q => q.Customer)
            .WithMany()
            .HasForeignKey(q => q.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // ── QuotationItem ────────────────────────────────────────────────────────────
    modelBuilder.Entity<QuotationItem>(entity =>
    {
      entity.ToTable("quotation_items");
      entity.HasKey(qi => qi.Id);
      entity.HasOne(qi => qi.Quotation)
            .WithMany(q => q.Items)
            .HasForeignKey(qi => qi.QuotationId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ── Wallet ───────────────────────────────────────────────────────────────────
    modelBuilder.Entity<Wallet>(entity =>
    {
      entity.ToTable("wallets");
      entity.HasKey(w => w.Id);
      entity.HasOne(w => w.User)
            .WithOne(u => u.Wallet)
            .HasForeignKey<Wallet>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ── WalletTransaction ────────────────────────────────────────────────────────
    modelBuilder.Entity<WalletTransaction>(entity =>
    {
      entity.ToTable("wallet_transactions");
      entity.HasKey(wt => wt.Id);
      entity.Property(wt => wt.Type).HasConversion<string>();
      entity.HasOne(wt => wt.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(wt => wt.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ── Withdrawal ───────────────────────────────────────────────────────────────
    modelBuilder.Entity<Withdrawal>(entity =>
    {
      entity.ToTable("withdrawals");
      entity.HasKey(w => w.Id);
      entity.Property(w => w.Status).HasConversion<string>();
      entity.HasOne(w => w.Wallet)
            .WithMany(wa => wa.Withdrawals)
            .HasForeignKey(w => w.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ── Dispute ──────────────────────────────────────────────────────────────────
    modelBuilder.Entity<Dispute>(entity =>
    {
      entity.ToTable("disputes");
      entity.HasKey(d => d.Id);
      entity.Property(d => d.Status).HasConversion<string>();
      
      entity.HasOne(d => d.Booking)
            .WithMany(b => b.Disputes)
            .HasForeignKey(d => d.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
            
      entity.HasOne(d => d.Customer)
            .WithMany()
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
            
      entity.HasOne(d => d.Worker)
            .WithMany()
            .HasForeignKey(d => d.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // ── DisputeEvidence ──────────────────────────────────────────────────────────
    modelBuilder.Entity<DisputeEvidence>(entity =>
    {
      entity.ToTable("dispute_evidences");
      entity.HasKey(de => de.Id);
      
      entity.HasOne(de => de.Dispute)
            .WithMany(d => d.Evidences)
            .HasForeignKey(de => de.DisputeId)
            .OnDelete(DeleteBehavior.Cascade);
            
      entity.HasOne(de => de.Uploader)
            .WithMany()
            .HasForeignKey(de => de.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // ── Refund ───────────────────────────────────────────────────────────────────
    modelBuilder.Entity<Refund>(entity =>
    {
      entity.ToTable("refunds");
      entity.HasKey(r => r.Id);
      entity.Property(r => r.RefundType).HasConversion<string>();
      entity.Property(r => r.Status).HasConversion<string>();
      
      entity.HasOne(r => r.Dispute)
            .WithMany(d => d.Refunds)
            .HasForeignKey(r => r.DisputeId)
            .OnDelete(DeleteBehavior.Cascade);
            
      entity.HasOne(r => r.AdminProcessor)
            .WithMany()
            .HasForeignKey(r => r.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);
    });
  }
}
