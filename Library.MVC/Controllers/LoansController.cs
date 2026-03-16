using Library.Domain;
using Library.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Controllers;

[Authorize]
public class LoansController : Controller
{
    private readonly ApplicationDbContext _db;
    public LoansController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var loans = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();
        return View(loans);
    }

    // GET /Loans/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Members = new SelectList(
            await _db.Members.OrderBy(m => m.FullName).ToListAsync(),
            "Id", "FullName");
        ViewBag.Books = new SelectList(
            await _db.Books.Where(b => b.IsAvailable).OrderBy(b => b.Title).ToListAsync(),
            "Id", "Title");
        return View();
    }

    // POST /Loans/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Loan loan)
    {
        // Business rule: no active loan for this book
        bool alreadyOnLoan = await _db.Loans.AnyAsync(l =>
            l.BookId == loan.BookId && l.ReturnedDate == null);

        if (alreadyOnLoan)
        {
            ModelState.AddModelError("", "This book is already on an active loan.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Members = new SelectList(
                await _db.Members.OrderBy(m => m.FullName).ToListAsync(), "Id", "FullName");
            ViewBag.Books = new SelectList(
                await _db.Books.Where(b => b.IsAvailable).OrderBy(b => b.Title).ToListAsync(),
                "Id", "Title");
            return View(loan);
        }

        loan.LoanDate = DateTime.Today;
        if (loan.DueDate == default)
            loan.DueDate = DateTime.Today.AddDays(14);

        var book = await _db.Books.FindAsync(loan.BookId);
        book!.IsAvailable = false;

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST /Loans/Return/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id)
    {
        var loan = await _db.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == id);
        if (loan == null) return NotFound();

        loan.ReturnedDate = DateTime.Today;
        loan.Book.IsAvailable = true;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
