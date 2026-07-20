using System.Security.Claims;

namespace IntranetTiendas.Web.Services;

/// <summary>
/// Autenticación contra el procedure de validación de RetailWare:
/// asp.PROC_Usuario_Select_R3_HM1 @RW_Usuario INT, @RW_Pwd INT.
/// El propio procedure valida la contraseña (no se hashea ni se compara en código).
/// Devuelve Error (bit: false = OK), DescError, y los datos del usuario:
///   RW_DescUsuario  -> nombre mostrado
///   RW_Almacen      -> almacén por defecto de la sesión (smallint)
///   RW_PermiteCambioAlm -> si puede elegir otra tienda/almacén
/// Todas las consultas se ejecutan siempre con User_For_IntranetTienda (appsettings).
/// </summary>
public class AuthService(DbService db)
{
    public record LoginResult(bool Ok, string? Error, ClaimsPrincipal? Principal);

    public async Task<LoginResult> ValidarAsync(string? usuario, string? password)
    {
        usuario = usuario?.Trim();
        password = password?.Trim();

        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            return new(false, "Introduce usuario y contraseña.", null);

        // El procedure recibe INT: descartamos no numéricos y desbordamientos sin que reviente.
        if (!int.TryParse(usuario, out var rwUsuario) || !int.TryParse(password, out var rwPwd))
            return new(false, "Usuario y contraseña deben ser numéricos.", null);

        var row = await db.QueryFirstOrDefaultProcAsync("asp.PROC_Usuario_Select_R3_HM1",
            new { RW_Usuario = rwUsuario, RW_Pwd = rwPwd });

        if (row is null)
            return new(false, "Usuario o contraseña incorrectos.", null);

        var d = (IDictionary<string, object>)row;

        // Error es bit: true => fallo de validación (DescError trae el motivo)
        if (!d.TryGetValue("Error", out var err) || Convert.ToBoolean(err ?? true))
        {
            var motivo = d.TryGetValue("DescError", out var de) ? de?.ToString() : null;
            return new(false, string.IsNullOrWhiteSpace(motivo) ? "Usuario o contraseña incorrectos." : motivo, null);
        }

        // En el camino OK, RW_Almacen no debería venir null; si lo hiciera, no concedemos sesión inválida.
        if (!d.TryGetValue("RW_Almacen", out var almVal) || almVal is null)
            return new(false, "El usuario no tiene un almacén asignado.", null);

        var alm = Convert.ToInt32(almVal);
        var nombre = (d.TryGetValue("RW_DescUsuario", out var nom) ? nom?.ToString() : null) ?? usuario;
        var permiteCambio = d.TryGetValue("RW_PermiteCambioAlm", out var pc) && pc is not null && Convert.ToBoolean(pc);

        var descAlm = await db.DescAlmacenAsync(alm);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, nombre),
            new("Usuario", rwUsuario.ToString()),
            new("Alm", alm.ToString()),
            new("DescAlm", descAlm),
            new("PermiteCambioAlm", permiteCambio ? "1" : "0")
        };
        var identity = new ClaimsIdentity(claims, "Cookies");
        return new(true, null, new ClaimsPrincipal(identity));
    }
}
