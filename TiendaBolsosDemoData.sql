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
