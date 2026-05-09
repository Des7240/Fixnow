# TÀI LIỆU HƯỚNG DẪN CÀI ĐẶT VÀ CHẠY DỰ ÁN (SETUP & RUN GUIDE)

Tài liệu này cung cấp các bước chi tiết để cài đặt môi trường, cấu hình và khởi chạy dự án Fixnow trên máy Local của Developer.

---

## 1. YÊU CẦU HỆ THỐNG (PREREQUISITES)

Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã cài đặt các phần mềm sau:
1. **.NET 8.0 SDK**: Dành cho Backend ([Tải xuống tại đây](https://dotnet.microsoft.com/download/dotnet/8.0)).
2. **Node.js**: Phiên bản LTS (18.x hoặc 20.x trở lên) dành cho Frontend ([Tải xuống tại đây](https://nodejs.org/)).
3. **Database**: 
   - SQL Server (Sử dụng SQL Server Management Studio - SSMS) HOẶC
   - PostgreSQL (Sử dụng pgAdmin/DBeaver). Tùy theo cấu hình dự án.
4. **IDE / Editor**: Visual Studio 2022 hoặc Visual Studio Code.
5. (Tuỳ chọn) **Postman** để test API.

---

## 2. HƯỚNG DẪN CHẠY BACKEND (.NET 8)

### Bước 2.1: Lấy code và cài đặt Package
Mở Terminal/Command Prompt và di chuyển vào thư mục Backend:
```bash
cd Fixnow_Code/Fixnow
dotnet restore
```

### Bước 2.2: Cấu hình `appsettings.Development.json`
Đảm bảo bạn đã sao chép hoặc tạo file `appsettings.Development.json` (nếu chưa có). Cấu hình chuỗi kết nối Database và các key cần thiết:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FixnowDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "CHOOSE_A_VERY_LONG_SECRET_KEY_FOR_LOCAL_DEV",
    "Issuer": "Fixnow",
    "Audience": "FixnowApp",
    "ExpiryMinutes": 10080
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```
*(Lưu ý: Thay đổi `ConnectionStrings` cho phù hợp với username/password hoặc local DB của bạn).*

### Bước 2.3: Tạo Database bằng Entity Framework (EF Core Migrations)
Chạy các lệnh sau để khởi tạo Cơ sở dữ liệu và các bảng:
```bash
dotnet tool install --global dotnet-ef  # Bỏ qua nếu đã cài ef tools
dotnet ef database update
```

### Bước 2.4: Khởi chạy Server
Chạy lệnh vả kiểm tra kết quả:
```bash
dotnet run
```
Backend sẽ khởi chạy tại:
- **HTTPS:** `https://localhost:7xxx`
- **HTTP:** `http://localhost:5xxx`
- **Swagger API Docs:** `https://localhost:7xxx/swagger`

---

## 3. HƯỚNG DẪN CHẠY FRONTEND (REACT / VITE)

### Bước 3.1: Mở thư mục Frontend
Mở một Terminal mới và di chuyển vào thư mục Frontend:
```bash
cd FontEnd
```

### Bước 3.2: Cài đặt Node Modules (Dependencies)
Chạy lệnh:
```bash
npm install
```

### Bước 3.3: Cấu hình biến môi trường
Tạo file `.env` ở thư mục gốc của `FontEnd` (ngang hàng với `package.json`) và trỏ URL base về phía API Backend đang chạy:
```env
VITE_API_BASE_URL=https://localhost:7xxx/api
```
*(Thay thế port `7xxx` bằng port thực tế Backend của bạn đang chạy ở bước 2.4).*

### Bước 3.4: Khởi chạy Frontend React Server
Chạy lệnh:
```bash
npm run dev
```
Trình duyệt sẽ hiển thị Frontend tại URL mặc định, thường là: `http://localhost:5173`. Có thể nhấn `o` kết hợp Enter trên Terminal Vite để mở nhanh.

---

## 4. XỬ LÝ LỖI THƯỜNG GẶP (TROUBLESHOOTING)

### 4.1 Lỗi liên quan đến Database (Backend)
- **Lỗi:** `A connection was successfully established with the server, but then an error occurred during the login process`
- **Xử lý:** Thêm `TrustServerCertificate=True;` vào chuỗi kết nối (`ConnectionStrings` trong file `appsettings.json`).

### 4.2 Lỗi CORS (Cross-Origin Resource Sharing)
- **Lỗi:** Bấm F12 trên Console trình duyệt, thấy API bị block bởi CORS.
- **Xử lý:** Đảm bảo Backend mở cấu hình CORS tại `Program.cs`.
  ```csharp
  builder.Services.AddCors(options => {
      options.AddPolicy("AllowAll",
          builder => builder.WithOrigins("http://localhost:5173") // URL Vite Frontend
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials());
  });
  ```

### 4.3 Lỗi thiếu thư viện / Node modules
- **Lỗi:** Lệnh `npm run dev` báo vắng file / "command not found".
- **Xử lý:** Xóa thư mục `node_modules` và file `package-lock.json`, sau đó chạy lại lệnh `npm install`.