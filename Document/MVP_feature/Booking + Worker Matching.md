1. Module Scope
Module	Description
Notification System	Push + in-app notifications
Booking Timeline	Booking event history
Booking History	Customer & worker booking logs
Review & Rating	Worker reviews & ratings
Notification Center	User notification inbox
Audit Events	Booking activity logs
2. Overall Architecture
flowchart TB

    BOOKING["Booking Service"]

    WORKER["Worker Service"]

    REVIEW["Review Service"]

    EVENTS["Event Dispatcher"]

    NOTIFICATION["Notification Service"]

    TIMELINE["Timeline Service"]

    FCM["Firebase FCM"]

    DB["PostgreSQL"]

    CLIENT["React Frontend"]

    BOOKING --> EVENTS

    WORKER --> EVENTS

    EVENTS --> NOTIFICATION
    EVENTS --> TIMELINE

    NOTIFICATION --> FCM

    NOTIFICATION --> DB
    TIMELINE --> DB
    REVIEW --> DB

    FCM --> CLIENT
3. Notification Workflow
flowchart TD

    START([Start])

    EVENT["Business Event"]

    CREATE["Create Notification"]

    SAVE["Save Notification"]

    PUSH["Send Push Notification"]

    INAPP["Store In-app Notification"]

    USER["User Receives Notification"]

    END([End])

    START --> EVENT

    EVENT --> CREATE

    CREATE --> SAVE

    SAVE --> PUSH

    SAVE --> INAPP

    PUSH --> USER

    INAPP --> USER

    USER --> END
4. Booking Timeline Workflow
flowchart TD

    CREATE["Booking Created"]

    MATCH["Worker Matched"]

    ACCEPT["Worker Accepted"]

    ONWAY["Worker On The Way"]

    WORKING["Working"]

    COMPLETE["Completed"]

    CANCEL["Cancelled"]

    CREATE --> MATCH

    MATCH --> ACCEPT

    ACCEPT --> ONWAY

    ONWAY --> WORKING

    WORKING --> COMPLETE

    CREATE --> CANCEL
    ACCEPT --> CANCEL
5. Review & Rating Workflow
flowchart TD

    COMPLETE["Booking Completed"]

    REVIEW["Customer Reviews Worker"]

    SAVE["Save Review"]

    UPDATE["Update Worker Rating"]

    DISPLAY["Display Public Rating"]

    COMPLETE --> REVIEW

    REVIEW --> SAVE

    SAVE --> UPDATE

    UPDATE --> DISPLAY
6. Swimlane – Notification Flow
flowchart LR

    subgraph SYSTEM["System"]
        S1["Booking Event"]
    end

    subgraph EVENT["Event Dispatcher"]
        E1["Create Notification"]
    end

    subgraph NOTI["Notification Service"]
        N1["Save Notification"]
        N2["Send Push"]
    end

    subgraph DB["PostgreSQL"]
        D1["notifications"]
    end

    subgraph FCM["Firebase"]
        F1["Push Delivery"]
    end

    subgraph USER["User"]
        U1["Receive Notification"]
    end

    S1 --> E1

    E1 --> N1
    E1 --> N2

    N1 --> D1

    N2 --> F1

    F1 --> U1
7. Swimlane – Booking Timeline
flowchart LR

    subgraph WORKER["Worker"]
        W1["Update Booking Status"]
    end

    subgraph API["Booking API"]
        A1["Update Booking"]
        A2["Create Timeline Event"]
    end

    subgraph DB["Database"]
        D1["bookings"]
        D2["booking_events"]
    end

    subgraph CUSTOMER["Customer"]
        C1["View Timeline"]
    end

    W1 --> A1

    A1 --> D1

    A1 --> A2

    A2 --> D2

    D2 --> C1
8. Swimlane – Review Submission
flowchart LR

    subgraph CUSTOMER["Customer"]
        C1["Submit Review"]
    end

    subgraph FRONTEND["Frontend"]
        F1["Validate Review"]
        F2["Call Review API"]
    end

    subgraph API["Review API"]
        A1["Validate Booking"]
        A2["Save Review"]
        A3["Update Rating"]
    end

    subgraph DB["Database"]
        D1["worker_reviews"]
        D2["worker_profiles"]
    end

    C1 --> F1

    F1 --> F2

    F2 --> A1
    A1 --> A2
    A2 --> A3

    A2 --> D1
    A3 --> D2
9. Sequence Diagram – Push Notification
sequenceDiagram

    participant BookingService
    participant NotificationService
    participant Database
    participant Firebase
    participant User

    BookingService->>NotificationService: BOOKING_ACCEPTED event

    NotificationService->>Database: Save notification

    Database-->>NotificationService: Saved

    NotificationService->>Firebase: Send push notification

    Firebase-->>User: Push delivered
10. Sequence Diagram – Booking Timeline
11. Sequence Diagram – Review & Rating
sequenceDiagram

    actor Customer

    participant Frontend
    participant ReviewAPI
    participant ReviewService
    participant Database

    Customer->>Frontend: Submit review

    Frontend->>ReviewAPI: POST /reviews

    ReviewAPI->>ReviewService: Validate completed booking

    ReviewService->>Database: Save review

    ReviewService->>Database: Update worker rating

    Database-->>ReviewService: Updated

    ReviewService-->>ReviewAPI: Success

    ReviewAPI-->>Frontend: Review submitted
