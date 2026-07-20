using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IntranetTiendas.Web.Pages;

[AllowAnonymous]
public class LoginModel(AuthService auth) : PageModel
{
    [BindProperty] public string? Usuario { get; set; }
    [BindProperty] public string? Password { get; set; }
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        var result = await auth.ValidarAsync(Usuario, Password);
        if (!result.Ok)
        {
            Error = result.Error;
            return Page();
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal!,
            new AuthenticationProperties { IsPersistent = true });

        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    public async Task<IActionResult> OnGetLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Login");
    }
}
