namespace Fixnow.DTOs.Admin;

public class DashboardSummaryDto
{
  public int TotalBookings { get; set; }
  public int CompletedBookings { get; set; }
  public double CancelRate { get; set; }
  
  public int TotalWorkers { get; set; }
  public int OnlineWorkers { get; set; }
  public int PendingKycs { get; set; }
  
  public double AverageSystemRating { get; set; }
}
