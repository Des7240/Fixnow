1. Module Scope
Module	Description
Payment Gateway	VNPay / MoMo integration
Booking Payment	Thanh toán đơn hàng
Payment Callback	Xử lý callback từ gateway
Transaction Management	Quản lý giao dịch
Payment Status	Theo dõi trạng thái thanh toán
Simple Commission	Tính phí nền tảng cơ bản
Invoice Summary	Chi tiết thanh toán
2. Payment MVP Scope
MVP chỉ nên hỗ trợ:
Feature	MVP
COD	✅
VNPay	✅
MoMo	✅
Online payment	✅
Payment callback	✅
Transaction history	✅
Simple platform fee	✅
MVP KHÔNG nên làm ngay
Feature
Wallet
Partial refund
Escrow
Split payment
Auto payout
Subscription
3. Payment Architecture
flowchart TB

    CUSTOMER["Customer"]

    FRONTEND["React Frontend"]

    PAYMENTAPI["Payment API"]

    BOOKING["Booking Service"]

    VNPay["VNPay"]

    MOMO["MoMo"]

    CALLBACK["Payment Callback"]

    DB["PostgreSQL"]

    CUSTOMER --> FRONTEND

    FRONTEND --> PAYMENTAPI

    PAYMENTAPI --> BOOKING

    PAYMENTAPI --> VNPay
    PAYMENTAPI --> MOMO

    VNPay --> CALLBACK
    MOMO --> CALLBACK

    CALLBACK --> DB
    CALLBACK --> BOOKING
4. Payment Workflow
flowchart TD

    BOOKING["Booking Completed"]

    SELECT["Select Payment Method"]

    CREATE["Create Payment"]

    REDIRECT["Redirect To Gateway"]

    CALLBACK["Gateway Callback"]

    VERIFY["Verify Signature"]

    SUCCESS["Payment Success"]

    FAILED["Payment Failed"]

    UPDATE["Update Booking"]

    BOOKING --> SELECT

    SELECT --> CREATE

    CREATE --> REDIRECT

    REDIRECT --> CALLBACK

    CALLBACK --> VERIFY

    VERIFY --> SUCCESS

    VERIFY --> FAILED

    SUCCESS --> UPDATE
5. Payment State Diagram
stateDiagram-v2

    [*] --> PENDING

    PENDING --> PROCESSING

    PROCESSING --> SUCCESS

    PROCESSING --> FAILED

    PROCESSING --> CANCELLED

    SUCCESS --> REFUNDED
6. Booking Payment Flow
flowchart TD

    COMPLETED["Booking Completed"]

    PAYMENT["Waiting Payment"]

    PAID["Paid"]

    UNPAID["Unpaid"]

    CANCELLED["Cancelled"]

    COMPLETED --> PAYMENT

    PAYMENT --> PAID

    PAYMENT --> UNPAID

    PAYMENT --> CANCELLED
7. Swimlane – VNPay Payment Flow
flowchart LR

    subgraph CUSTOMER["Customer"]
        C1["Choose VNPay"]
    end

    subgraph FRONTEND["Frontend"]
        F1["Call Payment API"]
        F2["Redirect To VNPay"]
    end

    subgraph API["Payment API"]
        A1["Create Payment URL"]
    end

    subgraph VNPAY["VNPay"]
        V1["Payment Gateway"]
    end

    subgraph CALLBACK["Callback API"]
        CB1["Verify Signature"]
        CB2["Update Transaction"]
    end

    subgraph DB["Database"]
        D1["payments"]
        D2["transactions"]
    end

    C1 --> F1

    F1 --> A1

    A1 --> F2

    F2 --> V1

    V1 --> CB1

    CB1 --> CB2

    CB2 --> D1
    CB2 --> D2
