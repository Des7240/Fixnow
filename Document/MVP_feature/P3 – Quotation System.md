1. Module Scope
Module	Description
Quotation Creation	Thợ tạo báo giá
Quote Approval	Khách duyệt báo giá
Booking Final Price	Sinh giá cuối cùng
Quote Revision	Chỉnh sửa báo giá
Quote Rejection	Từ chối báo giá
Quote Timeline	Lịch sử báo giá
Quote Notification	Push notification
2. Why Quotation System Is Critical
Với ngành sửa chữa:
Service	Fixed Price?
Sửa điện	❌
Sửa ống nước	❌
Điều hòa	❌
Máy giặt	❌
Vì:
Thợ phải khảo sát thực tế mới biết:
- mức độ hư hỏng
- linh kiện cần thay
- thời gian sửa
- chi phí thực tế
Quotation System giải quyết:
Worker khảo sát
→ tạo báo giá
→ khách approve
→ booking có final amount
→ mới thanh toán
3. High-Level Architecture
flowchart TB

    CUSTOMER["Customer"]

    WORKER["Worker"]

    FRONTEND["React Frontend"]

    QUOTEAPI["Quotation API"]

    BOOKING["Booking Service"]

    NOTI["Notification Service"]

    DB["PostgreSQL"]

    CUSTOMER --> FRONTEND
    WORKER --> FRONTEND

    FRONTEND --> QUOTEAPI

    QUOTEAPI --> BOOKING

    QUOTEAPI --> NOTI

    QUOTEAPI --> DB
4. Quotation Workflow
flowchart TD

    BOOKING["Booking Assigned"]

    INSPECTION["Worker Inspection"]

    CREATE["Create Quote"]

    NOTIFY["Notify Customer"]

    REVIEW["Customer Review"]

    APPROVE["Approve Quote"]

    REJECT["Reject Quote"]

    FINAL["Update Final Price"]

    WORK["Start Working"]

    BOOKING --> INSPECTION

    INSPECTION --> CREATE

    CREATE --> NOTIFY

    NOTIFY --> REVIEW

    REVIEW --> APPROVE

    REVIEW --> REJECT

    APPROVE --> FINAL

    FINAL --> WORK
5. Booking Lifecycle With Quotation
stateDiagram-v2

    [*] --> PENDING

    PENDING --> MATCHING

    MATCHING --> ASSIGNED

    ASSIGNED --> INSPECTING

    INSPECTING --> QUOTED

    QUOTED --> QUOTE_APPROVED

    QUOTED --> QUOTE_REJECTED

    QUOTE_APPROVED --> WORKING

    WORKING --> COMPLETED
6. Quotation State Diagram
stateDiagram-v2

    [*] --> PENDING

    PENDING --> APPROVED

    PENDING --> REJECTED

    PENDING --> EXPIRED

    REJECTED --> REVISED
7. Swimlane – Create Quotation
flowchart LR

    subgraph WORKER["Worker"]
        W1["Inspect Issue"]
        W2["Create Quote"]
    end

    subgraph FRONTEND["Frontend"]
        F1["Submit Quote"]
    end

    subgraph API["Quotation API"]
        A1["Validate Booking"]
        A2["Calculate Total"]
        A3["Save Quote"]
    end

    subgraph DB["Database"]
        D1["quotations"]
        D2["quotation_items"]
    end

    subgraph NOTI["Notification"]
        N1["Notify Customer"]
    end

    W1 --> W2

    W2 --> F1

    F1 --> A1

    A1 --> A2

    A2 --> A3

    A3 --> D1
    A3 --> D2

    A3 --> N1
8. Sequence Diagram – Create Quotation
sequenceDiagram

    actor Worker

    participant Frontend
    participant QuoteAPI
    participant BookingService
    participant Database
    participant NotificationService

    Worker->>Frontend: Create quotation

    Frontend->>QuoteAPI: POST /quotations

    QuoteAPI->>BookingService: Validate booking status

    BookingService-->>QuoteAPI: Valid

    QuoteAPI->>Database: Save quotation

    Database-->>QuoteAPI: Saved

    QuoteAPI->>NotificationService: Notify customer
9. Sequence Diagram – Approve Quotation
sequenceDiagram

    actor Customer

    participant Frontend
    participant QuoteAPI
    participant BookingService
    participant Database
    participant NotificationService

    Customer->>Frontend: Approve quote

    Frontend->>QuoteAPI: POST /approve

    QuoteAPI->>Database: Update quotation status

    QuoteAPI->>BookingService: Update final amount

    BookingService->>Database: Save booking price

    QuoteAPI->>NotificationService: Notify worker
10. Quotation Architecture
flowchart TB

    CONTROLLER["Quotation Controller"]

    SERVICE["Quotation Service"]

    BOOKING["Booking Service"]

    PRICING["Pricing Engine"]

    NOTI["Notification Service"]

    DB["PostgreSQL"]

    CONTROLLER --> SERVICE

    SERVICE --> BOOKING

    SERVICE --> PRICING

    SERVICE --> NOTI

    SERVICE --> DB
