ALTER TABLE [dbo].[ChangeLog] ADD CONSTRAINT [PK_ChangeLog_1] PRIMARY KEY CLUSTERED  ([Id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
