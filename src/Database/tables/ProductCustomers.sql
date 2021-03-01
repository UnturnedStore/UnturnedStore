CREATE TABLE [dbo].[ProductCustomers]
(
	Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductCustomers PRIMARY KEY,
	ProductId INT NOT NULL CONSTRAINT FK_ProductCustomers_ProductId FOREIGN KEY REFERENCES dbo.Products(Id),
	UserId INT NOT NULL CONSTRAINT FK_ProductCustomers_UserId FOREIGN KEY REFERENCES dbo.Users(Id),
	CreateDate DATETIME2(0) NOT NULL CONSTRAINT DF_ProductCustomers_CreateDate DEFAULT SYSDATETIME(),
	CONSTRAINT UK_ProductCustomers UNIQUE (ProductId, UserId)
)
