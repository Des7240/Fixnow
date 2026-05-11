1. Module Overview
Business Concept

Khác với flow Booking hiện tại:

Customer tạo booking
→ System auto match worker
→ Worker accept
Feature mới:
Customer đăng bài sửa chữa công khai
→ Worker gần đó chủ động gửi offer/báo giá
→ Customer so sánh & chọn worker tốt nhất
Đây là mô hình:
Marketplace Type
Grab = instant matching
Upwork = bidding marketplace
Feature này = hybrid marketplace
2. Why This Feature Is Important
Các dịch vụ thực tế KHÔNG phù hợp instant matching:
Service
Sửa điện âm tường
Sửa điều hòa
Sơn nhà
Chống thấm
Sửa nước phức tạp
Vì:
cần khảo sát
giá không cố định
worker cần phân tích
customer muốn so sánh nhiều thợ
Feature này giải quyết:
- tăng conversion
- tăng trust
- tăng cạnh tranh giá
- tăng tỷ lệ booking thành công
- customer có nhiều lựa chọn
3. Core Workflow
flowchart TD

    CUSTOMER["Customer"]

    CREATE["Create Open Job"]

    SEARCH["Find Nearby Workers"]

    NOTIFY["Notify Workers"]

    OFFER["Workers Send Offers"]

    REVIEW["Customer Reviews Offers"]

    SELECT["Select Worker"]

    BOOKING["Convert To Booking"]

    CUSTOMER --> CREATE

    CREATE --> SEARCH

    SEARCH --> NOTIFY

    NOTIFY --> OFFER

    OFFER --> REVIEW

    REVIEW --> SELECT

    SELECT --> BOOKING
4. Marketplace Architecture
flowchart TB

    CUSTOMER["Customer"]

    WORKER["Workers"]

    FRONTEND["React Frontend"]

    OPENJOB["Open Job Service"]

    MATCHING["Radius Matching Engine"]

    OFFER["Offer Service"]

    NOTIFICATION["Notification Service"]

    BOOKING["Booking Service"]

    DB["PostgreSQL + PostGIS"]

    CUSTOMER --> FRONTEND
    WORKER --> FRONTEND

    FRONTEND --> OPENJOB

    OPENJOB --> MATCHING

    MATCHING --> NOTIFICATION

    WORKER --> OFFER

    OFFER --> BOOKING

    OPENJOB --> DB
5. Main Entities
Entity	Description
OpenJob	Bài đăng sửa chữa
WorkerOffer	Offer/báo giá từ thợ
OfferAttachment	Ảnh/video phân tích
OfferSelection	Worker được chọn
6. Open Job Lifecycle
stateDiagram-v2

    [*] --> OPEN

    OPEN --> RECEIVING_OFFERS

    RECEIVING_OFFERS --> WORKER_SELECTED

    WORKER_SELECTED --> BOOKING_CREATED

    RECEIVING_OFFERS --> EXPIRED

    WORKER_SELECTED --> CANCELLED
7. Worker Offer Lifecycle
stateDiagram-v2

    [*] --> SUBMITTED

    SUBMITTED --> VIEWED

    VIEWED --> ACCEPTED

    VIEWED --> REJECTED

    ACCEPTED --> BOOKING_CREATED
8. Customer Journey
flowchart TD

    CREATE["Create Open Job"]

    LOCATION["Choose Radius"]

    IMAGES["Upload Images"]

    WAIT["Wait For Offers"]

    COMPARE["Compare Workers"]

    DETAIL["View Worker Detail"]

    SELECT["Select Worker"]

    PAYMENT["Payment"]

    CREATE --> LOCATION

    LOCATION --> IMAGES

    IMAGES --> WAIT

    WAIT --> COMPARE

    COMPARE --> DETAIL

    DETAIL --> SELECT

    SELECT --> PAYMENT
9. Worker Journey
flowchart TD

    NOTIFICATION["Receive Nearby Job"]

    VIEW["View Job"]

    ANALYSIS["Analyze Problem"]

    ESTIMATE["Estimated Price"]

    OFFER["Send Offer"]

    WAIT["Wait Response"]

    ACCEPT["Accepted"]

    BOOKING["Booking Created"]

    NOTIFICATION --> VIEW

    VIEW --> ANALYSIS

    ANALYSIS --> ESTIMATE

    ESTIMATE --> OFFER

    OFFER --> WAIT

    WAIT --> ACCEPT

    ACCEPT --> BOOKING
