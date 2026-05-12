1. Module Overview

Đây là nhóm tính năng:

production-hardening features
Bao gồm:
Module
Google Login
Forgot Password
Change Password
Withdrawal OTP Verification
Dynamic Commission Management
Dynamic Withdrawal Limits
Service CRUD Management
Đây là nhóm feature:
Level
Production Required
Security Critical
Financial Critical
Operations Critical
2. Updated System Architecture
flowchart TB

    AUTH["Authentication Service"]

    OTP["OTP Verification Service"]

    WALLET["Wallet Service"]

    COMMISSION["Commission Engine"]

    SERVICE["Service Catalog"]

    ADMIN["Admin Panel"]

    GOOGLE["Google OAuth"]

    DB["PostgreSQL"]

    AUTH --> GOOGLE

    AUTH --> OTP

    WALLET --> OTP

    WALLET --> COMMISSION

    ADMIN --> COMMISSION

    ADMIN --> SERVICE

    AUTH --> DB
3. Google Login Integration
Business Goal

Cho phép user:

đăng nhập nhanh
không cần nhớ password
onboarding nhanh hơn
tăng conversion mobile
Login Flow
sequenceDiagram

    actor User

    participant Frontend

    participant GoogleOAuth

    participant AuthAPI

    participant Database

    User->>Frontend: Login with Google

    Frontend->>GoogleOAuth: Google consent

    GoogleOAuth-->>Frontend: ID Token

    Frontend->>AuthAPI: Send token

    AuthAPI->>GoogleOAuth: Verify token

    AuthAPI->>Database: Create/find user

    AuthAPI-->>Frontend: JWT tokens
User Table Additions
Column	Type
google_id	varchar
auth_provider	varchar
email_verified	bool
Supported Providers
Provider
LOCAL
GOOGLE
APIs
Google Login
POST /api/v1/auth/google-login
Frontend Flow
flowchart TD

    LOGIN["Login Page"]

    GOOGLE["Google Button"]

    CONSENT["Google Consent"]

    SUCCESS["Login Success"]

    LOGIN --> GOOGLE

    GOOGLE --> CONSENT

    CONSENT --> SUCCESS
4. Forgot Password System
Business Goal

Cho phép user:

reset password an toàn
xác minh email/OTP
chống account takeover
Reset Password Flow
flowchart TD

    REQUEST["Request Reset"]

    OTP["Send OTP"]

    VERIFY["Verify OTP"]

    NEWPASS["Set New Password"]

    SUCCESS["Success"]

    REQUEST --> OTP

    OTP --> VERIFY

    VERIFY --> NEWPASS

    NEWPASS --> SUCCESS
APIs
Request Reset
POST /api/v1/auth/forgot-password
Verify OTP
POST /api/v1/auth/verify-reset-otp
Reset Password
POST /api/v1/auth/reset-password
Database Additions
password_reset_tokens
Column	Type
id	UUID
user_id	UUID
otp_code	varchar
expires_at	timestamp
used	bool
5. Change Password (Worker Profile)
Business Goal

Worker có thể:

đổi password
bảo mật account
logout all sessions nếu cần
Change Password Flow
flowchart TD

    PROFILE["Profile"]

    OLD["Old Password"]

    NEW["New Password"]

    CONFIRM["Confirm"]

    SUCCESS["Updated"]

    PROFILE --> OLD

    OLD --> NEW

    NEW --> CONFIRM

    CONFIRM --> SUCCESS
APIs
Change Password
POST /api/v1/profile/change-password
Security Rules
Rule
Require old password
Password strength validation
Revoke refresh tokens
Audit logging
6. Withdrawal OTP Verification
Business Goal

Rút tiền là:

financial critical operation
Bắt buộc:
OTP verification
anti-fraud
withdrawal protection
Withdrawal Flow
flowchart TD

    REQUEST["Request Withdraw"]

    OTP["Send OTP"]

    VERIFY["Verify OTP"]

    PROCESS["Process Withdraw"]

    SUCCESS["Success"]

    REQUEST --> OTP

    OTP --> VERIFY

    VERIFY --> PROCESS

    PROCESS --> SUCCESS
APIs
Request Withdraw
POST /api/v1/wallet/withdraw
Verify Withdraw OTP
POST /api/v1/wallet/verify-withdraw-otp
Database Additions
withdrawal_otps
Column	Type
id	UUID
wallet_transaction_id	UUID
otp_code	varchar
expires_at	timestamp
Security Rules
Rule
OTP expires in 5 min
Max retry count
Rate limit OTP
Lock suspicious attempts
7. Dynamic Commission System
Business Goal

Admin có thể:

đổi % commission
theo loại dịch vụ
không cần deploy code
Example
Service	Commission
Plumbing	10%
Electrical	12%
AC Repair	15%
Commission Architecture
flowchart LR

    ADMIN["Admin"]

    CONFIG["Commission Config"]

    ENGINE["Commission Engine"]

    PAYMENT["Payment"]

    WALLET["Wallet"]

    ADMIN --> CONFIG

    CONFIG --> ENGINE

    ENGINE --> PAYMENT

    PAYMENT --> WALLET
