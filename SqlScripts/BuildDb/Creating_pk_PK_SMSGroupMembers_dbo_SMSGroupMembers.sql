ALTER TABLE [dbo].[SMSGroupMembers] ADD CONSTRAINT [PK_SMSGroupMembers] PRIMARY KEY CLUSTERED  ([ID]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
