
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CustodialTaskList.Pages.Admin;

public class LoginModel : PageModel
{

    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    private readonly CustodialTaskList.Security
        .SupervisorPasswordService _passwordService;

    public LoginModel(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        CustodialTaskList.Security
            .SupervisorPasswordService passwordService)
    {
        _configuration = configuration;
        _environment = environment;
        _passwordService = passwordService;
    }


    [BindProperty]
    [Required]
    public string Password { get; set; } = "";

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();


        string? storedHash =
            _configuration["Supervisor:PasswordHash"];

        bool passwordValid = false;

        if (!string.IsNullOrWhiteSpace(storedHash))
        {
            // Production-style password verification.
            passwordValid = _passwordService.VerifyPassword(
                Password, storedHash);
        }
        else if (_environment.IsDevelopment())
        {
            // Temporary compatibility with our
            // existing Visual Studio User Secrets.
            string? developmentPassword =
                _configuration["Supervisor:Password"];

            if (!string.IsNullOrEmpty(developmentPassword))
            {
                byte[] enteredHash = SHA256.HashData(
                    Encoding.UTF8.GetBytes(Password));

                byte[] savedHash = SHA256.HashData(
                    Encoding.UTF8.GetBytes(developmentPassword));

                passwordValid =
                    CryptographicOperations.FixedTimeEquals(
                        enteredHash, savedHash);
            }
        }

        if (!passwordValid)
        {
            ModelState.AddModelError("",
                "Incorrect password or supervisor password not configured.");

            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "Supervisor"),
            new Claim(ClaimTypes.Role, "Supervisor")
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return RedirectToPage("/Admin/Index");
    }
}
