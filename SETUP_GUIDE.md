# Configuración del Proyecto - TiendaBolsos API

## Descripción General
Este proyecto es una API ASP.NET Core 8 para gestionar una tienda de bolsos, integrada con JWT para autenticación y conectada a SQL Server.

## Estructura de la Base de Datos

### Esquemas
- **Catalogos**: Contiene tablas de productos, categorías y marcas
- **Seguridad**: Contiene la tabla de usuarios
- **Ventas**: Contiene tablas de ventas y detalles de ventas

### Tablas

#### 1. Catalogos.Categoria
- IdCategoria (PK, Identity)
- Nombre (NVARCHAR(100), UNIQUE)
- FechaRegistro (DATETIME, Default GETDATE())

#### 2. Catalogos.Marca
- IdMarca (PK, Identity)
- Nombre (NVARCHAR(100), UNIQUE)
- FechaRegistro (DATETIME, Default GETDATE())

#### 3. Catalogos.Producto
- IdProducto (PK, Identity)
- Nombre (NVARCHAR(150), NOT NULL)
- Descripcion (NVARCHAR(500))
- Precio (DECIMAL(10,2), NOT NULL)
- Stock (INT, Default 0)
- ImagenUrl (NVARCHAR(500))
- Activo (BIT, Default 1)
- FechaRegistro (DATETIME, Default GETDATE())
- IdCategoria (FK -> Categoria)
- IdMarca (FK -> Marca)

#### 4. Seguridad.Usuario
- IdUsuario (PK, Identity)
- Nombre (NVARCHAR(100), NOT NULL)
- Correo (NVARCHAR(150), NOT NULL, UNIQUE)
- PasswordHash (NVARCHAR(255), NOT NULL)
- Rol (NVARCHAR(50), Default 'Usuario')
- Activo (BIT, Default 1)
- FechaRegistro (DATETIME, Default GETDATE())

#### 5. Ventas.Venta
- IdVenta (PK, Identity)
- Fecha (DATETIME, Default GETDATE())
- Total (DECIMAL(10,2), NOT NULL)
- MetodoPago (NVARCHAR(50))
- IdUsuario (FK -> Usuario)

#### 6. Ventas.DetalleVenta
- IdDetalleVenta (PK, Identity)
- IdVenta (FK -> Venta)
- IdProducto (FK -> Producto)
- Cantidad (INT, NOT NULL)
- PrecioUnitario (DECIMAL(10,2), NOT NULL)
- Subtotal (DECIMAL(10,2), NOT NULL)

#### 7. Catalogos.Inventory y Catalogos.InventoryMovement
- Inventory: inventario actual por producto, stock mínimo y fecha de actualización.
- InventoryMovement: entradas/salidas asociadas al inventario.

#### 8. Seguridad.Auditoria
- Auditoría de operaciones de catálogo registradas por el contexto de datos.

## Configuración

### 1. Base de Datos
Ejecuta el script `TiendaBolsosDB.sql` en SQL Server Management Studio:
```sql
sqlcmd -S localhost -i TiendaBolsosDB.sql
```

O desde SSMS:
- Abre SSMS
- Conecta a `localhost` (o la instancia configurada en SQL Server)
- Abre y ejecuta `TiendaBolsosDB.sql`

### 2. Connection String
El archivo `appsettings.json` ya está configurado con:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TiendaBolsosDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
Actualiza `Server` para que coincida con tu instancia de SQL Server.

### 3. JWT Settings
Los valores de JWT están configurados en `appsettings.json`:
- Key: `SUPER_SECRET_KEY_TIENDA_BOLSOS_2026`
- Issuer: `TiendaBolsos`
- Audience: `TiendaBolsosUsers`

## Modelos y Estructura del Proyecto

### Carpetas por Módulo
```
Core/Modules/
├── Auth/
│   ├── DTOs/ (RegisterDto, LoginDto)
│   ├── Interfaces/ (IAuthService, IAuthRepository)
│   ├── Models/ (User)
│   └── Services/ (AuthService)
├── Products/
│   ├── DTOs/
│   ├── Interfaces/ (IProductService, IProductRepository)
│   ├── Models/ (Product, Categoria, Marca)
│   └── Services/ (ProductService)
└── Sales/
    └── Models/ (Venta, DetalleVenta)
```