Database Design
service_commissions
Column	Type
id	UUID
service_id	UUID
commission_percent	numeric
active	bool
Commission Formula

PlatformFee=BookingAmount×CommissionPercent

APIs
Update Commission
PUT /api/v1/admin/service-commissions/{id}
8. Dynamic Withdrawal Limits
Business Goal

Admin có thể đổi:

min withdraw
max withdraw
daily limit
monthly limit
Example
Config
Min: 50k
Max: 20 triệu
Daily limit: 50 triệu
Database
system_configs
Column	Type
config_key	varchar
config_value	varchar
Example Keys
Key
MIN_WITHDRAW_AMOUNT
MAX_WITHDRAW_AMOUNT
DAILY_WITHDRAW_LIMIT
APIs
Update Config
PUT /api/v1/admin/system-configs
9. Service CRUD Management
Business Goal

Admin có thể:

thêm dịch vụ
sửa dịch vụ
disable dịch vụ
quản lý icon/banner
Service CRUD Flow
flowchart TD

    LIST["Service List"]

    CREATE["Create Service"]

    UPDATE["Update Service"]

    DELETE["Disable Service"]

    LIST --> CREATE

    LIST --> UPDATE

    LIST --> DELETE
Service Table
Column	Type
id	UUID
name	varchar
slug	varchar
description	text
icon_url	varchar
active	bool
APIs
Create Service
POST /api/v1/admin/services
Update Service
PUT /api/v1/admin/services/{id}
Delete Service
DELETE /api/v1/admin/services/{id}
10. Admin Configuration Dashboard
flowchart LR

    SERVICES["Services"]

    COMMISSION["Commission"]

    WITHDRAW["Withdraw Limits"]

    SECURITY["Security"]

    CONFIG["System Config"]

    SERVICES --> COMMISSION

    COMMISSION --> WITHDRAW

    WITHDRAW --> SECURITY

    SECURITY --> CONFIG
11. Frontend Screen Flows
Google Login
flowchart TD

    LOGIN["Login"]

    GOOGLE["Google Login"]

    SUCCESS["Dashboard"]

    LOGIN --> GOOGLE

    GOOGLE --> SUCCESS
Forgot Password
flowchart TD

    EMAIL["Enter Email"]

    OTP["Enter OTP"]

    NEWPASS["New Password"]

    SUCCESS["Success"]

    EMAIL --> OTP

    OTP --> NEWPASS

    NEWPASS --> SUCCESS
Withdraw Verification
flowchart TD

    WALLET["Wallet"]

    WITHDRAW["Withdraw"]

    OTP["OTP Verification"]

    SUCCESS["Success"]

    WALLET --> WITHDRAW

    WITHDRAW --> OTP

    OTP --> SUCCESS
Admin Config
flowchart TD

    DASHBOARD["Admin Dashboard"]

    SERVICES["Services"]

    COMMISSION["Commission"]

    LIMITS["Withdraw Limits"]

    CONFIG["Configs"]

    DASHBOARD --> SERVICES

    DASHBOARD --> COMMISSION

    DASHBOARD --> LIMITS

    DASHBOARD --> CONFIG
12. Notification Events
Event
PASSWORD_RESET_OTP
WITHDRAW_OTP
PASSWORD_CHANGED
COMMISSION_UPDATED
SERVICE_UPDATED
13. Security Hardening
Security
OTP expiration
OTP retry limit
Rate limiting
Device logging
Audit logging
Refresh token revoke
14. Audit Logging
Must log:
Action
Password change
Withdraw request
Commission change
Service CRUD
Login provider
15. Recommended Frontend Components
Component
GoogleLoginButton
OTPInput
PasswordStrength
WithdrawOTPModal
CommissionEditor
ServiceEditor
16. Final Expanded Architecture
flowchart TB

    AUTH["Auth Service"]

    GOOGLE["Google OAuth"]

    OTP["OTP Service"]

    WALLET["Wallet"]

    COMMISSION["Commission Engine"]

    CONFIG["System Config"]

    SERVICES["Service Catalog"]

    ADMIN["Admin Panel"]

    DB["PostgreSQL"]

    AUTH --> GOOGLE

    AUTH --> OTP

    WALLET --> OTP

    WALLET --> COMMISSION

    ADMIN --> CONFIG

    ADMIN --> SERVICES

    COMMISSION --> DB
17. MVP Priorities
Priority	Feature
🔥 P6	Google Login
🔥 P6	Forgot Password
🔥 P6	Withdraw OTP
🔥 P6	Change Password
🔥 P6	Service CRUD
🔥 P6	Dynamic Commission
HIGH	Dynamic Withdraw Limits
HIGH	Audit Logging
18. Final Outcome

Sau khi hoàn thiện nhóm feature này, FixNow sẽ đạt:

Capability	Status
Enterprise auth	✅
Social login	✅
Financial security	✅
Dynamic operations	✅
Admin business control	✅
Service management	✅