using System.Security.Cryptography;
using System.Text;
using System.Net;
using Fixnow.DTOs.Payment;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services.Providers;

public class VNPayProvider : IPaymentProvider
{
  public string ProviderName => "VNPAY";

  private readonly IConfiguration _config;

  public VNPayProvider(IConfiguration config)
  {
    _config = config;
  }

  public Task<string> CreatePaymentUrlAsync(PaymentRequestDto request)
  {
    var tmnCode = _config["VNPay:TmnCode"] ?? "TCB00011";
    var hashSecret = _config["VNPay:HashSecret"] ?? "RAO0S990OC9S2N5SBA9L912SV9P5JDPD";
    var baseUrl = _config["VNPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    var vnp_Params = new SortedList<string, string>
    {
      { "vnp_Version", "2.1.0" },
      { "vnp_Command", "pay" },
      { "vnp_TmnCode", tmnCode },
      { "vnp_Amount", ((long)(request.Amount * 100)).ToString() },
      { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
      { "vnp_CurrCode", "VND" },
      { "vnp_IpAddr", request.IpAddress },
      { "vnp_Locale", "vn" },
      { "vnp_OrderInfo", request.Description },
      { "vnp_OrderType", "other" },
      { "vnp_ReturnUrl", request.ReturnUrl },
      { "vnp_TxnRef", request.PaymentId.ToString() }
    };

    // Build raw data for signature (values NOT encoded)
    // Build query string for URL (values ARE encoded)
    var rawData = new StringBuilder();
    var queryPath = new StringBuilder();

    foreach (var kv in vnp_Params)
    {
      if (!string.IsNullOrEmpty(kv.Value))
      {
        rawData.Append(kv.Key + "=" + kv.Value + "&");
        queryPath.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
      }
    }

    var signData = rawData.ToString().TrimEnd('&');
    var vnp_SecureHash = HmacSHA512(hashSecret, signData);
    var paymentUrl = $"{baseUrl}?{queryPath}vnp_SecureHash={vnp_SecureHash}";

    return Task.FromResult(paymentUrl);
  }

  public Task<PaymentResultDto> VerifyCallbackAsync(IQueryCollection query)
  {
    var hashSecret = _config["VNPay:HashSecret"] ?? "RAO0S990OC9S2N5SBA9L912SV9P5JDPD";

    var vnpayData = new SortedList<string, string>();
    foreach (var k in query.Keys)
    {
      if (k.StartsWith("vnp_") && k != "vnp_SecureHash")
      {
        vnpayData.Add(k, query[k].ToString());
      }
    }

    var sb = new StringBuilder();
    foreach (var kv in vnpayData)
    {
      if (!string.IsNullOrEmpty(kv.Value))
      {
        sb.Append(kv.Key + "=" + kv.Value + "&");
      }
    }
    var signData = sb.ToString().TrimEnd('&');
    var checkSignature = HmacSHA512(hashSecret, signData);

    var vnp_SecureHash = query["vnp_SecureHash"].ToString();
    var txnRef = query["vnp_TxnRef"].ToString();
    var transactionNo = query["vnp_TransactionNo"].ToString();
    var amountStr = query["vnp_Amount"].ToString();
    var responseCode = query["vnp_ResponseCode"].ToString();
    
    decimal amount = 0;
    if (long.TryParse(amountStr, out var vnpAmount))
    {
      amount = vnpAmount / 100m;
    }

    bool isSignatureValid = checkSignature.Equals(vnp_SecureHash, StringComparison.InvariantCultureIgnoreCase);

    var result = new PaymentResultDto
    {
      IsSuccess = isSignatureValid && responseCode == "00",
      TransactionId = transactionNo,
      ErrorMessage = isSignatureValid ? (responseCode == "00" ? null : $"VNPay Error: {responseCode}") : "Invalid Signature",
      RawResponse = System.Text.Json.JsonSerializer.Serialize(query.ToDictionary(k => k.Key, k => k.Value.ToString())),
      Amount = amount
    };

    return Task.FromResult(result);
  }

  private static string HmacSHA512(string key, string inputData)
  {
    var hash = new StringBuilder();
    var keyBytes = Encoding.UTF8.GetBytes(key);
    var inputBytes = Encoding.UTF8.GetBytes(inputData);
    using (var hmac = new HMACSHA512(keyBytes))
    {
      var hashValue = hmac.ComputeHash(inputBytes);
      foreach (var theByte in hashValue)
      {
        hash.Append(theByte.ToString("X2"));
      }
    }
    return hash.ToString();
  }
}
