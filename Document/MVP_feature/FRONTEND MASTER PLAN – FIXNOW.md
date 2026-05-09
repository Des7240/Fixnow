1. Frontend Architecture Goal

Sau khi Backend MVP đã hoàn tất gần như toàn bộ:

Authentication
Booking Engine
Worker Matching
Quotation
Payment
Wallet
Chat
Notification
Dispute & Refund
Admin Operations

Frontend bây giờ phải chuyển sang:

state-driven architecture
+
journey-driven UI
+
role-based application shell
2. Frontend Core Principles
Principle	Description
State-first UI	UI render theo state
Flow-first design	Code theo user journey
Role isolation	Customer / Worker / Admin tách biệt
Shared components	Reuse UI
Realtime-ready	SignalR integrated
Mobile-first	Worker dùng mobile nhiều
Resilient UX	Không đứt gãy khi API delay
3. Complete Frontend Architecture
flowchart TB

    APP["React App"]

    ROUTER["React Router"]

    LAYOUT["Role Layouts"]

    STORE["Zustand Stores"]

    SIGNALR["SignalR Provider"]

    API["Axios API Layer"]

    UI["UI Components"]

    PAGES["Feature Pages"]

    APP --> ROUTER

    ROUTER --> LAYOUT

    LAYOUT --> PAGES

    PAGES --> STORE

    STORE --> API

    STORE --> SIGNALR

    PAGES --> UI
4. Frontend Folder Structure (Production Ready)
src/
│
├── app/
│   ├── router/
│   ├── providers/
│   ├── layouts/
│   └── guards/
│
├── modules/
│   ├── auth/
│   ├── booking/
│   ├── worker/
│   ├── payment/
│   ├── wallet/
│   ├── quotation/
│   ├── dispute/
│   ├── notification/
│   ├── review/
│   ├── admin/
│   └── chat/
│
├── shared/
│   ├── components/
│   ├── hooks/
│   ├── services/
│   ├── utils/
│   ├── constants/
│   └── types/
│
├── stores/
│
├── signalr/
│
├── api/
│
└── assets/
5. Global Application Shell
Customer Layout
flowchart LR

    NAV["Top Navigation"]

    SIDEBAR["Optional Sidebar"]

    CONTENT["Main Content"]

    CHAT["Floating Chat"]

    NOTI["Notification Bell"]

    NAV --> CONTENT

    SIDEBAR --> CONTENT

    CONTENT --> CHAT

    CONTENT --> NOTI
Worker Layout
flowchart LR

    HEADER["Worker Header"]

    STATUS["Online Toggle"]

    JOBS["Job Requests"]

    WALLET["Wallet"]

    CHAT["Chat"]

    HEADER --> STATUS

    STATUS --> JOBS

    JOBS --> CHAT

    JOBS --> WALLET
Admin Layout
flowchart LR

    SIDEBAR["Admin Sidebar"]

    DASHBOARD["Dashboard"]

    TABLES["Management Tables"]

    MODALS["Action Modals"]

    SIDEBAR --> DASHBOARD

    DASHBOARD --> TABLES

    TABLES --> MODALS
6. MASTER SCREEN FLOW (Entire System)
flowchart TD

    LANDING["Landing"]

    LOGIN["Authentication"]

    ROLE["Role Redirect"]

    CUSTOMER["Customer Journey"]

    WORKER["Worker Journey"]

    ADMIN["Admin Journey"]

    LANDING --> LOGIN

    LOGIN --> ROLE

    ROLE --> CUSTOMER

    ROLE --> WORKER

    ROLE --> ADMIN
7. CUSTOMER MASTER FLOW
flowchart TD

    HOME["Home"]

    SEARCH["Search Services"]

    DETAIL["Worker Detail"]

    BOOKING["Create Booking"]

    MATCHING["Matching"]

    QUOTE["Quotation"]

    PAYMENT["Payment"]

    CHAT["Chat"]

    WORKING["Working Status"]

    REVIEW["Review"]

    DISPUTE["Dispute"]

    HOME --> SEARCH

    SEARCH --> DETAIL

    DETAIL --> BOOKING

    BOOKING --> MATCHING

    MATCHING --> QUOTE

    QUOTE --> PAYMENT

    PAYMENT --> CHAT

    CHAT --> WORKING

    WORKING --> REVIEW

    WORKING --> DISPUTE
