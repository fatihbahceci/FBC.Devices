using FBC.DBRepository;
using System.Security.Claims;

namespace FBC.Devices.API.Services;

public class CurrentUserProvider(IHttpContextAccessor http) : ICurrentUserProvider
{
    public string? GetUserId()
        => http.HttpContext?.User?.FindFirst("UserId")?.Value;

    public string? GetUserName()
        => http.HttpContext?.User?.Identity?.Name;

    public string[] GetRoles()
        => http.HttpContext?.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray() ?? [];

    public bool IsInRole(string role)
        => http.HttpContext?.User?.IsInRole(role) ?? false;
}
