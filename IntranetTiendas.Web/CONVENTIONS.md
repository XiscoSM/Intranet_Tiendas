# Convenciones de migración ASP clásico → Razor Pages

Proyecto: `C:\GitHub\Intranet_Tienda\IntranetTiendas.Web` (net10.0, Razor Pages, Dapper).
Origen (fuera del repo, solo referencia): `C:\GitHub\Intranet_Tienda_asp\IntranetTiendas\*.asp`.

## Ejemplares de referencia (leer antes de escribir nada)
- `Pages/Producto.cshtml` + `Pages/Producto.cshtml.cs` — página de consulta con varios procedures, enlaces entre páginas y export Excel.
- `Pages/Index.cshtml(.cs)` — página simple.
- `Pages/BasePageModel.cs`, `Services/DbService.cs` — infraestructura.

## Reglas
1. **1 página .asp → `Pages/<Nombre>.cshtml` + `Pages/<Nombre>.cshtml.cs`.** Nombre sin espacios ni paréntesis: `Informe Cierre.asp` → `InformeCierre`, `InventariosLin(detallado).asp` → `InventariosLinDetallado`, `PedidosCentralLinPreparacion_2COL.asp` → `PedidosCentralLinPreparacion2Col`. NO migrar ficheros `_bak`, `_Test`, `Test`, `_anulado`.
2. PageModel hereda de `BasePageModel`, constructor primario `(DbService db) : BasePageModel(db)`. Propiedades disponibles: `Empresa` (string, de config), `Alm` (int, claim de sesión), `DescAlm`, `Usuario`, `Db`.
3. **Cookies EMPRESA/ALMACEN y querystring V1/V2 de empresa/almacén: eliminar.** Usar `Empresa` y `Alm` de la base. El resto de parámetros de querystring (P1, P2, P3, Doc, Pedido, etc.) **conservar con el mismo nombre** vía `[BindProperty(SupportsGet = true)] public string? P1 { get; set; }` para no romper enlaces entre páginas.
4. **Cada `ADODB.Recordset` → llamada Dapper**:
   - `rs.Source = "EXEC asp.Proc @Empresa='" & EMPRESA & "',@Alm=" & Almacen` →
     `await Db.QueryProcAsync("asp.Proc", new { Empresa, Alm })` (lista) o `QueryFirstOrDefaultProcAsync` (una fila).
   - Los nombres de parámetro del objeto anónimo deben coincidir EXACTAMENTE con los `@nombre` del EXEC original.
   - Parámetros que en el EXEC iban sin comillas eran numéricos: convertir con `long.TryParse(P1, out var p) ? p : 0`. Con comillas: string tal cual (`?? ""` o el default del ASP).
   - Acceso a campos en la vista: `var d = (IDictionary<string, object>)row;` y `@d["Campo"]`.
5. **Bucles `While NOT rs.EOF` → `@foreach (var r in Model.Lista)`** sobre `List<dynamic>`.
6. **HTML**: conservar la estructura de tablas y clases CSS originales (Titulos, tablaMedianoAzul, tablaMediano, tablaRojo, CabeceraTablas, Negrita, normal, info_pequeno...). Tablas de datos: añadir `class="datos"`. Corregir las tildes perdidas del origen (Secci�n → Sección) escribiendo UTF-8 correcto. No añadir `<html>/<head>/<body>` (lo pone `_Layout`).
7. **Export Word/Excel** (`Formato=Excel/Word`, `ExportPage(...)`): eliminar la lógica de `Formato`; añadir handler `OnGetExcelAsync()` que reejecuta la consulta principal y devuelve `Excel(rows, "Nombre")` (ver Producto). El icono: `<td class="export-bar" onclick="ExportPage()"><img src="~/img/excel.jpg" width="23" height="23" alt="Excel" /></td>`. Word: se elimina.
8. **Cookie `Bordes`**: eliminar (las tablas `datos` ya llevan borde fino).
9. **Enlaces**: `XXX.asp?...` → `/XXX?...` (mismo nombre de página convertido según regla 1). `Ejecuta procedure.asp?Proc=N&V1=x&V2=y` → `/Accion?Proc=N&V1=x&V2=y`. `ImageProd.asp` → `/ImageProd`. `DocBinary.asp` → `/DocBinary`. Popups `window.open` se conservan.
10. **Escrituras** (INSERT/UPDATE/DELETE/procedures de acción dentro de la página): hacerlas en `OnPostAsync` con formulario `method="post"` si en origen era un form POST; si en origen era un enlace GET de popup, mantener GET pero SIEMPRE parametrizado (nunca concatenar). SQL inline contra tablas NAV: `Db.ExecuteSqlAsync($"... dbo.[{Empresa}$Tabla] ...", new { ... })` — el nombre de tabla solo puede venir de `Empresa` (config), jamás de la querystring.
11. `FormatCurrency(x)` → `Convert.ToDecimal(x).ToString("C2", CultureInfo.GetCultureInfo("es-ES"))`. `date()`/`time()` → `DateTime.Now`. `Server.ScriptTimeout` → ignorar. Fechas: `DbService.NormalizarFechas` procesa toda fila de los procedures →  01/01/1753 (centinela) se pone a null (se muestra en blanco); fecha a medianoche (sin hora) se convierte a string "dd/MM/yy"; fecha con hora real (firmas/cierres) se conserva como DateTime. Por eso las vistas muestran `@d["Fecha"]` como dd/MM/yy sin tratamiento y los enlaces pasan dd/MM/yy a los procedures (la sesión SQL es español/dmy y lo acepta). Si una vista compara una fecha con una cadena literal, usar el formato corto (p.ej. el centinela "01/01/2000" → comparar con "01/01/00").
12. Comentario XML en cada PageModel indicando el .asp de origen y los procedures usados.
13. Subida de ficheros (upload2/uploadR2) y servido de documentos por ruta de archivo/UNC: **NO migrar**. El cliente confirmó (10-06-2026) que ya no se usan recursos de archivos ni rutas UNC; esas páginas y el endpoint /comunicadosR2 se eliminaron del proyecto.
