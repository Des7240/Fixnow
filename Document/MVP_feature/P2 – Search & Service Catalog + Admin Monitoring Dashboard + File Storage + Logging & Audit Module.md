1. Module Scope
Module	Description
Search & Service Catalog	Quản lý dịch vụ + tìm kiếm
Admin Monitoring Dashboard	Dashboard giám sát hệ thống
File Storage System	Upload & quản lý file
Logging & Audit	Audit trail & system logs
2. Overall Architecture
flowchart TB

    CLIENT["React Frontend"]

    SEARCH["Search Service"]

    CATALOG["Service Catalog"]

    ADMIN["Admin Dashboard"]

    STORAGE["File Storage"]

    AUDIT["Audit Service"]

    DB["PostgreSQL"]

    MINIO["MinIO"]

    CLIENT --> SEARCH
    CLIENT --> CATALOG
    CLIENT --> ADMIN

    CLIENT --> STORAGE

    SEARCH --> DB
    CATALOG --> DB
    ADMIN --> DB

    STORAGE --> MINIO
    STORAGE --> DB

    SEARCH --> AUDIT
    CATALOG --> AUDIT
    STORAGE --> AUDIT
    ADMIN --> AUDIT

    AUDIT --> DB
=========================================================
SEARCH & SERVICE CATALOG
=========================================================
3. Search & Service Catalog Workflow
flowchart TD

    HOME["Home"]

    CATEGORY["Select Category"]

    SEARCH["Search Service"]

    DETAIL["Service Detail"]

    BOOKING["Create Booking"]

    HOME --> CATEGORY

    CATEGORY --> SEARCH

    SEARCH --> DETAIL

    DETAIL --> BOOKING
4. Service Catalog Architecture
flowchart LR

    CATEGORY["Service Category"]

    SERVICE["Service"]

    SKILL["Required Skills"]

    PRICING["Pricing"]

    BOOKING["Booking"]

    CATEGORY --> SERVICE

    SERVICE --> SKILL

    SERVICE --> PRICING

    SERVICE --> BOOKING
5. Swimlane – Search Service
flowchart LR

    subgraph CUSTOMER["Customer"]
        C1["Enter Search Keyword"]
    end

    subgraph FRONTEND["Frontend"]
        F1["Call Search API"]
    end

    subgraph API["Service API"]
        A1["Search Services"]
    end

    subgraph DB["Database"]
        D1["services"]
    end

    C1 --> F1

    F1 --> A1

    A1 --> D1
6. Sequence Diagram – Search Service
sequenceDiagram

    actor Customer

    participant Frontend
    participant ServiceAPI
    participant Database

    Customer->>Frontend: Search service

    Frontend->>ServiceAPI: GET /services/search

    ServiceAPI->>Database: Query services

    Database-->>ServiceAPI: Service list

    ServiceAPI-->>Frontend: Return result
7. Service Catalog State Diagram
stateDiagram-v2

    [*] --> ACTIVE

    ACTIVE --> INACTIVE

    INACTIVE --> ACTIVE

    ACTIVE --> DELETED
8. Search & Service Catalog APIs
Get Categories
GET /api/v1/service-categories
Get Services
GET /api/v1/services
Search Services
GET /api/v1/services/search?q=electrical
Get Service Detail
GET /api/v1/services/{id}
9. Database Design – Service Catalog
service_categories
Column	Type
id	UUID
name	varchar
icon	text
description	text
is_active	boolean
services
Column	Type
id	UUID
category_id	UUID
name	varchar
description	text
base_price	numeric
estimated_duration	int
is_active	boolean
service_skill_requirements
Column	Type
service_id	UUID
skill_id	UUID
=========================================================
ADMIN MONITORING DASHBOARD
=========================================================
10. Admin Dashboard Workflow
flowchart TD

    DASHBOARD["Dashboard"]

    BOOKINGS["Bookings"]

    WORKERS["Workers"]

    KYC["KYC"]

    REPORTS["Reports"]

    ALERTS["Alerts"]

    DASHBOARD --> BOOKINGS

    DASHBOARD --> WORKERS

    DASHBOARD --> KYC

    DASHBOARD --> REPORTS

    DASHBOARD --> ALERTS
