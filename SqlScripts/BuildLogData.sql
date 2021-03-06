CREATE TABLE [dbo].[RequestLog](
	[dbname] [varchar](50) NULL,
	[dt] [datetime] NOT NULL,
	[userid] [varchar](50) NOT NULL,
	[method] [varchar](10) NULL,
	[controller] [varchar](25) NOT NULL,
	[action] [varchar](25) NOT NULL,
	[duration] [float] NOT NULL,
	[newui] [bit] NULL,
	[id] [uniqueidentifier] NOT NULL,
	[qs] [varchar](100) NULL,
	CONSTRAINT [PK_RequestLog] PRIMARY KEY CLUSTERED ( [id] ASC) 
		WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
		ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE PROCEDURE [dbo].[LogRequest](
	@dbname VARCHAR(50),
	@method VARCHAR(10), 
	@controller VARCHAR(25), 
	@action VARCHAR(25), 
	@userid VARCHAR(50),
	@newui BIT = NULL,
	@id UNIQUEIDENTIFIER
	)
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO dbo.RequestLog
	        ( dbname, dt, method, controller, [action], userid, duration, newui, id)
	VALUES  ( @dbname, GETDATE(), @method, @controller, @action, @userid, -1, @newui, @id)
END
GO
CREATE PROCEDURE [dbo].[LogRequest2](
	@dbname VARCHAR(50),
	@method VARCHAR(10), 
	@controller VARCHAR(25), 
	@action VARCHAR(25), 
	@userid VARCHAR(50),
	@newui BIT = NULL,
	@id UNIQUEIDENTIFIER,
	@qs VARCHAR(100), 
	)
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO dbo.RequestLog
	        ( dbname, dt, method, controller, [action], userid, duration, newui, id, qs)
	VALUES  ( @dbname, GETDATE(), @method, @controller, @action, @userid, -1, @newui, @id, @qs)
END
GO
CREATE TABLE [dbo].[Browsers](
	[stamp] [datetime] NULL,
	[browser] [nvarchar](50) NULL,
	[who] [nvarchar](50) NULL,
	[host] [nvarchar](50) NULL,
	[host2] [nchar](10) NULL
) ON [PRIMARY]
GO
CREATE PROCEDURE [dbo].[LogBrowser](
	@browser VARCHAR(50),
	@who VARCHAR(25), 
	@host VARCHAR(50)
	)
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO dbo.Browsers ( stamp , browser , who , host )
	VALUES  ( GETDATE(), @browser, @who, @host)
END
GO
