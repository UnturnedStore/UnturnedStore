CREATE TABLE dbo.CustomerServers
(
	CustomerId INT NOT NULL CONSTRAINT FK_CustomerServers_CustomerId FOREIGN KEY REFERENCES dbo.ProductCustomers(Id),
	ServerName NVARCHAR(255) NULL,
	Host VARCHAR(255) NOT NULL,
	Port INT NOT NULL,
	TimesLoaded INT NOT NULL CONSTRAINT DF_CustomerServers_TimesLoaded DEFAULT 1,
	UpdateDate DATETIME2(0) NOT NULL CONSTRAINT DF_CustomerServers_UpdateDate DEFAULT SYSDATETIME(),
	CreateDate DATETIME2(0) NOT NULL CONSTRAINT DF_CustomerServers_CreateDate DEFAULT SYSDATETIME()
	CONSTRAINT UK_CustomerServers_HostPort UNIQUE (Host, Port)
)
