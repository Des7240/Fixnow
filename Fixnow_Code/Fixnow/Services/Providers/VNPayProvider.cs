using System.Security.Cryptography;
using System.Text;
using System.Net;
using Fixnow.DTOs.Payment;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services.Providers;

/// <summary>
/// VNPay payment provider implementation using standard library logic for .NET 6+.
/// VNPay 2.1.0 spec: signature is computed on raw (unencoded) key=value pairs joined by '&'.
/// </summary>
public class VNPayProvider : IPaymentProvider
{
  public string ProviderName => "VNPAY";

  private readonly IConfiguration _config;

  public VNPayProvider(IConfiguration config)
  {
    _config = config;
  }

  /// <summary>
  /// Creates a VNPay payment URL with a valid HMAC-SHA512 signature.
  /// </summary>
  public Task<string> CreatePaymentUrlAsync(PaymentRequestDto request)
  {
    var tmnCode = _config["VNPay:TmnCode"] ?? "4YUP19I4";
    var hashSecret = _config["VNPay:HashSecret"] ?? "MDUIFDCRAKLNBPOFIAFNEKFRNMFBYEPX";
    var baseUrl = _config["VNPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    var vnpay = new VnPayLibrary();
    vnpay.AddRequestData("vnp_Version", "2.1.0");
    vnpay.AddRequestData("vnp_Command", "pay");
    vnpay.AddRequestData("vnp_TmnCode", tmnCode);
    vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());
    vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
    vnpay.AddRequestData("vnp_CurrCode", "VND");
    vnpay.AddRequestData("vnp_IpAddr", string.IsNullOrEmpty(request.IpAddress) ? "127.0.0.1" : request.IpAddress);
    vnpay.AddRequestData("vnp_Locale", "vn");
    vnpay.AddRequestData("vnp_OrderInfo", request.Description ?? "Payment");
    vnpay.AddRequestData("vnp_OrderType", "other");
    vnpay.AddRequestData("vnp_ReturnUrl", request.ReturnUrl);
    vnpay.AddRequestData("vnp_TxnRef", request.PaymentId.ToString());

    string paymentUrl = vnpay.CreateRequestUrl(baseUrl, hashSecret);
    return Task.FromResult(paymentUrl);
  }

  /// <summary>
  /// Verifies the HMAC-SHA512 signature of a VNPay callback.
  /// </summary>
  public Task<PaymentResultDto> VerifyCallbackAsync(IQueryCollection query)
  {
    var hashSecret = _config["VNPay:HashSecret"] ?? "MDUIFDCRAKLNBPOFIAFNEKFRNMFBYEPX";
    var vnpay = new VnPayLibrary();

    foreach (var k in query.Keys)
    {
      if (!string.IsNullOrEmpty(k) && k.StartsWith("vnp_"))
      {
        vnpay.AddResponseData(k, query[k].ToString());
      }
    }

    var vnp_SecureHash = query["vnp_SecureHash"].ToString();
    bool isSignatureValid = vnpay.ValidateSignature(vnp_SecureHash, hashSecret);

    var amountStr = query["vnp_Amount"].ToString();
    var responseCode = query["vnp_ResponseCode"].ToString();
    var transactionNo = query["vnp_TransactionNo"].ToString();

    decimal amount = 0;
    if (long.TryParse(amountStr, out var vnpAmount))
    {
      amount = vnpAmount / 100m;
    }

    return Task.FromResult(new PaymentResultDto
    {
      IsSuccess = isSignatureValid && responseCode == "00",
      TransactionId = transactionNo,
      ErrorMessage = isSignatureValid
        ? (responseCode == "00" ? null : $"VNPay Error: {responseCode}")
        : "Invalid Signature",
      RawResponse = System.Text.Json.JsonSerializer.Serialize(
        query.ToDictionary(k => k.Key, k => k.Value.ToString())),
      Amount = amount
    });
  }

  private class VnPayLibrary
  {
    private readonly SortedList<string, string> _requestData = new(new VnPayComparer());
    private readonly SortedList<string, string> _responseData = new(new VnPayComparer());

    public void AddRequestData(string key, string value) => _requestData.Add(key, value);
    public void AddResponseData(string key, string value) => _responseData.Add(key, value);

    /// <summary>
    /// Builds the VNPay payment URL.
    /// - URL query string: URL-encoded values.
    /// - Signature input: raw key=raw value (per VNPay 2.1.0 spec).
    /// </summary>
    public string CreateRequestUrl(string baseUrl, string hashSecret)
    {
      var queryBuilder = new StringBuilder();
      var signBuilder = new StringBuilder();

      foreach (var kv in _requestData)
      {
        if (string.IsNullOrEmpty(kv.Value)) continue;

        // Query string part: encode value
        queryBuilder.Append(WebUtility.UrlEncode(kv.Key));
        queryBuilder.Append('=');
        queryBuilder.Append(WebUtility.UrlEncode(kv.Value));
        queryBuilder.Append('&');

        // Signature part: raw value (VNPay 2.1.0 spec)
        signBuilder.Append(kv.Key);
        signBuilder.Append('=');
        signBuilder.Append(kv.Value);
        signBuilder.Append('&');
      }

      string rawSignData = signBuilder.ToString().TrimEnd('&');
      string secureHash = HmacSHA512(hashSecret, rawSignData);

      // Append hash to end of URL
      string paymentUrl = baseUrl + "?" + queryBuilder + "vnp_SecureHash=" + secureHash;
      return paymentUrl;
    }

    /// <summary>
    /// Validates signature from VNPay callback.
    /// Uses raw (unencoded) values consistent with CreateRequestUrl.
    /// </summary>
    public bool ValidateSignature(string inputHash, string hashSecret)
    {
      var signBuilder = new StringBuilder();

      foreach (var kv in _responseData)
      {
        if (string.IsNullOrEmpty(kv.Value)) continue;
        if (kv.Key == "vnp_SecureHashType" || kv.Key == "vnp_SecureHash") continue;

        // Raw key=raw value (VNPay 2.1.0 spec)
        signBuilder.Append(kv.Key);
        signBuilder.Append('=');
        signBuilder.Append(kv.Value);
        signBuilder.Append('&');
      }

      string rawData = signBuilder.ToString().TrimEnd('&');
      string myChecksum = HmacSHA512(hashSecret, rawData);
      return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string HmacSHA512(string key, string inputData)
    {
      var hash = new StringBuilder();
      byte[] keyBytes = Encoding.UTF8.GetBytes(key);
      byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
      using var hmac = new HMACSHA512(keyBytes);
      byte[] hashValue = hmac.ComputeHash(inputBytes);
      foreach (var b in hashValue)
      {
        hash.Append(b.ToString("x2"));
      }
      return hash.ToString();
    }
  }

  private class VnPayComparer : IComparer<string>
  {
    public int Compare(string? x, string? y)
    {
      if (x == y) return 0;
      if (x == null) return -1;
      if (y == null) return 1;
      return string.CompareOrdinal(x, y);
    }
  }
}
