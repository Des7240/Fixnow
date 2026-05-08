P0 – Booking + Worker Matching Module Design (FixNow)
1. Booking + Worker Matching Scope (MVP)
Chức năng thuộc P0
Feature	Description
Create Booking	Khách tạo đơn
Booking Detail	Xem chi tiết booking
Nearby Worker Search	Tìm thợ gần
Worker Matching	Match thợ phù hợp
Worker Accept/Reject	Thợ nhận/từ chối
Booking Assignment	Gán thợ
Booking Workflow	Trạng thái booking
Cancel Booking	Hủy đơn
Booking History	Lịch sử đơn
Push Notification	Gửi thông báo
2. Booking Workflow
flowchart TD

    START([Start])

    CREATE["Create Booking"]
    SAVE["Save Booking"]

    SEARCH["Search Nearby Workers"]
    MATCH["Worker Matching"]

    NOTIFY["Push Notification"]

    ACCEPT["Worker Accept"]
    REJECT["Worker Reject"]

    ASSIGN["Assign Worker"]

    ONWAY["Worker On The Way"]
    WORKING["Working"]
    COMPLETE["Completed"]

    CANCEL["Cancelled"]

    END([End])

    START --> CREATE
    CREATE --> SAVE

    SAVE --> SEARCH
    SEARCH --> MATCH

    MATCH --> NOTIFY

    NOTIFY --> ACCEPT
    NOTIFY --> REJECT

    ACCEPT --> ASSIGN

    ASSIGN --> ONWAY
    ONWAY --> WORKING
    WORKING --> COMPLETE

    CREATE --> CANCEL
    ASSIGN --> CANCEL

    COMPLETE --> END
    CANCEL --> END
3. Swimlane – Create Booking
flowchart LR

    subgraph CUSTOMER["Customer"]
        U1["Create Booking"]
    end

    subgraph FRONTEND["Frontend React"]
        F1["Validate Request"]
        F2["Call Booking API"]
        F3["Display Booking Status"]
    end

    subgraph API["Booking API"]
        A1["Validate Booking"]
        A2["Create Booking"]
        A3["Save Booking"]
        A4["Trigger Matching"]
    end

    subgraph DB["PostgreSQL"]
        D1["Bookings Table"]
    end

    U1 --> F1
    F1 --> F2

    F2 --> A1
    A1 --> A2
    A2 --> A3

    A3 --> D1

    A3 --> A4

    A4 --> F3
4. Swimlane – Worker Matching
flowchart LR

    subgraph SYSTEM["System"]
        S1["Booking Created"]
    end

    subgraph MATCHING["Matching Service"]
        M1["Get Customer GPS"]
        M2["Find Nearby Workers"]
        M3["Filter Available Workers"]
        M4["Sort By Distance"]
        M5["Send Notifications"]
    end

    subgraph DB["PostGIS"]
        D1["Worker Locations"]
    end

    subgraph WORKER["Worker"]
        W1["Receive Job"]
    end

    S1 --> M1
    M1 --> M2

    M2 --> D1

    D1 --> M3
    M3 --> M4
    M4 --> M5

    M5 --> W1
5. Swimlane – Worker Accept Booking
flowchart LR

    subgraph WORKER["Worker"]
        W1["Open Job"]
        W2["Accept Job"]
    end

    subgraph FRONTEND["Worker App"]
        F1["Call Accept API"]
        F2["Show Assignment"]
    end

    subgraph API["Booking API"]
        A1["Validate Booking"]
        A2["Check Status"]
        A3["Assign Worker"]
        A4["Update Booking"]
    end

    subgraph DB["PostgreSQL"]
        D1["Bookings"]
    end

    W1 --> W2

    W2 --> F1

    F1 --> A1
    A1 --> A2
    A2 --> A3
    A3 --> A4

    A4 --> D1

    A4 --> F2
