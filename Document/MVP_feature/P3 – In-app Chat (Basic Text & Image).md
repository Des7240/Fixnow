1. Module Scope
Module	Description
Chat Conversation	Customer ↔ Worker messaging
Real-time Messaging	Gửi/nhận tin nhắn realtime
Booking-bound Chat	Chat gắn với booking
Image Messaging	Gửi hình ảnh
Read Status	Đã đọc/chưa đọc
Conversation List	Danh sách cuộc trò chuyện
Notification Integration	Badge & notifications
2. Chat MVP Scope
MVP nên hỗ trợ
Feature	MVP
Text message	✅
Image message	✅
Real-time update	✅
Booking conversation	✅
Read receipt	✅
Conversation list	✅
Notification badge	✅
MVP KHÔNG nên làm ngay
Feature
Voice call
Video call
Message recall
Encryption E2E
Message reactions
Typing indicator
Online presence
3. Chat Architecture
flowchart TB

    CUSTOMER["Customer"]

    WORKER["Worker"]

    FRONTEND["React Frontend"]

    CHATAPI["Chat API"]

    WS["WebSocket Hub"]

    STORAGE["File Storage"]

    DB["PostgreSQL"]

    CUSTOMER --> FRONTEND
    WORKER --> FRONTEND

    FRONTEND --> CHATAPI

    FRONTEND --> WS

    CHATAPI --> DB

    CHATAPI --> STORAGE

    WS --> DB
4. Chat Workflow
flowchart TD

    BOOKING["Booking Assigned"]

    OPEN["Open Chat"]

    SEND["Send Message"]

    RECEIVE["Receive Message"]

    READ["Mark As Read"]

    BOOKING --> OPEN

    OPEN --> SEND

    SEND --> RECEIVE

    RECEIVE --> READ
5. Chat State Diagram
stateDiagram-v2

    [*] --> SENT

    SENT --> DELIVERED

    DELIVERED --> READ
6. Conversation Architecture
flowchart LR

    BOOKING["Booking"]

    CONVERSATION["Conversation"]

    MESSAGE["Messages"]

    USERS["Participants"]

    BOOKING --> CONVERSATION

    CONVERSATION --> MESSAGE

    CONVERSATION --> USERS
7. Swimlane – Send Text Message
flowchart LR

    subgraph CUSTOMER["Customer"]
        C1["Type Message"]
    end

    subgraph FRONTEND["Frontend"]
        F1["Send WebSocket Event"]
    end

    subgraph HUB["WebSocket Hub"]
        H1["Broadcast Message"]
    end

    subgraph API["Chat API"]
        A1["Save Message"]
    end

    subgraph DB["Database"]
        D1["messages"]
    end

    subgraph WORKER["Worker"]
        W1["Receive Message"]
    end

    C1 --> F1

    F1 --> H1

    H1 --> A1

    A1 --> D1

    H1 --> W1
8. Sequence Diagram – Send Text Message
sequenceDiagram

    actor Customer

    participant Frontend
    participant WebSocket
    participant ChatAPI
    participant Database
    participant WorkerFrontend

    Customer->>Frontend: Send message

    Frontend->>WebSocket: chat.send

    WebSocket->>ChatAPI: Persist message

    ChatAPI->>Database: Save message

    Database-->>ChatAPI: Saved

    WebSocket-->>WorkerFrontend: New message
9. Sequence Diagram – Send Image Message
sequenceDiagram

    actor Customer

    participant Frontend
    participant UploadAPI
    participant FileStorage
    participant ChatAPI
    participant WebSocket
    participant WorkerFrontend

    Customer->>Frontend: Select image

    Frontend->>UploadAPI: Upload image

    UploadAPI->>FileStorage: Store image

    FileStorage-->>UploadAPI: Image URL

    Frontend->>ChatAPI: Send image message

    ChatAPI->>WebSocket: Broadcast message

    WebSocket-->>WorkerFrontend: Receive image message
10. Chat Backend Architecture
flowchart TB

    CONTROLLER["Chat Controller"]

    SERVICE["Chat Service"]

    HUB["SignalR Hub / WebSocket"]

    STORAGE["File Storage"]

    DB["PostgreSQL"]

    CONTROLLER --> SERVICE

    SERVICE --> HUB

    SERVICE --> STORAGE

    SERVICE --> DB
