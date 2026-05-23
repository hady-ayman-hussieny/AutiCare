using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AutiCare.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendWelcomeEmailAsync(string to, string name);
}
