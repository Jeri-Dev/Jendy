using Microsoft.AspNetCore.Mvc;
using Jendy.Core.Application.ViewModels.User;
using Jendy.Core.Application.Interfaces.Services;
using Jendy.Core.Application.Helpers;
using WebApp.Jendy.Middlewares;

namespace Jendy.Controllers
{
    
    public class UserActionsController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserViewModel _uservm;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly ValidateUserSession _validateUserSession;


        public UserActionsController(IUserService userService, IHttpContextAccessor httpContextAccessor, ValidateUserSession validateUserSession ) {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _uservm = httpContextAccessor.HttpContext.Session.Get<UserViewModel>("user");
            _validateUserSession = validateUserSession;

        }
        public IActionResult Index()
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            return View(_uservm);
        }

        

    }
}
