1. Module Scope
Module	Description
Dispute Creation	Tạo khiếu nại
Evidence Management	Quản lý bằng chứng
Refund Processing	Hoàn tiền
Partial Refund	Refund một phần
Admin Resolution Center	Trung tâm xử lý tranh chấp
Financial Adjustment	Điều chỉnh ví/thanh toán
Worker Penalty	Phạt thợ
Dispute Audit	Audit toàn bộ xử lý
2. Why Dispute System Is Critical
Thực tế marketplace:
Situation	Example
Worker làm hỏng đồ	Vỡ lavabo
Service không hoàn thành	Sửa không xong
Overcharge	Báo giá gian lận
Customer fraud	Đòi refund sai
Payment conflict	Đã trả tiền nhưng chưa sửa
Nếu không có dispute system:
- mất khách hàng
- support xử lý thủ công
- không có audit
- refund lộn xộn
- khó scale operations
Dispute System giải quyết:
- chuẩn hóa xử lý khiếu nại
- quản lý refund
- admin có evidence đầy đủ
- audit tài chính minh bạch
- giữ uy tín nền tảng
3. High-Level Architecture
flowchart TB

    CUSTOMER["Customer"]

    WORKER["Worker"]

    FRONTEND["React Frontend"]

    DISPUTE["Dispute Service"]

    REFUND["Refund Service"]

    PAYMENT["Payment Gateway"]

    WALLET["Wallet Service"]

    CHAT["Chat Service"]

    BOOKING["Booking Service"]

    ADMIN["Admin Dashboard"]

    DB["PostgreSQL"]

    CUSTOMER --> FRONTEND
    WORKER --> FRONTEND

    FRONTEND --> DISPUTE

    DISPUTE --> BOOKING

    DISPUTE --> CHAT

    DISPUTE --> REFUND

    REFUND --> PAYMENT

    REFUND --> WALLET

    DISPUTE --> DB

    ADMIN --> DISPUTE
4. Dispute Workflow
flowchart TD

    ISSUE["Issue Occurs"]

    CREATE["Create Dispute"]

    EVIDENCE["Upload Evidence"]

    REVIEW["Admin Review"]

    DECISION["Decision"]

    REFUND["Refund"]

    CLOSE["Close Case"]

    ISSUE --> CREATE

    CREATE --> EVIDENCE

    EVIDENCE --> REVIEW

    REVIEW --> DECISION

    DECISION --> REFUND

    REFUND --> CLOSE
5. Dispute State Diagram
stateDiagram-v2

    [*] --> OPEN

    OPEN --> INVESTIGATING

    INVESTIGATING --> RESOLVED

    INVESTIGATING --> REFUNDED

    INVESTIGATING --> REJECTED

    RESOLVED --> CLOSED

    REFUNDED --> CLOSED
6. Refund Types
Refund Type	Description
FULL_REFUND	Hoàn toàn bộ
PARTIAL_REFUND	Hoàn một phần
WALLET_REFUND	Refund vào ví
PAYMENT_GATEWAY_REFUND	Refund qua VNPay/MoMo
7. Refund Formula
Partial refund

RefundAmount=TotalPaid×RefundPercentage

Example
Total Paid	Refund %	Refund
500,000	40%	200,000
8. Admin Resolution Workflow
flowchart TD

    DISPUTE["Open Dispute"]

    CHAT["Read Chat"]

    TIMELINE["View Timeline"]

    EVIDENCE["Check Evidence"]

    DECISION["Admin Decision"]

    REFUND["Refund Action"]

    PENALTY["Worker Penalty"]

    CLOSE["Close Case"]

    DISPUTE --> CHAT

    CHAT --> TIMELINE

    TIMELINE --> EVIDENCE

    EVIDENCE --> DECISION

    DECISION --> REFUND

    DECISION --> PENALTY

    REFUND --> CLOSE
9. Sequence Diagram – Create Dispute
sequenceDiagram

    actor Customer

    participant Frontend
    participant DisputeAPI
    participant Database
    participant NotificationService

    Customer->>Frontend: Submit dispute

    Frontend->>DisputeAPI: POST /disputes

    DisputeAPI->>Database: Save dispute

    Database-->>DisputeAPI: Created

    DisputeAPI->>NotificationService: Notify admin
10. Sequence Diagram – Refund Processing
sequenceDiagram

    participant Admin

    participant DisputeAPI

    participant RefundService

    participant WalletService

    participant PaymentGateway

    participant Database

    Admin->>DisputeAPI: Approve refund

    DisputeAPI->>RefundService: Process refund

    RefundService->>PaymentGateway: Refund payment

    RefundService->>WalletService: Adjust balances

    WalletService->>Database: Save ledger
11. Dispute Architecture
flowchart TB

    CONTROLLER["Dispute Controller"]

    SERVICE["Dispute Service"]

    REFUND["Refund Service"]

    EVIDENCE["Evidence Service"]

    AUDIT["Audit Service"]

    DB["PostgreSQL"]

    CONTROLLER --> SERVICE

    SERVICE --> REFUND

    SERVICE --> EVIDENCE

    SERVICE --> AUDIT

    SERVICE --> DB
