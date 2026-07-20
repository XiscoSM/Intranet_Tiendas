using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/Informe Balanzas.asp.
/// Procedures: rep.Balanzas (@Alm, @Fecha).
/// Parámetro de URL P2 (fecha de cierre, por defecto hoy) se conserva.
/// </summary>
public class InformeBalanzasModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }

    public List<dynamic> Balanzas { get; private set; } = [];

    /// <summary>En origen: P2 o cstr(Date) (fecha de hoy).</summary>
    public string Fecha => string.IsNullOrEmpty(P2) ? DateTime.Now.ToString("dd/MM/yyyy") : P2!;

    public async Task OnGetAsync()
    {
        Balanzas = await Db.QueryProcMadisaAsync("rep.Balanzas", new { Alm, Fecha });
    }
}
