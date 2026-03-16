using Library.Domain;
using Library.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Controllers;

[Authorize]
public class MembersController : Controller
{
    private readonly ApplicationDbContext _db;
    public MembersController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.Members.OrderBy(m => m.FullName).ToListAsync());

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member member)
    {
        if (!ModelState.IsValid) return View(member);
        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var m = await _db.Members.FindAsync(id);
        return m == null ? NotFound() : View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Member member)
    {
        if (id != member.Id) return BadRequest();
        if (!ModelState.IsValid) return View(member);
        _db.Update(member);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var m = await _db.Members.FindAsync(id);
        return m == null ? NotFound() : View(m);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var m = await _db.Members.FindAsync(id);
        if (m != null) { _db.Members.Remove(m); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}
