using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/Informe Cierre.asp.
/// Procedures: rep.Seccion, rep.TeffFirma, rep.CambiosPvp_Varios (todos @Alm, @Fecha).
/// Parámetro de URL P2 (fecha de cierre, por defecto hoy) se conserva.
/// </summary>
public class InformeCierreModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }

    public List<dynamic> Secciones { get; private set; } = [];
    public List<dynamic> TeffFirmas { get; private set; } = [];
    public List<dynamic> Cambios { get; private set; } = [];

    /// <summary>En origen: P2 o cstr(Date) (fecha de hoy).</summary>
    public string Fecha => string.IsNullOrEmpty(P2) ? DateTime.Now.ToString("dd/MM/yyyy") : P2!;

    /// <summary>Antes DoDateTime(x, 4, 1033): hora corta.</summary>
    public string Hora(object? v) =>
        v is null ? "" : Convert.ToDateTime(v).ToString("HH:mm");

    public async Task OnGetAsync()
    {
        Secciones = await Db.QueryProcMadisaAsync("rep.Seccion", new { Alm, Fecha });
        TeffFirmas = await Db.QueryProcMadisaAsync("rep.TeffFirma", new { Alm, Fecha });
        Cambios = await Db.QueryProcMadisaAsync("rep.CambiosPvp_Varios", new { Alm, Fecha });
    }
}
