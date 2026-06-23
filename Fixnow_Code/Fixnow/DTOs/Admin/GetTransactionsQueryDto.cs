using Fixnow.DTOs.Common;
using Fixnow.Enums;

namespace Fixnow.DTOs.Admin;

public class GetTransactionsQueryDto : PaginationRequestDto
{
  public DateTime? DateFrom { get; set; }
  public DateTime? DateTo { get; set; }
  public string? Type { get; set; } // "PAYMENT" or "WALLET_TRANSACTION"
  public string? SearchTerm { get; set; } // Search by TransactionCode or ReferenceId
}
