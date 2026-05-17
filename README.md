# 💰 RV Personal Finance

> 🚧 **Status:** In progress — Phase 1 (Project Foundation)

A personal finance REST API built with **ASP.NET Core Minimal APIs**, **Entity Framework Core** and **PostgreSQL**.

This project is part of my transition from legacy ERP development (**Progress 4GL / Datasul**) to modern backend development using **C#** and **.NET**, serving as a hands-on platform for practicing backend engineering, architecture, Docker, and automated testing.

---

## 🎯 Purpose

The main goal of this project is to build a real-world personal finance system while developing deep understanding of modern backend engineering concepts, including:

- Domain modeling and business rules
- REST API design with Minimal APIs
- Entity Framework Core with PostgreSQL
- Result Pattern (`OperationResult<T>`)
- JWT authentication and user ownership
- Automated testing (unit and integration)
- Docker and containerized infrastructure
- Structured logging and health checks

This project is intentionally built **without shortcuts** — each concept is understood before being implemented. The goal is engineering maturity, not just a working system.

---

## 🧱 Domain

### Entities

- **User** — authenticated owner of all data
- **Account** — personal financial account (checking, wallet, investment, etc.)
- **Category** — transaction classification (food, transport, salary, etc.)
- **Transaction** — financial movement, typed as `Income` or `Expense`

### Business Rules

- Account balance is **derived from transactions** — no stored balance field
- Every transaction belongs to an account and a category
- All entities belong to an authenticated user (ownership enforced)
- `Amount` must be greater than zero
- `TransactionType` is a C# enum persisted as string via EF Core

---

## 🚀 Features (MVP)

- [ ] CRUD: Accounts, Categories, Transactions
- [ ] Balance calculation derived from transactions (Income − Expense)
- [ ] Financial history queries
- [ ] Dashboard: total income, total expenses, balance, spending by category
- [ ] JWT authentication and user registration
- [ ] User ownership enforcement on all resources

---

## 🧪 Tests

Planned test coverage:

- **Unit tests** — service layer business rules (xUnit + Moq)
- **Integration tests** — real HTTP requests via WebApplicationFactory
- **Isolated PostgreSQL** for integration test environments

---

## 🗂 Project Structure

```
RvPersonalFinance/
│
├── src/
│   └── RvPersonalFinance.Api/
│       ├── Domain/
│       │   ├── Entities/      # Domain entities (User, Account, Category, Transaction)
│       │   └── Enums/         # TransactionType and future enums
│       ├── Infrastructure/
│       │   └── Persistence/   # AppDbContext and migrations
│       ├── Features/
│       │   ├── Accounts/      # AccountDtos, AccountService, AccountEndpoints
│       │   ├── Categories/    # CategoryDtos, CategoryService, CategoryEndpoints
│       │   └── Transactions/  # TransactionDtos, TransactionService, TransactionEndpoints
│       ├── Shared/            # OperationResult<T> and shared types
│       ├── Extensions/        # Endpoint registration and DI setup
│       └── Program.cs         # Application entry point
│
├── tests/
│   └── RvPersonalFinance.Tests/
│       ├── Unit/              # Service unit tests
│       └── Integration/       # API integration tests
│
├── docker-compose.yml
├── .env                       # Local environment variables (not committed)
├── .gitignore
├── README.md
└── RvPersonalFinance.sln
```

---

## 🧠 Design Decisions

- **Minimal APIs** — modern, low-boilerplate approach; endpoints organized via Extension Methods
- **PostgreSQL** — production-grade relational database from day one
- **UUID v7** — `Guid.CreateVersion7()` (.NET 9 native); time-ordered for index performance, safer than sequential IDs in public APIs
- **Derived balance** — `Account` has no `CurrentBalance` field; balance is computed from transactions (consistency over performance in MVP)
- **`TransactionType` as enum** — persisted as string via `HasConversion<string>()` in EF Core; domain stays decoupled from persistence
- **No Repository Pattern** — EF Core `DbContext` is used directly in Services; `DbSet<T>` already is a repository abstraction. Adding `ITransactionRepository` would be an abstraction over an abstraction with no real problem to solve — there is no requirement to swap ORM or database
- **Vertical Slice structure** — code organized by domain feature (`Features/Accounts/`, `Features/Transactions/`, etc.) rather than by technical layer; everything related to a feature lives together, reducing cognitive cost for both humans and AI coding agents
- **OperationResult<T>** — custom result pattern for standardized, predictable API responses
- **DTOs separate from entities** — API contract evolution independent from domain model
- **Docker from day one** — PostgreSQL runs in containers; development environment matches production

---

## 🛠 Tech Stack

- .NET 9
- ASP.NET Core Minimal APIs
- Entity Framework Core
- PostgreSQL
- Docker / Docker Compose
- xUnit
- Moq
- WebApplicationFactory
- ILogger (structured logging)

---

## ▶️ Running the Project

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)

### Steps

1. Clone the repository:

```bash
git clone https://github.com/rafaelvanhoni/rv-personal-finance.git
cd rv-personal-finance
```

2. Create a `.env` file in the root (use `.env.example` as reference):

```env
POSTGRES_DB=rv_personal_finance
POSTGRES_USER=rv_admin
POSTGRES_PASSWORD=your_password_here
```

3. Start the database:

```bash
docker compose up -d
```

4. Configure the connection string in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=rv_personal_finance;Username=rv_admin;Password=your_password_here"
  }
}
```

5. Apply migrations:

```bash
dotnet ef database update --project src/RvPersonalFinance.Api
```

6. Run the application:

```bash
dotnet run --project src/RvPersonalFinance.Api
```

7. Open Swagger UI:

```
http://localhost:5000/swagger
```

---

## 🗺 Roadmap

- [x] Solution and project structure
- [x] Domain entities (`User`, `Account`, `Category`, `Transaction`)
- [x] Git repository with organized `.gitignore`
- [ ] Docker + PostgreSQL setup
- [ ] Entity Framework Core configuration
- [ ] First database migration
- [ ] Swagger configured
- [ ] Health check endpoint
- [ ] DTOs and service layer
- [ ] CRUD for all entities
- [ ] Balance calculation
- [ ] JWT authentication
- [ ] User ownership enforcement
- [ ] Unit tests
- [ ] Integration tests
- [ ] Angular frontend
- [ ] Full Docker Compose (API + PostgreSQL + Frontend)
- [ ] Production deploy

---

## 📄 License

This project is for educational purposes.
