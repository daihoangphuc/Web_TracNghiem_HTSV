using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_TracNghiem_HTSV.Models;
using Web_TracNghiem_HTSV.Services;

namespace Web_TracNghiem_HTSV.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class UserRolesController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRolesController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Hiển thị danh sách người dùng
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            var users = from u in _userManager.Users
                        select new User
                        {
                            Id = u.Id,
                            UserName = u.UserName,
                            Email = u.Email
                            // Thêm các thuộc tính khác của User nếu cần
                        };

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(x => x.UserName.Contains(searchString));
            }

            int pageSize = 10; // Số lượng mục trên mỗi trang
            return View(await PaginatedList<User>.CreateAsync(users.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        // Xóa người dùng
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(nameof(Index));
        }

        // Thêm vai trò
        public IActionResult AddRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(string roleName)
        {
            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        public async Task<IActionResult> EditRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name,
                Selected = userRoles.Contains(r.Name)
            }).ToList();

            ViewBag.UserId = id;
            ViewBag.UserName = user.UserName;
            ViewBag.Roles = allRoles;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> EditRole(string id, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, roles.Except(userRoles));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add selected roles to user.");
                return View();
            }

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(roles));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove unselected roles from user.");
                return View();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
