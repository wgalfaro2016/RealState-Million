IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'RealEstateDb')
    CREATE DATABASE [RealEstateDb];
GO

USE [RealEstateDb];
GO

IF OBJECT_ID('dbo.Owner','U') IS NULL
BEGIN
    CREATE TABLE dbo.Owner
    (
        IdOwner     UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Owner_Id DEFAULT NEWID(),
        [Name]      NVARCHAR(150)    NOT NULL,
        [Address]   NVARCHAR(250)    NULL,
        Photo       NVARCHAR(300)    NULL,
        Birthday    DATE             NULL,

        CONSTRAINT PK_Owner PRIMARY KEY (IdOwner)
    );
END
GO

IF OBJECT_ID('dbo.Property','U') IS NULL
BEGIN
    CREATE TABLE dbo.Property
    (
        IdProperty    UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Property_Id DEFAULT NEWID(),
        [Name]        NVARCHAR(150)    NOT NULL,
        [Address]     NVARCHAR(250)    NOT NULL,
        Price         DECIMAL(18,2)    NOT NULL,
        CodeInternal  NVARCHAR(50)     NOT NULL,
        [Year]        INT              NOT NULL,
        IdOwner       UNIQUEIDENTIFIER NOT NULL,

        CONSTRAINT PK_Property PRIMARY KEY (IdProperty),
        CONSTRAINT UQ_Property_CodeInternal UNIQUE (CodeInternal),
        CONSTRAINT FK_Property_Owner_IdOwner
            FOREIGN KEY (IdOwner) REFERENCES dbo.Owner (IdOwner)
            ON DELETE NO ACTION ON UPDATE NO ACTION
    );

    CREATE INDEX IX_Property_Price ON dbo.Property (Price);
    CREATE INDEX IX_Property_Year  ON dbo.Property ([Year]);
END
GO


IF OBJECT_ID('dbo.PropertyImage','U') IS NULL
BEGIN
    CREATE TABLE dbo.PropertyImage
    (
        IdPropertyImage UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PropertyImage_Id DEFAULT NEWID(),
        IdProperty      UNIQUEIDENTIFIER NOT NULL,
        [File]          NVARCHAR(300)    NOT NULL,
        Enabled         BIT              NOT NULL CONSTRAINT DF_PropertyImage_Enabled DEFAULT (1),

        CONSTRAINT PK_PropertyImage PRIMARY KEY (IdPropertyImage),
        CONSTRAINT FK_PropertyImage_Property_IdProperty
            FOREIGN KEY (IdProperty) REFERENCES dbo.Property (IdProperty)
            ON DELETE CASCADE ON UPDATE NO ACTION
    );

    CREATE INDEX IX_PropertyImage_IdProperty ON dbo.PropertyImage (IdProperty);
END
GO


IF OBJECT_ID('dbo.PropertyTrace','U') IS NULL
BEGIN
    CREATE TABLE dbo.PropertyTrace
    (
        IdPropertyTrace UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PropertyTrace_Id DEFAULT NEWID(),
        DateSale        DATETIME2(0)     NOT NULL,
        [Name]          NVARCHAR(150)    NOT NULL,
        [Value]         DECIMAL(18,2)    NOT NULL,
        Tax             DECIMAL(18,2)    NOT NULL,
        IdProperty      UNIQUEIDENTIFIER NOT NULL,

        CONSTRAINT PK_PropertyTrace PRIMARY KEY (IdPropertyTrace),
        CONSTRAINT FK_PropertyTrace_Property_IdProperty
            FOREIGN KEY (IdProperty) REFERENCES dbo.Property (IdProperty)
            ON DELETE CASCADE ON UPDATE NO ACTION
    );

    CREATE INDEX IX_PropertyTrace_DateSale   ON dbo.PropertyTrace (DateSale);
    CREATE INDEX IX_PropertyTrace_IdProperty ON dbo.PropertyTrace (IdProperty);
END
GO
