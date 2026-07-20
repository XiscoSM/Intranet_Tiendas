using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/PedidosCentralLinPreparacion.asp (hoja de preparación imprimible).
/// Procedures: asp.PROC_PedidoCentralLinPrep_Select (@Empresa, @Fecha, @Pedido) — líneas,
/// asp.PROC_PedidoCentralCab_Select_Ped (@Empresa, @Fecha, @Pedido) — cabecera origen/destino.
/// P2 = fecha, P3 = pedido.
/// </summary>
public class PedidosCentralLinPreparacionModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }

    public dynamic? Cab { get; private set; }
    public List<dynamic> Lineas { get; private set; } = [];

    private string? Fecha => string.IsNullOrEmpty(P2) ? null : P2;
    private long? Pedido => long.TryParse(P3, out var p) ? p : null;

    public async Task OnGetAsync()
    {
        Lineas = await Db.QueryProcAsync("asp.PROC_PedidoCentralLinPrep_Select",
            new { Empresa, Fecha, Pedido });
        Cab = await Db.QueryFirstOrDefaultProcAsync("asp.PROC_PedidoCentralCab_Select_Ped",
            new { Empresa, Fecha, Pedido });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_PedidoCentralLinPrep_Select",
            new { Empresa, Fecha, Pedido });
        return Excel(rows, "PedidosCentralLinPreparacion");
    }
}