8. Sequence Diagram – VNPay Payment
sequenceDiagram

    actor Customer

    participant Frontend
    participant PaymentAPI
    participant VNPay
    participant CallbackAPI
    participant Database

    Customer->>Frontend: Pay booking

    Frontend->>PaymentAPI: POST /payments/vnpay

    PaymentAPI->>VNPay: Generate payment URL

    VNPay-->>Frontend: Redirect URL

    Customer->>VNPay: Complete payment

    VNPay->>CallbackAPI: Payment callback

    CallbackAPI->>Database: Update payment status

    Database-->>CallbackAPI: Saved
9. Sequence Diagram – MoMo Payment
sequenceDiagram

    actor Customer

    participant Frontend
    participant PaymentAPI
    participant MoMo
    participant CallbackAPI
    participant Database

    Customer->>Frontend: Pay booking

    Frontend->>PaymentAPI: POST /payments/momo

    PaymentAPI->>MoMo: Create payment request

    MoMo-->>Frontend: Payment URL

    Customer->>MoMo: Complete payment

    MoMo->>CallbackAPI: Payment callback

    CallbackAPI->>Database: Update payment

    Database-->>CallbackAPI: Saved
10. Payment Architecture – Backend
flowchart TB

    CONTROLLER["Payment Controller"]

    SERVICE["Payment Service"]

    GATEWAY["Gateway Provider"]

    VNPAY["VNPay Provider"]

    MOMO["MoMo Provider"]

    DB["PostgreSQL"]

    CONTROLLER --> SERVICE

    SERVICE --> GATEWAY

    GATEWAY --> VNPAY

    GATEWAY --> MOMO

    SERVICE --> DB
11. Recommended Payment Provider Pattern
Interface
public interface IPaymentProvider
{
    Task<string> CreatePaymentUrlAsync(PaymentRequest request);

    Task<PaymentResult> VerifyCallbackAsync(
        Dictionary<string, string> data
    );
}
Implementations
Provider
VNPayProvider
MoMoProvider
12. Payment APIs
Create VNPay Payment
POST /api/v1/payments/vnpay

Request:

{
  "bookingId": "uuid"
}

Response:

{
  "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2..."
}
Create MoMo Payment
POST /api/v1/payments/momo
Payment Callback
GET /api/v1/payments/vnpay/callback
Get Payment Detail
GET /api/v1/payments/{id}
13. Database Design
payments
Column	Type
id	UUID
booking_id	UUID
customer_id	UUID
provider	varchar
amount	numeric
status	varchar
transaction_code	varchar
created_at	timestamp
transactions
Column	Type
id	UUID
payment_id	UUID
gateway_transaction_id	varchar
provider_response	jsonb
status	varchar
created_at	timestamp
payment_callbacks
Column	Type
id	UUID
provider	varchar
payload	jsonb
verified	boolean
created_at	timestamp
14. Payment Status Enum
Status
PENDING
PROCESSING
SUCCESS
FAILED
CANCELLED
REFUNDED
15. Booking Payment Status
Status
UNPAID
PARTIALLY_PAID
PAID
REFUNDED
16. Payment Security Design
Security	Solution
Callback verification	Signature validation
Replay attack	Transaction uniqueness
Tampering	HMAC SHA512
Fake callback	Provider secret
Double payment	Idempotency check
17. VNPay Callback Verification Flow
flowchart TD

    CALLBACK["Receive Callback"]

    SORT["Sort Params"]

    HASH["Generate Secure Hash"]

    VERIFY["Compare Signature"]

    VALID["Valid Payment"]

    INVALID["Reject Callback"]

    CALLBACK --> SORT

    SORT --> HASH

    HASH --> VERIFY

    VERIFY --> VALID

    VERIFY --> INVALID
18. Simple Commission Logic
MVP Commission
Platform Fee = 10%
Worker Receives = 90%
Example
Item	Amount
Service Price	500,000
Platform Fee	50,000
Worker Income	450,000
Database
booking_financials
Column
booking_id
total_amount
platform_fee
worker_income
19. Payment Timeline Workflow
flowchart TD

    CREATED["Payment Created"]

    REDIRECT["Redirect Gateway"]

    PROCESSING["Processing"]

    SUCCESS["Success"]

    FAILED["Failed"]

    CREATED --> REDIRECT

    REDIRECT --> PROCESSING

    PROCESSING --> SUCCESS

    PROCESSING --> FAILED
