using System.Security.Claims;
using BoardGamesLibrary.Application.Interfaces;

namespace BoardGamesLibrary.API.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string GetUsername()
    {
        var user = httpContextAccessor.HttpContext?.User;
        var username = user?.FindFirstValue(ClaimTypes.Name)
            ?? user?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user?.Identity?.Name;

        return string.IsNullOrWhiteSpace(username) ? "system" : username;
    }
}