10. Admin Monitoring Flow
flowchart TD

    DASHBOARD["Admin Dashboard"]

    OPENJOBS["Open Jobs"]

    OFFERS["Worker Offers"]

    REPORTS["Abuse Reports"]

    DISPUTES["Disputes"]

    DASHBOARD --> OPENJOBS

    OPENJOBS --> OFFERS

    OFFERS --> REPORTS

    REPORTS --> DISPUTES
11. Radius Matching Logic
Customer chọn bán kính:
Radius
3km
5km
10km
20km
Worker matching:
Find workers:
- Online
- Verified
- Matching skills
- Inside radius
Geo formula

d=2rarcsin
sin
2
(
2
ϕ
2
	​

−ϕ
1
	​

	​

)+cos(ϕ
1
	​

)cos(ϕ
2
	​

)sin
2
(
2
λ
2
	​

−λ
1
	​

	​

)
	​


12. Worker Scoring Logic
Suggested ranking factors
Factor	Weight
Distance	30%
Rating	25%
Completed Jobs	20%
Response Speed	15%
Completion Rate	10%
Worker score formula

WorkerScore=DistanceScore+RatingScore+CompletionScore+ResponseScore

13. Offer Structure
Worker offer contains:
Field
Estimated price
Analysis
Estimated arrival
Estimated repair time
Images
Warranty note
Example
{
  "estimatedPrice": 450000,
  "analysis": "Likely broken water valve",
  "estimatedArrivalMinutes": 25,
  "estimatedRepairTimeMinutes": 60,
  "warrantyDays": 30
}
14. Database Design
open_jobs
Column	Type
id	UUID
customer_id	UUID
title	varchar
description	text
location	geography
radius_km	int
status	varchar
created_at	timestamp
open_job_images
Column	Type
id	UUID
open_job_id	UUID
file_url	varchar
worker_offers
Column	Type
id	UUID
open_job_id	UUID
worker_id	UUID
estimated_price	numeric
analysis	text
estimated_arrival	int
status	varchar
created_at	timestamp
offer_attachments
Column	Type
id	UUID
offer_id	UUID
file_url	varchar
15. APIs
Create Open Job
POST /api/v1/open-jobs

Request:

{
  "title": "Broken water pipe",
  "description": "Water leaking under sink",
  "latitude": 21.0285,
  "longitude": 105.8542,
  "radiusKm": 5
}
Get Nearby Open Jobs
GET /api/v1/open-jobs/nearby
Submit Offer
POST /api/v1/open-jobs/{id}/offers
Select Worker
POST /api/v1/open-jobs/{id}/select-worker
Get Offers
GET /api/v1/open-jobs/{id}/offers
16. Sequence Diagram – Create Open Job
sequenceDiagram

    actor Customer

    participant Frontend

    participant OpenJobAPI

    participant MatchingEngine

    participant NotificationService

    participant Database

    Customer->>Frontend: Create open job

    Frontend->>OpenJobAPI: POST /open-jobs

    OpenJobAPI->>Database: Save job

    OpenJobAPI->>MatchingEngine: Find nearby workers

    MatchingEngine->>NotificationService: Notify workers
17. Sequence Diagram – Worker Offer
sequenceDiagram

    actor Worker

    participant Frontend

    participant OfferAPI

    participant Database

    participant NotificationService

    Worker->>Frontend: Submit offer

    Frontend->>OfferAPI: POST /offers

    OfferAPI->>Database: Save offer

    OfferAPI->>NotificationService: Notify customer
18. Sequence Diagram – Select Worker
sequenceDiagram

    actor Customer

    participant Frontend

    participant OpenJobAPI

    participant BookingService

    participant NotificationService

    Customer->>Frontend: Select worker

    Frontend->>OpenJobAPI: Select offer

    OpenJobAPI->>BookingService: Create booking

    BookingService->>NotificationService: Notify worker
19. Offer Comparison UX
flowchart LR

    OFFER1["Worker A"]

    OFFER2["Worker B"]

    OFFER3["Worker C"]

    COMPARE["Compare"]

    DETAIL["Worker Detail"]

    SELECT["Select"]

    OFFER1 --> COMPARE

    OFFER2 --> COMPARE

    OFFER3 --> COMPARE

    COMPARE --> DETAIL

    DETAIL --> SELECT
