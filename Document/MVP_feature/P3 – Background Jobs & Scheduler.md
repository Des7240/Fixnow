1. Module Scope
Module	Description
Background Jobs	Xử lý tác vụ nền
Scheduler	Tác vụ chạy theo lịch
Delayed Jobs	Job chạy trễ
Retry Jobs	Retry khi thất bại
Queue Processing	Hàng đợi xử lý
Maintenance Jobs	Cleanup & maintenance
Monitoring Dashboard	Dashboard theo dõi jobs
2. Background Job MVP Scope
MVP nên hỗ trợ
Feature	MVP
Delayed jobs	✅
Scheduled jobs	✅
Retry failed jobs	✅
Queue processing	✅
Auto cleanup	✅
Monitoring dashboard	✅
Recurring jobs	✅
MVP KHÔNG nên làm ngay
Feature
Distributed queue
Event sourcing
Kafka/RabbitMQ
Workflow engine
Multi-region scheduler
3. Why Background Jobs Are Critical
Vì hệ thống FixNow hiện đã có:
matching
notifications
chat
payment
booking lifecycle

=> rất nhiều task KHÔNG nên chạy synchronous.

Ví dụ
Action	Should Run In Background
Send notification	✅
Retry payment callback	✅
Cleanup expired bookings	✅
Update worker statistics	✅
Send reminder	✅
Audit logging	✅
4. Architecture Overview
flowchart TB

    API["API Layer"]

    JOBS["Background Jobs"]

    SCHEDULER["Scheduler"]

    QUEUE["Job Queue"]

    WORKERS["Job Workers"]

    DB["PostgreSQL"]

    DASHBOARD["Hangfire Dashboard"]

    API --> JOBS

    JOBS --> QUEUE

    SCHEDULER --> QUEUE

    QUEUE --> WORKERS

    WORKERS --> DB

    DASHBOARD --> QUEUE
5. Recommended Technology
ASP.NET
Feature	Tech
Background jobs	Hangfire
Scheduler	Quartz.NET
Queue storage	PostgreSQL
Dashboard	Hangfire Dashboard
Recommendation
Use Hangfire first for MVP because:
- easier integration
- dashboard built-in
- retry built-in
- recurring jobs built-in
6. Job Categories
Category
Notification jobs
Payment jobs
Cleanup jobs
Analytics jobs
Reminder jobs
Retry jobs
7. Job Processing Workflow
flowchart TD

    EVENT["Business Event"]

    CREATE["Create Job"]

    QUEUE["Push To Queue"]

    WORKER["Worker Execute"]

    SUCCESS["Completed"]

    FAILED["Retry"]

    EVENT --> CREATE

    CREATE --> QUEUE

    QUEUE --> WORKER

    WORKER --> SUCCESS

    WORKER --> FAILED
8. State Diagram – Background Job
stateDiagram-v2

    [*] --> QUEUED

    QUEUED --> PROCESSING

    PROCESSING --> SUCCEEDED

    PROCESSING --> FAILED

    FAILED --> RETRYING

    RETRYING --> PROCESSING
9. Common Jobs In FixNow
Job
Auto cancel unpaid booking
Retry failed notification
Cleanup expired tokens
Send booking reminder
Aggregate worker rating
Remove temporary uploads
Expire inactive matching
Retry failed payment
10. Booking Expiration Workflow
flowchart TD

    CREATED["Booking Created"]

    WAIT["Waiting Worker"]

    TIMEOUT["Timeout 5 Minutes"]

    CANCEL["Auto Cancel"]

    NOTIFY["Notify Customer"]

    CREATED --> WAIT

    WAIT --> TIMEOUT

    TIMEOUT --> CANCEL

    CANCEL --> NOTIFY
11. Sequence Diagram – Auto Cancel Booking
sequenceDiagram

    participant BookingAPI

    participant Hangfire

    participant BookingService

    participant NotificationService

    participant Database

    BookingAPI->>Hangfire: Schedule cancellation job

    Hangfire->>BookingService: Execute after timeout

    BookingService->>Database: Cancel booking

    Database-->>BookingService: Updated

    BookingService->>NotificationService: Send notification
12. Sequence Diagram – Retry Failed Notification
sequenceDiagram

    participant NotificationService

    participant Queue

    participant Worker

    participant Database

    NotificationService->>Queue: Push failed job

    Queue->>Worker: Retry notification

    Worker->>Database: Update retry result

    Database-->>Worker: Saved
13. Scheduler Workflow
flowchart TD

    SCHEDULE["Cron Schedule"]

    CREATE["Create Recurring Job"]

    EXECUTE["Execute Job"]

    LOG["Store Logs"]

    SCHEDULE --> CREATE

    CREATE --> EXECUTE

    EXECUTE --> LOG
