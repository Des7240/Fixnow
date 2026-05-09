1. Module Scope
Module	Description
Rate Limiting	Giới hạn request
Anti-Spam Protection	Chống spam API
Brute-force Protection	Chống dò mật khẩu
DDoS Protection	Giảm tải flood request
OTP Protection	Chống spam OTP/SMS
API Security	Bảo vệ endpoint quan trọng
IP Blocking	Chặn IP bất thường
Request Monitoring	Theo dõi request abuse
2. Why This Module Is Critical
Nếu không có protection:
Attack	Impact
Spam OTP	Tốn tiền SMS
Brute force login	Hack account
Booking spam	Fake bookings
API flood	Server crash
Bot abuse	CPU/DB overload
Example thực tế:
Bot spam:
POST /auth/login
10000 lần/phút
Hệ thống sẽ:
CPU spike
DB overload
API timeout
SMS bill tăng mạnh
worker matching delay
3. Recommended Security Stack
Purpose	Technology
Rate limiting	ASP.NET RateLimiter
Distributed cache	Redis
IP blocking	Middleware
Bot protection	reCAPTCHA
Reverse proxy	Nginx
DDoS protection	Cloudflare
JWT security	ASP.NET Auth
MVP Recommendation
Start with:
- ASP.NET Rate Limiter
- Redis
- Nginx
- Cloudflare

before advanced WAF/CDN
4. Security Architecture
flowchart TB

    CLIENT["Client"]

    CLOUDFLARE["Cloudflare"]

    NGINX["Nginx Reverse Proxy"]

    RATELIMIT["Rate Limiter"]

    API["ASP.NET API"]

    REDIS["Redis"]

    DB["PostgreSQL"]

    CLIENT --> CLOUDFLARE

    CLOUDFLARE --> NGINX

    NGINX --> RATELIMIT

    RATELIMIT --> API

    RATELIMIT --> REDIS

    API --> DB
5. Request Protection Workflow
flowchart TD

    REQUEST["Incoming Request"]

    CHECK["Check Rate Limit"]

    ALLOW["Allow Request"]

    BLOCK["Block Request"]

    LOG["Log Abuse"]

    REQUEST --> CHECK

    CHECK --> ALLOW

    CHECK --> BLOCK

    BLOCK --> LOG
6. Rate Limiting Flow
flowchart TD

    REQUEST["API Request"]

    COUNT["Count Requests"]

    LIMIT["Check Threshold"]

    PASS["Continue"]

    REJECT["429 Too Many Requests"]

    REQUEST --> COUNT

    COUNT --> LIMIT

    LIMIT --> PASS

    LIMIT --> REJECT
7. Rate Limiting Strategies
Strategy	Description
Fixed Window	X request / minute
Sliding Window	Smooth limiting
Token Bucket	Burst traffic support
IP-based	Limit theo IP
User-based	Limit theo account
Recommendation
Use Sliding Window + Redis
for production APIs
8. Important APIs To Protect
API	Why
/auth/login	Brute force
/auth/register	Spam account
/auth/otp	SMS spam
/bookings	Fake booking
/payments	Payment abuse
/chat/messages	Spam chat
9. Rate Limit Rules
Endpoint	Limit
Login	5/min
OTP	3/5min
Booking Create	10/min
Chat Message	60/min
Payment	5/min
10. Login Protection Workflow
flowchart TD

    LOGIN["Login Attempt"]

    COUNT["Check Attempts"]

    VALID["Password Correct?"]

    FAIL["Increase Counter"]

    BLOCK["Temporary Block"]

    SUCCESS["Reset Counter"]

    LOGIN --> COUNT

    COUNT --> VALID

    VALID --> FAIL

    FAIL --> BLOCK

    VALID --> SUCCESS
11. Sequence Diagram – Rate Limited Request
sequenceDiagram

    actor User

    participant Cloudflare

    participant API

    participant Redis

    User->>Cloudflare: Request

    Cloudflare->>API: Forward request

    API->>Redis: Check request count

    Redis-->>API: Current count

    API-->>User: 429 Too Many Requests
12. Sequence Diagram – OTP Spam Protection
sequenceDiagram

    actor User

    participant API

    participant Redis

    participant SMSService

    User->>API: Request OTP

    API->>Redis: Check OTP limit

    Redis-->>API: Limit exceeded?

    API-->>User: Reject or Allow

    API->>SMSService: Send OTP
13. Redis Architecture
flowchart LR

    API["ASP.NET API"]

    REDIS["Redis"]

    LIMIT["Rate Limit Data"]

    BLOCK["Blocked IPs"]

    OTP["OTP Counters"]

    API --> REDIS

    REDIS --> LIMIT

    REDIS --> BLOCK

    REDIS --> OTP
