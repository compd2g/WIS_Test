USE [WIS_TEST]
GO

/****** Object:  StoredProcedure [dbo].[GetPlayerList]    Script Date: 8/4/2024 7:20:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE   PROCEDURE [dbo].[GetPlayerList] @id int = NULL
AS
SELECT 
		id,
		firstname as first_name,
		lastname as last_name,
		position,
		age,
		CONCAT(SUBSTRING(firstname, 1, 1), lastname) AS stub	
FROM Player
WHERE id = ISNULL(@id, id)

GO

