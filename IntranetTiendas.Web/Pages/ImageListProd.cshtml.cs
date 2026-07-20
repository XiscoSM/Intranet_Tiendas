using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>Migración de ImageListProd.asp (galería de imágenes de un producto). Procedure: asp.ImagenList.</summary>
public class ImageListProdModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? Prod { get; set; }

    public List<dynamic> Imagenes { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Imagenes = await Db.QueryProcAsync("asp.ImagenList",
            new { Prod = long.TryParse(Prod, out var p) ? p : 0 });
    }
}