## Endpoints de la API

### Autenticación
- `POST /api/auth/register` - Registrar nuevo usuario
  ```json
  {
    "nombre": "Juan",
    "correo": "juan@example.com",
    "password": "Password123!"
  }
  ```

- `POST /api/auth/login` - Login de usuario
  ```json
  {
    "correo": "juan@example.com",
    "password": "Password123!"
  }
  ```

### Productos
- `GET /api/product` - Obtener todos los productos
- `GET /api/product/{id}` - Obtener producto por ID
- `POST /api/product` - Crear nuevo producto (Requiere autenticación)
- `PUT /api/product/{id}` - Actualizar producto (Requiere autenticación)
- `DELETE /api/product/{id}` - Eliminar producto (Requiere autenticación)

## Como Ejecutar la Aplicación

### Prerequisitos
- .NET 8 SDK instalado
- SQL Server con SSMS
- Visual Studio o Visual Studio Code

### Pasos
1. Clona el repositorio
2. Abre la solución en Visual Studio
3. Ejecuta `TiendaBolsosDB.sql` en SQL Server para crear la base operativa y sus tablas.
4. Restaura los paquetes NuGet (Visual Studio lo hace automáticamente).
5. Configura las conexiones según la sección **Analítica y Dashboard**.
6. Ejecuta la API desde `TiendaBolsos.API`.

### Testing con Swagger
Una vez ejecutada la API, accede a Swagger en:
```
http://localhost:5059/swagger/index.html
```
Con el perfil HTTPS también está disponible en `https://localhost:7297/swagger`.

## Campos Actualizados

### Cambios en Usuario (User)
- `Contraseña` → `PasswordHash`
- Agregado: `Correo` (usado para login)
- Agregado: `Rol` (default: "Usuario")
- Agregado: `Activo` (default: true)

### Cambios en DTOs de Autenticación
- RegisterDto: `Nombre`, `Correo`, `Password`
- LoginDto: `Correo`, `Password`

### Cambios en Producto
- Agregado: `Precio`
- Agregado: `Stock`
- Agregado: `ImagenUrl`
- Agregado: `Activo`
- Agregado: `FechaRegistro`
- Removido: `IdUnidadMedida`

### Cambios en Categoria y Marca
- Agregado: `FechaRegistro`
- Constraint UNIQUE en `Nombre`

## Notas Importantes

1. **JWT Token**: Se genera al registrarse o hacer login. Válido para solicitudes subsecuentes en el header `Authorization: Bearer {token}`

2. **Roles**: Se usan roles como `Admin`, `Gerente` y `Cajero`. El registro asigna `Cajero`.

3. **Contraseñas**: Se hashean automáticamente usando BCrypt antes de guardarse

4. **Datos de acceso**: El registro crea usuarios con rol `Cajero`. Para probar los reportes administrativos, registra una cuenta y asígnale `Admin` o `Gerente` en `Seguridad.Usuario`.

5. **Datos iniciales**: `TiendaBolsosDB.sql` crea el esquema. `TiendaBolsosDemoData.sql` es opcional y crea productos e inventario de prueba; no crea ventas ni usuarios.

## Analítica y Dashboard

La solución analítica agrega datos de dos fuentes sin cambiar el origen transaccional:

- **SQL Server operativo** (`TiendaBolsosDB`): ventas, detalles, catálogo e inventario.
- **MongoDB Atlas**: telemetría técnica de las solicitudes a la API. Se guardan ruta de plantilla, método HTTP, código de respuesta, duración, fecha UTC y ambiente; no se guardan query strings ni cuerpos de solicitud.
- **Data Warehouse** (`TiendaBolsosDW`): modelo dimensional con `Analytics.DimDate`, `Analytics.DimProduct`, `Analytics.FactSale`, `Analytics.FactInventorySnapshot` y `Analytics.FactApiRequest`.

### Preparación

1. Ejecuta `TiendaBolsosDB.sql` y `TiendaBolsos.Analytics.ETL/sql/DataWarehouse.sql` en SQL Server. Los scripts crean las bases/tablas que falten.
2. Opcionalmente, ejecuta `TiendaBolsosDemoData.sql` para añadir productos e inventario de prueba.
3. Configura `Mongo:ConnectionString` en los *user secrets* de la API; no guardes la URI ni contraseñas en `appsettings.json`:
   ```powershell
   dotnet user-secrets init --project TiendaBolsos.API\TiendaBolsos.API.csproj
   dotnet user-secrets set "Mongo:ConnectionString" "TU_URI_DE_MONGO_ATLAS" --project TiendaBolsos.API\TiendaBolsos.API.csproj
   ```
