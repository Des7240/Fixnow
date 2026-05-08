P0 – Authentication Module Design (FixNow)
1. Authentication Scope (MVP)
Chức năng thuộc P0
Feature	Description
Register	Đăng ký
Login	Đăng nhập
JWT Authentication	Access Token
Refresh Token	Gia hạn phiên
Logout	Đăng xuất
Role-based Access	Customer / Worker / Admin
Password Hashing	BCrypt
Email/Phone Verification	Optional MVP
Worker KYC Submit	MVP worker onboarding
2. Authentication Workflow
```mermaid
flowchart TD

    START([Start])

    REGISTER["Register Account"]
    VERIFY["Verify User Info"]
    HASH["Hash Password"]
    SAVE["Save User"]
    TOKEN["Generate JWT"]
    LOGIN["Login"]
    VALIDATE["Validate Credentials"]
    REFRESH["Refresh Token"]
    LOGOUT["Logout"]

    END([End])

    START --> REGISTER
    REGISTER --> VERIFY
    VERIFY --> HASH
    HASH --> SAVE
    SAVE --> TOKEN
    TOKEN --> END

    START --> LOGIN
    LOGIN --> VALIDATE
    VALIDATE --> TOKEN
    TOKEN --> END

    START --> REFRESH
    REFRESH --> TOKEN
    TOKEN --> END

    START --> LOGOUT
    LOGOUT --> END
```
3. Swimlane – User Registration
```mermaid
flowchart LR

    subgraph USER["User"]
        U1["Fill Register Form"]
    end

    subgraph FRONTEND["Frontend React"]
        F1["Validate Form"]
        F2["Call Register API"]
        F3["Store JWT"]
    end

    subgraph API["Auth API"]
        A1["Validate Request"]
        A2["Check Existing User"]
        A3["Hash Password"]
        A4["Create User"]
        A5["Generate JWT"]
    end

    subgraph DB["PostgreSQL"]
        D1["Users Table"]
    end

    U1 --> F1
    F1 --> F2

    F2 --> A1
    A1 --> A2
    A2 --> A3
    A3 --> A4

    A4 --> D1

    A4 --> A5
    A5 --> F3
```
4. Swimlane – Login Flow
```mermaid
flowchart LR

    subgraph USER["User"]
        U1["Enter Email & Password"]
    end

    subgraph FRONTEND["Frontend"]
        F1["Submit Login"]
        F2["Store Access Token"]
        F3["Redirect Dashboard"]
    end

    subgraph API["Auth API"]
        A1["Validate Request"]
        A2["Find User"]
        A3["Compare Password"]
        A4["Generate JWT"]
        A5["Generate Refresh Token"]
    end

    subgraph DB["PostgreSQL"]
        D1["Users"]
        D2["Refresh Tokens"]
    end

    U1 --> F1

    F1 --> A1
    A1 --> A2
    A2 --> D1

    A2 --> A3

    A3 --> A4
    A4 --> A5

    A5 --> D2

    A5 --> F2
    F2 --> F3
```
5. Sequence Diagram – Register
```mermaid
sequenceDiagram

    actor User
    participant Frontend
    participant AuthAPI
    participant UserService
    participant Database

    User->>Frontend: Enter register info

    Frontend->>AuthAPI: POST /api/auth/register

    AuthAPI->>UserService: Validate request
    UserService->>Database: Check existing email

    Database-->>UserService: Not exists

    UserService->>UserService: Hash password

    UserService->>Database: Save user

    Database-->>UserService: User created

    UserService->>UserService: Generate JWT

    UserService-->>AuthAPI: Access Token

    AuthAPI-->>Frontend: Register success

    Frontend-->>User: Login success
```
6. Sequence Diagram – Login
```mermaid
sequenceDiagram

    actor User

    participant Frontend
    participant AuthAPI
    participant AuthService
    participant Database
    participant JWTProvider

    User->>Frontend: Login

    Frontend->>AuthAPI: POST /api/auth/login

    AuthAPI->>AuthService: Validate credentials

    AuthService->>Database: Find user by email

    Database-->>AuthService: User data

    AuthService->>AuthService: Compare password hash

    AuthService->>JWTProvider: Generate access token

    JWTProvider-->>AuthService: JWT

    AuthService->>Database: Save refresh token

    AuthService-->>AuthAPI: Auth response

    AuthAPI-->>Frontend: Access + Refresh Token

    Frontend-->>User: Login success
```
7. Sequence Diagram – Refresh Token
```mermaid
sequenceDiagram

    actor User

    participant Frontend
    participant AuthAPI
    participant AuthService
    participant Database
    participant JWTProvider

    User->>Frontend: Open App

    Frontend->>AuthAPI: POST /api/auth/refresh

    AuthAPI->>AuthService: Validate refresh token

    AuthService->>Database: Check token

    Database-->>AuthService: Valid token

    AuthService->>JWTProvider: Generate new access token

    JWTProvider-->>AuthService: New JWT

    AuthService-->>AuthAPI: Token response

    AuthAPI-->>Frontend: New Access Token
```
8. Sequence Diagram – Logout
```mermaid
sequenceDiagram

    actor User

    participant Frontend
    participant AuthAPI
    participant Database

    User->>Frontend: Logout

    Frontend->>AuthAPI: POST /api/auth/logout

    AuthAPI->>Database: Revoke refresh token

    Database-->>AuthAPI: Success

    AuthAPI-->>Frontend: Logout success

    Frontend-->>User: Remove local tokens
```
9. Authentication State Diagram
```mermaid
stateDiagram-v2

    [*] --> Unauthenticated

    Unauthenticated --> Registered
    Registered --> Authenticated

    Authenticated --> TokenExpired
    TokenExpired --> Authenticated : Refresh Token

    Authenticated --> LoggedOut
    LoggedOut --> Unauthenticated
```
10. JWT Authentication Architecture
```mermaid
flowchart TB

    USER["User"]

    FRONTEND["React Frontend"]

    AUTHAPI["Auth API"]

    JWT["JWT Provider"]

    REDIS["Redis / Refresh Token Store"]

    DB["PostgreSQL"]

    USER --> FRONTEND

    FRONTEND --> AUTHAPI

    AUTHAPI --> JWT

    AUTHAPI --> DB

    AUTHAPI --> REDIS
```
11. Authentication API Contract
Register
POST /api/v1/auth/register

