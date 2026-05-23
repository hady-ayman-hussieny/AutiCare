# AutiCare Backend

The AutiCare Backend is an ASP.NET Core 8 Web API providing telehealth and AI-powered autism screening capabilities, connecting Parents with Specialists (Doctors & Therapists).

## Core Architecture
- **Framework**: ASP.NET Core 8
- **Database**: PostgreSQL (Entity Framework Core)
- **Authentication**: Stateless JWT Bearer tokens
- **AI Integration**: HuggingFace Model API integration for screening

## Parent ↔ Specialist Telehealth Flow

The core workflow of AutiCare follows a modern telehealth booking system:

1. **AI Screening**
   - Parents register, add their child, and answer a 10-question behavioral questionnaire.
   - The backend communicates with a HuggingFace AI model to predict autism likelihood (`YES`/`NO`) and confidence scores.

2. **Booking Requests**
   - Parents browse available Specialists and create a `Booking` request for a specific date and time.
   - The booking initially has a `Pending` status.

3. **Specialist Confirmation**
   - Specialists log in and review their assigned `Pending` bookings via the Dashboard.
   - The Specialist updates the status to `Confirmed`.

4. **Session Communication & Zoom Links**
   - A `Chat` is initiated between the Parent and Specialist.
   - The Specialist sends a structured session confirmation message containing a **Zoom Meeting Link**, date, and time.
   - The message is tagged with `MessageType = "ZoomLink"` so client applications can render a distinct "Join Session" UI button.

## Security & Authorization

All endpoints are fully protected by Role-Based Access Control (RBAC) and strict Insecure Direct Object Reference (IDOR) ownership checks:
- **Parents** can only view/modify data (children, screenings, bookings, chats) that belong to them.
- **Specialists** can only view bookings and send messages in chats explicitly assigned to them.
- Unauthorized access attempts by authenticated users return `HTTP 403 Forbidden`.

## Local Development & Setup

### Prerequisites
- .NET 8 SDK
- PostgreSQL Server

### Environment Setup
1. Clone the repository.
2. Ensure PostgreSQL is running and update `ConnectionStrings:DefaultConnection` in `AutiCare.API/appsettings.json` if needed.
3. Apply migrations:
```bash
cd AutiCare.Infrastructure
dotnet ef database update --startup-project ../AutiCare.API
```

### Running the API
```bash
cd AutiCare.API
dotnet run
```
Swagger UI will be available at `http://localhost:<port>`.

### Seeded Test Accounts
The database automatically seeds standard roles and test Specialist accounts on startup. Use these to test the Specialist flows:

| Role | Email | Password |
|------|-------|----------|
| Doctor | omar.ahmed@auticare.com | AutiCare123! |
| Doctor | ahmed.ali@auticare.com | AutiCare123! |
| Therapist | sara.mohamed@auticare.com | AutiCare123! |

*(Note: Parent accounts must be manually registered via the `/api/auth/register` endpoint).*

## Known Limitations & Technical Debt
- **Zoom Link Integration**: Zoom links are generated manually by the Specialist and sent via the chat system. There is no automated Zoom API integration yet.
- **Real-time Notifications**: Real-time push via SignalR for chat messages is implemented on the backend but may require active polling on some frontends if WebSocket connections drop.
- **Legacy Entities**: The `Sessions` and `TreatmentPlans` tables are retained for backward compatibility with older client versions, though the primary workflow now uses the `Bookings` table.
