1. Module Overview

Sau khi có:

Open Job Posting
Worker Marketplace
Worker Offers/Bidding

thì hệ thống bắt buộc cần thêm:

Module
Customer Open Job Management
Admin Marketplace Management
Vì nếu không có:
- customer không quản lý được bài đăng
- admin không kiểm soát được marketplace
- không xử lý được spam/fraud
- không monitor được hệ thống
2. Customer Open Job Management
Business Goal

Customer có thể:

quản lý các bài đăng
xem trạng thái bài đăng
xem offer từ thợ
chỉnh sửa bài đăng
đóng bài đăng
chọn thợ
xem lịch sử
3. Customer Open Job Lifecycle
stateDiagram-v2

    [*] --> DRAFT

    DRAFT --> PUBLISHED

    PUBLISHED --> RECEIVING_OFFERS

    RECEIVING_OFFERS --> WORKER_SELECTED

    WORKER_SELECTED --> BOOKING_CREATED

    RECEIVING_OFFERS --> CLOSED

    RECEIVING_OFFERS --> EXPIRED

    WORKER_SELECTED --> CANCELLED
4. Customer Open Job Dashboard
flowchart LR

    ACTIVE["Active Jobs"]

    OFFERS["Offers"]

    HISTORY["History"]

    SAVED["Saved Workers"]

    ACTIVE --> OFFERS

    OFFERS --> HISTORY
5. Customer Main Screens
Screen
My Open Jobs
Open Job Detail
Worker Offers
Compare Workers
Edit Job
Closed Jobs
Job Analytics
6. Customer Workflow
flowchart TD

    CREATE["Create Job"]

    MANAGE["Manage Jobs"]

    VIEW["View Offers"]

    COMPARE["Compare Workers"]

    SELECT["Select Worker"]

    BOOKING["Booking Created"]

    CLOSE["Close Job"]

    CREATE --> MANAGE

    MANAGE --> VIEW

    VIEW --> COMPARE

    COMPARE --> SELECT

    SELECT --> BOOKING

    MANAGE --> CLOSE
7. Customer Features
Feature
View offer count
Edit posting
Pause posting
Reopen posting
Close posting
Reject offers
Shortlist workers
Compare workers
8. Customer Job Detail UI
flowchart TD

    INFO["Job Info"]

    OFFERS["Offers List"]

    MAP["Nearby Workers"]

    STATUS["Status Timeline"]

    ACTIONS["Actions"]

    INFO --> OFFERS

    OFFERS --> MAP

    MAP --> STATUS

    STATUS --> ACTIONS
9. Offer Comparison UI
Customer compares:
Field
Estimated price
ETA
Rating
Completed jobs
Reviews
Warranty
Response speed
10. Customer APIs
Get My Open Jobs
GET /api/v1/customer/open-jobs
Get Job Detail
GET /api/v1/customer/open-jobs/{id}
Edit Open Job
PUT /api/v1/customer/open-jobs/{id}
Close Open Job
POST /api/v1/customer/open-jobs/{id}/close
Reject Offer
POST /api/v1/customer/offers/{id}/reject
11. Customer Sequence Diagram
sequenceDiagram

    actor Customer

    participant Frontend

    participant OpenJobAPI

    participant Database

    Customer->>Frontend: Open my jobs

    Frontend->>OpenJobAPI: GET /my-open-jobs

    OpenJobAPI->>Database: Fetch jobs

    Database-->>Frontend: Job list
12. Customer Frontend Screen Flow
flowchart TD

    DASHBOARD["My Jobs"]

    DETAIL["Job Detail"]

    OFFERS["Offers"]

    COMPARE["Compare"]

    SELECT["Select Worker"]

    HISTORY["History"]

    DASHBOARD --> DETAIL

    DETAIL --> OFFERS

    OFFERS --> COMPARE

    COMPARE --> SELECT

    DASHBOARD --> HISTORY
13. Admin Marketplace Management
Business Goal

Admin cần:

kiểm soát toàn bộ marketplace
monitor jobs
detect fraud
moderate content
ban spam workers/customers
14. Admin Marketplace Dashboard
flowchart LR

    JOBS["Open Jobs"]

    OFFERS["Offers"]

    REPORTS["Reports"]

    USERS["Users"]

    ANALYTICS["Analytics"]

    JOBS --> OFFERS

    OFFERS --> REPORTS

    REPORTS --> USERS

    USERS --> ANALYTICS
