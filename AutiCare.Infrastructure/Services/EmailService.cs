using AutiCare.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutiCare.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // TODO: integrate SMTP / SendGrid
        _logger.LogInformation("Email to {To}: {Subject}", to, subject);
        await Task.CompletedTask;
    }

    public async Task SendWelcomeEmailAsync(string to, string name)
    {
        await SendEmailAsync(to, "Welcome to AutiCare",
            $"Dear {name}, welcome to AutiCare platform.");
    }
}
