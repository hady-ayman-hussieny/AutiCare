using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutiCare.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid GetUserId() =>
     Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    protected string GetUserRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? "";
}