20. Refund Flow (Future P3)
flowchart TD

    REQUEST["Refund Request"]

    REVIEW["Admin Review"]

    APPROVE["Approve Refund"]

    GATEWAY["Gateway Refund"]

    SUCCESS["Refund Success"]

    REQUEST --> REVIEW

    REVIEW --> APPROVE

    APPROVE --> GATEWAY

    GATEWAY --> SUCCESS
21. Frontend UX/UI Flow
Customer Payment Flow
flowchart TD

    BOOKING["Booking Completed"]

    PAYMENT["Select Payment"]

    GATEWAY["Redirect Gateway"]

    RESULT["Payment Result"]

    HISTORY["Payment History"]

    BOOKING --> PAYMENT

    PAYMENT --> GATEWAY

    GATEWAY --> RESULT

    RESULT --> HISTORY
Admin Financial Flow
flowchart TD

    DASHBOARD["Financial Dashboard"]

    PAYMENTS["Payments"]

    TRANSACTIONS["Transactions"]

    REFUNDS["Refunds"]

    DASHBOARD --> PAYMENTS

    PAYMENTS --> TRANSACTIONS

    TRANSACTIONS --> REFUNDS
22. Recommended Frontend Stack
Feature	Tech
Payment UI	Ant Design
Payment State	Zustand
Payment Redirect	React Router
Currency Format	Intl.NumberFormat
23. Recommended Backend Stack
ASP.NET Core
HttpClient
BackgroundService
EF Core
PostgreSQL

OR

Spring Boot
WebClient
PostgreSQL
Scheduler
24. Recommended MVP Rules
Rule
Chỉ thanh toán sau khi hoàn thành
Chỉ customer được thanh toán
Mỗi booking chỉ có 1 payment SUCCESS
Verify callback bắt buộc
Không trust frontend amount
25. Important Business Rules
Rule
Booking phải COMPLETED mới được thanh toán
Amount tính từ backend
Callback phải verify signature
Payment FAILED không update booking
Payment SUCCESS phải idempotent
26. Logging & Audit Events
Event
PAYMENT_CREATED
PAYMENT_SUCCESS
PAYMENT_FAILED
PAYMENT_CALLBACK_RECEIVED
REFUND_CREATED
27. Final Payment Architecture
flowchart TB

    FRONTEND["React"]

    PAYMENT["Payment API"]

    BOOKING["Booking"]

    VNPAY["VNPay"]

    MOMO["MoMo"]

    CALLBACK["Callback Handler"]

    AUDIT["Audit Logs"]

    DB["PostgreSQL"]

    FRONTEND --> PAYMENT

    PAYMENT --> BOOKING

    PAYMENT --> VNPAY
    PAYMENT --> MOMO

    VNPAY --> CALLBACK
    MOMO --> CALLBACK

    CALLBACK --> DB

    CALLBACK --> AUDIT
28. MVP Priority
Priority	Feature
🔥 P3	VNPay
🔥 P3	MoMo
🔥 P3	Callback Verify
🔥 P3	Payment History
🔥 P3	Transaction Logs
HIGH	Refund
HIGH	Wallet
HIGH	Payout
FUTURE	Escrow
29. Production Recommendations
Recommendation
Dùng sandbox trước
Tách Payment Service
Audit toàn bộ callback
Dùng idempotency
Retry callback safely
Không lưu secret trong code
30. Final Outcome

Sau khi hoàn thiện module này, FixNow sẽ có:

Capability	Status
Marketplace	✅
Worker Matching	✅
Booking Lifecycle	✅
KYC	✅
Reviews	✅
Notifications	✅
Admin Dashboard	✅
Payment Gateway	✅
Financial Flow	✅

