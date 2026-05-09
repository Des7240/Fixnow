1. Module Scope
Module	Description
Centralized Logging	Gom toàn bộ log hệ thống
Error Tracking	Theo dõi exception
Structured Logging	Log dạng JSON structure
Request Tracing	Trace request end-to-end
API Monitoring	Theo dõi API lỗi
Correlation ID	Trace xuyên services
Dashboard Monitoring	Dashboard realtime logs
Alerting	Cảnh báo lỗi nghiêm trọng
2. Why Logging & Error Tracking Is Critical
Khi production sẽ xảy ra:
Problem	Example
Payment failed	Callback lỗi
Booking crash	Null reference
API timeout	DB slow
Chat disconnect	SignalR lỗi
Random production bug	Chỉ xảy ra trên khách
Không có logging:
Dev sẽ:
- không biết lỗi ở API nào
- không biết user nào bị lỗi
- không biết request nào gây crash
- không reproduce được bug
Logging system giải quyết:
- trace toàn bộ request
- biết chính xác exception
- biết dòng code gây lỗi
- realtime monitoring
- debug production nhanh
3. Recommended Stack
Purpose	Technology
Logging	Serilog
Log Viewer	Seq
Cloud Monitoring	Application Insights
Correlation	Middleware
Metrics	OpenTelemetry
Exception Tracking	Serilog Exception
Recommendation MVP
Use:
- Serilog
- Seq
- Request Logging Middleware

before moving to:
- Application Insights
- ELK Stack
- Grafana
4. Logging Architecture
flowchart TB

    CLIENT["React Frontend"]

    API["ASP.NET API"]

    MIDDLEWARE["Logging Middleware"]

    SERILOG["Serilog"]

    SEQ["Seq Dashboard"]

    APPINSIGHT["Application Insights"]

    DB["PostgreSQL"]

    CLIENT --> API

    API --> MIDDLEWARE

    MIDDLEWARE --> SERILOG

    SERILOG --> SEQ

    SERILOG --> APPINSIGHT

    API --> DB
5. Logging Workflow
flowchart TD

    REQUEST["Incoming Request"]

    TRACE["Generate Correlation ID"]

    EXECUTE["Execute API"]

    SUCCESS["Success Log"]

    ERROR["Exception Log"]

    STORE["Store Logs"]

    REQUEST --> TRACE

    TRACE --> EXECUTE

    EXECUTE --> SUCCESS

    EXECUTE --> ERROR

    SUCCESS --> STORE

    ERROR --> STORE
6. Error Tracking Workflow
flowchart TD

    EXCEPTION["Exception Occurs"]

    CAPTURE["Capture Stack Trace"]

    CONTEXT["Collect Context"]

    LOG["Write Structured Log"]

    ALERT["Trigger Alert"]

    DASHBOARD["Display Dashboard"]

    EXCEPTION --> CAPTURE

    CAPTURE --> CONTEXT

    CONTEXT --> LOG

    LOG --> ALERT

    LOG --> DASHBOARD
7. Logging Pipeline
flowchart LR

    API["API"]

    MIDDLEWARE["Middleware"]

    LOGGER["Logger"]

    STORAGE["Log Storage"]

    DASHBOARD["Dashboard"]

    API --> MIDDLEWARE

    MIDDLEWARE --> LOGGER

    LOGGER --> STORAGE

    STORAGE --> DASHBOARD
8. Request Lifecycle Logging
sequenceDiagram

    actor User

    participant Frontend
    participant API
    participant Middleware
    participant Serilog
    participant Seq

    User->>Frontend: Action

    Frontend->>API: HTTP Request

    API->>Middleware: Process request

    Middleware->>Serilog: Write request log

    Serilog->>Seq: Store log

    API-->>Frontend: Response
9. Exception Tracking Sequence
sequenceDiagram

    participant API

    participant Service

    participant Middleware

    participant Serilog

    participant Seq

    API->>Service: Execute business logic

    Service-->>API: Throw exception

    API->>Middleware: Bubble exception

    Middleware->>Serilog: Log exception

    Serilog->>Seq: Store exception
10. Structured Logging Design
BAD
"Payment failed"
GOOD
{
  "event": "PAYMENT_FAILED",
  "bookingId": "uuid",
  "userId": "uuid",
  "paymentProvider": "VNPay",
  "amount": 500000,
  "error": "Invalid signature"
}
11. Logging Categories
Category
Request logs
Error logs
Payment logs
Booking logs
Security logs
Audit logs
Background job logs
12. Important Logs In FixNow
Event
USER_LOGIN
BOOKING_CREATED
MATCHING_STARTED
PAYMENT_SUCCESS
PAYMENT_FAILED
QUOTE_APPROVED
CHAT_MESSAGE_SENT
KYC_APPROVED
13. Log Levels
Level	Usage
Information	Business events
Warning	Recoverable issue
Error	Exception
Fatal	System crash
Debug	Development
14. Correlation ID Architecture
flowchart TD

    REQUEST["Request"]

    ID["Generate Correlation ID"]

    API["API Layer"]

    SERVICE["Services"]

    LOG["Attach To Logs"]

    REQUEST --> ID

    ID --> API

    API --> SERVICE

    SERVICE --> LOG
Example
X-Correlation-ID: 9f7a1d2c
15. ASP.NET Middleware Example
app.UseSerilogRequestLogging();
16. Serilog Setup Example
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.FromLogContext()
    .CreateLogger();
