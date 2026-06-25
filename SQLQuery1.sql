IF DB_ID(N'FavoriteProductsDb') IS NULL
BEGIN
    CREATE DATABASE FavoriteProductsDb;
END
GO

USE FavoriteProductsDb;
GO

DROP TABLE IF EXISTS dbo.UserFavoriteProducts;
DROP TABLE IF EXISTS dbo.AppUserRoles;
DROP TABLE IF EXISTS dbo.Products;
DROP TABLE IF EXISTS dbo.AppUsers;
DROP TABLE IF EXISTS dbo.Roles;
GO

CREATE TABLE dbo.AppUsers
(
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    UserName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,

    IsActive BIT NOT NULL 
        CONSTRAINT DF_AppUsers_IsActive DEFAULT (1),

    CreatedAt DATETIME2 NOT NULL 
        CONSTRAINT DF_AppUsers_CreatedAt DEFAULT (SYSUTCDATETIME()),

    UpdatedAt DATETIME2 NULL,

    CONSTRAINT UQ_AppUsers_UserName UNIQUE (UserName),
    CONSTRAINT UQ_AppUsers_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.Roles
(
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    Name NVARCHAR(50) NOT NULL,

    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.AppUserRoles
(
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    UserId INT NOT NULL,
    RoleId INT NOT NULL,

    CreatedAt DATETIME2 NOT NULL 
        CONSTRAINT DF_AppUserRoles_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_AppUserRoles_AppUsers
        FOREIGN KEY (UserId) REFERENCES dbo.AppUsers(Id),

    CONSTRAINT FK_AppUserRoles_Roles
        FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id),

    CONSTRAINT UQ_AppUserRoles_User_Role
        UNIQUE (UserId, RoleId)
);
GO

CREATE TABLE dbo.Products
(
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(100) NULL,
    Description NVARCHAR(500) NULL,

    IsActive BIT NOT NULL 
        CONSTRAINT DF_Products_IsActive DEFAULT (1),

    CreatedAt DATETIME2 NOT NULL 
        CONSTRAINT DF_Products_CreatedAt DEFAULT (SYSUTCDATETIME()),

    UpdatedAt DATETIME2 NULL
);
GO

CREATE UNIQUE INDEX UX_Products_Code
ON dbo.Products(Code)
WHERE Code IS NOT NULL;
GO

CREATE TABLE dbo.UserFavoriteProducts
(
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    UserId INT NOT NULL,
    ProductId INT NOT NULL,

    CreatedAt DATETIME2 NOT NULL 
        CONSTRAINT DF_UserFavoriteProducts_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_UserFavoriteProducts_AppUsers
        FOREIGN KEY (UserId) REFERENCES dbo.AppUsers(Id),

    CONSTRAINT FK_UserFavoriteProducts_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),

    CONSTRAINT UQ_UserFavoriteProducts_User_Product
        UNIQUE (UserId, ProductId)
);
GO

CREATE INDEX IX_AppUserRoles_UserId
ON dbo.AppUserRoles(UserId);
GO

CREATE INDEX IX_AppUserRoles_RoleId
ON dbo.AppUserRoles(RoleId);
GO

CREATE INDEX IX_UserFavoriteProducts_UserId
ON dbo.UserFavoriteProducts(UserId);
GO

CREATE INDEX IX_UserFavoriteProducts_ProductId
ON dbo.UserFavoriteProducts(ProductId);
GO

INSERT INTO dbo.Roles (Name)
VALUES
('User'),
('Admin');
GO

INSERT INTO dbo.Products
(
    Name,
    Code,
    Description
)
VALUES
(N'???? 1', 'P001', N'????? ???? 1'),
(N'???? 2', 'P002', N'????? ???? 2'),
(N'???? 3', 'P003', N'????? ???? 3'),
(N'???? 4', 'P004', N'????? ???? 4'),
(N'???? 5', 'P005', N'????? ???? 5');
GO

SELECT 
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

SELECT * FROM dbo.Roles;
GO

SELECT * FROM dbo.Products;
GO