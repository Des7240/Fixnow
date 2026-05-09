1. Module Scope
Module	Description
Internal Wallet	Ví nội bộ cho Worker
Commission Engine	Tính hoa hồng nền tảng
Earnings Management	Quản lý thu nhập
Withdrawal System	Rút tiền
Ledger System	Sổ cái giao dịch
Reconciliation	Đối soát
Financial Audit	Audit tài chính
Admin Financial Dashboard	Dashboard tài chính
2. Why Wallet & Commission System Is Critical
Khi hệ thống scale:
1000+ bookings/day
Không thể:
kế toán chuyển khoản thủ công
tính commission bằng Excel
đối soát bằng tay
kiểm tra worker earnings thủ công
Wallet system giải quyết:
- tự động chia tiền
- tự động giữ commission
- worker có số dư realtime
- tự động đối soát
- audit tài chính đầy đủ
3. Financial Architecture
flowchart TB

    CUSTOMER["Customer"]

    PAYMENT["Payment Gateway"]

    WALLET["Wallet Service"]

    COMMISSION["Commission Engine"]

    LEDGER["Ledger System"]

    WORKER["Worker Wallet"]

    ADMIN["Admin Dashboard"]

    DB["PostgreSQL"]

    CUSTOMER --> PAYMENT

    PAYMENT --> WALLET

    WALLET --> COMMISSION

    COMMISSION --> LEDGER

    LEDGER --> WORKER

    WALLET --> DB

    ADMIN --> DB
4. Payment Distribution Workflow
flowchart TD

    PAYMENT["Customer Payment"]

    COMMISSION["Calculate Commission"]

    PLATFORM["Platform Fee"]

    WORKER["Worker Income"]

    WALLET["Update Wallet"]

    LEDGER["Create Ledger"]

    PAYMENT --> COMMISSION

    COMMISSION --> PLATFORM

    COMMISSION --> WORKER

    WORKER --> WALLET

    WALLET --> LEDGER
5. Example Commission Logic
Example
Item	Amount
Booking Price	500,000
Platform Fee (10%)	50,000
Worker Receives	450,000
Formula

WorkerIncome=TotalAmount−(TotalAmount×CommissionRate)

6. Wallet Architecture
flowchart LR

    PAYMENT["Payments"]

    WALLET["Wallet Service"]

    BALANCE["Wallet Balance"]

    TRANSACTION["Transactions"]

    WITHDRAW["Withdrawals"]

    PAYMENT --> WALLET

    WALLET --> BALANCE

    WALLET --> TRANSACTION

    WALLET --> WITHDRAW
7. Wallet State Diagram
stateDiagram-v2

    [*] --> ACTIVE

    ACTIVE --> LOCKED

    LOCKED --> ACTIVE

    ACTIVE --> CLOSED
8. Ledger-Based Financial Design
IMPORTANT
NEVER update wallet balance directly without ledger.
Correct architecture:
Transaction
→ Ledger Entry
→ Wallet Balance Calculation
9. Ledger Workflow
flowchart TD

    EVENT["Financial Event"]

    ENTRY["Create Ledger Entry"]

    CREDIT["Credit"]

    DEBIT["Debit"]

    BALANCE["Update Balance"]

    EVENT --> ENTRY

    ENTRY --> CREDIT

    ENTRY --> DEBIT

    CREDIT --> BALANCE

    DEBIT --> BALANCE
10. Sequence Diagram – Booking Completed
sequenceDiagram

    participant BookingService

    participant WalletService

    participant CommissionEngine

    participant Ledger

    participant Database

    BookingService->>WalletService: Booking completed

    WalletService->>CommissionEngine: Calculate commission

    CommissionEngine-->>WalletService: Commission result

    WalletService->>Ledger: Create entries

    Ledger->>Database: Save transactions

    Database-->>WalletService: Updated
11. Sequence Diagram – Withdraw Money
sequenceDiagram

    actor Worker

    participant Frontend

    participant WalletAPI

    participant BankService

    participant Database

    Worker->>Frontend: Request withdrawal

    Frontend->>WalletAPI: POST /withdraw

    WalletAPI->>Database: Validate balance

    WalletAPI->>BankService: Transfer money

    BankService-->>WalletAPI: Success

    WalletAPI->>Database: Update balance
12. Financial Lifecycle
flowchart TD

    BOOKING["Booking Completed"]

    PAYMENT["Payment Success"]

    COMMISSION["Deduct Commission"]

    CREDIT["Credit Worker Wallet"]

    WITHDRAW["Withdraw Money"]

    BOOKING --> PAYMENT

    PAYMENT --> COMMISSION

    COMMISSION --> CREDIT

    CREDIT --> WITHDRAW
13. Database Design
wallets
Column	Type
id	UUID
user_id	UUID
balance	numeric
pending_balance	numeric
status	varchar
created_at	timestamp
wallet_transactions
Column	Type
id	UUID
wallet_id	UUID
type	varchar
amount	numeric
balance_before	numeric
balance_after	numeric
reference_id	UUID
created_at	timestamp
ledger_entries
Column	Type
id	UUID
transaction_id	UUID
entry_type	varchar
amount	numeric
created_at	timestamp
withdrawals
Column	Type
id	UUID
wallet_id	UUID
amount	numeric
bank_name	varchar
account_number	varchar
status	varchar
created_at	timestamp
14. Transaction Types
Type
BOOKING_INCOME
COMMISSION_FEE
WITHDRAWAL
REFUND
ADJUSTMENT
15. Withdrawal Status
Status
PENDING
PROCESSING
SUCCESS
FAILED
CANCELLED
16. Wallet APIs
Get Wallet
GET /api/v1/wallet
Get Transactions
GET /api/v1/wallet/transactions
Withdraw
POST /api/v1/wallet/withdraw

