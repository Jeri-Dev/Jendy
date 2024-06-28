using Jendy.Core.Application.Interfaces.Services;
using Jendy.Core.Application.ViewModels.Post;
using Jendy.Core.Application.ViewModels.Comment;
using Microsoft.AspNetCore.Mvc;
using WebApp.Jendy.Middlewares;
using System;

namespace WebApp.Jendy.Controllers
{
    public class PostController : Controller
    {
        public readonly IPostService _postService;
        public readonly ICommentService _commentService;
        public readonly IReplyService _replyService;
        public readonly ValidateUserSession _validateUserSession;


        public PostController(IPostService postService, ValidateUserSession validateUserSession, ICommentService commentService, IReplyService replyService)
        {
            _postService = postService;
            _commentService = commentService;
            _validateUserSession = validateUserSession;
            _replyService = replyService;
        }

        public IActionResult AddView()
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }
       
            return View(new SavePostViewModel());
        }


        [HttpPost]
        public async Task<IActionResult> AddAction(SavePostViewModel pvm, string VideoUrl = "")
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }
            if (!ModelState.IsValid)
            {
                return View("AddView", pvm);
            }

            if(VideoUrl != "")
            {
                int startIndex = VideoUrl.IndexOf("v=") + 2;
                var url = VideoUrl.Substring(startIndex);
                pvm.MultimediaUrl = url;
            }else
            {
                pvm.MultimediaUrl=VideoUrl;
            }

            pvm.CreatedTime = DateTime.Now;

            SavePostViewModel svm = await _postService.Add(pvm);

            if (svm.Id != 0 && svm != null && pvm.File != null)
            {

                svm.MultimediaUrl = UploadFile(pvm.File, svm.Id);

                Console.WriteLine(svm.MultimediaUrl);
                await _postService.Update(svm, svm.Id);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> AddComment(string msg, int postid, string ctrl)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            var cvm = new CommentViewModel();
            cvm.PostId = postid;
            cvm.Message = msg;

            await _commentService.Add(cvm, postid);

            return RedirectToAction("Index", ctrl);
        }

        public async Task<IActionResult> AddReply(string msg, int commentid, string ctrl)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            var cvm = new ReplyViewModel();
            cvm.CommentId = commentid;
            cvm.Message = msg;

            await _replyService.Add(cvm, commentid);

            return RedirectToAction("Index", ctrl);
        }
        public async Task<IActionResult> EditView(int id)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }
            
            SavePostViewModel post = await _postService.GetById(id);
            return View(post);
        }

        public async Task<IActionResult> EditAction(SavePostViewModel vm)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }
            if (!ModelState.IsValid)
            {
               
                return View("EditView", vm);
            }

            SavePostViewModel post = await _postService.GetById(vm.Id);

            if (vm.MultimediaUrl != null)
            {
                vm.MultimediaUrl = UploadFile(vm.File, vm.Id, true, post.MultimediaUrl);
            }
            if (vm.VideoUrl != null)
            {
                int startIndex = vm.VideoUrl.IndexOf("v=") + 2;
                var url = vm.VideoUrl.Substring(startIndex);
                vm.MultimediaUrl = url;
            }

            await _postService.Update(vm, vm.Id);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> DeleteView(int Id)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }
            SavePostViewModel post = await _postService.GetById(Id);
            return View(post);
        }

        public async Task<IActionResult> DeleteAction(int Id)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }
            await _postService.Remove(Id);

            string basePath = $"/Images/Users/{Id}";
            string path = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{basePath}");

            if (Directory.Exists(path))
            {
                DirectoryInfo directory = new(path);

                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo folder in directory.GetDirectories())
                {
                    folder.Delete(true);
                }

                Directory.Delete(path);
            }

            return RedirectToAction("Index", "Home");
        }

        private string UploadFile(IFormFile file, int id, bool isEditMode = false, string imagePath = "")
        {
            if (isEditMode)
            {
                if (file == null)
                {
                    return imagePath;
                }
            }
            string basePath = $"/Images/Estudiantes/{id}";
            string path = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{basePath}");

            //create folder if not exist
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //get file extension
            Guid guid = Guid.NewGuid();
            FileInfo fileInfo = new(file.FileName);
            string fileName = guid + fileInfo.Extension;

            string fileNameWithPath = Path.Combine(path, fileName);

            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            if (isEditMode)
            {
                string[] oldImagePart = imagePath.Split("/");
                string oldImagePath = oldImagePart[^1];
                string completeImageOldPath = Path.Combine(path, oldImagePath);

                if (System.IO.File.Exists(completeImageOldPath))
                {
                    System.IO.File.Delete(completeImageOldPath);
                }
            }
            //Console.WriteLine($"{basePath}/{fileName}");

            return $"{basePath}/{fileName}";
        }
    }
}
