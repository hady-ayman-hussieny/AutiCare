# AutiCare Backend 🧩

AutiCare is a production-grade backend system designed to support a platform for autism screening, specialist consultations, and developmental tracking. Built with ASP.NET Core 8 and PostgreSQL, it leverages external AI models to provide rapid developmental insights.

---

## 🚀 Tech Stack
-   **Framework**: ASP.NET Core 8 (Web API)
-   **Architecture**: Clean Architecture / Repository Pattern
-   **Database**: PostgreSQL
-   **Identity**: ASP.NET Core Identity (JWT Stateless)
-   **Validation**: FluentValidation
-   **Documentation**: Swagger / OpenAPI
-   **Deployment**: Railway

---

## 🔗 Live URLs
-   **Backend (Production)**: [https://auticare-production.up.railway.app](https://auticare-production.up.railway.app)
-   **Swagger UI**: [https://auticare-production.up.railway.app/swagger](https://auticare-production.up.railway.app/swagger)
-   **Frontend (Main)**: [https://auticare-frontend-main.vercel.app/](https://auticare-frontend-main.vercel.app/)
-   **External AI Endpoint**: `https://moaz2545-gradpro.hf.space/predict/all`

---

## 🔐 Authentication Flow
AutiCare uses a simplified, stateless JWT-only flow optimized for production stability:
1.  **Register**: User creates an account (Parent, Doctor, or Therapist). Login is immediate post-registration (Email verification is disabled for launch-readiness).
2.  **Login**: User receives a high-entropy JWT Access Token.
3.  **Token Expiration**: Access Tokens are valid for **30 days**.
4.  **Logout**: Stateless; client deletes the token locally.

---

## ⚙️ Environment Variables
The following variables are required in the production environment (Railway):
-   `DATABASE_URL`: Connection string for PostgreSQL (Auto-parsed).
-   `JwtSettings:Secret`: Long random string for JWT signature.
-   `AI_BASE_URL`: URL for the HuggingFace AI prediction service.
-   `AI_TIMEOUT_SECONDS`: Maximum wait time for AI response (Default: 30s).
-   `Email:SmtpServer`, `Email:SmtpPort`, `Email:SenderEmail`, `Email:Password`: SMTP configuration for password reset emails.

---

## 🛠️ Local Setup
1.  **Clone the repository**:
    ```bash
    git clone https://github.com/Shahd-Alaa/AutiCare
    ```
2.  **Update appsettings.json**: Configure your local PostgreSQL connection string.
3.  **Apply Migrations**:
    ```bash
    dotnet ef database update --project AutiCare.Infrastructure --startup-project AutiCare.API
    ```
4.  **Run the application**:
    ```bash
    dotnet run --project AutiCare.API
    ```

---

## 🤖 AI Integration Details
The screening module communicates with a HuggingFace-hosted model.
-   **Payload**: Sends 10 behavioral markers, age in months, gender, family history, and jaundice (Note: API expects field `Jauundice`).
-   **Confidence Calculation**: The system extracts probabilities from several specialized models and returns the maximum confidence score provided by the ensemble.

---

## 📊 API Usage Examples

### Submit Screening (Parent)
`POST /api/screening/submit`
```json
{
  "childId": 12,
  "answers": [
    { "questionId": 1, "answerValue": 1 },
    ...
    { "questionId": 10, "answerValue": 0 }
  ]
}
```

### Book a Specialist
`POST /api/bookings`
```json
{
  "specialistId": 5,
  "childId": 12,
  "bookingDate": "2026-05-10T00:00:00Z",
  "bookingTime": "14:00:00"
}
```

---

## ☁️ Railway Deployment Notes
-   The project includes a `ParseDatabaseUrl` utility in `Program.cs` to automatically convert the standard Railway internal `DATABASE_URL` into a .NET-compatible Npgsql connection string.
-   Ensure all environment variables are populated in the Railway Dashboard under "Variables".
