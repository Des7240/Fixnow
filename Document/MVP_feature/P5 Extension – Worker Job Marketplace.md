1. Feature Overview
Business Concept

Ngoài:

Realtime push notifications
Thêm:
Worker Job Marketplace
(Chợ đơn cho thợ)
Ý tưởng:

Worker có thể:

mở app
xem danh sách job gần mình
filter theo nhu cầu
tự chọn job muốn bid
Đây là:
Marketplace Type
Grab = push jobs
Shopee = marketplace
FixNow = hybrid realtime marketplace
2. Why Job Marketplace Is Important
Realtime matching có vấn đề:
Problem
Worker offline bỏ lỡ job
Notification miss
Worker muốn tự chọn job
Job tồn đọng
Worker muốn tối ưu tuyến đường
Job Marketplace giải quyết:
- tăng tỷ lệ nhận đơn
- giảm job tồn
- worker chủ động hơn
- tăng competition
- tăng engagement
3. Main Workflow
flowchart TD

    WORKER["Worker"]

    MARKET["Open Job Marketplace"]

    FILTER["Apply Filters"]

    DETAIL["View Job Detail"]

    OFFER["Submit Offer"]

    WAIT["Wait Customer"]

    BOOKING["Booking Created"]

    WORKER --> MARKET

    MARKET --> FILTER

    FILTER --> DETAIL

    DETAIL --> OFFER

    OFFER --> WAIT

    WAIT --> BOOKING
4. Marketplace Architecture
flowchart TB

    WORKER["Worker App"]

    MARKET["Marketplace API"]

    SEARCH["Geo Search Engine"]

    FILTER["Filter Engine"]

    OFFER["Offer Service"]

    DB["PostgreSQL + PostGIS"]

    WORKER --> MARKET

    MARKET --> SEARCH

    MARKET --> FILTER

    MARKET --> OFFER

    SEARCH --> DB
5. Worker Marketplace Page Structure
flowchart LR

    FILTERS["Filter Sidebar"]

    JOBLIST["Nearby Job List"]

    MAP["Mini Map"]

    DETAIL["Job Detail Drawer"]

    FILTERS --> JOBLIST

    JOBLIST --> DETAIL

    JOBLIST --> MAP
6. Main Marketplace Screens
Screen	Purpose
Nearby Jobs	Danh sách đơn gần
Job Detail	Chi tiết job
Submit Offer	Gửi báo giá
My Offers	Offer đã gửi
Saved Jobs	Đơn đã bookmark
7. Core Filters
Distance Filter
Distance
1km
3km
5km
10km
20km
Service Filter
Service
Plumbing
Electrical
AC Repair
Cleaning
Painting
Budget Filter
Budget
< 200k
200k-500k
500k-1tr
> 1tr
Time Filter
Time
Posted within 1h
Today
This week
Status Filter
Status
Open
Urgent
High budget
Few offers
Nearby only
8. Advanced Filters
Filter
Customer rating
Offer count
Estimated duration
Emergency jobs
Verified customer
Favorite locations
9. Sorting Options
Sort
Nearest
Highest budget
Latest
Least offers
Urgent first
10. Geo Search Logic
Worker location search
Find jobs:
- inside radius
- open status
- matching worker skills
- not expired
Geo query concept

ST
D
	​

Within(worker.location,openjob.location,radius)

11. Marketplace State Diagram
stateDiagram-v2

    [*] --> LOADING

    LOADING --> RESULTS

    RESULTS --> FILTERING

    FILTERING --> RESULTS

    RESULTS --> DETAIL

    DETAIL --> OFFER_SUBMITTED
12. Job Discovery Flow
flowchart TD

    OPEN["Open Marketplace"]

    LOCATION["Get Worker Location"]

    SEARCH["Search Nearby Jobs"]

    FILTER["Apply Filters"]

    DETAIL["Open Detail"]

    OFFER["Submit Offer"]

    OPEN --> LOCATION

    LOCATION --> SEARCH

    SEARCH --> FILTER

    FILTER --> DETAIL

    DETAIL --> OFFER
13. Database Optimization
IMPORTANT

Marketplace page sẽ cực nặng query.

Required indexes
Index
GIST(location)
service_type
created_at
status
budget
Example
CREATE INDEX idx_open_jobs_location
ON open_jobs
USING GIST(location);
14. Suggested Database Fields
open_jobs
Column	Type
budget_min	numeric
budget_max	numeric
urgency_level	varchar
expires_at	timestamp
offer_count	int
15. Marketplace APIs
Get Nearby Jobs
GET /api/v1/marketplace/jobs
Query Params
Param	Example
radius	5
serviceType	plumbing
sort	nearest
minBudget	200000
maxBudget	1000000
Example
GET /api/v1/marketplace/jobs?radius=5&serviceType=plumbing&sort=nearest
Get Job Detail
GET /api/v1/marketplace/jobs/{id}
Save Job
POST /api/v1/marketplace/jobs/{id}/save
Submit Offer
POST /api/v1/marketplace/jobs/{id}/offers
16. Sequence Diagram – Browse Marketplace
sequenceDiagram

    actor Worker

    participant Frontend

    participant MarketplaceAPI

    participant SearchEngine

    participant Database

    Worker->>Frontend: Open marketplace

    Frontend->>MarketplaceAPI: Get nearby jobs

    MarketplaceAPI->>SearchEngine: Geo query

    SearchEngine->>Database: Fetch jobs

    Database-->>Frontend: Job list
