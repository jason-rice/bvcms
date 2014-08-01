ALTER TABLE [dbo].[TagPerson] WITH NOCHECK  ADD CONSTRAINT [Tags__Person] FOREIGN KEY ([PeopleId]) REFERENCES [dbo].[People] ([PeopleId])
ALTER TABLE [dbo].[TagPerson] WITH NOCHECK  ADD CONSTRAINT [PersonTags__Tag] FOREIGN KEY ([Id]) REFERENCES [dbo].[Tag] ([Id])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
