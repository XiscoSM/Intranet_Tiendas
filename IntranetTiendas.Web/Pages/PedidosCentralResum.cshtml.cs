using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/PedidosCentral_Resum.asp (resumen de pedidos por almacén central).
/// Procedure: asp.PedidoCentral_Central_Resum_Select (@Empresa, @Fecha, @AlmCentral, @Estado).
/// P2 = fecha, P3 = estado (0 Abiertos, 1 Cerrados, 2 En Preparación, 3 Terminados),
/// P4 = filtro de vista (3 = solo líneas con diferencias).
/// @AlmCentral era la cookie ALMACEN: ahora Alm de la sesión.
/// </summary>
public class PedidosCentralResumModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P4 { get; set; }

    public List<dynamic> Lineas { get; private set; } = [];

    /// <summary>Filtro de vista: &lt;2 muestra todo, &gt;=2 solo líneas con diferencias.</summary>
    public double Filtro => double.TryParse(P4, out var f) ? f : 0;

    /// <summary>Estado del pedido (P3): 0 Abiertos, 1 Cerrados, 2 En Preparación, 3 Terminados.</summary>
    public long Estado => long.TryParse(P3, out var e) ? e : 0;

    private string? Fecha => string.IsNullOrEmpty(P2) ? null : P2;

    public async Task OnGetAsync()
    {
        Lineas = await Db.QueryProcAsync("asp.PedidoCentral_Central_Resum_Select",
            new { Empresa, Fecha, AlmCentral = Alm, Estado });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PedidoCentral_Central_Resum_Select",
            new { Empresa, Fecha, AlmCentral = Alm, Estado });
        return Excel(rows, "PedidosCentralResum");
    }
}
