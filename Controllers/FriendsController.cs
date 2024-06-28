using Jendy.Core.Application.Interfaces.Services;
using Jendy.Core.Application.ViewModels.User;
using Jendy.Core.Application.Helpers;  
using Microsoft.AspNetCore.Mvc;
using Jendy.Core.Application.Interfaces.Repositories;
using Jendy.Core.Application.ViewModels.Post;
using Jendy.Core.Application.Services;

namespace Jendy.Controllers
{
    public class FriendsController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserViewModel _uservm;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IRelationShipRepository _rsRepository;
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly IReplyService _replyService;



        public FriendsController(IUserService userService, IHttpContextAccessor httpContextAccessor, 
            IRelationShipRepository relationShip, IPostService postService, ICommentService commentService
, IReplyService replyService)
        {
            _userService = userService;
            _contextAccessor = httpContextAccessor;
            _uservm = httpContextAccessor.HttpContext.Session.Get<UserViewModel>("user");
            _rsRepository = relationShip;
            _postService = postService;
            _commentService = commentService;
            _replyService = replyService;
        }
        public async Task<IActionResult> Index()
        {
            var amigos = await _rsRepository.GetFriendsAsync();
            var noamigos = await _rsRepository.GetFriendsAsync();

            ICollection<PostViewModel> allPosts = await _postService.GetAllOther();
            List<PostViewModel> posts = new List<PostViewModel>();
            List<int> amigosIds = amigos[0].Where(x => x.Id != _uservm.Id).Select(amigo => amigo.Id).ToList();
            foreach (int id in amigosIds)
            {
                foreach (PostViewModel post in allPosts) {     
                    if(post.UserId == id){
                        if (post != null) posts.Add(post);
                    }
                }
            }

            ViewBag.Amigos = amigos[0].Where(x => x.Id != _uservm.Id).Select(a => new UserViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Email = a.Email,    
                ImageUrl = a.ImageUrl,
                LastName = a.LastName,
                UserName = a.UserName,
                Password = a.Password
            }).ToList();

            var noAmigos = noamigos[1].Where(x => x.Id != _uservm.Id).Select(a => new UserViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Email = a.Email,
                ImageUrl = a.ImageUrl,
                LastName = a.LastName,
                UserName = a.UserName,
                Password = a.Password
            }).ToList();

            ViewBag.Posts = posts.OrderByDescending(x => x.CreatedTime);

            var replies = await _replyService.GetAll();
            var comments = await _commentService.GetAll();

            ViewBag.Replies = replies;
            ViewBag.Comments = comments;

            return View(noAmigos);
        }

        public async Task<IActionResult> SearchFriends(string username)
        {
            var users = await _rsRepository.GetFriendsAsync();
            var todos = users[0].Concat(users[1]);
            ICollection<UserViewModel> filtered = todos.Select(a => new UserViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Email = a.Email,
                ImageUrl = a.ImageUrl,
                LastName = a.LastName,
                UserName = a.UserName,
                Password = a.Password
            }).Where(x => _uservm.Id != x.Id && x.UserName == username).ToList();

            return View(filtered);
            
        }
        public async Task<IActionResult> AddFriend(int idFriend)
        {

            await _userService.AddFriend(_uservm.Id, idFriend);
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> RemoveFriend(int id)
        {
            var relations = await _rsRepository.GetAllAsync();
            var relation = relations.Where(x => (x.Amigo1Id == _uservm.Id 
            || x.Amigo2Id == _uservm.Id)
            && (x.Amigo1Id == id
            || x.Amigo2Id == id)
            );

            if (relation != null) await _userService.RemoveFriend(relation.First().Amigo1Id, relation.First().Amigo2Id);
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> NewFriends()
        {

            var users = await _rsRepository.GetFriendsAsync();

            var noAmigos = users[1].Where(x => x.Id != _uservm.Id).Select(a => new UserViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Email = a.Email,
                ImageUrl = a.ImageUrl,
                LastName = a.LastName,
                UserName = a.UserName,
                Password = a.Password
            }).ToList();


            return View(noAmigos);

        }

    }
}
