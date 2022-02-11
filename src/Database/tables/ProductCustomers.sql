CREATE TABLE dbo.ProductCustomers
(
	Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductCustomers PRIMARY KEY,
	ProductId INT NOT NULL CONSTRAINT FK_ProductCustomers_ProductId FOREIGN KEY REFERENCES dbo.Products(Id),
	UserId INT NOT NULL CONSTRAINT FK_ProductCustomers_UserId FOREIGN KEY REFERENCES dbo.Users(Id),
	LicenseKey UNIQUEIDENTIFIER CONSTRAINT DF_ProductCustomers_LicenseKey DEFAULT NEWID(),
	IsBlocked BIT NOT NULL CONSTRAINT DF_ProductCustomers_IsBlocked DEFAULT 0,
	BlockDate DATETIME2(0) NULL,
	CreateDate DATETIME2(0) NOT NULL CONSTRAINT DF_ProductCustomers_CreateDate DEFAULT SYSDATETIME(),
	CONSTRAINT UK_ProductCustomers_ProductUserId UNIQUE (ProductId, UserId),
	CONSTRAINT UK_ProductCustomers_LicenseKey UNIQUE (LicenseKey)
)