Request:

{
  "email": "user@gmail.com",
  "password": "123456",
  "fullName": "John Doe",
  "role": "CUSTOMER"
}
Login
POST /api/v1/auth/login

Request:

{
  "email": "user@gmail.com",
  "password": "123456"
}

Response:

{
  "accessToken": "...",
  "refreshToken": "...",
  "expiresIn": 3600
}
12. Database Design
users
Column	Type
id	UUID
email	varchar
password_hash	varchar
role	varchar
status	varchar
created_at	timestamp
refresh_tokens
Column	Type
id	UUID
user_id	UUID
token	text
expires_at	timestamp
revoked	boolean
13. Security Design
Security	Solution
Password hashing	BCrypt
Auth	JWT
Refresh token	DB/Redis
API security	HTTPS
Rate limiting	Redis
CORS	Allowed origins
XSS	HttpOnly Cookie
CSRF	SameSite Cookie
14. Recommended Tech Stack
Frontend
Feature	Tech
Auth State	Zustand
HTTP	Axios
Route Guard	React Router
Token Refresh	Axios Interceptor
Backend
Spring Boot
Spring Security
JWT
BCrypt

OR

ASP.NET Core
JWT Bearer
Identity
BCrypt/PasswordHasher
15. MVP Authentication Priorities
Priority	Feature
P0	Register
P0	Login
P0	JWT
P0	Refresh Token
P0	Logout
P1	OTP
P1	Social Login
P2	MFA