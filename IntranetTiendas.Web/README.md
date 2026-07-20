# Intranet Tiendas — Migración a ASP.NET Core (.NET 10 LTS)

Migración de la intranet ASP clásico a Razor Pages. El código original queda fuera del repo,
como referencia, en `C:\GitHub\Intranet_Tienda_asp`.
Mantiene los mismos procedimientos almacenados `asp.*` de la BD NAVHM (Dynamics NAV);
solo se reescribe la capa web.

## Qué cambia respecto al ASP clásico

| Antes | Ahora |
|---|---|
| DSN ODBC + ADODB con SQL concatenado (inyección SQL) | Dapper + Microsoft.Data.SqlClient, todo parametrizado |
| Cookie `EMPRESA` / `ALMACEN` editable por el cliente | `Empresa` fija en `appsettings.json`; almacén en la cookie de autenticación firmada (claim) |
| Acceso anónimo | Login contra el procedure `asp.PROC_Usuario_Select_R3_HM1` (usuario y contraseña numéricos; el procedure valida). El almacén por defecto (`RW_Almacen`) y el permiso de cambio de tienda (`RW_PermiteCambioAlm`) salen del propio procedure |
| Export Excel = HTML con ContentType falso | .xlsx real (ClosedXML), handler `?handler=Excel` |
| `Ejecuta procedure.asp` (DELETE concatenado de querystring) | `/Accion` parametrizado, tabla derivada solo de config |
| WebKnight (WAF) parcheando los agujeros | Innecesario al desaparecer la concatenación |

Los nombres de página y parámetros de URL (P1, P2, Doc...) se conservan:
`Producto.asp?P1=123` → `/Producto?P1=123`.

## Puesta en marcha (desarrollo)

1. **Cadenas de conexión** (no van en el repositorio). Configúralas con User Secrets:
   ```
   dotnet user-secrets set "ConnectionStrings:NavR2"     "Server=<servidor>;Database=NAVHM;User Id=<usuario>;Password=<clave>;TrustServerCertificate=true;Encrypt=true"
   dotnet user-secrets set "ConnectionStrings:MadisaNet" "Server=<servidor>;Database=MadisaNet;User Id=<usuario>;Password=<clave>;TrustServerCertificate=true;Encrypt=true"
   ```
   `NavR2` es la BD de Navision con los procedures `asp.*`; `MadisaNet` tiene los `rep.*` de
   Balanzas/Cierre. El DSN antiguo `NavR2_CA1` (empresa CA1) no se usa: la empresa es fija (`HM1`).
2. `dotnet run` y entrar en `/Login` con un usuario y contraseña válidos de RetailWare
   (numéricos). No hay alta de usuarios en esta app: la valida el procedure existente.

## Despliegue en IIS (convivencia con el ASP clásico)

1. **Servidor**: instalar el *ASP.NET Core Hosting Bundle 10.x* (incluye el módulo ANCM). Reiniciar IIS.
2. **Publicar**: `dotnet publish -c Release -o F:\Webs_IIS\Tiendas\v2`
3. **IIS**: en el sitio actual, crear **aplicación** `/v2` apuntando a esa carpeta, con un
   **Application Pool propio**: .NET CLR Version = *No Managed Code*, modo integrado.
   La identidad del pool no necesita permisos especiales (la conexión SQL va por usuario/password).
4. La app queda en `http://servidor/v2/`. El ASP clásico sigue en la raíz.
5. **Migración por páginas**: cuando una página esté validada, redirigir la vieja con URL Rewrite
   en el web.config de la raíz, p.ej.:
   ```xml
   <rule name="Producto a v2" stopProcessing="true">
     <match url="^IntranetTiendas/Producto\.asp$" />
     <action type="Redirect" url="/v2/Producto?{QUERY_STRING}" appendQueryString="false" />
   </rule>
   ```
6. Al completar la migración: mover la app a la raíz del sitio, retirar el ASP clásico y WebKnight.

Notas:
- **Documentos de comunicados (directorio virtual de IIS)**: los enlaces "Ver" apuntan a
  `RutaWeb` de la BD, que es siempre `/comunicadosR2/<Empresa>/...` (verificado: los 7.239 registros
  con documento usan ese prefijo; ninguno usa el antiguo `/Comunicados/`). Esa ruta es un
  **directorio virtual de IIS** que apunta a una carpeta local del servidor con los ficheros; los
  sirve IIS directamente. La app **no** lo sirve ni lo necesita: solo emite el hipervínculo. Al
  desplegar, ese directorio virtual debe seguir existiendo en la raíz del sitio (como hoy). Como los
  enlaces son absolutos desde la raíz (`/comunicadosR2/...`), funcionan igual con la app montada en
  `/v2` o en la raíz. No se usa ninguna ruta UNC.
- `appsettings.Production.json` con la cadena real; no subir passwords al repositorio.
- El `web.config` de la publicación lo genera `dotnet publish`; no hace falta tocarlo.
- HTTPS recomendado incluso en intranet (la cookie de sesión viaja en cada petición).

## Estructura

- `Program.cs` — arranque, cookie auth, endpoints binarios (`/ImageProd`, `/DocBinary`, `/DocUrl`)
- `Services/DbService.cs` — Dapper sobre los procedures `asp.*`
- `Services/AuthService.cs` — login contra el procedure `asp.PROC_Usuario_Select_R3_HM1`
- `Pages/` — una página por .asp original (ver `CONVENTIONS.md` para el mapeo)
- `Pages/CambiarAlmacen.cshtml` — selector de tienda, solo si `RW_PermiteCambioAlm`

## Pendiente / decisiones tomadas

- `PedidoWebCab.asp` no migrado: en el menú original figura como "desactivado" y enlaza a una página inexistente.
- Páginas `*_bak`, `*Test*` y duplicados: descartados (eran copias de seguridad manuales).
- Export a Word: eliminado (era HTML disfrazado; el Excel real lo cubre).
- Selector "Cambiar de Tienda": reinstaurado pero condicionado a `RW_PermiteCambioAlm`. El almacén
  por defecto es `RW_Almacen` del usuario; al cambiarlo se re-emite la cookie firmada (no es una
  cookie manipulable), validando que el almacén exista en `asp.Almacen`.
- Acciones 1 y 2 del antiguo `Ejecuta procedure.asp` (DELETE sobre `[EMPRESA$Pedidos a tienda NET]`
  y `[EMPRESA$Offline NET]`): eliminadas. Verificado contra NAVHM que esas tablas ya no existen.
- Subida de comunicados (`upload2.asp`/`uploadR2.asp`): **no migrada** (ya no se usa) y el acceso
  por ruta UNC a un servidor de desarrollo ya retirado se eliminó. En Comunicados se conserva el listado,
  "marcar leído" y el enlace "Ver", que abre el documento a través del directorio virtual de
  IIS `/comunicadosR2` — ver Notas de despliegue.
- El toggle de "Bordes" por cookie: eliminado; las tablas de datos llevan borde fino siempre.
- Diseño moderno estilo Apple en `wwwroot/css/site.css` (las clases heredadas del ASP se remapean
  a la nueva estética, por lo que las páginas migradas se restilizan sin tocar su HTML).
