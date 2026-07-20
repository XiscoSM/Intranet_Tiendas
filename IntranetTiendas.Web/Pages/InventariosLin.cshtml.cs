using System.Globalization;
using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/InventariosLin.asp.
/// Procedures: asp.PROC_MovInventarioLin_Select.
/// P2 = fecha del inventario, P3 = programa (@Prog numérico, default NULL conservado),
/// P4 = diferencia mínima: filtro que en el ASP se aplicaba fila a fila al pintar
/// (ABS(ImporteCosteDif) &gt;= P4); aquí se filtra en memoria igual que el original.
/// </summary>
public class InventariosLinModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P4 { get; set; }

    public List<dynamic> Lineas { get; private set; } = [];
    public decimal Total1 { get; private set; }
    public decimal Total2 { get; private set; }

    private long? Prog => long.TryParse(P3, out var p) ? p : null;
    private decimal DifMin => decimal.TryParse(P4, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0;

    public string Moneda(object? v) =>
        Convert.ToDecimal(v ?? 0).ToString("C2", CultureInfo.GetCultureInfo("es-ES"));

    public async Task OnGetAsync() => Lineas = await CargarAsync();

    private async Task<List<dynamic>> CargarAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_MovInventarioLin_Select",
            new { Empresa, Fecha = P2, Prog, Alm });

        var filtradas = rows
            .Where(r => Math.Abs(Convert.ToDecimal(((IDictionary<string, object>)r)["ImporteCosteDif"])) >= DifMin)
            .ToList();

        Total1 = filtradas.Sum(r => Convert.ToDecimal(((IDictionary<string, object>)r)["ImporteCosteFisico"]));
        Total2 = filtradas.Sum(r => Convert.ToDecimal(((IDictionary<string, object>)r)["ImporteCosteDif"]));
        return filtradas;
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync() =>
        Excel(await CargarAsync(), $"Inventario_{P2?.Replace("/", "-")}");
}
