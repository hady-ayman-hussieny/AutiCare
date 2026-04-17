using System;
using System.Collections.Generic;

namespace AutiCare.Application.DTOs;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string Role,
    string? Phone,
    string? NationalId,
    string? Specialization,
    string? LicenseNumber
);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    string RefreshToken,
    string UserId,
    string FullName,
    string Email,
    string Role,
    DateTime ExpiresAt,
    string? VerificationToken = null
);

public record RefreshTokenRequest(string Token, string RefreshToken);

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record VerifyEmailRequest(string Email, string Token);

