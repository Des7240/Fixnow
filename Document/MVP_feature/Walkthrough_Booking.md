# Hướng dẫn Toàn tập (End-to-End) Test Booking + Matching Module

Tài liệu này hướng dẫn bạn test toàn bộ luồng từ việc **tạo tài khoản Customer, Worker, đăng ký Dịch vụ (Service)** cho đến lúc **đặt đơn, matching và hoàn thành đơn**.

Mọi API ở dưới, bạn có thể copy trực tiếp JSON Body để dán vào Postman hoặc Swagger (`https://localhost:<port>/swagger`).

---

## BƯỚC 1: Khởi tạo dữ liệu (Inject Accounts & Service)

Do hệ thống mới tinh, chúng ta cần tạo sẵn Customer, Worker và một Dịch vụ để test. Mặc định API Register sẽ tạo User ở trạng thái `ACTIVE`.

### 1.1 Tạo tài khoản Customer
- **Endpoint:** `POST /api/v1/auth/register`
- **Body JSON:**
  ```json
  {
    "email": "customer@fixnow.com",
    "password": "password123",
    "fullName": "Nguyen Van Khach",
    "role": "CUSTOMER"
  }
  ```
> 📌 **Action:** Sau khi gọi API, lấy `accessToken` ở Response và lưu lại (Đây là Token của Khách).

### 1.2 Tạo tài khoản Worker (Thợ)
- **Endpoint:** `POST /api/v1/auth/register`
- **Body JSON:**
  ```json
  {
    "email": "worker@fixnow.com",
    "password": "password123",
    "fullName": "Tran Van Tho",
    "role": "WORKER"
  }
  ```
> 📌 **Action:** Sau khi gọi API, lấy `accessToken` ở Response và lưu lại (Đây là Token của Thợ).

### 1.3 Tạo một Dịch vụ (Service)
*(Lưu ý: Nếu API `/api/v1/services` đang bị khóa bởi `Authorize(Roles = "ADMIN")`, bạn hãy tạm comment dòng đó trong `ServiceController.cs` để test, hoặc inject trực tiếp vào DB)*.

- **Endpoint:** `POST /api/v1/services`
- **Body JSON:**
  ```json
  {
    "name": "Sửa chữa Điều hòa",
    "description": "Bảo dưỡng, bơm ga, sửa điều hòa không mát",
    "iconUrl": "https://example.com/ac-icon.png"
  }
  ```
> 📌 **Action:** Lấy `id` của dịch vụ vừa được tạo trong Response để dùng ở bước đặt đơn. Giả sử ID là `3fa85f64-5717-4562-b3fc-2c963f66afa6`.

---

## BƯỚC 2: Thợ bật App và Bật Định vị (GPS)

Hệ thống chỉ tìm các thợ `ACTIVE` và có định vị GPS trong vòng 1 giờ qua.

- **Endpoint:** `PUT /api/v1/workers/location`
- **Auth:** Sử dụng **Bearer Token của Thợ** (từ bước 1.2)
- **Body JSON (Tọa độ giả lập tại khu vực Cầu Giấy, Hà Nội):**
  ```json
  {
    "lat": 21.0315,
    "lng": 105.8015
  }
  ```

---

## BƯỚC 3: Khách hàng Đặt Đơn (Booking)

Khách hàng đăng đơn, hệ thống sẽ tự tìm thợ trong bán kính 5km.

- **Endpoint:** `POST /api/v1/bookings`
- **Auth:** Sử dụng **Bearer Token của Khách** (từ bước 1.1)
- **Body JSON (Tọa độ khách cũng ở Cầu Giấy, cách thợ ~1km):**
  ```json
  {
    "serviceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", 
    "address": "Tòa nhà Lotte, 54 Liễu Giai, Ba Đình, Hà Nội",
    "lat": 21.0318,
    "lng": 105.8115,
    "description": "Điều hòa nhà tôi tự nhiên không mát, chảy nước"
  }
  ```
> 📌 **Action:** Response sẽ trả về ID của Booking mới, trạng thái là `MATCHING`. Bạn hãy **copy Booking ID** này lại. Nhìn vào cửa sổ Terminal đang chạy `dotnet run`, bạn sẽ thấy log thông báo: `[NOTIFY] Worker <WorkerID> → New booking <BookingID>...`

---

## BƯỚC 4: Thợ nhận đơn

Nhờ bước 3, Thợ đã được hệ thống "bắn" thông báo. Thợ sẽ ấn nút Accept.

- **Endpoint:** `POST /api/v1/bookings/{booking_id}/accept`
- **Thay `{booking_id}`** bằng Booking ID bạn copy ở Bước 3.
- **Auth:** Sử dụng **Bearer Token của Thợ**
- **Kết quả:** Trạng thái đơn đổi thành `ASSIGNED`. Đơn chính thức được gán cho thợ này. Khách hàng sẽ nhận được Push Notification.

---

## BƯỚC 5: Thợ đi đến và làm việc

Sau khi nhận đơn, thợ di chuyển đến nơi và thực hiện công việc. Thợ lần lượt update trạng thái.

- **Endpoint:** `PATCH /api/v1/bookings/{booking_id}/status`
- **Auth:** Sử dụng **Bearer Token của Thợ**
- **Các Body JSON để test lần lượt (chạy từng cái 1):**

  1. Thợ đang trên đường đi:
     ```json
     {
       "status": "ON_THE_WAY"
     }
     ```
  2. Thợ đến nơi và bắt đầu làm việc:
     ```json
     {
       "status": "WORKING"
     }
     ```
  3. Thợ làm xong, thanh toán hoàn tất:
     ```json
     {
       "status": "COMPLETED"
     }
     ```

---

## BƯỚC 6: Kiểm tra Lịch sử

Để kiểm tra lại hệ thống lưu trữ như thế nào, bạn có thể gọi các API sau:

- **Khách hàng xem lịch sử đơn:** Gọi `GET /api/v1/bookings` (Auth bằng Token của Khách).
- **Thợ xem lịch sử đơn:** Gọi `GET /api/v1/bookings` (Auth bằng Token của Thợ).
- **Xem chi tiết 1 đơn:** Gọi `GET /api/v1/bookings/{booking_id}`. Trong DB sẽ tự động lưu Log Audit ở bảng `booking_status_histories`.
