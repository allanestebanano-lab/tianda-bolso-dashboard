USE TiendaBolsosDB;
GO

IF NOT EXISTS (SELECT 1 FROM Catalogos.Categoria WHERE Nombre = N'Bolsos')
    INSERT INTO Catalogos.Categoria (Nombre) VALUES (N'Bolsos');
IF NOT EXISTS (SELECT 1 FROM Catalogos.Categoria WHERE Nombre = N'Accesorios')
    INSERT INTO Catalogos.Categoria (Nombre) VALUES (N'Accesorios');

IF NOT EXISTS (SELECT 1 FROM Catalogos.Marca WHERE Nombre = N'Demo Bags')
    INSERT INTO Catalogos.Marca (Nombre) VALUES (N'Demo Bags');
IF NOT EXISTS (SELECT 1 FROM Catalogos.Marca WHERE Nombre = N'Coleccion Demo')
    INSERT INTO Catalogos.Marca (Nombre) VALUES (N'Coleccion Demo');
GO

DECLARE @BolsosCategoryId int =
    (SELECT IdCategoria FROM Catalogos.Categoria WHERE Nombre = N'Bolsos');
DECLARE @AccessoriesCategoryId int =
    (SELECT IdCategoria FROM Catalogos.Categoria WHERE Nombre = N'Accesorios');
DECLARE @DemoBrandId int =
    (SELECT IdMarca FROM Catalogos.Marca WHERE Nombre = N'Demo Bags');
DECLARE @CollectionBrandId int =
    (SELECT IdMarca FROM Catalogos.Marca WHERE Nombre = N'Coleccion Demo');

INSERT INTO Catalogos.Producto
    (CodigoProducto, Nombre, Descripcion, Precio, Stock, Activo, IdCategoria, IdMarca)
SELECT N'DEMO-BOL-001', N'Bolso urbano demo', N'Producto de prueba para ETL y dashboard',
       49.99, 25, 1, @BolsosCategoryId, @DemoBrandId
WHERE NOT EXISTS
    (SELECT 1 FROM Catalogos.Producto WHERE CodigoProducto = N'DEMO-BOL-001');

INSERT INTO Catalogos.Producto
    (CodigoProducto, Nombre, Descripcion, Precio, Stock, Activo, IdCategoria, IdMarca)
SELECT N'DEMO-BOL-002', N'Bolso elegante demo', N'Producto de prueba para ventas',
       79.50, 12, 1, @BolsosCategoryId, @CollectionBrandId
WHERE NOT EXISTS
    (SELECT 1 FROM Catalogos.Producto WHERE CodigoProducto = N'DEMO-BOL-002');

INSERT INTO Catalogos.Producto
    (CodigoProducto, Nombre, Descripcion, Precio, Stock, Activo, IdCategoria, IdMarca)
SELECT N'DEMO-ACC-001', N'Accesorio demo', N'Producto de prueba de bajo costo',
       14.00, 4, 1, @AccessoriesCategoryId, @DemoBrandId
WHERE NOT EXISTS
    (SELECT 1 FROM Catalogos.Producto WHERE CodigoProducto = N'DEMO-ACC-001');

INSERT INTO Catalogos.Inventory (IdProducto, Stock, MinStock)
SELECT p.IdProducto, p.Stock, 5
FROM Catalogos.Producto AS p
WHERE p.CodigoProducto IN (N'DEMO-BOL-001', N'DEMO-BOL-002', N'DEMO-ACC-001')
  AND NOT EXISTS
      (SELECT 1 FROM Catalogos.Inventory AS i WHERE i.IdProducto = p.IdProducto);
GO

DECLARE @UserId int =
(
    SELECT TOP (1) IdUsuario
    FROM Seguridad.Usuario
    WHERE Rol IN (N'Admin', N'Gerente')
    ORDER BY IdUsuario
);

IF @UserId IS NULL
    THROW 51000, 'Registra una cuenta y asígnale el rol Admin o Gerente antes de insertar ventas demo.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM Ventas.Venta
    WHERE MetodoPago IN (N'DEMO-ETL-01', N'DEMO-ETL-02', N'DEMO-ETL-03')
)
BEGIN
    DECLARE @UrbanBagId int =
        (SELECT IdProducto FROM Catalogos.Producto WHERE CodigoProducto = N'DEMO-BOL-001');
    DECLARE @ElegantBagId int =
        (SELECT IdProducto FROM Catalogos.Producto WHERE CodigoProducto = N'DEMO-BOL-002');
    DECLARE @AccessoryId int =
        (SELECT IdProducto FROM Catalogos.Producto WHERE CodigoProducto = N'DEMO-ACC-001');

    IF @UrbanBagId IS NULL OR @ElegantBagId IS NULL OR @AccessoryId IS NULL
        THROW 51001, 'No se encontraron los tres productos demo esperados.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM Catalogos.Producto
        WHERE (IdProducto = @UrbanBagId AND Stock < 3)
           OR (IdProducto = @ElegantBagId AND Stock < 1)
           OR (IdProducto = @AccessoryId AND Stock < 1)
    )
        THROW 51002, 'El stock no alcanza para insertar las ventas demo.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Ventas.Venta (Fecha, Total, MetodoPago, IdUsuario)
        VALUES (DATEADD(DAY, -1, GETDATE()), 129.49, N'DEMO-ETL-01', @UserId);
        DECLARE @SaleOne int = SCOPE_IDENTITY();

        INSERT INTO Ventas.DetalleVenta
            (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal)
        VALUES
            (@SaleOne, @UrbanBagId, 1, 49.99, 49.99),
            (@SaleOne, @ElegantBagId, 1, 79.50, 79.50);

        INSERT INTO Ventas.Venta (Fecha, Total, MetodoPago, IdUsuario)
        VALUES (DATEADD(DAY, -2, GETDATE()), 99.98, N'DEMO-ETL-02', @UserId);
        DECLARE @SaleTwo int = SCOPE_IDENTITY();

        INSERT INTO Ventas.DetalleVenta
            (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal)
        VALUES (@SaleTwo, @UrbanBagId, 2, 49.99, 99.98);

        INSERT INTO Ventas.Venta (Fecha, Total, MetodoPago, IdUsuario)
        VALUES (DATEADD(DAY, -3, GETDATE()), 14.00, N'DEMO-ETL-03', @UserId);
        DECLARE @SaleThree int = SCOPE_IDENTITY();

        INSERT INTO Ventas.DetalleVenta
            (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal)
        VALUES (@SaleThree, @AccessoryId, 1, 14.00, 14.00);

        UPDATE Catalogos.Producto
        SET Stock = Stock - CASE
            WHEN IdProducto = @UrbanBagId THEN 3
            WHEN IdProducto = @ElegantBagId THEN 1
            WHEN IdProducto = @AccessoryId THEN 1
        END
        WHERE IdProducto IN (@UrbanBagId, @ElegantBagId, @AccessoryId);

        UPDATE Catalogos.Inventory
        SET Stock = Stock - CASE
            WHEN IdProducto = @UrbanBagId THEN 3
            WHEN IdProducto = @ElegantBagId THEN 1
            WHEN IdProducto = @AccessoryId THEN 1
        END,
        LastUpdate = GETDATE()
        WHERE IdProducto IN (@UrbanBagId, @ElegantBagId, @AccessoryId);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
