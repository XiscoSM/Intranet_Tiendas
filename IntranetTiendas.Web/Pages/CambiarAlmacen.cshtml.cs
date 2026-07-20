using System.Security.Claims;
using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Selector de tienda/almacén. Solo operativo si RW_PermiteCambioAlm.
/// El almacén seleccionado se guarda re-emitiendo la cookie de sesión firmada
/// (claim "Alm"/"DescAlm"), no en una cookie manipulable. Se valida que el almacén
/// elegido pertenezca a la lista real de la empresa (asp.Almacen).
/// </summary>
public class CambiarAlmacenModel(DbService db) : BasePageModel(db)
{
    public List<dynamic> Almacenes { get; private set; } = [];

    public async Task OnGetAsync()
    {
        if (PermiteCambioAlm)
            Almacenes = await Db.AlmacenesAsync();
    }

    public async Task<IActionResult> OnPostAsync(int nuevoAlm)
    {
        if (!PermiteCambioAlm)
            return RedirectToPage("/Index");

        var almacenes = await Db.AlmacenesAsync();
        var valido = almacenes.Any(a => Convert.ToInt32(((IDictionary<string, object>)a)["Alm"]) == nuevoAlm);
        if (!valido)
        {
            Almacenes = almacenes;
            return Page();
        }

        var descAlm = await Db.DescAlmacenAsync(nuevoAlm);

        // Re-emitir la sesión con el nuevo almacén, conservando el resto de claims
        var claims = User.Claims
            .Where(c => c.Type is not ("Alm" or "DescAlm"))
            .Append(new Claim("Alm", nuevoAlm.ToString()))
            .Append(new Claim("DescAlm", descAlm))
            .ToList();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies"));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });

        return RedirectToPage("/Index");
    }
}
