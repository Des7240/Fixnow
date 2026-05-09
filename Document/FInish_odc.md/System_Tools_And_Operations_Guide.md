# TÀI LIỆU HƯỚNG DẪN DÙNG CÁC CÔNG CỤ (TOOLS), LOGGING & BACKGROUND JOBS

Tài liệu này cung cấp hướng dẫn chi tiết cho Developer và DevOps về cách thiết lập, sử dụng và giám sát các tiện ích nội bộ (Jobs, Logs, Real-time message) có trong hệ thống Fixnow.

---

## 1. HỆ THỐNG GHI LOG (CENTRALIZED LOGGING)

Hệ thống sử dụng **Serilog** để thu thập log và lưu trữ tập trung (có thể tích hợp với Seq, Elasticsearch hoặc Application Insights).

### 1.1 Nguyên tắc ghi log
Sử dụng `ILogger<T>` được inject qua Dependency Injection. Mọi unhandled exception đã được catch tại Global Exception Middleware, do đó Dev **chỉ cần log các thông tin luồng nghiệp vụ quan trọng hoặc các lỗi kết nối bên thứ 3 (Payment, SMS).**

### 1.2 Mức độ Log (Log Levels)
- `LogInformation`: Giao dịch thanh toán thành công, Thợ nhận việc thành công.
- `LogWarning`: Lỗi logic nghiệp vụ bình thường (Ví dụ: Số dư không đủ, mã OTP sai), API trả về 400.
- `LogError`: Lỗi hệ thống, crash ứng dụng, gọi API bên thứ 3 thất bại (HTTP 500).

### 1.3 Cách sử dụng trong Code
```csharp
public class BookingService
{
    private readonly ILogger<BookingService> _logger;

    public BookingService(ILogger<BookingService> logger)
    {
        _logger = logger;
    }

    public async Task CreateBooking(...)
    {
        _logger.LogInformation("Bắt đầu tạo booking cho User {UserId}", userId);
        try
        {
            // Business Logic...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi tạo Booking cho User {UserId}", userId);
            throw;
        }
    }
}
```

### 1.4 Xem Log ở đâu?
- **Môi trường Dev:** Log hiển thị trực tiếp trên Console.
- **Môi trường Production:** 
  - Xem trong thư mục `Logs/` trên server.
  - Qua Dashboard màn hình tập trung (VD: `http://<domain_server>:<port_seq>` nếu sử dụng Seq).

---

## 2. BACKGROUND JOBS & SCHEDULER (TÁC VỤ CHẠY NGẦM)

Hệ thống sử dụng **Hangfire** (hoặc kiến trúc tương tự như Quartz) để quản lý các tác vụ nền, đồng bộ dữ liệu và lên lịch công việc.

### 2.1 Truy cập Dashboard quản lý Job
- **URL Dashboard:** `http://<domain>/hangfire` (Cần tài khoản Admin để truy cập).
- Tại đây, Quản lý dự án/Dev có thể xem các job đang chạy, job bị lỗi (Failed), và thực hiện cấu hình chạy lại (Requeue) một job thất bại.

### 2.2 Các loại Job và cách sử dụng

#### A. Fire-and-forget Jobs (Chạy ngay một lần ở background)
Sử dụng cho các chức năng như: Gửi email đăng ký, Gửi OTP SMS, Broadcast notification.
```csharp
BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(userEmail, "Welcome!"));
```

#### B. Delayed Jobs (Chạy sau một khoảng thời gian chờ)
Sử dụng cho các chức năng như: Hủy booking tự động nếu không có thợ nhận sau 15 phút.
```csharp
BackgroundJob.Schedule(
    () => _bookingService.AutoCancelBooking(bookingId), 
    TimeSpan.FromMinutes(15));
```

#### C. Recurring Jobs (Tác vụ lặp lại tự động định kỳ)
Sử dụng cho: Thống kê doanh thu cuối ngày, Dọn dẹp dữ liệu rác hàng tuần.
```csharp
RecurringJob.AddOrUpdate(
    "daily-revenue-report", 
    () => _reportService.GenerateDailyReport(), 
    Cron.Daily);
```

---

## 3. REAL-TIME SIGNALR (WEB SOCKETS)

Được sử dụng cho Chức năng **In-app Chat** và **Notification (Push trạng thái Booking)**.

### 3.1 Khai báo Hubs
- Tham khảo thư mục `Fixnow_Code/Fixnow/Hubs/`.
- Client (Frontend) cần kết nối theo chuẩn URL Socket: `ws://<domain>/hubs/chat` hoặc `/hubs/notification`.

### 3.2 Gửi sự kiện từ Server -> Client
Sử dụng `IHubContext` trong các logic nghiệp vụ.
```csharp
await _chatHubContext.Clients.User(workerId).SendAsync("ReceiveNewBooking", bookingDetails);
```

---

## 4. API DOCUMENTATION (SWAGGER)

- **Công cụ:** Swagger UI (Cài đặt qua Swashbuckle).
- **Mục đích:** Để Front-end dev đọc hiểu API của Backend và test nóng trực tiếp.
- **URL truy cập:** 
  - Môi trường Dev: `http://localhost:<port>/swagger`
  - Môi trường Staging/Prod: `https://<domain>/swagger`
- **Lưu ý:**
  - Cần lấy Token gắn vào nút `Authorize` trên góc phải Swagger để gọi các API có bảo mật (những API có `[Authorize]`).
  - Lấy token qua API `/api/Auth/login`.

---

## 5. FILE STORAGE (LƯU TRỮ TỆP TIN / HÌNH ẢNH)

Hỗ trợ cho việc Upload avatar, hình ảnh hiện trường hỏng hóc, hồ sơ KYC.
- **Dịch vụ sử dụng:** Amazon S3 / Cloudinary (Tùy cấu hình).
- **Cách Backend quy định:**
  - Các tệp tin đẩy từ Frontend lên thông qua API `/api/File/upload` dưới format `multipart/form-data`.
  - Backend ghi nhận tệp, đẩy sang dạng stream nối qua Cloud Storage và trả về 1 link URL CDN trực tiếp.
- **Giới hạn quy định:**
  - Kích thước ảo: Tối đa 5MB / file.
  - Loại tệp: JPG, PNG, PDF (cho hồ sơ xác thực).

---

## 6. KIỂM SOÁT BẢO MẬT (RATE LIMITING)

- Ngăn chặn Spam API và dDoS.
- Nếu gửi quá nhiều yêu cầu vào hệ thống (ví dụ: > 100 requests / 1 phút trên một IP), lỗi `429 Too Many Requests` sẽ bị ném ra.
- Đối với API Get OTP đăng ký, giới hạn nghiêm ngặt hơn: Chỉ cho phép gửi lại sau mỗi 60s. Setup giới hạn này nằm trong cấu hình `RateLimiting` của `Program.cs`.
