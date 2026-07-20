using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/InfoEtiquetas_Detall.asp (detalle de etiquetas por agrupación).
/// Procedure: asp.InfoEtiquetas_Detall (@Alm, @Fecha) — en origen no recibía @Empresa.
/// Parámetro de URL conservado: P1 (fecha, default 01/01/2000).
/// </summary>
public class InfoEtiquetasDetallModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P1 { get; set; }

    public List<dynamic> Detalle { get; private set; } = [];

    private string Fecha => string.IsNullOrEmpty(P1) ? "01/01/2000" : P1;

    public async Task OnGetAsync()
    {
        P1 = Fecha;
        Detalle = await Db.QueryProcAsync("asp.InfoEtiquetas_Detall", new { Alm, Fecha });
    }

    /// <summary>Exportación a Excel (el origen aceptaba Formato=Excel aunque no tenía icono).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.InfoEtiquetas_Detall", new { Alm, Fecha });
        return Excel(rows, "InfoEtiquetasDetall");
    }
}
