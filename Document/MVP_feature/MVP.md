| Priority | Module          |
| -------- | --------------- |
| P0       | Authentication  |
| P0       | Worker Matching |
| P0       | Booking         |
| P0       | Nearby Search   |
| P0       | Notification    |
| P1       | Route Direction |
| P1       | Admin           |
| P2       | Wallet          |
| P2       | AI Assistant    |



# FixNow - System Context Diagram

```mermaid
flowchart TB

    %% =========================
    %% EXTERNAL USERS
    %% =========================

    CUSTOMER["Customer
    (Khách hàng)"]

    WORKER["Worker
    (Thợ sửa chữa)"]

    ADMIN["Administrator
    (Quản trị viên)"]

    %% =========================
    %% MAIN SYSTEM
    %% =========================

    SYSTEM["FixNow Platform
    Service Marketplace System"]

    %% =========================
    %% EXTERNAL SERVICES
    %% =========================

    MAP["OpenStreetMap
    Map Service"]

    ROUTE["OpenRouteService
    Routing Service"]

    GEO["Nominatim
    Geocoding Service"]

    FCM["Firebase Cloud Messaging
    Push Notification"]

    STORAGE["MinIO Object Storage
    File Storage"]

    PAYMENT["Payment Gateway
    (Optional Future)"]

    %% =========================
    %% RELATIONSHIPS
    %% =========================

    CUSTOMER -->|"Search services
    Create booking
    Review worker"| SYSTEM

    WORKER -->|"Receive jobs
    Submit quotation
    Update status"| SYSTEM

    ADMIN -->|"Manage users
    Approve KYC
    Monitor system"| SYSTEM

    SYSTEM -->|"Map tiles"| MAP

    SYSTEM -->|"Directions & ETA"| ROUTE

    SYSTEM -->|"Address geocoding"| GEO

    SYSTEM -->|"Push notifications"| FCM

    SYSTEM -->|"Store images
    Documents"| STORAGE

    SYSTEM -->|"Online payment
    Wallet transaction"| PAYMENT
```
Context Diagram – MVP Version
```mermaid
flowchart LR

    CUSTOMER["Customer"]
    WORKER["Worker"]
    ADMIN["Admin"]

    FIXNOW["FixNow System"]

    MAP["OSM + OpenRouteService"]
    FCM["Firebase FCM"]

    CUSTOMER --> FIXNOW
    WORKER --> FIXNOW
    ADMIN --> FIXNOW

    FIXNOW --> MAP
    FIXNOW --> FCM
```
C4 Model – Level 1 Context Diagram
```mermaid
C4Context
    title FixNow System Context Diagram

    Person(customer, "Customer", "Books repair/home services")
    Person(worker, "Worker", "Accepts and performs jobs")
    Person(admin, "Administrator", "Manages platform")

    System(fixnow, "FixNow Platform", "Repair service marketplace platform")

    System_Ext(osm, "OpenStreetMap", "Map provider")
    System_Ext(ors, "OpenRouteService", "Routing and directions")
    System_Ext(fcm, "Firebase FCM", "Push notifications")
    System_Ext(storage, "MinIO", "Object storage")

    Rel(customer, fixnow, "Creates booking, reviews workers")
    Rel(worker, fixnow, "Accepts jobs, updates work status")
    Rel(admin, fixnow, "Manages system")

    Rel(fixnow, osm, "Loads maps")
    Rel(fixnow, ors, "Gets routes")
    Rel(fixnow, fcm, "Sends notifications")
    Rel(fixnow, storage, "Stores images/files")
```