17. Sequence Diagram – Filter Jobs
sequenceDiagram

    actor Worker

    participant Frontend

    participant MarketplaceAPI

    participant Database

    Worker->>Frontend: Apply filters

    Frontend->>MarketplaceAPI: Query jobs

    MarketplaceAPI->>Database: Filter query

    Database-->>Frontend: Filtered results
18. UI/UX Architecture
Desktop Layout
flowchart LR

    FILTER["Filters"]

    LIST["Job List"]

    MAP["Map"]

    FILTER --> LIST

    LIST --> MAP
Mobile Layout
flowchart TD

    SEARCH["Search Bar"]

    FILTER["Bottom Filter Sheet"]

    LIST["Job Cards"]

    DETAIL["Bottom Drawer"]

    SEARCH --> FILTER

    FILTER --> LIST

    LIST --> DETAIL
19. Recommended Job Card
UI Element
Service icon
Distance
Budget
Posted time
Offer count
Urgency badge
Customer rating
Quick offer button
20. Marketplace Realtime Events
Event
NEW_OPEN_JOB
JOB_EXPIRED
OFFER_ACCEPTED
OFFER_REJECTED
JOB_UPDATED
21. Realtime Update Flow
flowchart TD

    NEWJOB["New Open Job"]

    SIGNALR["SignalR"]

    STORE["Marketplace Store"]

    UI["Realtime UI Update"]

    NEWJOB --> SIGNALR

    SIGNALR --> STORE

    STORE --> UI
22. Saved Jobs Feature
Worker can:
Feature
Save jobs
Watch jobs
Receive reminders
Reopen viewed jobs
23. Smart Marketplace Features (Future)
Feature
AI recommended jobs
Smart routing
Auto bid suggestion
Price prediction
Traffic-aware ETA
24. Worker Productivity Features
Feature
Multi-offer management
Fast quote templates
Favorite service zones
Route optimization
25. Anti-Spam Protection
Protection
Offer cooldown
Limit fake offers
Limit low-quality spam
Detect mass bidding
26. Admin Marketplace Monitoring
flowchart TD

    DASHBOARD["Marketplace Dashboard"]

    JOBS["Open Jobs"]

    OFFERS["Offers"]

    SPAM["Spam Detection"]

    ANALYTICS["Analytics"]

    DASHBOARD --> JOBS

    JOBS --> OFFERS

    OFFERS --> SPAM

    DASHBOARD --> ANALYTICS
27. Analytics Metrics
Metric
Average offer count
Avg response time
Offer acceptance rate
Marketplace conversion
Jobs without offers
28. Frontend State Design
Store
marketplaceStore
filterStore
savedJobsStore
workerOfferStore
29. Recommended Frontend Components
Component
MarketplaceFilters
JobCard
JobMap
OfferModal
RadiusSlider
BudgetRange
SortDropdown
30. Performance Recommendations
Optimization
Infinite scroll
Virtualized list
Query caching
Debounced filters
Lazy load maps
31. Mobile UX Recommendations
Recommendation
Swipe job cards
Sticky filter button
Bottom drawer detail
One-tap offer
Offline cache
32. Final Marketplace Architecture
flowchart TB

    WORKER["Worker App"]

    MARKET["Marketplace"]

    SEARCH["Geo Search"]

    FILTER["Filter Engine"]

    OFFER["Offer System"]

    SIGNALR["Realtime"]

    DB["PostGIS"]

    WORKER --> MARKET

    MARKET --> SEARCH

    MARKET --> FILTER

    MARKET --> OFFER

    MARKET --> SIGNALR

    SEARCH --> DB
33. MVP Priorities
Priority	Feature
🔥 P5	Nearby jobs list
🔥 P5	Radius filter
🔥 P5	Service filter
🔥 P5	Offer submission
🔥 P5	Job detail
HIGH	Saved jobs
HIGH	Realtime updates
FUTURE	AI recommendations
34. Final Outcome

Sau khi hoàn thiện feature này, FixNow sẽ có:

Capability	Status
Instant matching	✅
Open job marketplace	✅
Competitive bidding	✅
Worker job discovery	✅
Geo marketplace	✅
Smart filtering	✅
Realtime marketplace	✅