20. Worker Profile Preview
Customer can view:
Info
Rating
Total completed jobs
KYC verified
Skills
Distance
Reviews
Response time
21. Recommended Offer Card UI
UI Element
Worker avatar
Rating badge
Estimated price
ETA
Analysis summary
Accept button
22. Realtime Events
Event
OPEN_JOB_CREATED
NEW_WORKER_OFFER
OFFER_ACCEPTED
OPEN_JOB_EXPIRED
BOOKING_CREATED
23. Notification Flow
flowchart TD

    OPENJOB["Open Job"]

    WORKERS["Nearby Workers"]

    OFFER["Offer Submitted"]

    CUSTOMER["Customer Notified"]

    SELECT["Worker Selected"]

    OPENJOB --> WORKERS

    WORKERS --> OFFER

    OFFER --> CUSTOMER

    CUSTOMER --> SELECT
24. Expiration Logic
Open jobs auto expire
Rule
No offer in 24h
Customer inactive
Worker selected
Customer cancelled
25. Fraud Prevention
Protection
Limit fake postings
Limit spam offers
Require verified worker
Prevent price manipulation
26. Moderation Rules
Rule
Block abusive content
Detect fake reviews
Detect fake offers
Monitor suspicious pricing
27. Admin Monitoring Dashboard
flowchart TD

    DASHBOARD["Marketplace Dashboard"]

    ACTIVE["Active Open Jobs"]

    OFFERS["Offers"]

    REPORTS["Reports"]

    ANALYTICS["Marketplace Analytics"]

    DASHBOARD --> ACTIVE

    ACTIVE --> OFFERS

    OFFERS --> REPORTS

    DASHBOARD --> ANALYTICS
28. Frontend UX/UI Flow
Customer Screens
flowchart TD

    HOME["Home"]

    CREATE["Create Open Job"]

    WAIT["Waiting Offers"]

    COMPARE["Compare Offers"]

    DETAIL["Worker Detail"]

    SELECT["Select Worker"]

    BOOKING["Booking Tracking"]

    HOME --> CREATE

    CREATE --> WAIT

    WAIT --> COMPARE

    COMPARE --> DETAIL

    DETAIL --> SELECT

    SELECT --> BOOKING
Worker Screens
flowchart TD

    NEARBY["Nearby Jobs"]

    DETAIL["Job Detail"]

    OFFER["Create Offer"]

    WAIT["Waiting"]

    ACCEPTED["Accepted"]

    BOOKING["Booking"]

    NEARBY --> DETAIL

    DETAIL --> OFFER

    OFFER --> WAIT

    WAIT --> ACCEPTED

    ACCEPTED --> BOOKING
29. Shared UI Components
Component
OfferCard
WorkerComparisonTable
RadiusSelector
WorkerRating
OpenJobCard
WorkerPreviewModal
30. Mobile UX Recommendations
Recommendation
Worker app mobile-first
Large accept buttons
Floating action CTA
Quick quote templates
Push notifications
31. Suggested Future AI Features
AI Feature
AI estimated pricing
AI worker recommendation
AI fraud detection
AI problem classification
32. Future Marketplace Expansion
Future
Auction-style bidding
Emergency priority jobs
Subscription workers
Smart auto-selection
33. Final Architecture
flowchart TB

    FRONTEND["React Frontend"]

    OPENJOB["Open Job Service"]

    OFFER["Offer Service"]

    MATCHING["Geo Matching"]

    NOTIFICATION["Realtime Notifications"]

    BOOKING["Booking Service"]

    PAYMENT["Payment Service"]

    REVIEW["Review System"]

    DB["PostgreSQL + PostGIS"]

    FRONTEND --> OPENJOB

    OPENJOB --> MATCHING

    MATCHING --> NOTIFICATION

    OFFER --> BOOKING

    BOOKING --> PAYMENT

    PAYMENT --> REVIEW

    OPENJOB --> DB
34. MVP Priorities
Priority	Feature
🔥 P5	Create open job
🔥 P5	Radius selection
🔥 P5	Worker offers
🔥 P5	Offer comparison
🔥 P5	Worker selection
HIGH	Offer attachments
HIGH	Offer ranking
FUTURE	AI pricing
35. Final Outcome

Sau khi hoàn thiện module này, FixNow sẽ có:

Capability	Status
Instant booking	✅
Competitive marketplace	✅
Worker bidding	✅
Radius-based marketplace	✅
Quote comparison	✅
Worker reputation system	✅
Marketplace scalability	✅