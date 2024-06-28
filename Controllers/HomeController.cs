using Microsoft.AspNetCore.Mvc;
using Jendy.Core.Application.Interfaces.Services;
using WebApp.Jendy.Middlewares;
using Jendy.Core.Application.ViewModels.User;
using Jendy.Core.Application.Helpers;

namespace Jendy.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostService _postService;
        private readonly IReplyService _replyService;
        private readonly ICommentService _commentService;
        public readonly ValidateUserSession _validateUserSession;
        private readonly UserViewModel _uservm;


        public HomeController(IUserService userService, IHttpContextAccessor httpContextAccessor,
            ValidateUserSession validateUserSession, IPostService postService, ICommentService commentService
            , IReplyService replyService)
        {
            _commentService = commentService;
            _validateUserSession = validateUserSession;
            IHttpContextAccessor _contextAccessor;
            _uservm = httpContextAccessor.HttpContext.Session.Get<UserViewModel>("user");
            _postService = postService;
            _replyService = replyService;
        }

        public async Task<IActionResult> Index()
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            var posts = await _postService.GetAll();
            var orderedPosts = posts.OrderByDescending(x => x.CreatedTime).ToList();

            var replies = await _replyService.GetAll();
            var comments = await _commentService.GetAll();

            ViewBag.Comments = comments;
            ViewBag.Replies = replies;

            return View(orderedPosts);
        }

    }
}
