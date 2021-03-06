
IF OBJECT_ID('[dbo].[PledgesSummary]') IS NOT NULL
DROP FUNCTION [dbo].[PledgesSummary] 
GO
CREATE FUNCTION [dbo].[PledgesSummary] ( @pid INT )
RETURNS
@pledgesSummary TABLE (FundId INT NOT NULL, FundName NVARCHAR(max), AmountPledged DECIMAL(38,2) NOT NULL, AmountContributed DECIMAL(38,2), Balance DECIMAL(38,2) NOT NULL)
AS
BEGIN
	with summary as (
		SELECT cf.FundId, cf.FundName, SUM(c.ContributionAmount) AmountPledged
		FROM Contribution c
		JOIN ContributionFund cf ON cf.FundId = c.FundId
		JOIN lookup.ContributionType ct ON c.ContributionTypeId = ct.Id 
		WHERE ct.Description = 'Pledge'
		AND c.PeopleId = @pid
		GROUP BY cf.FundId, cf.FundName, ct.Description
	),
	contributed as (
		SELECT cf.FundId, cf.FundName, SUM(c.ContributionAmount) AmountContributed
		FROM Contribution c
		JOIN ContributionFund cf ON cf.FundId = c.FundId
		JOIN lookup.ContributionType ct ON c.ContributionTypeId = ct.Id 
		WHERE ct.Description <> 'Pledge'
		AND c.PeopleId = @pid
		GROUP BY cf.FundId, cf.FundName
	)
	INSERT INTO @pledgesSummary
	SELECT c.FundId, c.FundName, s.AmountPledged, c.AmountContributed, IIF(s.AmountPledged - c.AmountContributed < 0, 0, s.AmountPledged - c.AmountContributed) Balance 
	FROM summary s
	JOIN contributed c ON c.FundId = s.FundId

	RETURN
END
GO
