using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/PedidosCentralLin.asp.
/// Procedure: asp.PROC_PedidoCentralLin_Select (@Empresa, @Fecha, @Pedido).
/// P2 = fecha, P3 = pedido, P4 = filtro de vista (3 = solo líneas no preparadas/diferencias).
/// </summary>
public class PedidosCentralLinModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P4 { get; set; }

    public List<dynamic> Lineas { get; private set; } = [];

    /// <summary>Filtro de vista: &lt;2 muestra todo, &gt;=2 solo líneas con diferencias.</summary>
    public double Filtro => double.TryParse(P4, out var f) ? f : 0;

    private string? Fecha => string.IsNullOrEmpty(P2) ? null : P2;
    private long? Pedido => long.TryParse(P3, out var p) ? p : null;

    public async Task OnGetAsync()
    {
        Lineas = await Db.QueryProcAsync("asp.PROC_PedidoCentralLin_Select",
            new { Empresa, Fecha, Pedido });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_PedidoCentralLin_Select",
            new { Empresa, Fecha, Pedido });
        return Excel(rows, "PedidosCentralLin");
    }
}
