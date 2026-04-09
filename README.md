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

### 2. Deployment Steps
1. Connect your GitHub repository to Railway.
2. Add a **PostgreSQL** service to your Railway project.
3. Railway will detect the `Dockerfile` and build the application automatically.
4. Database migrations will run automatically at startup.

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
