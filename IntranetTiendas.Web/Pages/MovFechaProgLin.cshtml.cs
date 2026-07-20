using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/MovFechaProgLin.asp (Detalle de Movimientos por Fecha y Sección).
/// Procedure: asp.PROC_MovFechaProg_Select (@Empresa, @Fecha, @Alm, @Prog).
/// Parámetros de URL conservados: P2 (fecha, default hoy) y P3 (sección/programa, default 0).
/// Los V1/V2 de empresa/almacén del origen se eliminan (van por sesión).
/// </summary>
public class MovFechaProgLinModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }

    public List<dynamic> Movimientos { get; private set; } = [];

    private long Prog => long.TryParse(P3, out var p) ? p : 0;

    public async Task OnGetAsync()
    {
        P2 = string.IsNullOrEmpty(P2) ? DateTime.Now.ToString("dd/MM/yyyy") : P2;
        P3 = string.IsNullOrEmpty(P3) ? "0" : P3;

        Movimientos = await Db.QueryProcAsync("asp.PROC_MovFechaProg_Select",
            new { Empresa, Fecha = P2, Alm, Prog });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var fecha = string.IsNullOrEmpty(P2) ? DateTime.Now.ToString("dd/MM/yyyy") : P2;
        var rows = await Db.QueryProcAsync("asp.PROC_MovFechaProg_Select",
            new { Empresa, Fecha = fecha, Alm, Prog });
        return Excel(rows, "MovFechaProgLin");
    }
}
