sequenceDiagram
    actor W as Thợ (Worker)
    participant S as Hệ thống FixNow
    actor A as Admin
    actor C as Khách hàng

    %% Bước 1: Đăng ký
    W->>S: 1. Đăng ký tài khoản (Role: WORKER)
    S-->>W: Tài khoản tạo thành công (Chưa được làm việc)
    
    %% Bước 2: Thiết lập hồ sơ & KYC
    W->>S: 2. Nộp hồ sơ KYC (CCCD, Selfie)
    S-->>W: Trạng thái KYC: PENDING
    W->>S: 3. Đăng ký kỹ năng dịch vụ (Sửa điện, Nước...)
    S-->>W: Trạng thái Kỹ năng: PENDING
    
    %% Bước 3: Admin Xét duyệt
    A->>S: 4. Admin duyệt hồ sơ KYC
    S-->>W: Gửi Thông báo: KYC đã được duyệt (APPROVED)
    
    A->>S: 5. Admin duyệt kỹ năng dịch vụ
    S-->>W: Gửi Thông báo: Kỹ năng được duyệt (APPROVED)
    
    %% Bước 4: Sẵn sàng nhận đơn
    W->>S: 6. Bật trạng thái Hoạt động (ONLINE)
    
    %% Bước 5: Matching và Nhận đơn
    C->>S: 7. Đặt đơn dịch vụ
    S->>S: Kiểm tra: Thợ Online + Kỹ năng khớp + Gần nhất
    S->>W: 8. Gửi yêu cầu đơn hàng (SignalR Notification)
    W->>S: 9. Chấp nhận đơn (Accept)
    S-->>C: Thông báo "Thợ đã nhận đơn"
