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
3. Swimlane – Worker Profile Creation
4. Swimlane – Worker KYC Submission
5. Swimlane – Admin Review KYC
6. Swimlane – Worker Availability Update
7. Sequence Diagram – Create Worker Profile
8. Sequence Diagram – Submit Worker KYC
9. Sequence Diagram – Admin Review KYC
10. Sequence Diagram – Worker Availability
11. Worker KYC State Diagram
12. Worker Availability State Diagram
13. Worker Management Architecture
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