11. Admin Dashboard Architecture
flowchart TB

    ADMIN["Admin"]

    FRONTEND["Dashboard UI"]

    ANALYTICS["Analytics Service"]

    BOOKING["Booking Metrics"]

    WORKER["Worker Metrics"]

    REVIEW["Review Metrics"]

    DB["PostgreSQL"]

    ADMIN --> FRONTEND

    FRONTEND --> ANALYTICS

    ANALYTICS --> BOOKING
    ANALYTICS --> WORKER
    ANALYTICS --> REVIEW

    BOOKING --> DB
    WORKER --> DB
    REVIEW --> DB
12. Swimlane – Admin Dashboard
flowchart LR

    subgraph ADMIN["Admin"]
        A1["Open Dashboard"]
    end

    subgraph FRONTEND["Frontend"]
        F1["Load Dashboard"]
    end

    subgraph API["Admin API"]
        API1["Aggregate Metrics"]
    end

    subgraph DB["Database"]
        D1["bookings"]
        D2["workers"]
        D3["reviews"]
    end

    A1 --> F1

    F1 --> API1

    API1 --> D1
    API1 --> D2
    API1 --> D3
13. Sequence Diagram – Dashboard Metrics
sequenceDiagram

    actor Admin

    participant Frontend
    participant AdminAPI
    participant AnalyticsService
    participant Database

    Admin->>Frontend: Open dashboard

    Frontend->>AdminAPI: GET /dashboard

    AdminAPI->>AnalyticsService: Load metrics

    AnalyticsService->>Database: Aggregate metrics

    Database-->>AnalyticsService: Metrics

    AnalyticsService-->>AdminAPI: Dashboard data

    AdminAPI-->>Frontend: Render dashboard
14. Dashboard APIs
Dashboard Summary
GET /api/v1/admin/dashboard
Booking Metrics
GET /api/v1/admin/metrics/bookings
Worker Metrics
GET /api/v1/admin/metrics/workers
15. Dashboard KPIs
KPI
Total bookings
Completed bookings
Cancel rate
Pending KYC
Online workers
Average rating
=========================================================
FILE STORAGE SYSTEM
=========================================================
16. File Storage Workflow
flowchart TD

    SELECT["Select File"]

    VALIDATE["Validate File"]

    UPLOAD["Upload File"]

    STORE["Store Metadata"]

    RETURN["Return File URL"]

    SELECT --> VALIDATE

    VALIDATE --> UPLOAD

    UPLOAD --> STORE

    STORE --> RETURN
17. File Storage Architecture
flowchart LR

    CLIENT["Frontend"]

    API["Upload API"]

    STORAGE["MinIO"]

    DB["PostgreSQL"]

    CLIENT --> API

    API --> STORAGE

    API --> DB
18. Swimlane – File Upload
flowchart LR

    subgraph USER["User"]
        U1["Select File"]
    end

    subgraph FRONTEND["Frontend"]
        F1["Upload File"]
    end

    subgraph API["Upload API"]
        A1["Validate File"]
        A2["Store Object"]
        A3["Save Metadata"]
    end

    subgraph STORAGE["MinIO"]
        S1["Object Storage"]
    end

    subgraph DB["Database"]
        D1["uploaded_files"]
    end

    U1 --> F1

    F1 --> A1

    A1 --> A2

    A2 --> S1

    A2 --> A3

    A3 --> D1
19. Sequence Diagram – File Upload
sequenceDiagram

    actor User

    participant Frontend
    participant UploadAPI
    participant MinIO
    participant Database

    User->>Frontend: Upload file

    Frontend->>UploadAPI: POST /upload

    UploadAPI->>MinIO: Store object

    MinIO-->>UploadAPI: Object key

    UploadAPI->>Database: Save metadata

    Database-->>UploadAPI: Saved

    UploadAPI-->>Frontend: File URL
