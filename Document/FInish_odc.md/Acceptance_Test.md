# TÀI LIỆU ACCEPTANCE TEST (UAT) - FIXNOW

Tài liệu này định nghĩa các kịch bản kiểm thử chấp nhận (Acceptance Test) cho toàn bộ hệ thống Fixnow, đảm bảo các tính năng hoạt động theo đúng logic nghiệp vụ của MVP.

## 1. NGUỜI DÙNG (CUSTOMER)

### 1.1 Khách hàng - Đăng ký & Đăng nhập
- **AT-C-01:** Khách hàng có thể đăng ký tài khoản mới bằng Số điện thoại/Email và nhận mã OTP xác thực thành công.
- **AT-C-02:** Khách hàng có thể đăng nhập bằng tài khoản đã đăng ký và hệ thống trả về token hợp lệ.
- **AT-C-03:** Khách hàng bị chặn đăng ký nếu email/số điện thoại đã tồn tại.

### 1.2 Khách hàng - Tạo Booking & Tìm thợ
- **AT-C-04:** Khách hàng xem được danh sách dịch vụ với giá tương ứng.
- **AT-C-05:** Khách hàng có thể tạo một yêu cầu (Booking) với mô tả, hình ảnh và vị trí hiện tại.
- **AT-C-06:** Ngay sau khi tạo, hệ thống gửi thông báo (Notification) tới các Thợ đang rảnh trong bán kính quy định.
- **AT-C-07:** Khách hàng có thể hủy yêu cầu trước khi có thợ nhận (Hủy không mất phí).

### 1.3 Khách hàng - Thanh toán & Đánh giá
- **AT-C-08:** Sau khi thợ hoàn thành, khách hàng có thể chọn thanh toán qua Ví (Wallet) hoặc tiền mặt.
- **AT-C-09:** Ví của khách bị trừ tiền đúng bằng số tiền dịch vụ khi chọn thanh toán qua Ví.
- **AT-C-10:** Khách hàng có thể rate (1-5 sao) và viết review cho thợ sau khi hoàn thành.

---

## 2. THỢ (WORKER)

### 2.1 Thợ - Đăng ký & KYC
- **AT-W-01:** Thợ đăng ký tài khoản và cập nhật hồ sơ KYC (CMND/CCCD, Chứng chỉ nghề).
- **AT-W-02:** Tài khoản Thợ ở trạng thái "Pending" và không thể nhận việc cho đến khi Admin duyệt hồ sơ.
- **AT-W-03:** Thợ có thể cập nhật trạng thái "Available/Busy" (Sẵn sàng nhận việc).

### 2.2 Thợ - Nhận lệnh (Booking) & Cấp báo giá (Quotation)
- **AT-W-04:** Thợ nhận được thông báo về công việc mới gần khu vực của mình.
- **AT-W-05:** Thợ chấp nhận yêu cầu và trạng thái Booking chuyển sang "Accepted".
- **AT-W-06:** Thợ có thể đưa ra Báo giá (Quotation) cho khách hàng nếu có chi phí phát sinh trước khi bắt đầu công việc.
- **AT-W-07:** Thợ nhấn "Bắt đầu chuyến đi" và "Hoàn thành công việc".

### 2.3 Thợ - Ví & Thu nhập
- **AT-W-08:** Thợ nhận được tiền vào ví sau khi công việc hoàn thành (sau khi đã trừ % hoa hồng của hệ thống).
- **AT-W-09:** Thợ tạo yêu cầu Rút tiền (Withdraw) về tài khoản ngân hàng và trạng thái chuyển sang Pending.

---

## 3. QUẢN TRỊ VIÊN (ADMIN)

### 3.1 Admin - Duyệt KYC & Quản lý Thợ
- **AT-A-01:** Admin xem được danh sách Thợ đang chờ duyệt KYC.
- **AT-A-02:** Admin duyệt (Approve) hoặc Từ chối (Reject) hồ sơ kèm theo lý do.
- **AT-A-03:** Admin có thể khóa (Ban) một người dùng hoặc thợ khi có vi phạm.

### 3.2 Admin - Giải quyết Tranh chấp (Dispute) & Hoàn tiền (Refund)
- **AT-A-04:** Khi có Khiếu nại (Dispute), Admin có thể xem chi tiết: lịch sử chat, hình ảnh, log của booking.
- **AT-A-05:** Admin quyết định "Hoàn tiền cho khách" (Refund) hoặc "Thanh toán cho thợ" (Release payment).
- **AT-A-06:** Số dư trong Ví tự động cập nhật đúng theo quyết định của Admin.

## 4. HỆ THỐNG GIAO TIẾP (CHAT & NOTIFICATION)
- **AT-SYS-01:** Khách và thợ có thể gửi tin nhắn Text qua lại sau khi Booking được chấp nhận.
- **AT-SYS-02:** Gửi ảnh qua tin nhắn Load thành công và an toàn (sử dụng FileController/S3).
- **AT-SYS-03:** Push notification hoạt động realtime khi có trạng thái booking mới qua SignalR.
