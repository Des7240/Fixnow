using Fixnow.Enums;

namespace Fixnow.Entities;

/// <summary>
/// Represents a registered user in the FixNow system.
/// </summary>
public class User
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Email { get; set; } = string.Empty;
  public string PasswordHash { get; set; } = string.Empty;
  public string FullName { get; set; } = string.Empty;
  public UserRole Role { get; set; } = UserRole.CUSTOMER;
  public string Status { get; set; } = "ACTIVE";
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
  public ICollection<Booking> CustomerBookings { get; set; } = new List<Booking>();
  public ICollection<Booking> WorkerBookings { get; set; } = new List<Booking>();
  public WorkerLocation? CurrentLocation { get; set; }
  public Wallet? Wallet { get; set; }

  // Worker Management Navigation
  public WorkerProfile? WorkerProfile { get; set; }
  public ICollection<WorkerKyc> WorkerKycs { get; set; } = new List<WorkerKyc>();
  public ICollection<WorkerService> WorkerServices { get; set; } = new List<WorkerService>();
  public ICollection<WorkerLocationHistory> LocationHistories { get; set; } = new List<WorkerLocationHistory>();
  public ICollection<WorkerReview> ReviewsGiven { get; set; } = new List<WorkerReview>();
  public ICollection<WorkerReview> ReviewsReceived { get; set; } = new List<WorkerReview>();
}
