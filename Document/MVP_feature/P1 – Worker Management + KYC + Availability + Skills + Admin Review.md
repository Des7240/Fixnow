1. Worker Management Scope (MVP)
Feature	Description
Worker Profile	Hồ sơ thợ
Worker Skills	Kỹ năng/ngành nghề
Worker Availability	Online/Offline/Busy
Worker GPS	Cập nhật vị trí
Worker KYC	Xác minh giấy tờ
Worker Verification	Duyệt KYC
Worker Rating	Đánh giá
Worker Status	Active/Banned
Admin Review	Quản lý KYC
Worker Search	Tìm kiếm thợ
2. Worker Management Workflow
flowchart TD

    START([Start])

    REGISTER["Register Worker"]

    PROFILE["Create Worker Profile"]

    SKILL["Add Skills"]

    KYC["Submit KYC"]

    REVIEW["Admin Review"]

    APPROVE["Approve Worker"]

    REJECT["Reject Worker"]

    AVAILABLE["Worker Online"]

    MATCHING["Available For Booking"]

    END([End])

    START --> REGISTER

    REGISTER --> PROFILE

    PROFILE --> SKILL

    SKILL --> KYC

    KYC --> REVIEW

    REVIEW --> APPROVE
    REVIEW --> REJECT

    APPROVE --> AVAILABLE

    AVAILABLE --> MATCHING

    MATCHING --> END
3. Swimlane – Worker Profile Creation
flowchart LR

    subgraph WORKER["Worker"]
        W1["Fill Profile"]
    end

    subgraph FRONTEND["Frontend"]
        F1["Validate Form"]
        F2["Call Worker API"]
    end

    subgraph API["Worker API"]
        A1["Validate Request"]
        A2["Create Worker Profile"]
        A3["Save Profile"]
    end

    subgraph DB["PostgreSQL"]
        D1["worker_profiles"]
    end

    W1 --> F1

    F1 --> F2

    F2 --> A1
    A1 --> A2
    A2 --> A3

    A3 --> D1
4. Swimlane – Worker KYC Submission
flowchart LR

    subgraph WORKER["Worker"]
        W1["Upload Documents"]
        W2["Submit KYC"]
    end

    subgraph FRONTEND["Frontend"]
        F1["Upload Files"]
        F2["Call KYC API"]
    end

    subgraph API["KYC API"]
        A1["Validate Documents"]
        A2["Save KYC"]
        A3["Set PENDING"]
    end

    subgraph STORAGE["MinIO"]
        S1["Store Files"]
    end

    subgraph DB["PostgreSQL"]
        D1["worker_kyc"]
    end

    W1 --> F1

    F1 --> S1

    W2 --> F2

    F2 --> A1
    A1 --> A2
    A2 --> A3

    A3 --> D1
5. Swimlane – Admin Review KYC
flowchart LR

    subgraph ADMIN["Admin"]
        AD1["Open KYC"]
        AD2["Approve/Reject"]
    end

    subgraph FRONTEND["Admin Panel"]
        F1["Call Review API"]
    end

    subgraph API["Admin API"]
        A1["Validate Admin"]
        A2["Update KYC Status"]
        A3["Update Worker Status"]
    end

    subgraph DB["PostgreSQL"]
        D1["worker_kyc"]
        D2["worker_profiles"]
    end

    AD1 --> AD2

    AD2 --> F1

    F1 --> A1
    A1 --> A2
    A2 --> A3

    A2 --> D1
    A3 --> D2
6. Swimlane – Worker Availability Update
flowchart LR

    subgraph WORKER["Worker"]
        W1["Toggle Availability"]
    end

    subgraph FRONTEND["Worker App"]
        F1["Call Availability API"]
    end

    subgraph API["Worker API"]
        A1["Validate Worker"]
        A2["Update Availability"]
    end

    subgraph DB["PostgreSQL"]
        D1["worker_profiles"]
    end

    W1 --> F1

    F1 --> A1
    A1 --> A2

    A2 --> D1
7. Sequence Diagram – Create Worker Profile
sequenceDiagram

    actor Worker

    participant Frontend
    participant WorkerAPI
    participant WorkerService
    participant Database

    Worker->>Frontend: Create profile

    Frontend->>WorkerAPI: POST /workers/profile

    WorkerAPI->>WorkerService: Validate request

    WorkerService->>Database: Save profile

    Database-->>WorkerService: Created

    WorkerService-->>WorkerAPI: Success

    WorkerAPI-->>Frontend: Profile created
8. Sequence Diagram – Submit Worker KYC
sequenceDiagram

    actor Worker

    participant Frontend
    participant KYCAPI
    participant KYCService
    participant Storage
    participant Database

    Worker->>Frontend: Upload KYC

    Frontend->>Storage: Upload files

    Storage-->>Frontend: File URLs

    Frontend->>KYCAPI: Submit KYC

    KYCAPI->>KYCService: Validate documents

    KYCService->>Database: Save KYC

    Database-->>KYCService: KYC saved

    KYCService-->>KYCAPI: PENDING

    KYCAPI-->>Frontend: Submission success
9. Sequence Diagram – Admin Review KYC
sequenceDiagram

    actor Admin

    participant Frontend
    participant AdminAPI
    participant KYCService
    participant Database
    participant NotificationService

    Admin->>Frontend: Review KYC

    Frontend->>AdminAPI: PATCH /admin/kyc/{id}

    AdminAPI->>KYCService: Update KYC status

    KYCService->>Database: Save APPROVED

    Database-->>KYCService: Updated

    KYCService->>NotificationService: Notify worker

    NotificationService-->>KYCService: Sent

    KYCService-->>AdminAPI: Success

    AdminAPI-->>Frontend: Review completed
