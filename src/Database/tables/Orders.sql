﻿CREATE TABLE dbo.Orders
(
	Id INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
	PaymentId UNIQUEIDENTIFIER NOT NULL CONSTRAINT UK_Orders UNIQUE,
	BuyerId INT NOT NULL CONSTRAINT FK_Orders_BuyerId FOREIGN KEY REFERENCES dbo.Users(Id),
	SellerId INT NOT NULL CONSTRAINT FK_Orders_SellerId FOREIGN KEY REFERENCES dbo.Users(Id),
	TotalPrice DECIMAL(9, 2) NOT NULL,
	Currency CHAR(3) NOT NULL,
	PaymentMethod NVARCHAR(255) NOT NULL,
	PaymentReceiver NVARCHAR(MAX) NOT NULL,
	PaymentSender NVARCHAR(MAX) NULL,
	TransactionId NVARCHAR(255) NULL,
	Status VARCHAR(255) NOT NULL,
	LastUpdate DATETIME2(0) NOT NULL CONSTRAINT DF_Orders_LastUpdate DEFAULT SYSDATETIME(),
	CreateDate DATETIME2(0) NOT NULL CONSTRAINT DF_Orders_CreateDate DEFAULT SYSDATETIME()
);