20. File Storage APIs
Upload File
POST /api/v1/files/upload
Delete File
DELETE /api/v1/files/{id}
Get File Metadata
GET /api/v1/files/{id}
21. Database Design – File Storage
uploaded_files
Column	Type
id	UUID
file_name	varchar
content_type	varchar
file_size	bigint
bucket	varchar
object_key	varchar
uploaded_by	UUID
created_at	timestamp
22. File Security Design
Security	Solution
Private KYC files	private bucket
Public avatar	public bucket
MIME validation	backend validation
File size limit	upload constraints
Signed URLs	secure download
=========================================================
LOGGING & AUDIT SYSTEM
=========================================================
23. Logging & Audit Workflow
flowchart TD

    ACTION["Business Action"]

    LOG["Create Audit Log"]

    STORE["Save Audit Log"]

    REVIEW["Admin Review"]

    ACTION --> LOG

    LOG --> STORE

    STORE --> REVIEW
24. Logging Architecture
flowchart LR

    SERVICES["Services"]

    AUDIT["Audit Service"]

    LOGS["Application Logs"]

    DB["Audit Logs"]

    ADMIN["Admin"]

    SERVICES --> AUDIT

    SERVICES --> LOGS

    AUDIT --> DB

    ADMIN --> DB
25. Swimlane – Audit Logging
flowchart LR

    subgraph SERVICE["Business Service"]
        S1["Execute Action"]
    end

    subgraph AUDIT["Audit Service"]
        A1["Create Audit Event"]
    end

    subgraph DB["Database"]
        D1["audit_logs"]
    end

    S1 --> A1

    A1 --> D1
26. Sequence Diagram – Audit Event
sequenceDiagram

    participant Service

    participant AuditService

    participant Database

    Service->>AuditService: Log event

    AuditService->>Database: Save audit log

    Database-->>AuditService: Saved
27. Audit APIs
Get Audit Logs
GET /api/v1/admin/audit-logs
Get Audit Detail
GET /api/v1/admin/audit-logs/{id}
28. Database Design – Audit Logs
audit_logs
Column	Type
id	UUID
actor_id	UUID
actor_role	varchar
action	varchar
entity_type	varchar
entity_id	UUID
old_data	jsonb
new_data	jsonb
ip_address	varchar
created_at	timestamp
29. Audit Event Types
Event
LOGIN_SUCCESS
LOGIN_FAILED
BOOKING_CANCELLED
KYC_APPROVED
WORKER_SUSPENDED
ROLE_CHANGED
30. Logging Levels
Level
INFO
WARN
ERROR
SECURITY
31. UX/UI Flow – Audit Logs
flowchart TD

    DASHBOARD["Admin Dashboard"]

    AUDIT["Audit Logs"]

    DETAIL["Audit Detail"]

    FILTER["Filter Logs"]

    DASHBOARD --> AUDIT

    AUDIT --> DETAIL

    AUDIT --> FILTER
32. Recommended Frontend Stack
Feature	Tech
Search UI	Ant Design
Dashboard Charts	Recharts
File Upload	React Dropzone
Audit Table	TanStack Table
Filters	React Hook Form
33. Recommended Backend Stack
Spring Boot
Spring Data JPA
Hibernate Search
MinIO SDK
Logback

OR

ASP.NET Core
EF Core
MinIO SDK
Serilog
PostgreSQL
34. MVP Priorities
Priority	Feature
P2	Service Categories
P2	Service Search
P2	Dashboard Summary
P2	File Upload
P2	File Metadata
P2	Audit Logs
P2	Admin Metrics
P3	Elasticsearch
P3	File CDN
P3	SIEM Integration
35. Final Architecture After Completion
flowchart TB

    AUTH["Authentication"]

    BOOKING["Booking"]

    WORKER["Worker"]

    NOTI["Notification"]

    REVIEW["Review"]

    SEARCH["Search"]

    STORAGE["Storage"]

    AUDIT["Audit"]

    ADMIN["Admin"]

    DB["PostgreSQL"]

    AUTH --> DB
    BOOKING --> DB
    WORKER --> DB
    NOTI --> DB
    REVIEW --> DB
    SEARCH --> DB
    STORAGE --> DB
    AUDIT --> DB
    ADMIN --> DB