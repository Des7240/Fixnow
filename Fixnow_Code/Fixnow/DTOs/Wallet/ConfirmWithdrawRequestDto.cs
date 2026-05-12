using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.Wallet;

public record ConfirmWithdrawRequestDto(
    [Required] string OtpCode,
    [Required] decimal Amount,
    [Required] string BankName,
    [Required] string AccountNumber,
    [Required] string AccountName
);
