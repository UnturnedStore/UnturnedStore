﻿CREATE TABLE dbo.Branches
(
	Id INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Branches PRIMARY KEY,
	ProductId INT NOT NULL CONSTRAINT FK_Branches_ProductId FOREIGN KEY REFERENCES dbo.Products(Id),
	Name NVARCHAR(50) NOT NULL,
	Description NVARCHAR(255) NOT NULL,
	IsEnabled BIT NOT NULL CONSTRAINT DF_Branches_IsEnabled DEFAULT 1,
	CreateDate DATETIME2(0) NOT NULL CONSTRAINT DF_Branches_CreateDate DEFAULT SYSDATETIME(),
	CONSTRAINT UK_Branches_ProductIdName UNIQUE (ProductId, Name)
)
