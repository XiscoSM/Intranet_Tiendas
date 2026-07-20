using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/ComprasLin.asp (detalle de un albarán de compra).
/// Procedures: asp.PROC_MovCompraLin_Select (líneas), asp.PROC_MovCompraCab (cabecera).
/// Parámetros de URL P2 (fecha) y P3 (albarán) se conservan para mantener los enlaces desde ComprasCab.
/// </summary>
public class ComprasLinModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }

    public List<dynamic> Lineas { get; private set; } = [];
    public dynamic? Cab { get; private set; }

    private string? Fecha => string.IsNullOrEmpty(P2) ? null : P2;
    private long? Albaran => long.TryParse(P3, out var a) ? a : null;

    public async Task OnGetAsync()
    {
        Lineas = await Db.QueryProcAsync("asp.PROC_MovCompraLin_Select",
            new { Empresa, Fecha, Albaran });

        Cab = await Db.QueryFirstOrDefaultProcAsync("asp.PROC_MovCompraCab",
            new { Empresa, Fecha, Albaran });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_MovCompraLin_Select",
            new { Empresa, Fecha, Albaran });
        return Excel(rows, $"Compra_{P3}");
    }
}
