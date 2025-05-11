using qyn_figure.Areas.Admin.Models.ViewModels;
using qyn_figure.Models;
using qyn_figure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace book_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var roleId = "";

                // Lấy RoleId đầu tiên nếu user có role
                if (roles.Any())
                {
                    var role = await _roleManager.FindByNameAsync(roles.First());
                    roleId = role?.Id;
                }

                userRolesViewModel.Add(new UserRoleViewModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = roleId,
                    Roles = new List<string>(roles)
                });
            }

            return View(userRolesViewModel);
        }

        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(new AppUserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AppUserModel appUserModel)
        {
            ViewBag.Roles = _roleManager.Roles.ToList();
            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(appUserModel, appUserModel.PasswordHash);
                if (result.Succeeded)
                {
                    TempData["success"] = "Thêm người dùng thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(appUserModel);
        }

        public async Task<IActionResult> Remove(string UserId)
        {
            var userRemove = await _userManager.FindByIdAsync(UserId);
            if (User == null)
            {
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(userRemove);
            if (result.Succeeded)
            {
                TempData["success"] = "Xóa người dùng thành công";
                return RedirectToAction("Index");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return (NotFound());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string UserId)
        {
            var userEdit = await _userManager.FindByIdAsync(UserId);
            if (userEdit == null)
            {
                return NotFound();
            }
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(userEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUserModel appUserModel, string UserId)
        {
            var userEdit = await _userManager.FindByIdAsync(UserId);
            if (userEdit == null)
            {
                return NotFound();
            }
            ViewBag.Roles = _roleManager.Roles.ToList();
            if (ModelState.IsValid)
            {
                userEdit.UserName = appUserModel.UserName;
                userEdit.Email = appUserModel.Email;
                userEdit.PhoneNumber = appUserModel.PhoneNumber;
                var result = await _userManager.UpdateAsync(userEdit);
                if (result.Succeeded)
                {
                    TempData["success"] = "Cập nhật người dùng thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(appUserModel);
        }
    }
}
