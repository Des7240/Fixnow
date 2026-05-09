# TÀI LIỆU HƯỚNG DẪN DÀNH CHO DEVELOPER & QUẢN LÝ DỰ ÁN

Tài liệu này cung cấp cái nhìn toàn diện về cấu trúc, cách thức vận hành, và quy trình phát triển cho dự án Fixnow.

## 1. TỔNG QUAN KIẾN TRÚC & TECH STACK

**Fixnow** sử dụng mô hình Client - Server tách biệt hoàn toàn:
- **Backend (`/Fixnow_Code`):** .NET 8 / C# Web API.
  - *Database:* SQL Server / PostgreSQL (via Entity Framework Core).
  - *Real-time:* SignalR cho Chat & Notification.
  - *Background Jobs:* Quartz.NET / Hangfire cho các tác vụ định kỳ.
- **Frontend (`/FontEnd`):** ReactJS.
  - *Build tool:* Vite.
  - *Language:* TypeScript.

---

## 2. CẤU TRÚC THƯ MỤC CỐT LÕI

### 2.1 Backend (`Fixnow_Code/Fixnow`)
- `Controllers/`: Chứa các API Endpoints tiếp nhận Request từ Client. (VD: `BookingController`, `AuthController`).
- `Services/`: Nơi chứa toàn bộ **Business Logic**. Không viết logic trực tiếp trong Controller.
- `Repositories/`: Xử lý giao tiếp với CSDL (áp dụng Repository Pattern).
- `DTOs/` (Data Transfer Objects): Các class sử dụng định dạng dữ liệu Input/Output giữa Client và Server. Tách bạch với `Entities`.
- `Entities/`: Model thao tác với Database thông qua EF Core.
- `Hubs/`: Cấu hình SignalR cho WebSocket (Real-time Chat, Tracking).
- `Middlewares/`: Exception handling tập trung, Authentication, Logging.

### 2.2 Frontend (`FontEnd/src`)
- `api/`: Cấu hình Axios / Fetch client để gọi sang Backend API.
- `components/`: Các UI Component tái sử dụng (Button, Input, Modal...).
- `pages/`: Chứa các màn hình chính (Route) (VD: Homepage, Dashboard, BookingDetail).
- `stores/`: Quản lý State toàn cục (React Context / Redux / Zustand).
- `signalr/`: Xử lý kết nối websocket.

---

## 3. QUY TRÌNH PHÁT TRIỂN (DEVELOPMENT WORKFLOW)

### 3.1 Setup Môi trường
**Backend:**
1. Cài đặt .NET 8 SDK.
2. Cập nhật chuỗi kết nối Database tại `appsettings.Development.json`.
3. Mở Terminal tại `Fixnow_Code/Fixnow`, chạy lệnh:
   ```bash
   dotnet ef database update
   dotnet run
   ```

**Frontend:**
1. Cài đặt Node.js (phiên bản LTS).
2. Di chuyển vào thư mục `FontEnd`:
   ```bash
   npm install
   npm run dev
   ```

### 3.2 Quy trình Code & Review (Git Workflow)
- **Nhánh chính (Trunk):** `main` (sẵn sàng deploy lên Production) và `develop` (môi trường tích hợp).
- **Tạo nhánh tính năng:** 
  - Đặt tên theo format: `feature/<tên-chức-năng>` hoặc `bugfix/<tên-bug>`. VD: `feature/dispute-module`.
- **Commit:** Ghi log rõ ràng. VD: `feat(booking): add pricing logic`.
- **Pull Request (PR):** Phải được tạo vào nhánh `develop`. Lead Dev hoặc các thành viên khác phải review code trước khi merge. Cắm cờ "Approved" khi không có conflict, pass các rule.

### 3.3 Chuẩn Mực Code (Coding Conventions)
- **Backend (.NET):** 
  - Sử dụng Dependency Injection một cách triệt để tại `Program.cs`.
  - Luôn bọc Try-Catch cho các external API calls, sử dụng Global Exception Middleware cho unhandled exceptions.
  - Sử dụng Async/Await xuyên suốt (Controller -> Service -> Repository).
- **Frontend (Tyscript):** 
  - Khai báo Type/Interface rõ ràng cho mọi props và state.
  - Tách nhỏ Component, tuân thủ nguyên tắc Single Responsibility.

---

## 4. QUẢN LÝ DỰ ÁN & VẬN HÀNH

- **Tài liệu đặc tả:** Nằm trong thư mục `Document/MVP_feature`. Developer BẮT BUỘC phải đọc kỹ nghiệp vụ của phần mình đang làm trước khi code.
- **Log & Error Tracking:** 
  - Mọi lỗi Server (500) phải được ghi log (Sử dụng Serilog trong .NET).
  - Các Exception lớn gửi bắn alert về Telegram/Slack của dev team (Setup tại Logger của backend).
- **Test:** Developer phải tự test Postman (hoặc dựa trên Swagger `/swagger`) đối với Backend. Frontend test kỹ UI/UX trước khi tạo PR.
- **Release:** Khi `develop` đã ổn định, Lead dev sẽ gộp code sang `main` để public phiên bản.