14. Redis Keys Example
Login attempts
login:ip:192.168.1.1
OTP limit
otp:user:uuid
Booking spam
booking:user:uuid
15. ASP.NET Rate Limiter Example
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        "login-policy",
        config =>
        {
            config.PermitLimit = 5;
            config.Window = TimeSpan.FromMinutes(1);
        });
});
16. Middleware Security Pipeline
flowchart TD

    REQUEST["Request"]

    CLOUDFLARE["Cloudflare"]

    NGINX["Nginx"]

    RATELIMIT["Rate Limiter"]

    AUTH["Authentication"]

    API["Business API"]

    REQUEST --> CLOUDFLARE

    CLOUDFLARE --> NGINX

    NGINX --> RATELIMIT

    RATELIMIT --> AUTH

    AUTH --> API
17. OTP Security Workflow
flowchart TD

    REQUEST["OTP Request"]

    LIMIT["Check Limit"]

    CAPTCHA["Captcha"]

    SEND["Send OTP"]

    EXPIRE["Expire OTP"]

    REQUEST --> LIMIT

    LIMIT --> CAPTCHA

    CAPTCHA --> SEND

    SEND --> EXPIRE
18. CAPTCHA Integration
Feature	Recommendation
Signup	reCAPTCHA
OTP	reCAPTCHA
Login abuse	reCAPTCHA
Flow
flowchart TD

    USER["User"]

    CAPTCHA["Captcha Validation"]

    API["API"]

    USER --> CAPTCHA

    CAPTCHA --> API
19. DDoS Protection Architecture
flowchart LR

    ATTACKER["Bots"]

    CLOUDFLARE["Cloudflare"]

    NGINX["Nginx"]

    API["ASP.NET"]

    ATTACKER --> CLOUDFLARE

    CLOUDFLARE --> NGINX

    NGINX --> API
20. IP Blocking Workflow
flowchart TD

    REQUEST["Request"]

    SCORE["Calculate Abuse Score"]

    NORMAL["Allow"]

    BLOCK["Block IP"]

    LOG["Security Log"]

    REQUEST --> SCORE

    SCORE --> NORMAL

    SCORE --> BLOCK

    BLOCK --> LOG
21. Security Event Logging
Event
LOGIN_RATE_LIMITED
OTP_BLOCKED
SUSPICIOUS_IP
BRUTE_FORCE_ATTEMPT
PAYMENT_ABUSE
22. Monitoring Dashboard
flowchart TD

    DASHBOARD["Security Dashboard"]

    RATE["Rate Limited Requests"]

    OTP["OTP Abuse"]

    BLOCK["Blocked IPs"]

    ATTACK["Attack Monitoring"]

    DASHBOARD --> RATE

    DASHBOARD --> OTP

    DASHBOARD --> BLOCK

    DASHBOARD --> ATTACK
23. Security Metrics
Metric
Requests/min
Failed logins
OTP requests
Blocked IP count
Rate limit hits
24. Frontend UX/UI Flow
Too Many Requests
flowchart TD

    REQUEST["User Action"]

    LIMIT["429 Error"]

    MESSAGE["Show Cooldown Message"]

    RETRY["Retry Later"]

    REQUEST --> LIMIT

    LIMIT --> MESSAGE

    MESSAGE --> RETRY
Example UI
Too many requests.
Please try again in 60 seconds.
25. Security Headers
Header
X-Frame-Options
X-Content-Type-Options
Strict-Transport-Security
Content-Security-Policy
26. Recommended Nginx Rules
limit_req_zone $binary_remote_addr zone=login:10m rate=5r/m;
27. Production Recommendations
Recommendation
Use Redis distributed limit
Protect OTP APIs
Use Cloudflare
Log all abuse
Add captcha after failed attempts
Block suspicious IPs
28. Security Anti-Patterns
Bad Practice
No rate limiting
OTP without cooldown
Unlimited login attempts
Trust client IP blindly
No abuse monitoring
29. Final Security Architecture
flowchart TB

    CLIENT["Clients"]

    CLOUDFLARE["Cloudflare"]

    NGINX["Nginx"]

    RATELIMITER["Rate Limiter"]

    CAPTCHA["Captcha"]

    API["ASP.NET API"]

    REDIS["Redis"]

    LOGGING["Security Logging"]

    DB["PostgreSQL"]

    CLIENT --> CLOUDFLARE

    CLOUDFLARE --> NGINX

    NGINX --> RATELIMITER

    RATELIMITER --> CAPTCHA

    CAPTCHA --> API

    RATELIMITER --> REDIS

    API --> LOGGING

    API --> DB
30. MVP Priorities
Priority	Feature
🔥 P3	Login rate limit
🔥 P3	OTP protection
🔥 P3	Redis counters
🔥 P3	Cloudflare
🔥 P3	Security logging
HIGH	IP blocking
HIGH	Captcha
FUTURE	WAF
31. Final Outcome

Sau khi hoàn thiện module này, FixNow sẽ có:

Capability	Status
Marketplace	✅
Payments	✅
Chat	✅
Logging	✅
Scheduler	✅
Rate Limiting	✅
OTP Protection	✅
Basic DDoS Protection	✅