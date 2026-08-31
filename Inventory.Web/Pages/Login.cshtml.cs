using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Inventory.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventory.Web.Pages;

/// <summary>
/// Modelo de página para autenticación de usuario.
/// Valida credenciales contra la API y genera la cookie de sesión con el token JWT.
/// </summary>
[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IInventoryApiService _apiService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(IInventoryApiService apiService, ILogger<LoginModel> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [TempData]
    public string? StatusTitle { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? StatusType { get; set; }

    public string? ReturnUrl { get; set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            StatusTitle = "Advertencia";
            StatusMessage = "Por favor, completa los campos requeridos.";
            StatusType = "warning";
            return Page();
        }

        var (success, token, errorMessage) = await _apiService.LoginAsync(Input.Username, Input.Password);

        if (!success || string.IsNullOrWhiteSpace(token))
        {
            StatusTitle = "Error de Autenticación";
            StatusMessage = !string.IsNullOrWhiteSpace(errorMessage)
                ? errorMessage
                : "Credenciales inválidas. Usa admin / admin123 para acceder.";
            StatusType = "error";
            return Page();
        }

        // Crear Claims del usuario e incluir el token JWT de la API
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, Input.Username),
            new(ClaimTypes.Role, "Administrator"),
            new("JwtToken", token)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = Input.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        _logger.LogInformation("Usuario '{Username}' inició sesión correctamente.", Input.Username);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage("/Index");
    }
}

public class LoginInput
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = "admin";

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "admin123";

    [Display(Name = "Recordar sesión")]
    public bool RememberMe { get; set; } = true;
}