15. Admin Core Features
Feature
View all jobs
Moderate postings
Remove spam
Suspend worker
Suspend customer
View offer analytics
Detect suspicious pricing
Monitor abuse
16. Admin Monitoring Workflow
flowchart TD

    MONITOR["Monitor Marketplace"]

    REPORT["Reports"]

    REVIEW["Review Job"]

    ACTION["Take Action"]

    LOG["Audit Log"]

    MONITOR --> REPORT

    REPORT --> REVIEW

    REVIEW --> ACTION

    ACTION --> LOG
17. Admin Filters
Filter
Active jobs
Expired jobs
High-value jobs
Spam suspected
Too many offers
No offers
Reported jobs
18. Admin Marketplace States
stateDiagram-v2

    [*] --> NORMAL

    NORMAL --> FLAGGED

    FLAGGED --> REVIEWING

    REVIEWING --> APPROVED

    REVIEWING --> REMOVED

    REVIEWING --> BANNED
19. Admin APIs
Get Marketplace Jobs
GET /api/v1/admin/open-jobs
Moderate Job
POST /api/v1/admin/open-jobs/{id}/moderate
Remove Job
DELETE /api/v1/admin/open-jobs/{id}
Suspend User
POST /api/v1/admin/users/{id}/suspend
20. Admin Sequence Diagram
sequenceDiagram

    actor Admin

    participant Dashboard

    participant AdminAPI

    participant Database

    Admin->>Dashboard: Review jobs

    Dashboard->>AdminAPI: GET /open-jobs

    AdminAPI->>Database: Fetch marketplace data

    Database-->>Dashboard: Job list
21. Marketplace Analytics
Important metrics
Metric
Active jobs
Avg offers/job
Conversion rate
Spam rate
Worker response rate
Customer selection rate
22. Fraud Detection Features
Detection
Fake jobs
Fake offers
Abnormal pricing
Offer spam
Suspicious accounts
23. Admin Realtime Monitoring
flowchart TD

    MARKET["Marketplace Events"]

    SIGNALR["Realtime Gateway"]

    ADMIN["Admin Dashboard"]

    ALERTS["Alerts"]

    MARKET --> SIGNALR

    SIGNALR --> ADMIN

    ADMIN --> ALERTS
24. Notifications
Customer notifications
Event
New offer
Offer accepted
Offer rejected
Job expired
Admin notifications
Event
Spam suspected
High-value job
Abuse reports
Fraud alerts
25. Database Additions
open_jobs
Column	Type
moderation_status	varchar
report_count	int
expires_at	timestamp
closed_reason	varchar
worker_offers
Column	Type
moderation_status	varchar
spam_score	numeric
26. Shared UI Components
Component
OpenJobCard
OfferComparison
WorkerPreview
JobModerationPanel
MarketplaceAnalytics
SpamAlert
27. Customer Mobile UX
UX
Swipe offers
Quick compare
Push notifications
One-tap select worker
28. Admin UX Recommendations
UX
Table virtualization
Bulk moderation
Live analytics
Real-time alerts
29. Performance Recommendations
Optimization
Infinite scroll
Cached filters
Lazy loading
Geo indexing
Query pagination
30. Final Architecture
flowchart TB

    CUSTOMER["Customer"]

    WORKER["Worker"]

    ADMIN["Admin"]

    FRONTEND["React Frontend"]

    MARKETPLACE["Marketplace Service"]

    OFFER["Offer Service"]

    MODERATION["Moderation Service"]

    ANALYTICS["Analytics"]

    DB["PostgreSQL + PostGIS"]

    CUSTOMER --> FRONTEND

    WORKER --> FRONTEND

    ADMIN --> FRONTEND

    FRONTEND --> MARKETPLACE

    MARKETPLACE --> OFFER

    OFFER --> MODERATION

    MODERATION --> ANALYTICS

    MARKETPLACE --> DB
31. MVP Priorities
Priority	Feature
🔥 P5	Customer job management
🔥 P5	Worker offers
🔥 P5	Offer comparison
🔥 P5	Admin marketplace dashboard
🔥 P5	Marketplace moderation
HIGH	Fraud detection
HIGH	Analytics
FUTURE	AI moderation
32. Final Outcome

Sau khi hoàn thiện phần này, FixNow sẽ có:

Capability	Status
Customer-managed marketplace	✅
Worker bidding marketplace	✅
Admin moderation	✅
Marketplace analytics	✅
Fraud monitoring	✅
Geo service marketplace	✅