8. WORKER MASTER FLOW
flowchart TD

    ONBOARD["Worker Onboarding"]

    KYC["KYC Upload"]

    APPROVAL["Waiting Approval"]

    DASHBOARD["Worker Dashboard"]

    REQUEST["Job Requests"]

    QUOTE["Create Quote"]

    CHAT["Chat"]

    WORKING["Working"]

    COMPLETE["Complete Job"]

    WALLET["Wallet"]

    ONBOARD --> KYC

    KYC --> APPROVAL

    APPROVAL --> DASHBOARD

    DASHBOARD --> REQUEST

    REQUEST --> QUOTE

    QUOTE --> CHAT

    CHAT --> WORKING

    WORKING --> COMPLETE

    COMPLETE --> WALLET
9. ADMIN MASTER FLOW
flowchart TD

    DASHBOARD["Admin Dashboard"]

    KYC["KYC Approval"]

    USERS["User Management"]

    DISPUTE["Dispute Resolution"]

    REFUND["Refund"]

    ANALYTICS["Analytics"]

    LOGS["Audit Logs"]

    DASHBOARD --> KYC

    DASHBOARD --> USERS

    DASHBOARD --> DISPUTE

    DISPUTE --> REFUND

    DASHBOARD --> ANALYTICS

    DASHBOARD --> LOGS
10. Realtime Architecture (Critical)
flowchart LR

    SIGNALR["SignalR Connection"]

    EVENTS["Realtime Events"]

    STORE["Global Store"]

    UI["Realtime UI"]

    SIGNALR --> EVENTS

    EVENTS --> STORE

    STORE --> UI
11. Realtime Events Matrix
Event	Consumer
BOOKING_MATCHED	Worker
QUOTE_CREATED	Customer
PAYMENT_SUCCESS	Both
NEW_CHAT_MESSAGE	Both
BOOKING_STATUS_CHANGED	Both
KYC_APPROVED	Worker
DISPUTE_CREATED	Admin
12. Shared Global Stores
Store	Purpose
authStore	User/Auth
bookingStore	Booking lifecycle
chatStore	Chat realtime
notificationStore	Notifications
workerStore	Worker status
walletStore	Wallet data
disputeStore	Dispute state
13. Core Shared Components
Component
AppShell
ProtectedRoute
RoleGuard
BookingCard
UserAvatar
StatusBadge
Timeline
MoneyDisplay
ChatBox
NotificationBell
ConfirmDialog
LoadingOverlay
14. Design System Rules
Rule
One button style
One spacing system
One modal pattern
One form validation pattern
One loading state
One error state
One empty state
15. Shared UI States (VERY IMPORTANT)
Loading State
Skeleton loaders
NOT blank white screen
Error State
Retry button
Clear message
Empty State
Illustration + CTA
16. Authentication Screen Flow
flowchart TD

    LOGIN["Login"]

    REGISTER["Register"]

    OTP["OTP Verification"]

    ROLE["Role Redirect"]

    LOGIN --> ROLE

    REGISTER --> OTP

    OTP --> ROLE
17. Booking Engine Screen Flow
flowchart TD

    CREATE["Create Booking"]

    SEARCH["Finding Worker"]

    MATCH["Worker Matched"]

    QUOTE["Quotation"]

    PAYMENT["Payment"]

    TRACK["Track Booking"]

    COMPLETE["Completed"]

    CREATE --> SEARCH

    SEARCH --> MATCH

    MATCH --> QUOTE

    QUOTE --> PAYMENT

    PAYMENT --> TRACK

    TRACK --> COMPLETE
18. Wallet Screen Flow
flowchart TD

    WALLET["Wallet Dashboard"]

    HISTORY["Transactions"]

    WITHDRAW["Withdraw"]

    BANK["Bank Account"]

    STATUS["Withdrawal Status"]

    WALLET --> HISTORY

    WALLET --> WITHDRAW

    WITHDRAW --> BANK

    BANK --> STATUS
19. Dispute Screen Flow
flowchart TD

    BOOKING["Booking Detail"]

    REPORT["Report Issue"]

    EVIDENCE["Upload Evidence"]

    STATUS["Track Dispute"]

    REFUND["Refund Result"]

    BOOKING --> REPORT

    REPORT --> EVIDENCE

    EVIDENCE --> STATUS

    STATUS --> REFUND
20. Admin Dispute Resolution Flow
flowchart TD

    LIST["Dispute List"]

    DETAIL["Dispute Detail"]

    CHAT["Chat Logs"]

    TIMELINE["Booking Timeline"]

    REFUND["Refund Action"]

    CLOSE["Close Case"]

    LIST --> DETAIL

    DETAIL --> CHAT

    CHAT --> TIMELINE

    TIMELINE --> REFUND

    REFUND --> CLOSE
