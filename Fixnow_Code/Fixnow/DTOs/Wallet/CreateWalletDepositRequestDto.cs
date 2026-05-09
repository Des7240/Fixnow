using Fixnow.Enums;

namespace Fixnow.DTOs.Wallet;

public class CreateWalletDepositRequestDto
{
  public decimal Amount { get; set; }
  public PaymentProvider Provider { get; set; } = PaymentProvider.VNPAY;
}
