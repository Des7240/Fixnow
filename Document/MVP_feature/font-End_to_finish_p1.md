FixNow – UX/UI Flow Design (MVP + P1)

Phạm vi:

Authentication
Booking
Worker Matching
Worker Management
Worker KYC
Worker Availability
Admin Review
1. UX/UI Architecture
2. Frontend Route Structure
Customer App
/
├── /login
├── /register
├── /home
├── /services
├── /booking/create
├── /booking/:id
├── /my-bookings
├── /profile
└── /notifications
Worker App
/worker
├── /worker/login
├── /worker/dashboard
├── /worker/profile
├── /worker/skills
├── /worker/kyc
├── /worker/bookings
├── /worker/booking/:id
├── /worker/history
└── /worker/settings
Admin Panel
/admin
├── /admin/login
├── /admin/dashboard
├── /admin/workers
├── /admin/kyc
├── /admin/bookings
├── /admin/users
└── /admin/reports
3. Authentication UX Flow
4. Authentication Screen Flow
5. Customer Main UX Flow
6. Customer Screen Flow
7. Booking Creation UX Flow
8. Worker Main UX Flow
9. Worker Dashboard Layout
10. Worker KYC UX Flow
11. Worker Availability UX
12. Worker Booking Flow
13. Admin UX Flow
14. Admin Panel Layout
15. Frontend Component Architecture
16. React Folder Structure
src/
├── app/
├── routes/
├── layouts/
├── pages/
│
├── modules/
│   ├── auth/
│   ├── booking/
│   ├── worker/
│   ├── kyc/
│   ├── admin/
│   └── notification/
│
├── components/
├── services/
├── hooks/
├── stores/
├── utils/
└── assets/
17. Zustand Store Structure
stores/
├── authStore.ts
├── bookingStore.ts
├── workerStore.ts
├── mapStore.ts
└── notificationStore.ts
18. UI State Management Flow
19. Navigation Structure
Customer Bottom Navigation
Tab	Screen
Home	Home
Bookings	My Bookings
Notifications	Notifications
Profile	Profile
Worker Bottom Navigation
Tab	Screen
Dashboard	Worker Dashboard
Jobs	Current Jobs
History	Booking History
Profile	Profile
20. UI Design Recommendations
Area	Recommendation
Design System	Material UI / Ant Design
Mobile UI	Responsive-first
Forms	React Hook Form
Validation	Zod
State	Zustand
API	Axios
Maps	React Leaflet
File Upload	React Dropzone
Table	TanStack Table
21. MVP Responsive Strategy
Device	Priority
Mobile	Highest
Tablet	Medium
Desktop	High
PWA	Recommended
22. Suggested UI Pages
Customer
Screen
Login
Register
Home
Service List
Create Booking
Booking Detail
Booking History
Profile
Worker
Screen
Dashboard
Profile
Skills
KYC
Incoming Jobs
Current Job
Booking History
Admin
Screen
Dashboard
KYC Review
Worker List
Booking List
User Management
23. Recommended Frontend Stack
Layer	Technology
Framework	React + Vite
Routing	React Router
State	Zustand
Styling	TailwindCSS
UI Library	Ant Design
Forms	React Hook Form
Validation	Zod
HTTP	Axios
Maps	React Leaflet
Realtime	Socket.IO
Notifications	Firebase
24. Recommended Frontend Architecture
25. MVP Frontend Priorities
Priority	Screen
P0	Login/Register
P0	Customer Home
P0	Create Booking
P0	Worker Dashboard
P0	Booking Detail
P1	Worker KYC
P1	Admin KYC Review
P1	Notifications
P2	Live Tracking
P2	Wallet
P2	Analytics