﻿CREATE TABLE dbo.Versions
(
	Id INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Versions PRIMARY KEY,
	BranchId INT NOT NULL CONSTRAINT FK_Versions_BranchId FOREIGN KEY REFERENCES dbo.Branches(Id),
	Name VARCHAR(255) NOT NULL,
	Changelog NVARCHAR(2000) NULL,
	FileName NVARCHAR(255) NOT NULL,
	ContentType NVARCHAR(255) NOT NULL,
	Content VARBINARY(MAX) NOT NULL,
	DownloadsCount INT NOT NULL CONSTRAINT DF_Versions_DownloadsCount DEFAULT 0,
	IsEnabled BIT NOT NULL CONSTRAINT DF_Versions_IsEnabled DEFAULT 0,
	CreateDate DATETIME2(0) NOT NULL CONSTRAINT DF_Versions_CreateDate DEFAULT SYSDATETIME(),
    PluginHash NVARCHAR(128) COLLATE SQL_Latin1_General_CP1_CI_AS
)