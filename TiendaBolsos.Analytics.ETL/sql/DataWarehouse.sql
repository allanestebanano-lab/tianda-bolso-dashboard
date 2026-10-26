IF DB_ID(N'TiendaBolsosDW') IS NULL
BEGIN
    CREATE DATABASE TiendaBolsosDW;
END;
GO

USE TiendaBolsosDW;
GO

IF SCHEMA_ID(N'Analytics') IS NULL
    EXEC(N'CREATE SCHEMA Analytics');
GO

IF OBJECT_ID(N'Analytics.DimDate', N'U') IS NULL
BEGIN
    CREATE TABLE Analytics.DimDate
    (
        DateKey int NOT NULL CONSTRAINT PK_DimDate PRIMARY KEY,
        [Date] date NOT NULL CONSTRAINT UQ_DimDate_Date UNIQUE,
        [Year] smallint NOT NULL,
        [Quarter] tinyint NOT NULL,
        [Month] tinyint NOT NULL,
        MonthName nvarchar(20) NOT NULL,
        [Day] tinyint NOT NULL,
        DayOfWeek tinyint NOT NULL
    );
END;
GO

IF OBJECT_ID(N'Analytics.DimProduct', N'U') IS NULL
BEGIN
    CREATE TABLE Analytics.DimProduct
    (
        ProductKey int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DimProduct PRIMARY KEY,
        SourceProductId int NOT NULL CONSTRAINT UQ_DimProduct_SourceProductId UNIQUE,
        ProductName nvarchar(150) NOT NULL,
        CategoryName nvarchar(100) NOT NULL,
        BrandName nvarchar(100) NOT NULL,
        UnitPrice decimal(10,2) NOT NULL,
        IsActive bit NOT NULL,
        UpdatedAtUtc datetime2(0) NOT NULL
            CONSTRAINT DF_DimProduct_UpdatedAtUtc DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF OBJECT_ID(N'Analytics.FactSale', N'U') IS NULL
BEGIN
    CREATE TABLE Analytics.FactSale
    (
        SourceDetailId int NOT NULL CONSTRAINT PK_FactSale PRIMARY KEY,
        SourceSaleId int NOT NULL,
        DateKey int NOT NULL,
        ProductKey int NOT NULL,
        PaymentMethod nvarchar(50) NULL,
        Quantity int NOT NULL,
        UnitPrice decimal(10,2) NOT NULL,
        Amount decimal(10,2) NOT NULL,
        CONSTRAINT FK_FactSale_DimDate FOREIGN KEY (DateKey)
            REFERENCES Analytics.DimDate(DateKey),
        CONSTRAINT FK_FactSale_DimProduct FOREIGN KEY (ProductKey)
            REFERENCES Analytics.DimProduct(ProductKey)
    );

    CREATE INDEX IX_FactSale_DateKey ON Analytics.FactSale(DateKey);
    CREATE INDEX IX_FactSale_ProductKey ON Analytics.FactSale(ProductKey);
END;
GO

IF OBJECT_ID(N'Analytics.FactInventorySnapshot', N'U') IS NULL
BEGIN
    CREATE TABLE Analytics.FactInventorySnapshot
    (
        DateKey int NOT NULL,
        ProductKey int NOT NULL,
        StockQuantity int NOT NULL,
        MinimumStock int NOT NULL,
        CONSTRAINT PK_FactInventorySnapshot PRIMARY KEY (DateKey, ProductKey),
        CONSTRAINT FK_FactInventorySnapshot_DimDate FOREIGN KEY (DateKey)
            REFERENCES Analytics.DimDate(DateKey),
        CONSTRAINT FK_FactInventorySnapshot_DimProduct FOREIGN KEY (ProductKey)
            REFERENCES Analytics.DimProduct(ProductKey)
    );
END;
GO

IF OBJECT_ID(N'Analytics.FactApiRequest', N'U') IS NULL
BEGIN
    CREATE TABLE Analytics.FactApiRequest
    (
        RequestId uniqueidentifier NOT NULL CONSTRAINT PK_FactApiRequest PRIMARY KEY,
        DateKey int NOT NULL,
        TimestampUtc datetime2(3) NOT NULL,
        HttpMethod nvarchar(10) NOT NULL,
        Route nvarchar(300) NOT NULL,
        StatusCode int NOT NULL,
        DurationMilliseconds decimal(12,3) NOT NULL,
        IsServerError bit NOT NULL,
        Environment nvarchar(100) NOT NULL,
        CONSTRAINT FK_FactApiRequest_DimDate FOREIGN KEY (DateKey)
            REFERENCES Analytics.DimDate(DateKey)
    );

    CREATE INDEX IX_FactApiRequest_TimestampUtc
        ON Analytics.FactApiRequest(TimestampUtc);
END;
GO

IF OBJECT_ID(N'Analytics.EtlWatermark', N'U') IS NULL
BEGIN
    CREATE TABLE Analytics.EtlWatermark
    (
        PipelineName nvarchar(100) NOT NULL CONSTRAINT PK_EtlWatermark PRIMARY KEY,
        LastSourceId bigint NOT NULL CONSTRAINT DF_EtlWatermark_LastSourceId DEFAULT 0,
        LastTimestampUtc datetime2(3) NULL,
        UpdatedAtUtc datetime2(3) NOT NULL
            CONSTRAINT DF_EtlWatermark_UpdatedAtUtc DEFAULT SYSUTCDATETIME()
    );
END;
GO
