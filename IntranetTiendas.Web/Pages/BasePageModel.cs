using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Base de todas las páginas: expone Empresa (config), Almacén y su descripción (claims de la sesión)
/// y el servicio de datos. Sustituye a las cookies EMPRESA/ALMACEN del ASP clásico.
/// </summary>
public abstract class BasePageModel(DbService db) : PageModel
{
    public DbService Db { get; } = db;

    public string Empresa => Db.Empresa;

    public int Alm => int.TryParse(User.FindFirst("Alm")?.Value, out var a) ? a : 0;

    public string DescAlm => User.FindFirst("DescAlm")?.Value ?? "";

    public string Usuario => User.Identity?.Name ?? "";

    /// <summary>RW_PermiteCambioAlm: si el usuario puede elegir otra tienda/almacén.</summary>
    public bool PermiteCambioAlm => User.FindFirst("PermiteCambioAlm")?.Value == "1";

    /// <summary>FileResult de Excel listo para devolver desde un handler OnGetExcel.</summary>
    protected FileContentResult Excel(IEnumerable<dynamic> rows, string nombre) =>
        File(ExcelExport.ToXlsx(rows, nombre),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            nombre + ".xlsx");
}
