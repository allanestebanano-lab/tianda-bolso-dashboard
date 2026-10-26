IF DB_ID(N'TiendaBolsosDB') IS NULL
BEGIN
    CREATE DATABASE TiendaBolsosDB;
END;
GO

USE TiendaBolsosDB;
GO

IF SCHEMA_ID(N'Catalogos') IS NULL
    EXEC(N'CREATE SCHEMA Catalogos');
GO

IF SCHEMA_ID(N'Seguridad') IS NULL
    EXEC(N'CREATE SCHEMA Seguridad');
GO

IF SCHEMA_ID(N'Ventas') IS NULL
    EXEC(N'CREATE SCHEMA Ventas');
GO

IF OBJECT_ID(N'Catalogos.Categoria', N'U') IS NULL
BEGIN
    CREATE TABLE Catalogos.Categoria
    (
        IdCategoria int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categoria PRIMARY KEY,
        Nombre nvarchar(100) NOT NULL,
        FechaRegistro datetime NOT NULL CONSTRAINT DF_Categoria_FechaRegistro DEFAULT GETDATE(),
        CONSTRAINT UQ_Categoria_Nombre UNIQUE (Nombre)
    );
END;
GO

IF OBJECT_ID(N'Catalogos.Marca', N'U') IS NULL
BEGIN
    CREATE TABLE Catalogos.Marca
    (
        IdMarca int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Marca PRIMARY KEY,
        Nombre nvarchar(100) NOT NULL,
        FechaRegistro datetime NOT NULL CONSTRAINT DF_Marca_FechaRegistro DEFAULT GETDATE(),
        CONSTRAINT UQ_Marca_Nombre UNIQUE (Nombre)
    );
END;
GO

IF OBJECT_ID(N'Seguridad.Usuario', N'U') IS NULL
BEGIN
    CREATE TABLE Seguridad.Usuario
    (
        IdUsuario int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuario PRIMARY KEY,
        Nombre nvarchar(100) NOT NULL,
        Correo nvarchar(150) NOT NULL,
        PasswordHash nvarchar(255) NOT NULL,
        Rol nvarchar(50) NOT NULL CONSTRAINT DF_Usuario_Rol DEFAULT N'Usuario',
        Activo bit NOT NULL CONSTRAINT DF_Usuario_Activo DEFAULT 1,
        FechaRegistro datetime NOT NULL CONSTRAINT DF_Usuario_FechaRegistro DEFAULT GETDATE(),
        CONSTRAINT UQ_Usuario_Correo UNIQUE (Correo)
    );
END;
GO

IF OBJECT_ID(N'Catalogos.Producto', N'U') IS NULL
BEGIN
    CREATE TABLE Catalogos.Producto
    (
        IdProducto int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Producto PRIMARY KEY,
        CodigoProducto nvarchar(50) NULL,
        Nombre nvarchar(150) NOT NULL,
        Descripcion nvarchar(500) NULL,
        Precio decimal(10,2) NOT NULL,
        Stock int NOT NULL CONSTRAINT DF_Producto_Stock DEFAULT 0,
        ImagenUrl nvarchar(500) NULL,
        Activo bit NOT NULL CONSTRAINT DF_Producto_Activo DEFAULT 1,
        FechaRegistro datetime NOT NULL CONSTRAINT DF_Producto_FechaRegistro DEFAULT GETDATE(),
        IdCategoria int NOT NULL,
        IdMarca int NOT NULL,
        CONSTRAINT FK_Producto_Categoria FOREIGN KEY (IdCategoria)
            REFERENCES Catalogos.Categoria(IdCategoria),
        CONSTRAINT FK_Producto_Marca FOREIGN KEY (IdMarca)
            REFERENCES Catalogos.Marca(IdMarca)
    );
END;
GO

IF OBJECT_ID(N'Catalogos.Inventory', N'U') IS NULL
BEGIN
    CREATE TABLE Catalogos.Inventory
    (
        IdInventory int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory PRIMARY KEY,
        IdProducto int NOT NULL,
        Stock int NOT NULL CONSTRAINT DF_Inventory_Stock DEFAULT 0,
        MinStock int NOT NULL CONSTRAINT DF_Inventory_MinStock DEFAULT 5,
        LastUpdate datetime NOT NULL CONSTRAINT DF_Inventory_LastUpdate DEFAULT GETDATE(),
        CONSTRAINT UQ_Inventory_IdProducto UNIQUE (IdProducto),
        CONSTRAINT FK_Inventory_Producto FOREIGN KEY (IdProducto)
            REFERENCES Catalogos.Producto(IdProducto)
    );
END;
GO

IF OBJECT_ID(N'Catalogos.InventoryMovement', N'U') IS NULL
BEGIN
    CREATE TABLE Catalogos.InventoryMovement
    (
        IdMovement int IDENTITY(1,1) NOT NULL CONSTRAINT PK_InventoryMovement PRIMARY KEY,
        IdInventory int NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        Quantity int NOT NULL,
        [Date] datetime NOT NULL CONSTRAINT DF_InventoryMovement_Date DEFAULT GETDATE(),
        CONSTRAINT FK_InventoryMovement_Inventory FOREIGN KEY (IdInventory)
            REFERENCES Catalogos.Inventory(IdInventory) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'Ventas.Venta', N'U') IS NULL
BEGIN
    CREATE TABLE Ventas.Venta
    (
        IdVenta int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Venta PRIMARY KEY,
        Fecha datetime NOT NULL CONSTRAINT DF_Venta_Fecha DEFAULT GETDATE(),
        Total decimal(10,2) NOT NULL,
        MetodoPago nvarchar(50) NULL,
        IdUsuario int NOT NULL,
        CONSTRAINT FK_Venta_Usuario FOREIGN KEY (IdUsuario)
            REFERENCES Seguridad.Usuario(IdUsuario)
    );
END;
GO

IF OBJECT_ID(N'Ventas.DetalleVenta', N'U') IS NULL
BEGIN
    CREATE TABLE Ventas.DetalleVenta
    (
        IdDetalleVenta int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DetalleVenta PRIMARY KEY,
        IdVenta int NOT NULL,
        IdProducto int NOT NULL,
        Cantidad int NOT NULL,
        PrecioUnitario decimal(10,2) NOT NULL,
        Subtotal decimal(10,2) NOT NULL,
        CONSTRAINT FK_DetalleVenta_Venta FOREIGN KEY (IdVenta)
            REFERENCES Ventas.Venta(IdVenta) ON DELETE CASCADE,
        CONSTRAINT FK_DetalleVenta_Producto FOREIGN KEY (IdProducto)
            REFERENCES Catalogos.Producto(IdProducto)
    );
END;
GO

IF OBJECT_ID(N'Seguridad.Auditoria', N'U') IS NULL
BEGIN
    CREATE TABLE Seguridad.Auditoria
    (
        IdAuditoria int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Auditoria PRIMARY KEY,
        Tabla nvarchar(100) NOT NULL,
        Accion nvarchar(50) NOT NULL,
        UsuarioAccion nvarchar(100) NULL,
        Fecha datetime NOT NULL CONSTRAINT DF_Auditoria_Fecha DEFAULT GETDATE(),
        Datos nvarchar(max) NULL
    );
END;
GO
