using System.Globalization;
using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/InventarioCab.asp.
/// Procedures: asp.PROC_MovInventarioCabProg_Select_Alm.
/// P2 = fecha tope (FechaTop) para paginar: el enlace "Siguiente Pagina" pasa la última fecha mostrada.
/// </summary>
public class InventarioCabModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }

    public List<dynamic> Inventarios { get; private set; } = [];
    public string UltimaFecha { get; private set; } = "";

    private string? FechaTop => string.IsNullOrEmpty(P2) ? null : P2;

    public string Moneda(object? v) =>
        Convert.ToDecimal(v ?? 0).ToString("C2", CultureInfo.GetCultureInfo("es-ES"));

    public async Task OnGetAsync()
    {
        Inventarios = await Db.QueryProcAsync("asp.PROC_MovInventarioCabProg_Select_Alm",
            new { Empresa, Alm, FechaTop });

        if (Inventarios.Count > 0)
        {
            var ultima = (IDictionary<string, object>)Inventarios[^1];
            UltimaFecha = $"{ultima["Fecha"]}";
        }
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_MovInventarioCabProg_Select_Alm",
            new { Empresa, Alm, FechaTop });
        return Excel(rows, "Inventarios");
    }
}
