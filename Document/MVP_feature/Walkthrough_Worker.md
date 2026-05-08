# Hướng dẫn Toàn tập (End-to-End) Worker Management & KYC

Tài liệu này hướng dẫn test luồng tính năng mới được triển khai: **Hồ sơ thợ, Kỹ năng, KYC, và Admin xét duyệt**.

Mọi API ở dưới, bạn có thể copy trực tiếp JSON Body để dán vào Postman hoặc Swagger (`https://localhost:<port>/swagger`).

---

## BƯỚC 1: Đăng ký & Nộp hồ sơ KYC (Thợ)

Ngay sau khi đăng ký tài khoản (Worker), thợ cần nộp hồ sơ KYC để Admin xét duyệt.

- **Endpoint:** `POST /api/v1/workers/kyc`
- **Auth:** Bearer Token của `WORKER`
- **FormData (chọn kiểu form-data trên Postman/Swagger):**
  - `citizenIdNumber` (text): `001099001234`
  - `frontImage` (file): Chọn 1 ảnh bất kỳ
  - `backImage` (file): Chọn 1 ảnh bất kỳ
  - `selfieImage` (file): Chọn 1 ảnh bất kỳ

> 📌 **Action:** Sau khi nộp thành công, trạng thái KYC sẽ là `PENDING`. Ảnh sẽ được lưu ở `wwwroot/uploads/kyc/{worker_id}/`. Copy ID của phiếu KYC vừa tạo để Admin duyệt.

---

## BƯỚC 2: Admin xét duyệt hồ sơ

Admin kiểm tra tính hợp lệ của hồ sơ KYC và duyệt (APPROVED) hoặc từ chối (REJECTED).

- **Endpoint:** `PATCH /api/v1/admin/kyc/{kyc_id}`
- **Auth:** Bearer Token của `ADMIN`
- **Body JSON:**
  ```json
  {
    "status": "APPROVED",
    "reason": "Hồ sơ đầy đủ, rõ nét."
  }
  ```

---

## BƯỚC 3: Thợ cập nhật Profile & Kỹ năng (Skills)

Sau khi được duyệt, thợ thiết lập hồ sơ và chọn những kỹ năng (Dịch vụ) mà họ có thể làm. Nếu không có kỹ năng, thợ sẽ không nhận được bất kỳ đơn hàng nào.

### 3.1 Cập nhật thông tin Profile
- **Endpoint:** `POST /api/v1/workers/profile`
- **Auth:** Bearer Token của `WORKER`
- **Body JSON:**
  ```json
  {
    "bio": "Thợ điện nước 5 năm kinh nghiệm",
    "experienceYears": 5
  }
  ```

### 3.2 Đăng ký Kỹ năng (Mapping với Service)
- **Endpoint:** `POST /api/v1/workers/profile/skills`
- **Auth:** Bearer Token của `WORKER`
- **Body JSON:**
  ```json
  {
    "serviceIds": [
      "<ID_CỦA_SERVICE_1>",
      "<ID_CỦA_SERVICE_2>"
    ]
  }
  ```

---

## BƯỚC 4: Bật trạng thái Sẵn sàng (ONLINE) & Vị trí (GPS)

Lúc này, Matching Engine chỉ gọi những thợ có trạng thái **ONLINE** và sở hữu **ServiceId** của đơn hàng.

### 4.1 Bật ONLINE
- **Endpoint:** `PATCH /api/v1/workers/profile/availability`
- **Auth:** Bearer Token của `WORKER`
- **Body JSON:**
  ```json
  {
    "status": "ONLINE"
  }
  ```

### 4.2 Cập nhật vị trí GPS
- **Endpoint:** `PUT /api/v1/workers/location`
- **Body JSON:**
  ```json
  {
    "lat": 21.0315,
    "lng": 105.8015
  }
  ```
*(Hệ thống sẽ tự động tạo luôn lịch sử di chuyển trong bảng `worker_location_histories` phục vụ việc tracking sau này).*

---

## BƯỚC 5: Khách hàng Đặt Đơn (Matching Engine)

Bây giờ khi khách hàng đặt đơn với đúng `<ID_CỦA_SERVICE_1>`, Booking System sẽ:
1. Tìm các thợ cách bán kính 5km.
2. Lọc ra các thợ đang **ONLINE**.
3. Lọc ra các thợ có kỹ năng khớp với **ServiceId**.
4. Tạo `BookingMatchingLog` và gửi Notification.

Quy trình nhận đơn, làm việc diễn ra bình thường theo tài liệu `Walkthrough_Booking.md`.