11. Recommended Technology
ASP.NET
Feature	Tech
Realtime	SignalR
Messaging	WebSocket
File upload	MinIO
Database	PostgreSQL
Spring Boot
Feature	Tech
Realtime	STOMP WebSocket
Messaging	SockJS
Database	PostgreSQL
12. Database Design
conversations
Column	Type
id	UUID
booking_id	UUID
customer_id	UUID
worker_id	UUID
created_at	timestamp
messages
Column	Type
id	UUID
conversation_id	UUID
sender_id	UUID
message_type	varchar
content	text
is_read	boolean
created_at	timestamp
message_attachments
Column	Type
id	UUID
message_id	UUID
file_id	UUID
13. Message Types
Type
TEXT
IMAGE
SYSTEM
14. Chat APIs
Get Conversations
GET /api/v1/chat/conversations
Get Messages
GET /api/v1/chat/conversations/{id}/messages
Send Message
POST /api/v1/chat/messages

Request:

{
  "conversationId": "uuid",
  "messageType": "TEXT",
  "content": "I am coming now"
}
Mark As Read
POST /api/v1/chat/messages/read
15. WebSocket Events
Event
chat.send
chat.receive
chat.read
chat.typing (future)
16. Message Security Rules
Rule
Chỉ customer & assigned worker được chat
Chat phải gắn booking
Không chat nếu booking cancelled
Validate image MIME type
Limit upload size
17. Read Receipt Workflow
flowchart TD

    MESSAGE["Receive Message"]

    OPEN["Open Conversation"]

    READ["Mark As Read"]

    UPDATE["Update Read Status"]

    MESSAGE --> OPEN

    OPEN --> READ

    READ --> UPDATE
18. Notification Integration
Khi có tin nhắn mới
Action
Update badge
Push in-app notification
Show unread count
Sequence
sequenceDiagram

    participant ChatService

    participant NotificationService

    participant Frontend

    ChatService->>NotificationService: New message

    NotificationService-->>Frontend: Update unread badge
19. File Upload Integration
Image Upload Flow
flowchart TD

    SELECT["Select Image"]

    VALIDATE["Validate"]

    UPLOAD["Upload To Storage"]

    URL["Generate URL"]

    MESSAGE["Attach To Message"]

    SELECT --> VALIDATE

    VALIDATE --> UPLOAD

    UPLOAD --> URL

    URL --> MESSAGE
20. Frontend UX/UI Flow
Customer Chat Flow
flowchart TD

    BOOKING["Booking Detail"]

    CHAT["Open Chat"]

    MESSAGE["Send Message"]

    IMAGE["Send Image"]

    READ["Read Messages"]

    BOOKING --> CHAT

    CHAT --> MESSAGE

    CHAT --> IMAGE

    CHAT --> READ
Worker Chat Flow
flowchart TD

    JOB["Assigned Job"]

    CHAT["Open Conversation"]

    REPLY["Reply Message"]

    PHOTO["Send Work Photo"]

    JOB --> CHAT

    CHAT --> REPLY

    CHAT --> PHOTO
21. UI Components
Component
Conversation list
Chat window
Message bubble
Image preview
Unread badge
22. Recommended Frontend Stack
Feature	Tech
Chat UI	Ant Design
Realtime	SignalR client
State	Zustand
Infinite scroll	React Query
Image upload	React Dropzone
23. Recommended Backend Rules
Rule
Persist before broadcast
Message ordering
Use pagination
Soft delete messages
Limit image size
24. Pagination Strategy
Messages
GET /messages?page=1&pageSize=30
Infinite scroll
Direction
Load older messages upward
25. Logging & Audit Events
Event
MESSAGE_SENT
MESSAGE_READ
IMAGE_UPLOADED
CONVERSATION_CREATED
26. Future Enhancements
Feature
Typing indicator
Voice message
Message recall
Push notification
AI moderation
E2E encryption
27. Production Recommendations
Recommendation
SignalR scale-out
Redis backplane
CDN for images
Message retention policy
Upload antivirus scan
28. Final Chat Architecture
flowchart TB

    FRONTEND["React"]

    SIGNALR["SignalR Hub"]

    CHATAPI["Chat API"]

    STORAGE["MinIO"]

    NOTI["Notification Service"]

    DB["PostgreSQL"]

    FRONTEND --> SIGNALR

    FRONTEND --> CHATAPI

    CHATAPI --> STORAGE

    CHATAPI --> DB

    SIGNALR --> DB

    CHATAPI --> NOTI
29. MVP Priorities
Priority	Feature
🔥 P3	Text Chat
🔥 P3	Image Message
🔥 P3	Booking-bound Chat
🔥 P3	Read Receipt
🔥 P3	Notification Badge
HIGH	Infinite Scroll
HIGH	Push Notification
FUTURE	Voice/Video
30. Final Outcome

Sau khi hoàn thiện module này, FixNow sẽ có:

Capability	Status
Marketplace	✅
Worker Matching	✅
Notifications	✅
Timeline	✅
Reviews	✅
Payments	✅
Real-time Chat	✅
Image Messaging	✅