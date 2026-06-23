using Fixnow.DTOs.Common;
using Fixnow.Enums;

namespace Fixnow.DTOs.Admin;

public class GetBookingsQueryDto : PaginationRequestDto
{
  public DateTime? DateFrom { get; set; }
  public DateTime? DateTo { get; set; }
  public BookingStatus? Status { get; set; }
  public string? SearchTerm { get; set; } // Search by Booking ID, Customer Name, or Worker Name
}
