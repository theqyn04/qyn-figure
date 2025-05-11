using qyn_figure.Models;
using qyn_figure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace qyn_figure.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly QynFigureContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUserModel> _userManager;
        public RoleController(QynFigureContext context, RoleManager<IdentityRole> roleManager, UserManager<AppUserModel> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            if (_roleManager.Roles == null)
            {
                return Problem("RoleManager.Roles is null.");
            }

            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        //Thêm role dùng get method
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        //Thêm role dùng post method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(IdentityRole role)
        {
            //Kiểm tra role đã tồn tại chưa trong hệ thống
            if (!_roleManager.RoleExistsAsync(role.Name).GetAwaiter().GetResult())
            {
                //Nếu chưa tồn tại thì thêm mới
                TempData["success"] = "Thêm role thành công";
                _roleManager.CreateAsync(role).GetAwaiter().GetResult();
            }
            return Redirect("Index");
        }

        //Chỉnh sửa role dùng get method
        [HttpGet]
        public IActionResult Edit(string RoleId)
        {
            if (RoleId == null)
            {
                return NotFound();
            }
            var role = _roleManager.Roles.FirstOrDefault(x => x.Id == RoleId);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        //Chỉnh sửa role dùng post method
        [HttpPost]
        public async Task<IActionResult> Edit(string id, IdentityRole role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var updateRole = await _roleManager.FindByIdAsync(id);
                    updateRole.Name = role.Name;
                    await _roleManager.UpdateAsync(updateRole);
                    TempData["success"] = "Chỉnh sửa role thành công";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _roleManager.RoleExistsAsync(role.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(role);
        }

        //Xóa role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string RoleId)
        {
            if (string.IsNullOrEmpty(RoleId))
            {
                TempData["error"] = "Không tìm thấy RoleId";
                return RedirectToAction("Index");
            }

            var role = await _roleManager.FindByIdAsync(RoleId);
            if (role == null)
            {
                TempData["error"] = "Role không tồn tại";
                return RedirectToAction("Index");
            }

            // Tiếp tục xử lý xóa
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["success"] = "Xóa role thành công";
            }
            else
            {
                TempData["error"] = "Xóa role thất bại: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Index");
        }
    }
}