Request:

{
  "amount": 500000,
  "bankName": "Vietcombank",
  "accountNumber": "123456789"
}
Get Withdrawals
GET /api/v1/wallet/withdrawals
17. Wallet Security Rules
Rule
Never trust frontend amount
All balance changes through ledger
Use DB transaction
Withdrawal requires KYC
Lock wallet on suspicious activity
18. Withdrawal Workflow
flowchart TD

    REQUEST["Withdraw Request"]

    VALIDATE["Validate Balance"]

    KYC["Check KYC"]

    CREATE["Create Withdrawal"]

    BANK["Bank Transfer"]

    UPDATE["Update Wallet"]

    REQUEST --> VALIDATE

    VALIDATE --> KYC

    KYC --> CREATE

    CREATE --> BANK

    BANK --> UPDATE
19. Reconciliation Workflow
flowchart TD

    PAYMENT["Payment Gateway"]

    WALLET["Wallet Transactions"]

    COMPARE["Compare Amount"]

    MATCH["Matched"]

    MISMATCH["Mismatch"]

    PAYMENT --> WALLET

    WALLET --> COMPARE

    COMPARE --> MATCH

    COMPARE --> MISMATCH
20. Admin Financial Dashboard
flowchart TD

    DASHBOARD["Financial Dashboard"]

    REVENUE["Revenue"]

    COMMISSION["Commission"]

    WITHDRAW["Withdrawals"]

    AUDIT["Audit Logs"]

    DASHBOARD --> REVENUE

    DASHBOARD --> COMMISSION

    DASHBOARD --> WITHDRAW

    DASHBOARD --> AUDIT
21. Financial Metrics
Metric
Total revenue
Platform earnings
Worker payouts
Pending withdrawals
Failed transactions
22. Background Job Integration
Job
Auto reconcile
Withdrawal processing
Retry failed payout
Daily financial report
Example
RecurringJob.AddOrUpdate(
    "daily-reconciliation",
    () => reconcileService.Run(),
    Cron.Daily
);
23. Frontend UX/UI Flow
Worker Wallet Flow
flowchart TD

    DASHBOARD["Worker Dashboard"]

    WALLET["Wallet"]

    HISTORY["Transactions"]

    WITHDRAW["Withdraw"]

    STATUS["Withdrawal Status"]

    DASHBOARD --> WALLET

    WALLET --> HISTORY

    WALLET --> WITHDRAW

    WITHDRAW --> STATUS
Admin Finance Flow
flowchart TD

    ADMIN["Admin Dashboard"]

    REVENUE["Revenue"]

    COMMISSION["Commission"]

    PAYOUT["Payouts"]

    AUDIT["Audit"]

    ADMIN --> REVENUE

    ADMIN --> COMMISSION

    ADMIN --> PAYOUT

    ADMIN --> AUDIT
24. UI Components
Component
Wallet balance card
Transaction table
Withdrawal modal
Revenue charts
Financial summary
25. Recommended Frontend Stack
Feature	Tech
Financial table	Ant Design Table
Charts	Recharts
Currency format	Intl.NumberFormat
State	Zustand
26. Fraud Protection
Protection
Duplicate payout prevention
Withdrawal cooldown
Suspicious activity detection
Balance integrity checks
27. Audit Logging
Event
WALLET_CREDIT
WALLET_DEBIT
WITHDRAWAL_CREATED
WITHDRAWAL_SUCCESS
COMMISSION_DEDUCTED
28. Production Recommendations
Recommendation
Use decimal for money
Use DB transaction
Immutable ledger
Financial audit logs
Separate finance service later
29. Future Scaling Path
Future
Escrow system
Instant payout
Multi-currency
Tax system
Accounting integration
30. Final Financial Architecture
flowchart TB

    PAYMENT["Payment Gateway"]

    WALLET["Wallet Service"]

    COMMISSION["Commission Engine"]

    LEDGER["Ledger"]

    WITHDRAW["Withdrawal Service"]

    JOBS["Background Jobs"]

    ADMIN["Admin Dashboard"]

    DB["PostgreSQL"]

    PAYMENT --> WALLET

    WALLET --> COMMISSION

    COMMISSION --> LEDGER

    WALLET --> WITHDRAW

    WITHDRAW --> JOBS

    LEDGER --> DB

    ADMIN --> DB
31. MVP Priorities
Priority	Feature
🔥 P4	Worker wallet
🔥 P4	Commission engine
🔥 P4	Ledger entries
🔥 P4	Withdrawal
🔥 P4	Transaction history
HIGH	Reconciliation
HIGH	Financial dashboard
FUTURE	Escrow
32. Final Outcome

Sau khi hoàn thiện module này, FixNow sẽ có:

Capability	Status
Marketplace	✅
Payments	✅
Quotation	✅
Wallet	✅
Commission	✅
Financial Audit	✅
Auto Reconciliation	✅
Worker Earnings	✅