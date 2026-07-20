using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/InventarioDetallLin.asp.
/// Procedures: asp.PROC_InventarioLin_Select.
/// P2 = fecha del inventario, P3 = producto (en el ASP @Prod iba sin comillas: numérico,
/// con default NULL que se conserva).
/// </summary>
public class InventarioDetallLinModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }

    public List<dynamic> Lineas { get; private set; } = [];

    private long? Prod => long.TryParse(P3, out var p) ? p : null;

    public async Task OnGetAsync()
    {
        Lineas = await Db.QueryProcAsync("asp.PROC_InventarioLin_Select",
            new { Empresa, Fecha = P2, Prod, Alm });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_InventarioLin_Select",
            new { Empresa, Fecha = P2, Prod, Alm });
        return Excel(rows, $"InventarioDetalle_{Prod}");
    }
}
