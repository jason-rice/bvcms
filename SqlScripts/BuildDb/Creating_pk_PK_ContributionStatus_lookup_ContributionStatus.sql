ALTER TABLE [lookup].[ContributionStatus] ADD CONSTRAINT [PK_ContributionStatus] PRIMARY KEY CLUSTERED  ([Id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