6. Sequence Diagram – Create Booking
7. Sequence Diagram – Nearby Worker Search
8. Sequence Diagram – Worker Accept Booking
9. Sequence Diagram – Booking Status Update
sequenceDiagram

    actor Worker

    participant Frontend
    participant BookingAPI
    participant BookingService
    participant Database

    Worker->>Frontend: Update booking status

    Frontend->>BookingAPI: PATCH /bookings/{id}/status

    BookingAPI->>BookingService: Validate transition

    BookingService->>Database: Update status

    Database-->>BookingService: Updated

    BookingService-->>BookingAPI: Success

    BookingAPI-->>Frontend: Updated
10. Booking State Diagram
stateDiagram-v2

    [*] --> PENDING

    PENDING --> MATCHING

    MATCHING --> ASSIGNED

    ASSIGNED --> ON_THE_WAY

    ON_THE_WAY --> WORKING

    WORKING --> COMPLETED

    PENDING --> CANCELLED

    ASSIGNED --> CANCELLED
11. Worker Matching Architecture
flowchart TB

    CUSTOMER["Customer"]

    FRONTEND["React Frontend"]

    BOOKINGAPI["Booking API"]

    MATCHING["Worker Matching Service"]

    POSTGIS["PostGIS"]

    REDIS["Redis"]

    FCM["Firebase FCM"]

    WORKER["Worker"]

    CUSTOMER --> FRONTEND

    FRONTEND --> BOOKINGAPI

    BOOKINGAPI --> MATCHING

    MATCHING --> POSTGIS

    MATCHING --> REDIS

    MATCHING --> FCM

    FCM --> WORKER
12. Booking API Contract
Create Booking
POST /api/v1/bookings

Request:

{
  "serviceId": "uuid",
  "address": "Hoan Kiem, Ha Noi",
  "lat": 21.0285,
  "lng": 105.8542,
  "description": "Điều hòa không lạnh"
}

Response:

{
  "bookingId": "uuid",
  "status": "MATCHING"
}
Get Booking Detail
GET /api/v1/bookings/{id}
Cancel Booking
PATCH /api/v1/bookings/{id}/cancel
Worker Accept Booking
POST /api/v1/bookings/{id}/accept
Worker Reject Booking
POST /api/v1/bookings/{id}/reject
Update Booking Status
PATCH /api/v1/bookings/{id}/status

Request:

{
  "status": "ON_THE_WAY"
}
13. Database Design
bookings
Column	Type
id	UUID
customer_id	UUID
worker_id	UUID
service_id	UUID
status	varchar
address	text
lat	decimal
lng	decimal
location	geography(Point)
description	text
created_at	timestamp
booking_status_histories
Column	Type
id	UUID
booking_id	UUID
old_status	varchar
new_status	varchar
updated_by	UUID
created_at	timestamp
worker_locations
Column	Type
worker_id	UUID
location	geography(Point)
updated_at	timestamp
booking_matching_logs
Column	Type
id	UUID
booking_id	UUID
worker_id	UUID
distance	numeric
status	varchar
created_at	timestamp
14. Geo Query Design
Nearby Search
SELECT *
FROM worker_locations
WHERE ST_DWithin(
    location,
    ST_MakePoint(:lng, :lat)::geography,
    5000
)
ORDER BY ST_Distance(
    location,
    ST_MakePoint(:lng, :lat)::geography
)
LIMIT 20;
15. Booking Security Design
Security	Solution
Authentication	JWT
Authorization	Role-based
Geo validation	GPS validation
Booking ownership	Customer-only access
Worker assignment	Single assignment lock
Rate limit	Redis
API security	HTTPS
16. Recommended Tech Stack
Frontend
Feature	Tech
Booking State	Zustand
API	Axios
Map	React Leaflet
GPS	Browser Geolocation
Notification	Firebase
Backend

ASP.NET Core
EF Core
NetTopologySuite
Redis
Firebase Admin SDK
17. MVP Booking Priorities
Priority	Feature
P0	Create Booking
P0	Nearby Worker Search
P0	Worker Matching
P0	Accept Booking
P0	Booking Workflow
P0	Notification
P1	Realtime Tracking
P1	Multi-worker bidding
P2	AI Matching
P2	Dynamic Pricing