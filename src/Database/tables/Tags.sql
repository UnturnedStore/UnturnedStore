﻿CREATE TABLE dbo.Tags
(
	Id INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Tags PRIMARY KEY,
	Title NVARCHAR(35) NOT NULL CONSTRAINT UK_Tags_Title UNIQUE,
	Color NVARCHAR(7) NOT NULL CONSTRAINT DF_Tags_Color DEFAULT '#6c757d',
	BackgroundColor NVARCHAR(7) NOT NULL CONSTRAINT DF_Tags_BackgroundColor DEFAULT '#ebebef'
)
