using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AutiCare.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IParentRepository _parentRepo;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IParentRepository parentRepo,
        IDoctorRepository doctorRepo,
        IJwtService jwtService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _parentRepo = parentRepo;
        _doctorRepo = doctorRepo;
        _jwtService = jwtService;
        _emailService = emailService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
            throw new InvalidOperationException("Email already registered");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            Role = request.Role
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        //  ???? ?????? ?? ?????? ??? ??? ???????
        var createdUser = await _userManager.FindByEmailAsync(request.Email)
        ?? throw new Exception("User Creation Failed");

        await _userManager.AddToRoleAsync(createdUser!, request.Role);

        // Create profile record based on role
        if (request.Role == "Parent")
        {
            await _parentRepo.AddAsync(new Parent
            {
                UserId = createdUser.Id,
                Name = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                NationalId = request.NationalId
            });

            await _parentRepo.SaveChangesAsync();
        }
        else if (request.Role is "Doctor" or "Therapist")
        {
            await _doctorRepo.AddAsync(new Specialist
            {
                UserId = createdUser.Id,
                Name = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                NationalId = request.NationalId,
                Specialization = request.Specialization,
                LicenseNumber = request.LicenseNumber
            });

            await _doctorRepo.SaveChangesAsync();
        }

        var token = _jwtService.GenerateToken(createdUser!);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var emailVerificationToken =
            await _userManager.GenerateEmailConfirmationTokenAsync(createdUser);

        Console.WriteLine("VERIFY TOKEN:");
        Console.WriteLine(emailVerificationToken);

        await _emailService.SendEmailAsync(
            createdUser.Email!,
            "Verify your email",
            $"Use this token to verify your email:\n\n{emailVerificationToken}"
        );

        return new AuthResponse(
            token,
            refreshToken,
            createdUser.Id.ToString(),
            createdUser.FullName,
            createdUser.Email!,
            request.Role,
            DateTime.UtcNow.AddMinutes(15)
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (user.IsDeleted)
            throw new UnauthorizedAccessException("Account is deactivated");

        var token = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new AuthResponse(token, refreshToken, user.Id.ToString(),
            user.FullName, user.Email!, user.Role, DateTime.UtcNow.AddMinutes(15));
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var userId = _jwtService.ValidateRefreshToken(request.RefreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token");

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("User not found");

        var token = _jwtService.GenerateToken(user);
        var newRefresh = _jwtService.GenerateRefreshToken();

        return new AuthResponse(token, newRefresh, user.Id.ToString(),
            user.FullName, user.Email!, user.Role, DateTime.UtcNow.AddMinutes(15));
    }

    public async Task LogoutAsync(Guid userId)
    {
        // Invalidate refresh token stored in cache/db if implemented
        await Task.CompletedTask;
    }
   
    
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.IsDeleted) return; // Do not reveal if user does not exist

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"http://localhost:3000/reset-password?email={user.Email}&token={Uri.EscapeDataString(token)}";
        
        await _emailService.SendEmailAsync(user.Email!, "Reset your password", $"Please reset your password using this link: {resetLink}");
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("User not found");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException("Failed to reset password: " + string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("User not found");

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.Select(e => e.Description))
            );
        }
    }
}