12. Database Design
disputes
Column	Type
id	UUID
booking_id	UUID
customer_id	UUID
worker_id	UUID
reason	text
status	varchar
created_at	timestamp
dispute_messages
Column	Type
id	UUID
dispute_id	UUID
sender_id	UUID
message	text
created_at	timestamp
dispute_evidences
Column	Type
id	UUID
dispute_id	UUID
file_url	varchar
uploaded_by	UUID
created_at	timestamp
refunds
Column	Type
id	UUID
dispute_id	UUID
amount	numeric
refund_type	varchar
status	varchar
processed_by	UUID
created_at	timestamp
13. Dispute Status Enum
Status
OPEN
INVESTIGATING
RESOLVED
REFUNDED
REJECTED
CLOSED
14. Refund Status Enum
Status
PENDING
PROCESSING
SUCCESS
FAILED
15. Evidence Types
Evidence
Image
Video
Chat logs
Timeline logs
Payment proof
16. Admin Evidence Center
flowchart TD

    CHAT["Chat Logs"]

    BOOKING["Booking Timeline"]

    PAYMENT["Payment Logs"]

    FILES["Evidence Files"]

    DECISION["Admin Decision"]

    CHAT --> DECISION

    BOOKING --> DECISION

    PAYMENT --> DECISION

    FILES --> DECISION
17. Booking Timeline Integration
Admin should view:
Data
Booking status history
Worker updates
Arrival time
Completion logs
Quotation history
18. Chat Integration
Admin should access:
Data
Customer-worker messages
Images
Quote negotiation
Abuse messages
19. Refund Financial Flow
flowchart TD

    CUSTOMER["Customer Paid"]

    PLATFORM["Platform Wallet"]

    WORKER["Worker Wallet"]

    REFUND["Refund"]

    ADJUST["Adjust Balances"]

    CUSTOMER --> PLATFORM

    PLATFORM --> WORKER

    WORKER --> REFUND

    REFUND --> ADJUST
20. Worker Penalty System
Penalty
Wallet deduction
Temporary suspension
Rating reduction
Strike system
Permanent ban
21. Admin Dashboard
flowchart TD

    DASHBOARD["Dispute Dashboard"]

    OPEN["Open Cases"]

    REFUND["Refund Requests"]

    WORKER["Worker Violations"]

    AUDIT["Financial Audit"]

    DASHBOARD --> OPEN

    DASHBOARD --> REFUND

    DASHBOARD --> WORKER

    DASHBOARD --> AUDIT
22. Refund APIs
Create Dispute
POST /api/v1/disputes

Request:

{
  "bookingId": "uuid",
  "reason": "Worker damaged sink"
}
Upload Evidence
POST /api/v1/disputes/{id}/evidences
Get Dispute Detail
GET /api/v1/disputes/{id}
Refund
POST /api/v1/disputes/{id}/refund

Request:

{
  "amount": 200000,
  "refundType": "PARTIAL_REFUND"
}
23. Security Rules
Rule
Only booking participants can dispute
Refund requires admin role
Refund must audit log
Prevent duplicate refunds
Evidence immutable
24. Audit Logging
Event
DISPUTE_CREATED
EVIDENCE_UPLOADED
REFUND_PROCESSED
WORKER_PENALIZED
DISPUTE_CLOSED
25. Background Jobs
Job
Auto-close inactive dispute
Refund retry
Notify pending dispute
Escalation reminders
Example
RecurringJob.AddOrUpdate(
    "close-old-disputes",
    () => disputeService.CloseExpired(),
    Cron.Daily
);
26. Frontend UX/UI Flow
Customer Flow
flowchart TD

    BOOKING["Booking Detail"]

    REPORT["Report Issue"]

    EVIDENCE["Upload Evidence"]

    STATUS["Track Status"]

    REFUND["Refund Result"]

    BOOKING --> REPORT

    REPORT --> EVIDENCE

    EVIDENCE --> STATUS

    STATUS --> REFUND
Admin Flow
flowchart TD

    DASHBOARD["Admin Dashboard"]

    CASE["Open Case"]

    CHAT["View Chat"]

    TIMELINE["View Timeline"]

    DECISION["Decision"]

    REFUND["Refund"]

    CLOSE["Close Case"]

    DASHBOARD --> CASE

    CASE --> CHAT

    CHAT --> TIMELINE

    TIMELINE --> DECISION

    DECISION --> REFUND

    REFUND --> CLOSE
27. UI Components
Component
Dispute list
Evidence gallery
Refund modal
Chat viewer
Timeline viewer
28. Financial Integrity Rules
Rule
Refund never bypass ledger
Refund linked to dispute
Worker balance can go negative
All financial changes auditable
29. Production Recommendations
Recommendation
Snapshot booking state
Keep immutable evidence
Log all admin actions
Use financial transactions
Add escalation flow later
30. Future Enhancements
Future
AI fraud detection
Auto dispute classification
Arbitration workflow
Insurance integration
Video evidence analysis
31. Final Architecture
flowchart TB

    FRONTEND["React"]

    DISPUTE["Dispute Service"]

    REFUND["Refund Service"]

    WALLET["Wallet Service"]

    PAYMENT["Payment Gateway"]

    CHAT["Chat Service"]

    TIMELINE["Booking Timeline"]

    AUDIT["Audit Logging"]

    DB["PostgreSQL"]

    FRONTEND --> DISPUTE

    DISPUTE --> REFUND

    REFUND --> WALLET

    REFUND --> PAYMENT

    DISPUTE --> CHAT

    DISPUTE --> TIMELINE

    DISPUTE --> AUDIT

    DISPUTE --> DB
32. MVP Priorities
Priority	Feature
🔥 P4	Create dispute
🔥 P4	Evidence upload
🔥 P4	Admin review
🔥 P4	Partial refund
🔥 P4	Refund ledger
HIGH	Worker penalties
HIGH	Auto-close disputes
FUTURE	AI fraud
33. Final Outcome

Sau khi hoàn thiện module này, FixNow sẽ có:

Capability	Status
Marketplace	✅
Payments	✅
Wallet	✅
Refunds	✅
Dispute Resolution	✅
Financial Audit	✅
Admin Operations	✅
Customer Protection	✅