ALTER TABLE [dbo].[ContactExtra] ADD CONSTRAINT [PK_ContactExtra] PRIMARY KEY CLUSTERED  ([ContactId], [Field]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
