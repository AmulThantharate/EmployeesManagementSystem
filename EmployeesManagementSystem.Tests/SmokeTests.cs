using System.Net;
using System.Text.RegularExpressions;
using EmployeesManagementSystem.Data;
using EmployeesManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeesManagementSystem.Tests;

public sealed partial class SmokeTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SmokeTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PublicShell_RendersDashboardNavigationAndIdentityEntrypoints()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Employees Management System", content);
        Assert.Contains("Welcome to EMS Dashboard", content);
        Assert.Contains("Total Employees", content);
        Assert.Contains("Recent Activity", content);
        Assert.Contains("href=\"/\"", content);
        Assert.Contains("href=\"/Home/Privacy\"", content);
        Assert.Contains("href=\"/Identity/Account/Register\"", content);
        Assert.Contains("href=\"/Identity/Account/Login\"", content);
        Assert.DoesNotContain("href=\"/Employees\"", content);
    }

    [Fact]
    public async Task PublicPages_LoadSuccessfully()
    {
        var privacyResponse = await _client.GetAsync("/Home/Privacy");
        var loginResponse = await _client.GetAsync("/Identity/Account/Login");
        var registerResponse = await _client.GetAsync("/Identity/Account/Register");

        Assert.Equal(HttpStatusCode.OK, privacyResponse.StatusCode);
        Assert.Contains("Privacy Policy", await privacyResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Contains("Log in", await loginResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        Assert.Contains("Create a new account", await registerResponse.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task AnonymousDashboard_AddEmployeeCallToActionPointsToLogin()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Add Employee", content);
        Assert.Contains("href=\"/Identity/Account/Login\"", content);
    }

    [Fact]
    public async Task Registration_CreatesIdentityUserAndShowsConfirmation()
    {
        var response = await PostFormAsync("/Identity/Account/Register", new Dictionary<string, string>
        {
            ["Input.Email"] = "smoke.user@example.com",
            ["Input.Password"] = "SmokeTest123!",
            ["Input.ConfirmPassword"] = "SmokeTest123!"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Identity/Account/RegisterConfirmation", response.Headers.Location?.OriginalString);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(await db.Users.AnyAsync(user => user.Email == "smoke.user@example.com"));
    }

    [Fact]
    public async Task EmployeesIndex_DisplaysSeededEmployeeRecordsAndCrudActions()
    {
        await SignInAsync();
        var employee = await SeedEmployeeAsync(
            firstName: "Ada",
            lastName: "Lovelace",
            position: "Engineer",
            emailAddress: "ada@example.com");

        var response = await _client.GetAsync("/Employees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Add New Employee", content);
        Assert.Contains("First Name", content);
        Assert.Contains("Email Address", content);
        Assert.Contains("Ada", content);
        Assert.Contains("Lovelace", content);
        Assert.Contains($"/Employees/Edit/{employee.Id}", content);
        Assert.Contains($"/Employees/Details/{employee.Id}", content);
        Assert.Contains($"/Employees/Delete/{employee.Id}", content);
    }

    [Fact]
    public async Task CreateEmployee_FormPersistsRecordAndRedirectsToList()
    {
        var userId = await SignInAsync();
        var getResponse = await _client.GetAsync("/Employees/Create");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var form = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("Add New Employee", form);
        Assert.Contains("name=\"FirstName\"", form);

        var postResponse = await PostFormAsync("/Employees/Create", ValidEmployeeForm(
            firstName: "Grace",
            lastName: "Hopper",
            position: "Developer",
            emailAddress: "grace@example.com"));

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal("/Employees", postResponse.Headers.Location?.OriginalString);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var created = await db.Employees.SingleOrDefaultAsync(employee => employee.EmailAddress == "grace@example.com");

        Assert.NotNull(created);
        Assert.Equal("Grace", created!.FirstName);
        Assert.Equal("Hopper", created.LastName);
        Assert.Equal(userId, created.CreatedById);
    }

    [Fact]
    public async Task EmployeeDetails_RendersSelectedRecord()
    {
        await SignInAsync();
        var employee = await SeedEmployeeAsync(
            firstName: "Katherine",
            lastName: "Johnson",
            position: "Mathematician",
            emailAddress: "katherine@example.com");

        var response = await _client.GetAsync($"/Employees/Details/{employee.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Details", content);
        Assert.Contains("Katherine", content);
        Assert.Contains("Johnson", content);
        Assert.Contains("Mathematician", content);
        Assert.Contains("katherine@example.com", content);
        Assert.Contains($"/Employees/Edit/{employee.Id}", content);
        Assert.Contains("href=\"/Employees\"", content);
    }

    [Fact]
    public async Task EditEmployee_FormUpdatesExistingRecordAndRedirectsToList()
    {
        await SignInAsync();
        var employee = await SeedEmployeeAsync(
            firstName: "Alan",
            lastName: "Turing",
            position: "Researcher",
            emailAddress: "alan@example.com");

        var getResponse = await _client.GetAsync($"/Employees/Edit/{employee.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var form = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("Update Employee Details", form);
        Assert.Contains("value=\"Alan\"", form);

        var postResponse = await PostFormAsync($"/Employees/Edit/{employee.Id}", ValidEmployeeForm(
            id: employee.Id,
            firstName: "Alan",
            lastName: "Turing",
            position: "Mathematician",
            emailAddress: "alan@example.com",
            createdById: employee.CreatedById,
            createdAt: employee.CreatedAt));

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal("/Employees", postResponse.Headers.Location?.OriginalString);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updated = await db.Employees.SingleAsync(record => record.Id == employee.Id);

        Assert.Equal("Mathematician", updated.Position);
    }

    [Fact]
    public async Task DeleteEmployee_ConfirmationPageRemovesRecordAndRedirectsToList()
    {
        await SignInAsync();
        var employee = await SeedEmployeeAsync(
            firstName: "Margaret",
            lastName: "Hamilton",
            position: "Developer",
            emailAddress: "margaret@example.com");

        var getResponse = await _client.GetAsync($"/Employees/Delete/{employee.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var confirmation = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("Are you sure you want to delete this?", confirmation);
        Assert.Contains("Margaret", confirmation);

        var postResponse = await PostFormAsync($"/Employees/Delete/{employee.Id}", new Dictionary<string, string>
        {
            ["Id"] = employee.Id.ToString()
        });

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal("/Employees", postResponse.Headers.Location?.OriginalString);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.False(await db.Employees.AnyAsync(record => record.Id == employee.Id));
    }

    [Theory]
    [InlineData("/Employees/Details/999")]
    [InlineData("/Employees/Edit/999")]
    [InlineData("/Employees/Delete/999")]
    public async Task EmployeeRecordPages_ReturnNotFoundForMissingRecords(string url)
    {
        await SignInAsync();
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<HttpResponseMessage> PostFormAsync(string url, Dictionary<string, string> formData)
    {
        var formResponse = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, formResponse.StatusCode);

        var formHtml = await formResponse.Content.ReadAsStringAsync();
        formData["__RequestVerificationToken"] = ExtractAntiForgeryToken(formHtml);

        return await _client.PostAsync(url, new FormUrlEncodedContent(formData));
    }

    private async Task<Employee> SeedEmployeeAsync(
        string firstName,
        string lastName,
        string position,
        string emailAddress)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var employee = new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Position = position,
            DateOfBirth = new DateTime(1990, 1, 15),
            Country = "United States",
            EmailAddress = emailAddress,
            Address = "100 Main Street",
            City = "Seattle",
            PostalCode = "98101",
            PhoneNumber = "2065550100",
            CreatedAt = DateTime.UtcNow,
            CreatedById = "smoke-test-user"
        };

        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        return employee;
    }

    private async Task<string> SignInAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = new IdentityUser
        {
            UserName = "employee.smoke@example.com",
            Email = "employee.smoke@example.com",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, "SmokeTest123!");
        Assert.True(createResult.Succeeded, string.Join("; ", createResult.Errors.Select(error => error.Description)));

        var loginResponse = await PostFormAsync("/Identity/Account/Login", new Dictionary<string, string>
        {
            ["Input.Email"] = user.Email,
            ["Input.Password"] = "SmokeTest123!",
            ["Input.RememberMe"] = "false"
        });

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        return user.Id;
    }

    private static Dictionary<string, string> ValidEmployeeForm(
        string firstName,
        string lastName,
        string position,
        string emailAddress,
        int? id = null,
        string createdById = "smoke-test-user",
        DateTime? createdAt = null)
    {
        var form = new Dictionary<string, string>
        {
            ["FirstName"] = firstName,
            ["LastName"] = lastName,
            ["Position"] = position,
            ["DateOfBirth"] = "1990-01-15",
            ["Country"] = "United States",
            ["EmailAddress"] = emailAddress,
            ["Address"] = "100 Main Street",
            ["City"] = "Seattle",
            ["PostalCode"] = "98101",
            ["PhoneNumber"] = "2065550100"
        };

        if (id is not null)
        {
            form["Id"] = id.Value.ToString();
            form["CreatedById"] = createdById;
            form["CreatedAt"] = (createdAt ?? DateTime.UtcNow).ToString("O");
        }

        return form;
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        var tokenInput = AntiForgeryInputRegex().Match(html);
        Assert.True(tokenInput.Success, "The form did not render an anti-forgery token input.");

        var tokenValue = InputValueRegex().Match(tokenInput.Value);
        Assert.True(tokenValue.Success, "The anti-forgery token input did not include a value.");

        return WebUtility.HtmlDecode(tokenValue.Groups["value"].Value);
    }

    [GeneratedRegex("<input[^>]*name=\"__RequestVerificationToken\"[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex AntiForgeryInputRegex();

    [GeneratedRegex("value=\"(?<value>[^\"]+)\"", RegexOptions.IgnoreCase)]
    private static partial Regex InputValueRegex();
}