11. Quotation Database Design
quotations
Column	Type
id	UUID
booking_id	UUID
worker_id	UUID
customer_id	UUID
subtotal	numeric
total_amount	numeric
note	text
status	varchar
expires_at	timestamp
created_at	timestamp
quotation_items
Column	Type
id	UUID
quotation_id	UUID
item_name	varchar
quantity	int
unit_price	numeric
total_price	numeric
quotation_revisions
Column	Type
id	UUID
quotation_id	UUID
revision_number	int
old_data	jsonb
created_at	timestamp
12. Quote Item Example
Item	Qty	Price
Replace pipe	1	200,000
Labor fee	1	150,000
Travel fee	1	50,000
Total
Total = 400,000 VND
13. Quotation APIs
Create Quote
POST /api/v1/quotations

Request:

{
  "bookingId": "uuid",
  "items": [
    {
      "itemName": "Replace pipe",
      "quantity": 1,
      "unitPrice": 200000
    }
  ],
  "note": "Need to replace broken pipe"
}
Get Quote Detail
GET /api/v1/quotations/{id}
Approve Quote
POST /api/v1/quotations/{id}/approve
Reject Quote
POST /api/v1/quotations/{id}/reject
Revise Quote
POST /api/v1/quotations/{id}/revise
14. Quotation Status Enum
Status
PENDING
APPROVED
REJECTED
EXPIRED
REVISED
15. Business Rules
Rule
Chỉ assigned worker được tạo quote
Booking phải ASSIGNED/INSPECTING
Customer mới được approve
Chỉ 1 quote APPROVED
APPROVED quote lock final amount
Không edit sau APPROVED
16. Booking Final Amount Logic
Before approval
Booking.totalAmount = NULL
After approval
Booking.totalAmount = ApprovedQuote.totalAmount
17. Notification Integration
Events
Event
QUOTE_CREATED
QUOTE_APPROVED
QUOTE_REJECTED
QUOTE_EXPIRED
Workflow
flowchart TD

    CREATE["Quote Created"]

    PUSH["Push Notification"]

    CUSTOMER["Customer Review"]

    CREATE --> PUSH

    PUSH --> CUSTOMER
18. Quote Expiration Workflow
flowchart TD

    CREATED["Quote Created"]

    WAIT["Waiting Approval"]

    EXPIRE["Timeout 24h"]

    STATUS["Mark Expired"]

    NOTIFY["Notify Worker"]

    CREATED --> WAIT

    WAIT --> EXPIRE

    EXPIRE --> STATUS

    STATUS --> NOTIFY
19. Background Job Integration
Recommended
Job
Expire quotes
Reminder notifications
Cleanup revisions
Example
BackgroundJob.Schedule(
    () => quoteService.ExpireQuote(id),
    TimeSpan.FromHours(24)
);
20. Frontend UX/UI Flow
Customer Flow
flowchart TD

    BOOKING["Booking Detail"]

    QUOTE["View Quote"]

    APPROVE["Approve"]

    REJECT["Reject"]

    PAYMENT["Proceed Payment"]

    BOOKING --> QUOTE

    QUOTE --> APPROVE

    QUOTE --> REJECT

    APPROVE --> PAYMENT
Worker Flow
flowchart TD

    JOB["Assigned Job"]

    INSPECT["Inspect Issue"]

    CREATE["Create Quote"]

    WAIT["Wait Approval"]

    START["Start Work"]

    JOB --> INSPECT

    INSPECT --> CREATE

    CREATE --> WAIT

    WAIT --> START
21. UI Components
Component
Quote item table
Price summary
Approve button
Reject dialog
Quote timeline
22. Recommended Frontend Stack
Feature	Tech
Form	React Hook Form
Table	Ant Design Table
Currency	Intl.NumberFormat
State	Zustand
23. Security Rules
Rule
Validate backend amount
Prevent duplicate approval
Quote immutable after approval
Audit every revision
Validate worker ownership
24. Logging & Audit Events
Event
QUOTE_CREATED
QUOTE_APPROVED
QUOTE_REJECTED
QUOTE_REVISED
QUOTE_EXPIRED
25. Future Enhancements
Feature
AI price recommendation
Material inventory
Multi-worker quote
Negotiation
Partial approval
26. Production Recommendations
Recommendation
Use decimal for money
Snapshot approved quote
Prevent race condition
Audit all price changes
Expire stale quotes
27. Final Architecture
flowchart TB

    FRONTEND["React"]

    QUOTEAPI["Quotation API"]

    BOOKING["Booking Service"]

    PAYMENT["Payment Service"]

    NOTI["Notification Service"]

    JOBS["Background Jobs"]

    DB["PostgreSQL"]

    FRONTEND --> QUOTEAPI

    QUOTEAPI --> BOOKING

    QUOTEAPI --> PAYMENT

    QUOTEAPI --> NOTI

    QUOTEAPI --> JOBS

    QUOTEAPI --> DB
28. MVP Priorities
Priority	Feature
🔥 P3	Create quote
🔥 P3	Approve/reject
🔥 P3	Quote items
🔥 P3	Final booking amount
🔥 P3	Notifications
HIGH	Quote expiration
HIGH	Quote revisions
FUTURE	AI pricing
29. Final Outcome

Sau khi hoàn thiện module này, FixNow sẽ có:

Capability	Status
Marketplace	✅
Matching	✅
Booking Lifecycle	✅
Chat	✅
Notifications	✅
Payments	✅
Dynamic Pricing Flow	✅
Quotation Workflow	✅