ALTER TABLE [dbo].[FamilyExtra] ADD CONSTRAINT [FK_FamilyExtra_Family] FOREIGN KEY ([FamilyId]) REFERENCES [dbo].[Families] ([FamilyId])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
