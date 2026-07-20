using System.Globalization;
using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/MovProducto.asp.
/// Procedure: asp.Info_Producto_MovProducto (@Empresa, @Prod, @Alm).
/// Parámetros de URL conservados: P1 (producto, en origen default NULL), P2 (almacén, default el de la sesión),
/// P3 (stock), P4 (stock mínimo), P5 (stock máximo), P6 (descripción del producto).
/// </summary>
public class MovProductoModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P1 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P4 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P5 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P6 { get; set; }

    public List<dynamic> Movimientos { get; private set; } = [];

    /// <summary>En el ASP original P1 vacío se pasaba como NULL al procedure.</summary>
    private long? Prod => long.TryParse(P1, out var p) ? p : null;

    /// <summary>P2 (almacén consultado); si no viene, el almacén de la sesión (antes cookie ALMACEN).</summary>
    public int AlmMov => int.TryParse(P2, out var a) ? a : Alm;

    /// <summary>Equivalente al CDBL del ASP sobre valores que llegan por querystring.</summary>
    public static decimal Dec(string? s) =>
        decimal.TryParse(s?.Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0;

    public async Task OnGetAsync()
    {
        Movimientos = await Db.QueryProcAsync("asp.Info_Producto_MovProducto",
            new { Empresa, Prod, Alm = AlmMov });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.Info_Producto_MovProducto",
            new { Empresa, Prod, Alm = AlmMov });
        return Excel(rows, $"MovProducto_{P1}");
    }
}
