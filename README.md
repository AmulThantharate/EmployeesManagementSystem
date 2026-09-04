# 🏢 Employees Management System

🚀 **Project Overview**

Employees Management System is a robust web application built with ASP.NET Core MVC to manage employee records. It provides a comprehensive interface for creating, reading, updating, and deleting (CRUD) employee information, while ensuring secure access through built-in user authentication. The project demonstrates modern .NET development practices including Entity Framework Core with SQL Server, containerization via Docker, and automated testing.

## ✨ Features

- **Employee Management:** Full CRUD capabilities for employee profiles (Name, Position, Contact Info, Address, etc.).
- **User Authentication:** Secure login and registration powered by ASP.NET Core Identity.
- **Database Migrations:** Automatic database migrations executed safely on application startup.
- **Container Ready:** Fully containerized setup using Docker and Docker Compose for easy deployment.
- **Continuous Integration:** Automated smoke and integration tests run on GitHub Actions.
- **Responsive UI:** Built using standard ASP.NET Core MVC views (Razor) and styled for all devices.

## 🛠️ Tech Stack

- **Framework:** .NET 10.0 / ASP.NET Core MVC
- **Data Access:** Entity Framework Core 10.0
- **Database:** Microsoft SQL Server (Containerized)
- **Authentication:** ASP.NET Core Identity
- **Testing:** xUnit, `Microsoft.AspNetCore.Mvc.Testing`
- **Containerization:** Docker & Docker Compose
- **CI/CD:** GitHub Actions

## 📁 Project Structure

```text
EmployeesManagementSystem/
├── .github/workflows/          # GitHub Actions CI pipelines
├── Areas/Identity/             # ASP.NET Core Identity pages for Auth
├── Controllers/                # MVC Controllers (e.g., EmployeesController)
├── Data/                       # Entity Framework DbContext and Migrations
├── Models/                     # Domain Entities (e.g., Employee)
├── Views/                      # Razor Views for the UI
├── wwwroot/                    # Static assets (CSS, JS, images)
├── EmployeesManagementSystem.Tests/ # xUnit test project
├── Dockerfile                  # Application Docker image configuration
├── docker-compose.yml          # Multi-container orchestration (App + SQL Server)
├── EmployeesManagementSystem.slnx # .NET Solution File
└── Program.cs                  # Application entry point and service registration
```

## ⚙️ Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (Optional, for containerized database and app execution)
- IDE (Visual Studio 2022, JetBrains Rider, or VS Code)

## 📦 Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/AmulThantharate/EmployeesManagementSystem.git
   cd EmployeesManagementSystem
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore EmployeesManagementSystem.slnx
   ```

## 🔧 Configuration

### Using Docker (Recommended)

You can spin up the entire application along with the SQL Server database using Docker Compose:

```bash
docker-compose up --build
```

The application will be accessible at `http://localhost:8080`.

### Running Locally (Without Docker)

1. Update the `DefaultConnection` string in `appsettings.Development.json` to point to your local SQL Server instance.
2. Run the application:
   ```bash
   dotnet run --project EmployeesManagementSystem.csproj
   ```
*Note: The application is configured to automatically apply pending database migrations on startup.*

## 🔐 Environment Variables

When running via Docker Compose, the following environment variables are utilized (found in `docker-compose.yml`):

- `ASPNETCORE_ENVIRONMENT`: Set to `Development` by default in docker-compose.
- `ConnectionStrings__DefaultConnection`: The SQL Server connection string.
- `MSSQL_SA_PASSWORD`: The strong password used for the SQL Server SA account.

## 🧪 Running Smoke Tests

Smoke tests ensure that the core components of the application are functioning correctly. They are located in the `EmployeesManagementSystem.Tests` project.

To run the smoke tests:
```bash
dotnet test EmployeesManagementSystem.Tests/EmployeesManagementSystem.Tests.csproj
```

## ✅ Running All Tests

To execute all unit and integration tests across the entire solution:

```bash
dotnet test EmployeesManagementSystem.slnx
```

## 📊 Test Reports

Tests are integrated into the GitHub Actions CI pipeline (`.github/workflows/dotnet-smoke-tests.yml`). Every push or pull request to the `main` or `master` branch will automatically trigger the test suite, and the results can be viewed in the "Actions" tab of the GitHub repository.

## 📚 Best Practices Followed

- **Separation of Concerns:** Clear MVC architecture separating logic, data, and presentation.
- **Dependency Injection:** Extensive use of built-in IoC container for services and DbContext.
- **Secure Defaults:** Identity authentication requiring confirmed accounts.
- **Infrastructure as Code:** Docker compose encapsulates dependencies (SQL Server) for frictionless onboarding.

## 👨‍💻 Author

**Amul Thantharate** - [GitHub Profile](https://github.com/AmulThantharate)

## 🙏 Acknowledgements

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Docker](https://www.docker.com/)