17. Seq Architecture
flowchart LR

    API["ASP.NET APIs"]

    SERILOG["Serilog"]

    SEQ["Seq"]

    DASHBOARD["Realtime Dashboard"]

    API --> SERILOG

    SERILOG --> SEQ

    SEQ --> DASHBOARD
18. Seq Dashboard Features
Feature
Search logs
Filter by level
View exceptions
Trace correlation
Live monitoring
19. Application Insights Architecture
flowchart TB

    API["ASP.NET API"]

    TELEMETRY["Telemetry SDK"]

    APPINSIGHT["Azure App Insights"]

    DASHBOARD["Azure Dashboard"]

    ALERT["Alert Rules"]

    API --> TELEMETRY

    TELEMETRY --> APPINSIGHT

    APPINSIGHT --> DASHBOARD

    APPINSIGHT --> ALERT
20. API Monitoring Workflow
flowchart TD

    REQUEST["Request"]

    LATENCY["Measure Duration"]

    ERROR["Capture Status Code"]

    METRIC["Store Metrics"]

    DASHBOARD["Monitoring Dashboard"]

    REQUEST --> LATENCY

    LATENCY --> ERROR

    ERROR --> METRIC

    METRIC --> DASHBOARD
21. Performance Metrics
Metric
API response time
Error rate
Request count
DB query duration
Memory usage
CPU usage
22. Database Design (Optional)
application_logs
Column	Type
id	UUID
level	varchar
message	text
exception	text
correlation_id	varchar
created_at	timestamp
api_request_logs
Column	Type
id	UUID
method	varchar
path	varchar
status_code	int
duration_ms	int
correlation_id	varchar
23. Logging In Booking Flow
flowchart TD

    CREATE["Booking Created"]

    MATCH["Matching"]

    ASSIGN["Worker Assigned"]

    COMPLETE["Booking Completed"]

    PAYMENT["Payment"]

    CREATE --> MATCH

    MATCH --> ASSIGN

    ASSIGN --> COMPLETE

    COMPLETE --> PAYMENT
Each step should log:
Event
BOOKING_CREATED
MATCHING_STARTED
WORKER_ASSIGNED
BOOKING_COMPLETED
PAYMENT_SUCCESS
24. Logging In Payment Flow
flowchart TD

    CREATE["Payment Created"]

    REDIRECT["Redirect Gateway"]

    CALLBACK["Gateway Callback"]

    VERIFY["Verify Signature"]

    SUCCESS["Payment Success"]

    FAILED["Payment Failed"]

    CREATE --> REDIRECT

    REDIRECT --> CALLBACK

    CALLBACK --> VERIFY

    VERIFY --> SUCCESS

    VERIFY --> FAILED
25. Background Job Logging
Event
JOB_STARTED
JOB_FAILED
JOB_RETRY
JOB_COMPLETED
26. Frontend Error Tracking
Recommendation
Tool
Sentry
React Error Boundary
Frontend Workflow
flowchart TD

    UI["React UI"]

    ERROR["Frontend Crash"]

    BOUNDARY["Error Boundary"]

    LOG["Send Error"]

    DASHBOARD["Monitoring"]

    UI --> ERROR

    ERROR --> BOUNDARY

    BOUNDARY --> LOG

    LOG --> DASHBOARD
27. Security Logging
Event
Failed login
Unauthorized API
Suspicious payment
Token abuse
Rate limit hit
28. Alerting Rules
Alert
API error spike
Payment failures
DB unavailable
High response time
Worker crash
29. Recommended Log Structure
{
  "timestamp": "2026-05-09T10:00:00",
  "level": "Error",
  "event": "PAYMENT_FAILED",
  "correlationId": "abc123",
  "userId": "uuid",
  "bookingId": "uuid",
  "message": "VNPay callback invalid",
  "exception": "SignatureException"
}
30. Production Recommendations
Recommendation
Never log passwords
Mask tokens
Use correlation IDs
Separate Error/Fatal alerts
Retention policy
Centralized dashboards
31. Logging Anti-Patterns
Bad Practice
Log everything
Log sensitive data
String-only logs
No correlation IDs
Swallow exceptions
32. Final Monitoring Architecture
flowchart TB

    FRONTEND["React"]

    API["ASP.NET APIs"]

    MIDDLEWARE["Logging Middleware"]

    SERILOG["Serilog"]

    SEQ["Seq"]

    APPINSIGHT["Application Insights"]

    ALERT["Alerts"]

    DB["PostgreSQL"]

    FRONTEND --> API

    API --> MIDDLEWARE

    MIDDLEWARE --> SERILOG

    SERILOG --> SEQ

    SERILOG --> APPINSIGHT

    APPINSIGHT --> ALERT

    API --> DB
33. MVP Priorities
Priority	Feature
🔥 P3	Serilog
🔥 P3	Seq
🔥 P3	Request logging
🔥 P3	Exception tracking
🔥 P3	Correlation ID
HIGH	API metrics
HIGH	Frontend tracking
FUTURE	OpenTelemetry
34. Final Outcome

Sau khi hoàn thiện module này, FixNow sẽ có:

Capability	Status
Marketplace	✅
Matching	✅
Payments	✅
Chat	✅
Scheduler	✅
Centralized Logging	✅
Error Tracking	✅
Monitoring	✅