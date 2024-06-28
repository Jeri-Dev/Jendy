using Jendy.Core.Application.Interfaces.Services;
using Jendy.Core.Application.ViewModels.User;
using Jendy.Core.Application.Helpers;
using Microsoft.AspNetCore.Mvc;
using WebApp.Jendy.Middlewares;

namespace WebApp.Jendy.Controllers
{
    public class UserController : Controller
    {

        public readonly IUserService _service;
        public readonly ValidateUserSession _validateUserSession;
        private readonly UserViewModel _uservm;
        private readonly IHttpContextAccessor _contextAccessor;

        public UserController(IUserService service, ValidateUserSession validateUserSession, IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _validateUserSession = validateUserSession;
            _contextAccessor = httpContextAccessor;
            _uservm = httpContextAccessor.HttpContext.Session.Get<UserViewModel>("user");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel lvm)
        {
            if (_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            if (!ModelState.IsValid)
            {
                return View("Index", lvm);
            }
            


            UserViewModel _user = await _service.Login(lvm);

            if (_user != null)
            {
                HttpContext.Session.Set("user", _user);
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            return RedirectToAction("Index");
        }

        public IActionResult RegisterView()
        {
            if (_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            return View(new SaveViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(SaveViewModel uvm)
        {
            if (_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            if (!ModelState.IsValid)
            {
                return View("RegisterView", uvm);
            }
            SaveViewModel svm = await _service.Add(uvm);

            if (svm.Id != 0 && svm != null)
            {

                svm.ImageUrl = UploadFile(uvm.File, svm.Id);

                Console.WriteLine(svm.ImageUrl);
                await _service.Update(svm, svm.Id);
            }


            return RedirectToAction("Index", "User");
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("user");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditView(int id)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            SaveViewModel user = await _service.GetById(id);
            
            return View(user);
        }

        public async Task<IActionResult> EditAction(SaveViewModel vm)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            if(vm.Password == null)
            {
                vm.Password = _uservm.Password;
                vm.ConfirmPassword = _uservm.Password;
            }else
            {
                vm.Password = PasswordEncryptation.ToSha256Hash(vm.Password);
            }

            if (!ModelState.IsValid)
            {

                return View("EditView", vm);
            }

            SaveViewModel user = await _service.GetById(vm.Id);

            vm.ImageUrl = UploadFile(vm.File, vm.Id, true, user.ImageUrl);


            
            

            await _service.Update(vm, vm.Id);
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
            string basePath = $"/Images/Users/{id}";
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
