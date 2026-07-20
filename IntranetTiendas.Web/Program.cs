using System.Globalization;
using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    // Toda la aplicación requiere sesión iniciada salvo lo marcado [AllowAnonymous]
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Login");
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        // Cookie persistente de larga duración: la tienda no vuelve a introducir credenciales.
        // Sliding => cada visita renueva los 30 días; solo caduca tras 30 días sin usarse.
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.Name = "IntranetTiendas.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.MaxAge = TimeSpan.FromDays(30);
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<DbService>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Cultura es-ES para formatos de moneda y fecha (equivalente a FormatCurrency / date() del ASP)
var esES = new CultureInfo("es-ES");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(esES),
    SupportedCultures = [esES],
    SupportedUICultures = [esES]
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// ---------------------------------------------------------------------------
// Endpoints binarios (sustituyen a ImageProd.asp / DocBinary.asp).
// Sirven <img>/<a> renderizados por el propio servidor; no son una API de datos.
// ---------------------------------------------------------------------------

// ImageProd.asp -> /ImageProd?Prod=&Ean=&SizeImage=&Cont=&FA=
app.MapGet("/ImageProd", async (DbService db, HttpContext ctx,
    string? Prod, string? Ean, string? SizeImage, string? Cont, string? FA) =>
{
    if (ctx.User.Identity?.IsAuthenticated != true) return Results.Unauthorized();
    var proc = FA == "1" ? "asp.ImgFoodInfo" : "asp.Imagen";
    var row = await db.QueryFirstOrDefaultProcAsync(proc, new
    {
        Size = long.TryParse(SizeImage, out var s) ? s : 0,
        Prod = long.TryParse(Prod, out var p) ? p : 0,
        Ean = long.TryParse(Ean, out var e) ? (long?)e : null,
        Cont = long.TryParse(Cont, out var c) ? c : -1
    });
    if (row is null) return Results.NotFound();
    var dict = (IDictionary<string, object>)row;
    return Results.File((byte[])dict["JPG2"], "image/jpeg", $"{dict["Ean"]}.JPG");
}).RequireAuthorization();

// DocBinary.asp -> /DocBinary?P1=TableName&P2=Code&P3=IdDoc&P4=SizeDocImage
app.MapGet("/DocBinary", async (DbService db, HttpContext ctx,
    string? P1, string? P2, string? P3, string? P4) =>
{
    if (ctx.User.Identity?.IsAuthenticated != true) return Results.Unauthorized();
    var row = await db.QueryFirstOrDefaultProcAsync("asp.Document_Select", new
    {
        Empresa = db.Empresa,
        TableName = long.TryParse(P1, out var t) ? t : 0,
        Code = P2 ?? "0",
        IdDoc = long.TryParse(P3, out var i) ? i : 0,
        SizeDocImage = long.TryParse(P4, out var z) ? z : 0
    });
    if (row is null) return Results.NotFound();
    var dict = (IDictionary<string, object>)row;
    return Results.File((byte[])dict["DocBinary"], (string)dict["ContentType"]);
}).RequireAuthorization();

// DocUrl.asp -> /DocUrl?url= (proxy de PDFs internos; restringido a hosts permitidos)
app.MapGet("/DocUrl", async (IConfiguration cfg, HttpContext ctx, string url) =>
{
    if (ctx.User.Identity?.IsAuthenticated != true) return Results.Unauthorized();
    var permitidos = cfg.GetSection("DocUrlHostsPermitidos").Get<string[]>() ?? [];
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !permitidos.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
        return Results.BadRequest("Host no permitido");
    using var http = new HttpClient();
    var resp = await http.GetAsync(uri);
    if (!resp.IsSuccessStatusCode) return Results.StatusCode((int)resp.StatusCode);
    return Results.File(await resp.Content.ReadAsByteArrayAsync(), "application/pdf");
}).RequireAuthorization();

app.Run();