14. Recurring Jobs
Job	Schedule
Cleanup expired uploads	Daily
Aggregate ratings	Hourly
Delete old logs	Weekly
Generate analytics	Daily
Cleanup inactive sessions	Daily
15. Quartz.NET Cron Example
0 */5 * * * ?
Run every 5 minutes
16. Hangfire Architecture
flowchart LR

    APP["ASP.NET API"]

    HANGFIRE["Hangfire Server"]

    STORAGE["PostgreSQL Storage"]

    DASHBOARD["Dashboard"]

    APP --> HANGFIRE

    HANGFIRE --> STORAGE

    DASHBOARD --> STORAGE
17. Recommended Job Types
Fire-and-forget
BackgroundJob.Enqueue(
    () => notificationService.SendAsync()
);
Delayed
BackgroundJob.Schedule(
    () => bookingService.CancelExpired(id),
    TimeSpan.FromMinutes(5)
);
Recurring
RecurringJob.AddOrUpdate(
    "cleanup-job",
    () => cleanupService.Run(),
    Cron.Daily
);
18. Database Design
background_jobs
Column	Type
id	UUID
job_name	varchar
job_type	varchar
status	varchar
payload	jsonb
retry_count	int
created_at	timestamp
job_execution_logs
Column	Type
id	UUID
job_id	UUID
status	varchar
error_message	text
executed_at	timestamp
19. Job Status Enum
Status
QUEUED
PROCESSING
SUCCEEDED
FAILED
RETRYING
20. Queue Strategy
Queue
notifications
payments
cleanup
analytics
emails
21. Background Job APIs
Get Job List
GET /api/v1/admin/jobs
Retry Job
POST /api/v1/admin/jobs/{id}/retry
Cancel Job
POST /api/v1/admin/jobs/{id}/cancel
22. Admin Monitoring Dashboard
flowchart TD

    DASHBOARD["Jobs Dashboard"]

    QUEUED["Queued Jobs"]

    FAILED["Failed Jobs"]

    RETRY["Retry Jobs"]

    LOGS["Execution Logs"]

    DASHBOARD --> QUEUED

    DASHBOARD --> FAILED

    DASHBOARD --> RETRY

    DASHBOARD --> LOGS
23. Hangfire Dashboard Features
Feature
Queued jobs
Failed jobs
Retry
Recurring jobs
Job history
Execution duration
24. Retry Strategy
Retry Policy Example
Attempt	Delay
1	30s
2	1m
3	5m
Flow
flowchart TD

    FAILED["Job Failed"]

    WAIT["Wait Delay"]

    RETRY["Retry Job"]

    SUCCESS["Success"]

    DEAD["Dead Letter"]

    FAILED --> WAIT

    WAIT --> RETRY

    RETRY --> SUCCESS

    RETRY --> DEAD
25. Security & Reliability Rules
Rule
Idempotent jobs
Retry safe
Log all failures
Avoid long transactions
Timeout protection
26. Performance Recommendations
Recommendation
Separate queues
Dedicated workers
Batch processing
Avoid blocking I/O
Use cancellation token
27. Logging & Audit Events
Event
JOB_CREATED
JOB_FAILED
JOB_RETRIED
JOB_CANCELLED
SCHEDULER_EXECUTED
28. Frontend UX/UI Flow
Admin Job Monitoring
flowchart TD

    DASHBOARD["Admin Dashboard"]

    JOBS["Background Jobs"]

    FAILED["Failed Jobs"]

    DETAIL["Job Detail"]

    RETRY["Retry Job"]

    DASHBOARD --> JOBS

    JOBS --> FAILED

    FAILED --> DETAIL

    DETAIL --> RETRY
29. Production Recommendations
Recommendation
Separate worker instance
Use Redis/PostgreSQL storage
Enable retries
Monitor dead jobs
Add metrics
30. Future Scaling Path
Future
RabbitMQ
Kafka
Distributed workers
Event-driven architecture
CQRS
31. Final Architecture
flowchart TB

    API["ASP.NET API"]

    HANGFIRE["Hangfire"]

    QUARTZ["Quartz.NET"]

    QUEUE["Queues"]

    WORKERS["Workers"]

    DB["PostgreSQL"]

    DASHBOARD["Dashboard"]

    API --> HANGFIRE

    API --> QUARTZ

    HANGFIRE --> QUEUE

    QUARTZ --> QUEUE

    QUEUE --> WORKERS

    WORKERS --> DB

    DASHBOARD --> DB
32. Recommended MVP Priority
Priority	Feature
🔥 P3	Delayed jobs
🔥 P3	Retry jobs
🔥 P3	Recurring jobs
🔥 P3	Auto booking timeout
🔥 P3	Cleanup jobs
HIGH	Analytics jobs
HIGH	Dead letter queue
FUTURE	Distributed queues
33. Final Outcome

Sau khi hoàn thiện module này, FixNow sẽ có:

Capability	Status
Marketplace	✅
Notifications	✅
Payments	✅
Chat	✅
Matching	✅
Background Processing	✅
Retry System	✅
Scheduler	✅