CREATE TABLE dbo.ProductCoupons
(
	Id INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_ProductCoupons PRIMARY KEY,
	ProductId INT NOT NULL CONSTRAINT FK_ProductCoupons_ProductId FOREIGN KEY REFERENCES dbo.Products(Id),
	CouponName NVARCHAR(30) NOT NULL,
	CouponCode NVARCHAR(16) NOT NULL,
	CouponMultiplier DECIMAL(9, 2) NOT NULL,
	MaxUses INT NOT NULL,
	IsEnabled BIT NOT NULL,
	IsDeleted BIT NOT NULL CONSTRAINT DF_ProductCoupons_IsDeleted DEFAULT 0,
	CONSTRAINT UK_ProductCoupons UNIQUE (ProductId, CouponCode)
)