10. Sequence Diagram – Worker Availability
sequenceDiagram

    actor Worker

    participant Frontend
    participant WorkerAPI
    participant WorkerService
    participant Database

    Worker->>Frontend: Toggle online

    Frontend->>WorkerAPI: PATCH /workers/availability

    WorkerAPI->>WorkerService: Update availability

    WorkerService->>Database: Save ONLINE

    Database-->>WorkerService: Updated

    WorkerService-->>WorkerAPI: Success

    WorkerAPI-->>Frontend: Status updated
11. Worker KYC State Diagram
stateDiagram-v2

    [*] --> PENDING

    PENDING --> UNDER_REVIEW

    UNDER_REVIEW --> APPROVED

    UNDER_REVIEW --> REJECTED

    REJECTED --> RESUBMITTED

    RESUBMITTED --> UNDER_REVIEW
12. Worker Availability State Diagram
stateDiagram-v2

    [*] --> OFFLINE

    OFFLINE --> ONLINE

    ONLINE --> BUSY

    BUSY --> ONLINE

    ONLINE --> OFFLINE

    OFFLINE --> BANNED

    ONLINE --> BANNED
13. Worker Management Architecture
flowchart TB

    WORKER["Worker App"]

    ADMIN["Admin Panel"]

    FRONTEND["React Frontend"]

    WORKERAPI["Worker API"]

    KYCAPI["KYC API"]

    ADMINAPI["Admin API"]

    STORAGE["MinIO"]

    DB["PostgreSQL"]

    REDIS["Redis"]

    FCM["Firebase"]

    WORKER --> FRONTEND

    ADMIN --> FRONTEND

    FRONTEND --> WORKERAPI
    FRONTEND --> KYCAPI
    FRONTEND --> ADMINAPI

    WORKERAPI --> DB
    KYCAPI --> DB
    ADMINAPI --> DB

    KYCAPI --> STORAGE

    WORKERAPI --> REDIS

    ADMINAPI --> FCM
14. Worker API Contract
Create Worker Profile
POST /api/v1/workers/profile

Request:

{
  "fullName": "Nguyen Van A",
  "phone": "0901234567",
  "bio": "5 years electrician",
  "experienceYears": 5
}
Update Worker Availability
PATCH /api/v1/workers/availability

Request:

{
  "status": "ONLINE"
}
Update Worker Location
PATCH /api/v1/workers/location

Request:

{
  "lat": 21.0285,
  "lng": 105.8542
}
Add Worker Skills
POST /api/v1/workers/skills

Request:

{
  "skillIds": [
    "uuid1",
    "uuid2"
  ]
}
15. KYC API Contract
Submit KYC
POST /api/v1/workers/kyc

FormData:

Field	Type
citizenIdNumber	text
frontImage	file
backImage	file
selfieImage	file
certificateFile	file
Get KYC Status
GET /api/v1/workers/kyc
16. Admin API Contract
Review KYC
PATCH /api/v1/admin/kyc/{id}

Request:

{
  "status": "APPROVED",
  "reason": ""
}
Suspend Worker
PATCH /api/v1/admin/workers/{id}/suspend
17. Database Design
worker_profiles
Column	Type
id	UUID
user_id	UUID
full_name	varchar
phone	varchar
avatar_url	text
bio	text
experience_years	int
average_rating	numeric
total_jobs	int
availability_status	varchar
current_location	geography(Point)
created_at	timestamp
worker_kyc
Column	Type
id	UUID
worker_id	UUID
citizen_id_number	varchar
citizen_front_url	text
citizen_back_url	text
selfie_url	text
certificate_url	text
status	varchar
rejection_reason	text
verified_by	UUID
verified_at	timestamp
submitted_at	timestamp
skills
Column	Type
id	UUID
name	varchar
category	varchar
worker_skills
Column	Type
worker_id	UUID
skill_id	UUID
worker_location_histories
Column	Type
id	UUID
worker_id	UUID
location	geography(Point)
created_at	timestamp
worker_reviews
Column	Type
id	UUID
booking_id	UUID
customer_id	UUID
worker_id	UUID
rating	int
comment	text
18. Worker Matching Query
SELECT wp.*
FROM worker_profiles wp
JOIN worker_skills ws
    ON ws.worker_id = wp.id
WHERE
    wp.availability_status = 'ONLINE'
AND
    ws.skill_id = :skillId
AND
    ST_DWithin(
        wp.current_location,
        ST_MakePoint(:lng, :lat)::geography,
        5000
    )
ORDER BY
    ST_Distance(
        wp.current_location,
        ST_MakePoint(:lng, :lat)::geography
    )
LIMIT 20;
19. Worker Security Design
Security	Solution
KYC files	Private MinIO bucket
Access control	RBAC
GPS validation	Server-side validation
File upload	MIME validation
Admin audit	Review logs
Worker lock	Prevent double matching
JWT	Secure API
20. Recommended Tech Stack
Frontend
Feature	Tech
Worker State	Zustand
File Upload	React Dropzone
Maps	React Leaflet
GPS	Browser Geolocation
Admin UI	Ant Design
Backend
Spring Boot
Spring Security
Hibernate Spatial
MinIO SDK
Redis
Firebase SDK

OR

ASP.NET Core
EF Core
NetTopologySuite
MinIO SDK
Redis
Firebase Admin SDK
21. MVP Priorities
Priority	Feature
P1	Worker Profile
P1	Worker Skills
P1	Worker Availability
P1	Worker GPS
P1	Worker KYC
P1	Admin Review
P1	Worker Rating
P2	OCR KYC
P2	AI Fraud Detection
P2	Live Tracking
P2	Smart Matching