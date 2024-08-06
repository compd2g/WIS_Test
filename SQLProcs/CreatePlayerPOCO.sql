USE [WIS_TEST]
GO

/****** Object:  StoredProcedure [dbo].[CreatePlayerPOCO]    Script Date: 8/4/2024 7:19:38 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[CreatePlayerPOCO]

AS BEGIN

declare @tableName varchar(2000) = 'Player'
declare @columnName varchar(200)
declare @nullable varchar(50)
declare @datatype varchar(50)
declare @maxlen int

declare @sType varchar(50)
declare @sProperty varchar(200)

DECLARE table_cursor CURSOR LOCAL FOR 
SELECT TABLE_NAME
FROM [INFORMATION_SCHEMA].[TABLES]

OPEN table_cursor

FETCH NEXT FROM table_cursor 
INTO @tableName

WHILE @@FETCH_STATUS = 0
BEGIN


PRINT 
	
	'namespace WhatIfSportsTest.Models
	{
		public class ' + @tableName + ' {'

    DECLARE column_cursor CURSOR FOR 
    SELECT COLUMN_NAME, IS_NULLABLE, DATA_TYPE, isnull(CHARACTER_MAXIMUM_LENGTH,'-1') 
    FROM [INFORMATION_SCHEMA].[COLUMNS] 
	WHERE [TABLE_NAME] = @tableName
	ORDER BY [ORDINAL_POSITION]

    OPEN column_cursor
    FETCH NEXT FROM column_cursor INTO @columnName, @nullable, @datatype, @maxlen

    WHILE @@FETCH_STATUS = 0
    BEGIN

	-- datatype
	SELECT @sType = case @datatype
	when 'int' then 'int'
	when 'smallint' then 'Int16?'
	when 'decimal' then 'decimal?'
	when 'money' then 'decimal?'
	when 'char' then 'string?'
	when 'nchar' then 'string?'
	when 'varchar' then 'string?'
	when 'nvarchar' then 'string?'
	when 'uniqueidentifier' then 'guid?'
	when 'datetime' then 'dateTime?'
	when 'bit' then 'bool?'
	else 'string?'
	END

		If (@nullable = 'NO')
			PRINT '[Required]'
		if (@sType = 'String' and @maxLen <> '-1')
			Print '[MaxLength(' +  convert(varchar(4),@maxLen) + ')]'
		SELECT @sProperty = '			public ' + @sType + ' ' + @columnName + ' { get; set;}'
		PRINT @sProperty

		print ''
		FETCH NEXT FROM column_cursor INTO @columnName, @nullable, @datatype, @maxlen
	END
    CLOSE column_cursor
    DEALLOCATE column_cursor

	print '	}'
	print '}'
	print ''
    FETCH NEXT FROM table_cursor 
    INTO @tableName
END
CLOSE table_cursor
END
GO