21. Chat UX Architecture
flowchart LR

    SIDEBAR["Conversation List"]

    CHAT["Chat Messages"]

    INPUT["Message Input"]

    IMAGE["Image Upload"]

    SIDEBAR --> CHAT

    CHAT --> INPUT

    INPUT --> IMAGE
22. Notification UX
flowchart TD

    BELL["Notification Bell"]

    DROPDOWN["Notification Dropdown"]

    DETAIL["Navigate To Detail"]

    BELL --> DROPDOWN

    DROPDOWN --> DETAIL
23. Payment UX
flowchart TD

    QUOTE["Quotation"]

    APPROVE["Approve"]

    REDIRECT["Redirect VNPay"]

    RESULT["Payment Result"]

    STATUS["Booking Updated"]

    QUOTE --> APPROVE

    APPROVE --> REDIRECT

    REDIRECT --> RESULT

    RESULT --> STATUS
24. Critical Frontend Technical Documents Needed
Document	Priority
Route Map	🔥
UI State Map	🔥
API Contract Mapping	🔥
SignalR Event Mapping	🔥
Component Design System	🔥
Permission Matrix	🔥
Responsive Rules	HIGH
Error Handling Rules	HIGH
25. MUST-HAVE FRONTEND DOCUMENTS
A. Route Map Document

Example:

Route	Role
/login	Public
/bookings	Customer
/worker/jobs	Worker
/admin/disputes	Admin
B. API Mapping Document
Screen	APIs
Booking Detail	GET /bookings/:id
Wallet	GET /wallet
Dispute Detail	GET /disputes/:id
C. SignalR Event Mapping
Event	UI Action
NEW_MESSAGE	Append message
BOOKING_UPDATED	Update timeline
PAYMENT_SUCCESS	Refresh booking
26. Responsive Design Rules
Device	Priority
Mobile	Worker
Tablet	Customer
Desktop	Admin
27. UI Consistency Rules
Rule
Same status colors
Same modal style
Same typography
Same loading UX
Same button spacing
28. State Transition Rules
IMPORTANT
UI MUST be driven by booking status.
Example
Booking Status	UI
PENDING	Searching animation
MATCHING	Waiting screen
QUOTED	Quote approval UI
WORKING	Live progress
COMPLETED	Review form
DISPUTED	Locked state
29. Frontend Anti-Patterns
Bad Practice
API calls inside random components
Massive global store
Duplicate modal logic
Inconsistent loading
No optimistic updates
No realtime sync
30. Frontend Production Checklist
Checklist
Error boundaries
Realtime reconnect
Refresh token handling
Role guards
Mobile responsiveness
Skeleton loading
Global toast system
Central modal system
31. Final Frontend Architecture
flowchart TB

    APP["React App"]

    ROUTER["React Router"]

    AUTH["Auth Guard"]

    LAYOUTS["Role Layouts"]

    MODULES["Feature Modules"]

    STORE["Zustand"]

    SIGNALR["SignalR"]

    API["Axios Layer"]

    UI["Shared UI"]

    APP --> ROUTER

    ROUTER --> AUTH

    AUTH --> LAYOUTS

    LAYOUTS --> MODULES

    MODULES --> STORE

    STORE --> SIGNALR

    STORE --> API

    MODULES --> UI
32. FINAL IMPLEMENTATION ORDER
Sprint	Goal
S1	Foundation + Auth
S2	Worker + Catalog
S3	Booking + Payment + Realtime
S4	Chat + Wallet + Dispute
S5	Polish + Responsive + UAT
33. Recommended Immediate Actions
1. Verify foundation

Kiểm tra:

React Router
Zustand
Axios Instance
Tailwind/AntD setup
ProtectedRoute
2. Build design system first

KHÔNG code feature trước khi có:

Button system
Form system
Modal system
Layout system
3. Build global realtime layer

Tạo:

SignalRProvider
+
event dispatcher
+
reconnect strategy
4. Create unified booking state machine

Đây là trái tim frontend.

stateDiagram-v2

    [*] --> PENDING

    PENDING --> MATCHING

    MATCHING --> QUOTED

    QUOTED --> PAYMENT_PENDING

    PAYMENT_PENDING --> WORKING

    WORKING --> COMPLETED

    WORKING --> DISPUTED
34. Final Outcome

Sau khi hoàn thiện frontend theo flow này, FixNow sẽ đạt:

Capability	Status
Seamless UX	✅
Realtime Marketplace UX	✅
Multi-role Architecture	✅
Enterprise-grade Frontend	✅
Scalable React Architecture	✅
Production-ready UI Flows	✅