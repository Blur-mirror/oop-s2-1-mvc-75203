using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library.MVC.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    public AdminController(RoleManager<IdentityRole> roleManager) =>
        _roleManager = roleManager;

    // GET /Admin/Roles
    public IActionResult Roles() =>
        View(_roleManager.Roles.OrderBy(r => r.Name).ToList());

    // POST /Admin/Roles/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        if (!string.IsNullOrWhiteSpace(roleName) &&
            !await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
        }
        return RedirectToAction(nameof(Roles));
    }

    // POST /Admin/Roles/Delete
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role != null) await _roleManager.DeleteAsync(role);
        return RedirectToAction(nameof(Roles));
    }
}
