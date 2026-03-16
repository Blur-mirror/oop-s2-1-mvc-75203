using Library.Domain;
using Library.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Controllers;

[Authorize]
public class BooksController : Controller
{
    private readonly ApplicationDbContext _db;
    public BooksController(ApplicationDbContext db) => _db = db;

    // GET /Books
    public async Task<IActionResult> Index(string? search, string? category, string? availability)
    {
        IQueryable<Book> query = _db.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b =>
                b.Title.Contains(search) || b.Author.Contains(search));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(b => b.Category == category);

        if (availability == "available")
            query = query.Where(b => b.IsAvailable);
        else if (availability == "onloan")
            query = query.Where(b => !b.IsAvailable);

        query = query.OrderBy(b => b.Title);

        var categories = await _db.Books
            .Select(b => b.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.Category = category;
        ViewBag.Availability = availability;

        return View(await query.ToListAsync());
    }

    // GET /Books/Create
    public IActionResult Create() => View();

    // POST /Books/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book)
    {
        if (!ModelState.IsValid) return View(book);
        book.IsAvailable = true;
        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET /Books/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book == null) return NotFound();
        return View(book);
    }

    // POST /Books/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book)
    {
        if (id != book.Id) return BadRequest();
        if (!ModelState.IsValid) return View(book);
        _db.Update(book);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET /Books/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book == null) return NotFound();
        return View(book);
    }

    // POST /Books/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book != null) { _db.Books.Remove(book); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}
