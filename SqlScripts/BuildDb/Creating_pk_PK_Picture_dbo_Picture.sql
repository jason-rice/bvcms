ALTER TABLE [dbo].[Picture] ADD CONSTRAINT [PK_Picture] PRIMARY KEY CLUSTERED  ([PictureId]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
