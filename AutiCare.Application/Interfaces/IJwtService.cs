using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using System.Security.Claims;

namespace AutiCare.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
    string GenerateRefreshToken();
    string? ValidateRefreshToken(string token);
    ClaimsPrincipal GetPrincipalFromToken(string token);
}
