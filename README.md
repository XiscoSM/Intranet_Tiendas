# Intranet Tienda

Intranet de tiendas de Hipercentro, en ASP.NET Core sobre IIS.

## Estructura

| Carpeta | Qué es |
|---|---|
| `IntranetTiendas.Web/` | La aplicación. Ver su [README](IntranetTiendas.Web/README.md) para puesta en marcha, despliegue y decisiones de migración, y [CONVENTIONS.md](IntranetTiendas.Web/CONVENTIONS.md) para las reglas de migración página a página. |

El código ASP clásico original del que procede esta migración está **fuera de este repositorio**,
en `C:\GitHub\Intranet_Tienda_asp` (solo referencia histórica, no se despliega).

## Resumen técnico

- .NET 10 LTS · Razor Pages · Dapper sobre los procedimientos existentes `asp.*` (BD `NAVHM`) y `rep.*` (BD `MadisaNet`).
- Autenticación contra el procedure de RetailWare `asp.PROC_Usuario_Select_R3_HM1` (cookie de sesión firmada, 30 días).
- Sin WebAPI expuesta: todo el acceso a datos ocurre en el servidor.
- Empresa fija por configuración (`HM1`); el almacén sale del usuario (`RW_Almacen`).

> Las cadenas de conexión reales no deben subirse al repositorio: usa `appsettings.Production.json`
> (ignorado por git) o User Secrets.
