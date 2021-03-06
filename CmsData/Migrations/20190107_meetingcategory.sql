IF NOT EXISTS (SELECT * FROM sys.tables t JOIN sys.schemas s ON (t.schema_id = s.schema_id)
WHERE s.name = 'dbo' AND t.name = 'MeetingCategory')
BEGIN
	CREATE TABLE [dbo].[MeetingCategory](
		[Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
		[Description] [NVARCHAR](100) NOT NULL,
		[NotBeforeDate] [datetime] NULL,
		[NotAfterDate] [datetime] NULL,
		[IsExpired] [bit] NOT NULL DEFAULT(0)
	)
END
GO