4. Configura las conexiones operativa y analítica en los *user secrets* del ETL:
   ```powershell
   dotnet user-secrets init --project TiendaBolsos.Analytics.ETL\TiendaBolsos.Analytics.ETL.csproj
   dotnet user-secrets set "Mongo:ConnectionString" "TU_URI_DE_MONGO_ATLAS" --project TiendaBolsos.Analytics.ETL\TiendaBolsos.Analytics.ETL.csproj
   dotnet user-secrets set "ConnectionStrings:OperationalConnection" "TU_CONEXION_TIENDA_BOLSOS" --project TiendaBolsos.Analytics.ETL\TiendaBolsos.Analytics.ETL.csproj
   dotnet user-secrets set "ConnectionStrings:WarehouseConnection" "TU_CONEXION_TIENDA_BOLSOS_DW" --project TiendaBolsos.Analytics.ETL\TiendaBolsos.Analytics.ETL.csproj
   ```
   La API ya apunta por defecto a `http://localhost:5059` y a SQL Server local; ajusta `appsettings.json` si tu instancia usa otro servidor.
5. Inicia la API en una terminal:
   ```powershell
   dotnet run --project TiendaBolsos.API\TiendaBolsos.API.csproj
   ```
   Registra un usuario en `/api/auth/register` y, desde SQL Server, asígnale el rol para el dashboard:
   ```sql
   USE TiendaBolsosDB;
   GO
   UPDATE Seguridad.Usuario SET Rol = N'Admin' WHERE Correo = N'tu-correo';
   ```
6. Para generar una venta de prueba, inicia sesión, consulta `GET /api/product` y usa uno de sus `idProducto` en `POST /api/ventas`:
   ```json
   {
     "metodoPago": "Tarjeta",
     "detalles": [{ "idProducto": 1, "cantidad": 1 }]
   }
   ```
7. Inicia el ETL y el Dashboard en terminales separadas:
   ```powershell
   dotnet run --project TiendaBolsos.Analytics.ETL\TiendaBolsos.Analytics.ETL.csproj
   dotnet run --project TiendaBolsos.Dashboard\TiendaBolsos.Dashboard.csproj
   ```
   El ETL se ejecuta al arrancar y vuelve a ejecutarse cada 5 minutos. El intervalo se configura con `Etl:IntervalMinutes`.
8. Abre la URL mostrada por `dotnet run` para el Dashboard e inicia sesión con la cuenta `Admin`/`Gerente`. Los datos aparecen después de la primera carga ETL.

### KPIs implementados

- **Negocio** (`GET /api/analytics/business`): ventas acumuladas (suma de subtotales), pedidos únicos, ticket promedio, unidades vendidas, productos bajo el mínimo de inventario y ventas diarias de los últimos 7 días.
- **Técnicos** (`GET /api/analytics/technical`): solicitudes de las últimas 24 horas, errores HTTP 5xx, porcentaje de errores 5xx y latencia promedio; incluye solicitudes y errores diarios de los últimos 7 días.
- Ambos endpoints requieren un JWT de un usuario con rol `Admin` o `Gerente`. El dashboard Blazor consume la API con ese token y adapta su presentación a pantallas móviles.

La primera carga de telemetría puede transferir todo el historial existente en la colección `apiTelemetry`; posteriores cargas usan marcas de agua e identificadores idempotentes. Si la API no tiene `Mongo:ConnectionString`, arranca con telemetría desactivada y lo informa en el log.

## Troubleshooting

### Error de Conexión a BD
```
Provider: MSSQL, error: 0 - Named Pipes Provider
```
Solución: Verifica que SQL Server esté corriendo y el nombre del servidor es correcto en appsettings.json

### Error en Migrations
Si necesitas actualizar la BD después de cambios en modelos:
```bash
dotnet ef migrations add NuevaMigracion
dotnet ef database update
```

## Contacto
Para reportar issues o sugerencias, contacta al equipo de desarrollo.
