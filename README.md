# TiendaBolsos

API de una tienda de bolsos desarrollada con .NET 8, SQL Server y una arquitectura analítica con ETL, MongoDB Atlas y un Data Warehouse independiente. Incluye un dashboard Blazor adaptable a escritorio y móvil.

## Contenido

- [Arquitectura](#arquitectura)
- [KPIs y modelo analítico](#kpis-y-modelo-analítico)
- [Requisitos](#requisitos)
- [Preparación de SQL Server](#preparación-de-sql-server)
- [Configuración de MongoDB Atlas](#configuración-de-mongodb-atlas)
- [Ejecución](#ejecución)
- [Prueba completa con datos de ejemplo](#prueba-completa-con-datos-de-ejemplo)
- [Endpoints principales](#endpoints-principales)
- [Solución de problemas](#solución-de-problemas)

## Arquitectura

```text
                         ┌─────────────────────────────┐
                         │ Dashboard Blazor responsive │
                         └──────────────┬──────────────┘
                                        │ JWT / HTTP
                                        ▼
┌───────────────────────┐    ┌─────────────────────────┐
│ API ASP.NET Core 8    │───▶│ SQL Server Data         │
│ JWT + EF Core         │    │ Warehouse: TiendaBolsosDW│
└───────┬─────────┬─────┘    └────────────▲────────────┘
        │         │                        │
        │         └── telemetría ──▶ MongoDB Atlas
        │                                  │
        └── ventas / inventario           │
             SQL Server                    │
             TiendaBolsosDB                │
                    └──────── .NET ETL ────┘
```

- **SQL Server operativo (`TiendaBolsosDB`)**: usuarios, catálogo, ventas, detalles e inventario.
- **MongoDB Atlas (`TiendaBolsos.apiTelemetry`)**: datos técnicos de cada solicitud HTTP. No se almacenan cuerpos ni parámetros de las solicitudes.
- **Data Warehouse (`TiendaBolsosDW`)**: base analítica independiente para consultas de KPIs.
- **ETL .NET (`TiendaBolsos.Analytics.ETL`)**: carga los hechos de ventas e inventario desde SQL Server y, si Atlas está configurado, las solicitudes técnicas desde MongoDB. Se ejecuta al iniciar y después cada cinco minutos.
- **API (`TiendaBolsos.API`)**: autentica usuarios con JWT y expone los indicadores del warehouse.
- **Dashboard (`TiendaBolsos.Dashboard`)**: inicia sesión mediante la API y presenta los KPIs en un diseño adaptable a móviles.

Cuando no hay conexión configurada a Atlas, el ETL continúa cargando la información comercial y deja constancia en el log de que omitió la telemetría. Los KPIs técnicos serán cero hasta conectar Atlas y generar solicitudes.

## KPIs y modelo analítico

### KPIs del negocio

Endpoint: `GET /api/analytics/business`

| KPI | Definición |
| --- | --- |
| Ventas acumuladas | Suma de los subtotales de las líneas de venta cargadas. |
| Pedidos | Cantidad de identificadores de venta distintos. |
| Ticket promedio | Ventas acumuladas divididas entre pedidos. |
| Unidades vendidas | Suma de las cantidades de las líneas. |
| Productos con stock bajo | Productos cuyo stock de la última fotografía es menor o igual al mínimo configurado. |
| Ventas por día | Suma de subtotales por fecha para los últimos siete días. |

### KPIs técnicos

Endpoint: `GET /api/analytics/technical`

| KPI | Definición |
| --- | --- |
| Solicitudes API | Solicitudes capturadas por el middleware durante las últimas 24 horas. |
| Errores 5xx | Solicitudes con código de estado HTTP entre 500 y 599. |
| Tasa de errores | Errores 5xx divididos entre solicitudes, expresado como porcentaje. |
| Latencia promedio | Promedio de duración de las solicitudes capturadas, en milisegundos. |
| Solicitudes por día | Solicitudes y errores diarios de los últimos siete días. |

Ambos endpoints requieren un usuario autenticado con rol `Admin` o `Gerente`.

### Tablas analíticas

El script [DataWarehouse.sql](./TiendaBolsos.Analytics.ETL/sql/DataWarehouse.sql) crea:

- `Analytics.DimDate`: dimensión de fecha.
- `Analytics.DimProduct`: catálogo, categoría y marca.
- `Analytics.FactSale`: una fila por detalle de venta.
- `Analytics.FactInventorySnapshot`: fotografía diaria del inventario.
- `Analytics.FactApiRequest`: una fila por solicitud de API.
- `Analytics.EtlWatermark`: marcas de agua para continuar las cargas.

## Requisitos

- .NET 8 SDK.
- SQL Server local o remoto. En los comandos se usa SQL Server Express (`.\SQLEXPRESS`); adapta el nombre de instancia si es diferente.
- MongoDB Atlas para habilitar las métricas técnicas.
- PowerShell o SQL Server Management Studio.

## Preparación de SQL Server

Abre PowerShell en la carpeta que contiene este README:

```powershell
$sqlServer = '.\SQLEXPRESS'
sqlcmd -S $sqlServer -E -C -b -i .\TiendaBolsosDB.sql
sqlcmd -S $sqlServer -E -C -b -i .\TiendaBolsos.Analytics.ETL\sql\DataWarehouse.sql
```

`-E` utiliza la autenticación de Windows, `-C` confía en el certificado local de SQL Server y `-b` hace que `sqlcmd` informe un error si falla el script. Los scripts crean bases y tablas que no existan; no eliminan datos existentes. Para otra instancia, cambia `.\SQLEXPRESS`.

El archivo opcional [TiendaBolsosDemoData.sql](./TiendaBolsosDemoData.sql) añade productos, inventario y tres ventas identificadas como `DEMO-ETL-01` a `DEMO-ETL-03`. Para ejecutar la carga completa, primero debes iniciar la API, registrar una cuenta y asignarle el rol `Admin` o `Gerente`. El script de datos de prueba es idempotente: no vuelve a agregar las ventas si ya encuentra los marcadores demo.

## Configuración de MongoDB Atlas

La conexión se guarda fuera del código fuente mediante *user secrets*. Reemplaza los valores de ejemplo por la cadena de conexión de tu propio clúster; no compartas ni publiques la URI ni sus credenciales.

```powershell
dotnet user-secrets init --project .\TiendaBolsos.API\TiendaBolsos.API.csproj
dotnet user-secrets set "Mongo:ConnectionString" "TU_URI_DE_MONGO_ATLAS" --project .\TiendaBolsos.API\TiendaBolsos.API.csproj

dotnet user-secrets init --project .\TiendaBolsos.Analytics.ETL\TiendaBolsos.Analytics.ETL.csproj
dotnet user-secrets set "Mongo:ConnectionString" "TU_URI_DE_MONGO_ATLAS" --project .\TiendaBolsos.Analytics.ETL\TiendaBolsos.Analytics.ETL.csproj
dotnet user-secrets set "ConnectionStrings:OperationalConnection" "Server=.\SQLEXPRESS;Database=TiendaBolsosDB;Trusted_Connection=True;TrustServerCertificate=True;" --project .\TiendaBolsos.Analytics.ETL\TiendaBolsos.Analytics.ETL.csproj
dotnet user-secrets set "ConnectionStrings:WarehouseConnection" "Server=.\SQLEXPRESS;Database=TiendaBolsosDW;Trusted_Connection=True;TrustServerCertificate=True;" --project .\TiendaBolsos.Analytics.ETL\TiendaBolsos.Analytics.ETL.csproj
```

La API obtiene sus cadenas de SQL Server de `ConnectionStrings:DefaultConnection` y `ConnectionStrings:WarehouseConnection` en `appsettings.json` o de variables de entorno. Si usas Express, establece estas variables en la terminal desde la que ejecutarás la API:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=.\SQLEXPRESS;Database=TiendaBolsosDB;Trusted_Connection=True;TrustServerCertificate=True;'
$env:ConnectionStrings__WarehouseConnection = 'Server=.\SQLEXPRESS;Database=TiendaBolsosDW;Trusted_Connection=True;TrustServerCertificate=True;'
```

Si tu cuenta de Atlas restringe el acceso por IP, agrega la IP de la máquina de desarrollo a la lista de acceso de red de Atlas. La base y colección predeterminadas son `TiendaBolsos` y `apiTelemetry`.

## Ejecución

Ejecuta cada proceso en una terminal diferente, desde la carpeta del proyecto:

```powershell
dotnet run --project .\TiendaBolsos.API\TiendaBolsos.API.csproj
```

La API expone Swagger en `http://localhost:5059/swagger` si ejecutas el perfil HTTP predeterminado. Inicia el ETL en otra terminal:

```powershell
dotnet run --project .\TiendaBolsos.Analytics.ETL\TiendaBolsos.Analytics.ETL.csproj
```

Inicia el dashboard en una tercera terminal:

```powershell
dotnet run --project .\TiendaBolsos.Dashboard\TiendaBolsos.Dashboard.csproj
```

Abre en el navegador la URL que indique el dashboard al iniciar (normalmente `http://localhost:5xxx`). El cliente de Blazor usa por defecto la API `http://localhost:5059`; puedes cambiarla en `TiendaBolsos.Dashboard\appsettings.json`, sección `Api:BaseAddress`.

## Prueba completa con datos de ejemplo

### 1. Crear la cuenta de prueba

Con la API iniciada, registra una cuenta de ejemplo:

```powershell
$body = @{
    nombre = 'Allan'
    correo = 'allan@example.com'
    password = 'Password123!'
} | ConvertTo-Json

Invoke-RestMethod -Uri 'http://localhost:5059/api/auth/register' `
    -Method Post -ContentType 'application/json' -Body $body
```

La contraseña es solo para desarrollo local. Cambia la cuenta por una contraseña propia para cualquier uso no desechable. El registro asigna el rol `Cajero`; para habilitar el dashboard, promueve la cuenta en la base operativa:

```sql
USE TiendaBolsosDB;
UPDATE Seguridad.Usuario
SET Rol = N'Admin'
WHERE Correo = N'allan@example.com';
```

### 2. Insertar ventas y productos demo

Desde PowerShell:

```powershell
sqlcmd -S '.\SQLEXPRESS' -E -C -b -i .\TiendaBolsosDemoData.sql
```

Si tu instancia no es Express, reemplaza `.\SQLEXPRESS`. El script requiere al menos un usuario con rol `Admin` o `Gerente`, crea los productos demo si no existen, inserta inventario inicial y agrega tres ventas de los últimos días. El script rebaja el inventario y no duplica las ventas demo si se vuelve a ejecutar.

### 3. Cargar el Data Warehouse

Inicia el ETL como se describe arriba. Ejecuta una carga inmediata al arrancar y después repite cada cinco minutos. Si Atlas aún no está configurado, verás una advertencia de telemetría omitida, pero las ventas y el inventario se cargarán desde SQL Server.

### 4. Comprobar los datos cargados

Ejecuta en SQL Server Management Studio:

```sql
USE TiendaBolsosDW;

SELECT COUNT(*) AS FilasVentas,
       SUM(Amount) AS VentasAcumuladas,
       COUNT(DISTINCT SourceSaleId) AS Pedidos,
       SUM(Quantity) AS UnidadesVendidas
FROM Analytics.FactSale;

SELECT d.[Date], SUM(f.Amount) AS VentasDelDia
FROM Analytics.FactSale AS f
JOIN Analytics.DimDate AS d ON d.DateKey = f.DateKey
GROUP BY d.[Date]
ORDER BY d.[Date];

SELECT p.ProductName, s.StockQuantity, s.MinimumStock
FROM Analytics.FactInventorySnapshot AS s
JOIN Analytics.DimProduct AS p ON p.ProductKey = s.ProductKey
WHERE s.DateKey = (SELECT MAX(DateKey) FROM Analytics.FactInventorySnapshot)
ORDER BY p.ProductName;
```

En una base nueva, la carga demo debe mostrar **4 filas de detalle**, **3 pedidos**, **5 unidades** y **243.47** en ventas. El ticket promedio esperado es **81.1567** (aproximadamente **81.16**). Los importes se expresan en la misma moneda/unidad que los precios almacenados en la base operativa. Debe aparecer al menos un producto bajo stock mínimo. Si ya existen ventas en la base, los acumulados serán mayores.

### 5. Comprobar los KPIs de la API y el dashboard

Inicia sesión para obtener un token y consulta los indicadores:

```powershell
$loginBody = @{
    correo = 'allan@example.com'
    password = 'Password123!'
} | ConvertTo-Json
$login = Invoke-RestMethod -Uri 'http://localhost:5059/api/auth/login' `
    -Method Post -ContentType 'application/json' -Body $loginBody
$headers = @{ Authorization = "Bearer $($login.token)" }

Invoke-RestMethod -Uri 'http://localhost:5059/api/analytics/business' -Headers $headers
Invoke-RestMethod -Uri 'http://localhost:5059/api/analytics/technical' -Headers $headers
```

Con los datos demo y una base inicialmente vacía, `business` devuelve ventas `243.47`, pedidos `3`, ticket promedio `81.1567`, unidades `5` y al menos un producto con bajo stock. Sin Atlas, el endpoint técnico devuelve cero solicitudes y latencia; esto es esperado.

Abre el dashboard, inicia sesión con la cuenta demo y verifica los mismos KPIs. Una vez conectado Atlas, realiza algunas solicitudes a la API y vuelve a ejecutar el ETL para que las métricas técnicas aparezcan.

## Endpoints principales

| Método | Ruta | Acceso |
| --- | --- | --- |
| `POST` | `/api/auth/register` | Público |
| `POST` | `/api/auth/login` | Público |
| `GET` | `/api/product` | JWT |
| `POST` | `/api/ventas` | JWT de Admin, Gerente o Cajero |
| `GET` | `/api/analytics/business` | JWT de Admin o Gerente |
| `GET` | `/api/analytics/technical` | JWT de Admin o Gerente |

Para llamadas protegidas, envía el token en la cabecera `Authorization: Bearer <token>`.

## Solución de problemas

- **Error de conexión a SQL Server**: comprueba que el servicio esté iniciado y que `Server` coincida con la instancia instalada (por ejemplo, `.\SQLEXPRESS`).
- **Error de certificado en `sqlcmd`**: agrega `-C` a la llamada de `sqlcmd` para confiar en el certificado de desarrollo local.
- **El dashboard muestra cero ventas**: confirma que el ETL terminó una carga correctamente y consulta las filas de `Analytics.FactSale` en `TiendaBolsosDW`.
- **El dashboard muestra cero métricas técnicas**: confirma que la URI de Atlas está configurada tanto en la API como en el ETL; genera solicitudes a la API y espera la siguiente carga.
- **Error 401**: inicia sesión otra vez para obtener un JWT vigente.
- **Error 403**: asigna a la cuenta un rol `Admin` o `Gerente` en `Seguridad.Usuario`.
- **El ETL no carga las ventas demo**: confirma que `TiendaBolsosDB` y `TiendaBolsosDW` existen, que las tres tablas fuente tienen los datos esperados y que el usuario ETL puede leer el origen y escribir en el warehouse.
- **Cambiaste tablas de una base existente**: los scripts crean objetos faltantes, no actualizan tablas previas con estructuras diferentes. Revisa la definición antes de aplicar cambios manuales en una base con datos.