12. Notification State Diagram
stateDiagram-v2

    [*] --> CREATED

    CREATED --> SENT

    SENT --> DELIVERED

    DELIVERED --> READ

    SENT --> FAILED
13. Booking Timeline State Diagram
stateDiagram-v2

    [*] --> BOOKING_CREATED

    BOOKING_CREATED --> WORKER_MATCHED

    WORKER_MATCHED --> WORKER_ACCEPTED

    WORKER_ACCEPTED --> ON_THE_WAY

    ON_THE_WAY --> WORKING

    WORKING --> COMPLETED

    BOOKING_CREATED --> CANCELLED

    WORKER_ACCEPTED --> CANCELLED
14. Notification Architecture
flowchart TB

    BOOKING["Booking Module"]

    WORKER["Worker Module"]

    REVIEW["Review Module"]

    EVENTS["Event Dispatcher"]

    NOTI["Notification Service"]

    FCM["Firebase FCM"]

    DB["PostgreSQL"]

    CLIENT["React Client"]

    BOOKING --> EVENTS
    WORKER --> EVENTS
    REVIEW --> EVENTS

    EVENTS --> NOTI

    NOTI --> DB

    NOTI --> FCM

    FCM --> CLIENT
15. Notification API Contract
Get Notifications
GET /api/v1/notifications

Response:

[
  {
    "id": "uuid",
    "title": "Worker accepted your booking",
    "type": "BOOKING_ACCEPTED",
    "isRead": false,
    "createdAt": "2026-05-09T10:00:00"
  }
]
Mark Notification Read
PATCH /api/v1/notifications/{id}/read
16. Booking Timeline API Contract
Get Booking Timeline
GET /api/v1/bookings/{id}/timeline

Response:

[
  {
    "eventType": "BOOKING_CREATED",
    "createdAt": "2026-05-09T10:00:00"
  },
  {
    "eventType": "WORKER_ACCEPTED",
    "createdAt": "2026-05-09T10:05:00"
  }
]
Get Booking History
GET /api/v1/my-bookings
17. Review API Contract
Submit Review
POST /api/v1/reviews

Request:

{
  "bookingId": "uuid",
  "rating": 5,
  "comment": "Worker arrived quickly and fixed the issue."
}
Get Worker Reviews
GET /api/v1/workers/{id}/reviews
18. Database Design
notifications
Column	Type
id	UUID
user_id	UUID
title	varchar
content	text
type	varchar
is_read	boolean
created_at	timestamp
booking_events
Column	Type
id	UUID
booking_id	UUID
event_type	varchar
metadata	jsonb
created_at	timestamp
worker_reviews
Column	Type
id	UUID
booking_id	UUID
customer_id	UUID
worker_id	UUID
rating	int
comment	text
created_at	timestamp
worker_rating_summaries
Column	Type
worker_id	UUID
average_rating	numeric
total_reviews	int
updated_at	timestamp
19. Review Aggregation Logic
UPDATE worker_rating_summaries
SET
    average_rating = (
        SELECT AVG(rating)
        FROM worker_reviews
        WHERE worker_id = :workerId
    ),
    total_reviews = (
        SELECT COUNT(*)
        FROM worker_reviews
        WHERE worker_id = :workerId
    )
WHERE worker_id = :workerId;
20. Notification Security Design
Security	Solution
Notification ownership	User-only access
Review validation	Completed booking only
Spam review prevention	One review per booking
Push token validation	Firebase token check
Timeline ownership	Customer/worker access only
21. UX/UI Flows
Notification Flow
flowchart TD

    EVENT["Booking Event"]

    PUSH["Push Notification"]

    CENTER["Notification Center"]

    DETAIL["Open Booking Detail"]

    EVENT --> PUSH

    PUSH --> CENTER

    CENTER --> DETAIL
Booking Timeline UI
flowchart TD

    DETAIL["Booking Detail"]

    TIMELINE["Booking Timeline"]

    STATUS["Current Status"]

    COMPLETE["Completed"]

    DETAIL --> TIMELINE

    TIMELINE --> STATUS

    STATUS --> COMPLETE
Review Submission UI
flowchart TD

    COMPLETE["Booking Completed"]

    RATING["Choose Rating"]

    COMMENT["Write Comment"]

    SUBMIT["Submit Review"]

    COMPLETE --> RATING

    RATING --> COMMENT

    COMMENT --> SUBMIT
22. Recommended Frontend Stack
Feature	Tech
Push Notification	Firebase
Notification Center	Zustand
Timeline UI	Ant Design Timeline
Review Form	React Hook Form
Rating UI	Ant Design Rate
Polling	React Query
23. Recommended Backend Stack
Spring Boot
Firebase Admin SDK
Spring Scheduler
Redis (optional)
PostgreSQL

OR

ASP.NET Core
Firebase Admin SDK
BackgroundService
PostgreSQL
24. MVP Priorities
Priority	Feature
P2	Push Notification
P2	In-app Notification
P2	Booking Timeline
P2	Booking History
P2	Review Submission
P2	Worker Ratings
P2	Notification Center
P3	Email Notifications
P3	SMS
P3	Smart Notification Rules