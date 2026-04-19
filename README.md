# AutiCare Backend - Production Ready

Enterprise-grade backend for the AutiCare platform, built with .NET 8, PostgreSQL, and Clean Architecture.

## 🚀 Key Features
- **Clean Architecture**: Decoupled layers (API, Application, Domain, Infrastructure).
- **PostgreSQL**: Production-grade database on Railway.
- **Identity & Security**: Guid-based Identity, JWT authentication, and IDOR protection.
- **AI Screening**: Real-time autism screening powered by HuggingFace ML models.
- **SignalR**: Real-time communication for chat and notifications.
- **Swagger/OpenAPI**: Comprehensive API documentation.

---

## 🛠️ Tech Stack
- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL (Npgsql)
- **ORM**: Entity Framework Core 8.0
- **AI Model**: HuggingFace Spaces (ASD Prediction API)
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

### Environment Variables
| Variable | Description |
|---|---|
| `DATABASE_URL` | Auto-provided by Railway PostgreSQL |
| `JwtSettings__Secret` | Strong secret key (min 32 chars) |
| `JwtSettings__Issuer` | `AutiCareAPI` |
| `JwtSettings__Audience` | `AutiCareClient` |
| `AI_BASE_URL` | `https://moaz2545-gradpro.hf.space` |
| `AI_TIMEOUT_SECONDS` | `30` |

### Deployment Steps
1. Connect your GitHub repository to Railway.
2. Add a **PostgreSQL** service to your Railway project.
3. Railway will detect the `Dockerfile` and build automatically.
4. Database migrations run automatically at startup.

---

## 🤖 Autism Screening Module

The Screening module is the **official and only** AI-powered autism prediction system. It integrates with a real ML model hosted on HuggingFace Spaces.

### Endpoints

| Method | Route | Description | Auth |
|---|---|---|---|
| `POST` | `/api/screening/start` | Start a screening session | Parent |
| `GET` | `/api/screening/questions` | Get 10 screening questions | Any |
| `POST` | `/api/screening/submit` | Submit answers & get AI prediction | Parent |
| `GET` | `/api/screening/results/{childId}` | Get child's result history | Any |
| `GET` | `/api/screening/analytics/{childId}` | Get child's analytics summary | Any |

### AI Model Integration

- **Model**: HuggingFace ASD Prediction API (`/predict/all`)
- **Method**: Majority vote across AdaBoost, Gradient Boosting, and Random Forest
- **Payload**: 10 screening answers (0/1) + child demographics (Age, Sex, Jaundice, Family ASD history)
- **Response**: Prediction class (YES/NO) + confidence score

#### Example Submit Request
```json
POST /api/screening/submit
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

#### Example Response
```json
{
  "predictionClass": "YES",
  "confidenceScore": 0.95,
  "createdAt": "2026-04-19T12:00:00Z"
}
```

### Analytics Response Example
```json
GET /api/screening/analytics/{childId}
{
  "totalTests": 3,
  "highRiskCount": 1,
  "lowRiskCount": 2,
  "lastPrediction": "NO",
  "latestConfidenceScore": 0.82
}
```

---

## 🔒 Security
- **IDOR Protection**: All screening, children, bookings, and notes endpoints validate resource ownership.
- **JWT Authentication**: Protected endpoints require a valid Bearer token.
- **Data Validation**: Strict validation rules using FluentValidation.
- **Role-Based Access**: Parent, Doctor, and Therapist roles enforced.

---

## 📁 Project Structure
- **AutiCare.API**: Controllers, Hubs, Middleware, and Configuration.
- **AutiCare.Application**: Services, DTOs, Mappings, Validators, and Interfaces.
- **AutiCare.Domain**: Core entities and business rules.
- **AutiCare.Infrastructure**: DbContext, Repositories, Migrations, AI Client, and Security.

---

## 📜 License
This project is licensed under the MIT License.
