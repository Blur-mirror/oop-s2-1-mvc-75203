# Community Library Desk

An internal library management system built with ASP.NET Core MVC, EF Core (SQLite), and ASP.NET Identity.

## Features

- Book, Member, and Loan management (full CRUD)
- Loan workflow: create loans, mark returned, overdue detection
- Books index: search by title/author, filter by category and availability
- Admin role management page (list, create, delete roles)
- Seed data: 20 books, 10 members, 15 loans (active, returned, overdue)
- CI pipeline with xUnit tests and code coverage report via GitHub Pages

## Getting Started

### Prerequisites

- .NET 8 SDK
- EF Core tools: `dotnet tool install --global dotnet-ef`

### Run locally

```bash
git clone <repo-url>
cd Library.MVC
dotnet ef database update
dotnet run
```

The database is seeded automatically on first run.

## Test credentials

| Role  | Email               | Password   |
|-------|---------------------|------------|
| Admin | <admin@library.ie>    | Admin123!  |

The Admin account has access to `/Admin/Roles` for role management.

## Running tests

```bash
dotnet test -c Release --verbosity normal
```

## Tech stack

- ASP.NET Core 8 MVC
- Entity Framework Core 8 (SQLite)
- ASP.NET Identity
- Bogus (seed data)
- xUnit + coverlet (tests)
- GitHub Actions (CI + GitHub Pages coverage report)
