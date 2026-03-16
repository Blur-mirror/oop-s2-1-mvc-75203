using Bogus;
using Library.Domain;
using Microsoft.AspNetCore.Identity;

namespace Library.MVC.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Roles & admin user
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await roleManager.RoleExistsAsync("Staff"))
            await roleManager.CreateAsync(new IdentityRole("Staff"));

        const string adminEmail = "admin@library.ie";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        //Skip this step if data already exists
        if (db.Books.Any()) return;

        await db.Database.EnsureCreatedAsync();

        // Books
        var categories = new[] { "Fiction", "Non-Fiction", "Science", "History", "Biography", "Technology" };

        var bookFaker = new Faker<Book>()
            .RuleFor(b => b.Title, f => f.Lorem.Sentence(3).TrimEnd('.'))
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.Isbn, f => f.Commerce.Ean13())
            .RuleFor(b => b.Category, f => f.PickRandom(categories))
            .RuleFor(b => b.IsAvailable, _ => true);

        var books = bookFaker.Generate(20);
        db.Books.AddRange(books);

        // Members
        var memberFaker = new Faker<Member>()
            .RuleFor(m => m.FullName, f => f.Name.FullName())
            .RuleFor(m => m.Email, (f, m) => f.Internet.Email(m.FullName))
            .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber("08#-###-####"));

        var members = memberFaker.Generate(10);
        db.Members.AddRange(members);

        await db.SaveChangesAsync();

        //  Loans (15 total: some returned, some active, some overdue)
        var today = DateTime.Today;
        var rng = new Random(42);

        // 5 returned loans
        for (int i = 0; i < 5; i++)
        {
            var book = books[i];
            var loanDate = today.AddDays(-rng.Next(20, 60));
            db.Loans.Add(new Loan
            {
                Book = book,
                Member = members[rng.Next(members.Count)],
                LoanDate = loanDate,
                DueDate = loanDate.AddDays(14),
                ReturnedDate = loanDate.AddDays(rng.Next(3, 13))
            });
            // book stays available after return
        }

        // 5 active (not overdue) loans
        for (int i = 5; i < 10; i++)
        {
            var book = books[i];
            book.IsAvailable = false;
            var loanDate = today.AddDays(-rng.Next(1, 7));
            db.Loans.Add(new Loan
            {
                Book = book,
                Member = members[rng.Next(members.Count)],
                LoanDate = loanDate,
                DueDate = today.AddDays(rng.Next(3, 10)),
                ReturnedDate = null
            });
        }

        // 5 overdue loans
        for (int i = 10; i < 15; i++)
        {
            var book = books[i];
            book.IsAvailable = false;
            var loanDate = today.AddDays(-30);
            db.Loans.Add(new Loan
            {
                Book = book,
                Member = members[rng.Next(members.Count)],
                LoanDate = loanDate,
                DueDate = loanDate.AddDays(14),   // overdue: 16+ days ago
                ReturnedDate = null
            });
        }

        await db.SaveChangesAsync();
    }
}
