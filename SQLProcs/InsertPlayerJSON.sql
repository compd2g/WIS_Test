USE [WIS_TEST]
GO

/****** Object:  StoredProcedure [dbo].[InsertPlayerJSON]    Script Date: 8/4/2024 7:20:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[InsertPlayerJSON]

AS BEGIN

Declare @JSON varchar(max)
SELECT @JSON=BulkColumn
FROM OPENROWSET(BULK 'C:\results.JSON', SINGLE_CLOB) AS j
SELECT * INTO dbo.Player
FROM OPENJSON(@JSON)
WITH
(
	[id] int,
    [firstname] varchar(50), 
    [lastname] varchar(50),
	[fullname] varchar(100),
	[pro_team] varchar(3),
	[pro_status] char,
	[elias_id] varchar(20),
	[age] varchar(3),
	[jersey] varchar(2),
	[position] varchar(10),
	[eligible_for_offense_and_defense] bit,
	[photo] varchar(100),
	[eligible_positions_display] varchar(10),
	[icons] varchar(50),
	[stub] varchar(5)
)
END
GO

