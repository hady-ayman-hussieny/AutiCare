# AutiCare Backend - Production Ready

Enterprise-grade backend for the AutiCare platform, built with .NET 8, PostgreSQL, and Clean Architecture.

## 🚀 Key Features
- **Clean Architecture**: Decoupled layers (API, Application, Domain, Infrastructure).
- **PostgreSQL**: Fully migrated from SQLite for production scalability.
- **Identity & Security**: Guid-based Identity, JWT authentication, and IDOR protection.
- **AI Assessment**: Automated risk assessment logic for child developmental monitoring.
- **SignalR**: Real-time communication for chat and notifications.
- **Swagger/OpenAPI**: Comprehensive API documentation.

---

## 🛠️ Tech Stack
- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL (Npgsql)
- **ORM**: Entity Framework Core 8.0
- **Logging**: Serilog
- **Mapping**: AutoMapper
- **Validation**: FluentValidation

---

## ⚙️ Local Setup

### 1. Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)

### 2. Configuration
Update `AutiCare.API/appsettings.json` with your local PostgreSQL credentials:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=auticare;Username=postgres;Password=your_password"
}
```

### 3. Database Migration
Run the following commands in the root directory:
```bash
dotnet ef database update --project AutiCare.Infrastructure --startup-project AutiCare.API
```

### 4. Run the Application
```bash
dotnet run --project AutiCare.API
```
The API will be available at `http://localhost:8080/swagger`.

---

## ☁️ Railway Deployment

### 1. Environment Variables
Ensure the following variables are set in your Railway project:
- `DATABASE_URL`: Automatically provided by Railway PostgreSQL.
- `JwtSettings__Secret`: A strong secret key (min 32 characters).
- `JwtSettings__Issuer`: `AutiCareAPI`
- `JwtSettings__Audience`: `AutiCareClient`
- `ALLOWED_ORIGINS`: JSON array of allowed origins (e.g., `["https://your-frontend.vercel.app"]`).
- `AI_BASE_URL`: The URL for the AI screening model.
- `AI_API_KEY`: The API key for the AI screening model.
- `AI_TIMEOUT_SECONDS`: Timeout duration for the model in seconds.

### 2. Deployment Steps
1. Connect your GitHub repository to Railway.
2. Add a **PostgreSQL** service to your Railway project.
3. Railway will detect the `Dockerfile` and build the application automatically.
4. Database migrations will run automatically at startup.

---

## 🤖 Autism Screening AI Integration
The backend integrates seamlessly with an external ML screening model.

### Environment Variables Configuration
- Set `AI_BASE_URL` to configure the real ML provider URL.
- We currently map this internally using `IAiClientProvider` for strict type-safety and easy mock substitution.

### Strict Payload Specification
The dataset format strictly expects:
- `A1` to `A10`: Integers (`0` or `1`).
- `Age`: Integer representing months (auto-calculated from child's DOB).
- `Sex`: Lowercase `'m'` or `'f'`.
- `Jaundice`: Lowercase `'yes'` or `'no'`.
- `Family_ASD`: Lowercase `'yes'` or `'no'`.

#### Example API Request (`POST /api/screening/submit`)
```json
{
  "childId": 1,
  "answers": [
    { "questionId": 1, "answerValue": 1 },
    { "questionId": 2, "answerValue": 0 },
    { "questionId": 3, "answerValue": 1 },
    { "questionId": 4, "answerValue": 0 },
    { "questionId": 5, "answerValue": 1 },
    { "questionId": 6, "answerValue": 1 },
    { "questionId": 7, "answerValue": 0 },
    { "questionId": 8, "answerValue": 1 },
    { "questionId": 9, "answerValue": 0 },
    { "questionId": 10, "answerValue": 1 }
  ]
} 
```
*Note: Exactly 10 answers must be provided, or the request avoids being sent to the AI and fails locally.*

#### Example API Response
```json
{
  "id": 1,
  "childId": 1,
  "childName": "John Doe",
  "predictionClass": "YES",
  "confidenceScore": null,
  "createdAt": "2026-04-17T00:00:00Z"
}
```

---

## 🔒 Security
- **IDOR Protection**: All resource-related services (Children, Bookings, Test Results) validate ownership before performing actions.
- **JWT Authentication**: Protected endpoints require a valid Bearer token.
- **Data Validation**: Strict validation rules using FluentValidation.

---

## 📁 Project Structure
- **AutiCare.API**: Controllers, Hubs, Middlewares, and Configuration.
- **AutiCare.Application**: Service implementations, DTOs, Mappings, and Interfaces.
- **AutiCare.Domain**: Core entities and business rules.
- **AutiCare.Infrastructure**: EfCore Context, Repositories, Migrations, and Security services.

---

## 📜 License
This project is licensed under the MIT License.
