CREATE FUNCTION [dbo].[EnrollmentHistory](@pid INT, @orgid INT) 
RETURNS TABLE  
AS 
RETURN  
( 
	WITH hist AS ( 
		SELECT 
			e.*, 
			ISNULL(LAG(e.TransactionTypeId) OVER (ORDER BY e.TransactionDate, e.TransactionTypeId), 0) PrevTranType 
		FROM dbo.EnrollmentTransaction e 
		WHERE e.PeopleId = @pid AND e.OrganizationId = @orgid 
	), 
	good AS ( 
		SELECT h.*, 
		isgood = IIF((h.TransactionTypeId IN (3,5) AND h.PrevTranType IN (1,3)) 
				OR (h.TransactionTypeId = 1 AND h.PrevTranType IN (5, 0)),  
			1, 0) 
		FROM hist h 
	) 
	SELECT *, mt.Description MemberType  
	FROM good g 
	JOIN lookup.MemberType mt ON mt.Id = g.MemberTypeId 
) 
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
