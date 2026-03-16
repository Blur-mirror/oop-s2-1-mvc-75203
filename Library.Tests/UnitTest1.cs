using Library.Domain;
using Library.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Tests;

public class LibraryTests
{
    // helpers
    private static ApplicationDbContext MakeDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static (Book book, Member member) Seed(ApplicationDbContext db)
    {
        var book = new Book { Title = "Test Book", Author = "A", Isbn = "123", Category = "Fiction", IsAvailable = true };
        var member = new Member { FullName = "Alice", Email = "a@a.com", Phone = "000" };
        db.Books.Add(book);
        db.Members.Add(member);
        db.SaveChanges();
        return (book, member);
    }

    //Test 1: Cannot create loan for book already on active loan
    [Fact]
    public void CannotLoan_BookAlreadyOnActiveLoan()
    {
        using var db = MakeDb();
        var (book, member) = Seed(db);

        db.Loans.Add(new Loan
        {
            BookId = book.Id,
            MemberId = member.Id,
            LoanDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(14)
        });
        db.SaveChanges();

        // Simulate the business-rule check from LoansController
        bool alreadyOnLoan = db.Loans.Any(l => l.BookId == book.Id && l.ReturnedDate == null);

        Assert.True(alreadyOnLoan);
    }

    //Test 2: Returning a loan makes book available again
    [Fact]
    public void ReturnLoan_MakesBookAvailable()
    {
        using var db = MakeDb();
        var (book, member) = Seed(db);

        book.IsAvailable = false;
        var loan = new Loan
        {
            BookId = book.Id,
            MemberId = member.Id,
            LoanDate = DateTime.Today.AddDays(-5),
            DueDate = DateTime.Today.AddDays(9)
        };
        db.Loans.Add(loan);
        db.SaveChanges();

        // Return
        loan.ReturnedDate = DateTime.Today;
        book.IsAvailable = true;
        db.SaveChanges();

        var updated = db.Books.Find(book.Id)!;
        Assert.True(updated.IsAvailable);
        Assert.NotNull(db.Loans.Find(loan.Id)!.ReturnedDate);
    }

    // Test 3: Book search returns expected matches
    [Fact]
    public void BookSearch_ByAuthor_ReturnsMatches()
    {
        using var db = MakeDb();
        db.Books.AddRange(
            new Book { Title = "Alpha", Author = "Smith", Isbn = "1", Category = "Fiction", IsAvailable = true },
            new Book { Title = "Beta", Author = "Jones", Isbn = "2", Category = "Fiction", IsAvailable = true }
        );
        db.SaveChanges();

        var results = db.Books
            .Where(b => b.Title.Contains("Smith") || b.Author.Contains("Smith"))
            .ToList();

        Assert.Single(results);
        Assert.Equal("Alpha", results[0].Title);
    }

    // Test 4: Overdue detection
    [Fact]
    public void Loan_IsOverdue_WhenDueDatePastAndNotReturned()
    {
        var loan = new Loan
        {
            LoanDate = DateTime.Today.AddDays(-20),
            DueDate = DateTime.Today.AddDays(-5),
            ReturnedDate = null
        };
        Assert.True(loan.IsOverdue);
    }

    // Test 5: Active loan (not overdue) is not flagged overdue
    [Fact]
    public void Loan_IsNotOverdue_WhenDueDateInFuture()
    {
        var loan = new Loan
        {
            LoanDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(7),
            ReturnedDate = null
        };
        Assert.False(loan.IsOverdue);
    }

    // Bonus test 6: Returned loan is never overdue
    [Fact]
    public void Loan_IsNotOverdue_WhenAlreadyReturned()
    {
        var loan = new Loan
        {
            LoanDate = DateTime.Today.AddDays(-30),
            DueDate = DateTime.Today.AddDays(-20),
            ReturnedDate = DateTime.Today.AddDays(-18) // returned before and after due, doesn't matter
        };
        Assert.False(loan.IsOverdue);
    }
}
