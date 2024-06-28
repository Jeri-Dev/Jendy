using Jendy.Core.Application.Helpers;
using Jendy.Core.Application.ViewModels.User;

namespace WebApp.Jendy.Middlewares;

public class ValidateUserSession
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ValidateUserSession(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool HasUser()
    {
        UserViewModel userViewModel = _httpContextAccessor.HttpContext.Session.Get<UserViewModel>("user");

        return userViewModel != null;
    }
}