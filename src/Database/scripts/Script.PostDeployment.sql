UPDATE dbo.Products 
SET 
	ReleaseDate = CreateDate
WHERE ReleaseDate IS NULL AND Status = 4;

/*
ProductStatus:
New = 0,
WaitingForApproval = 1,
Approved = 2,
Rejected = 3,
Released = 4,
Disabled = 5
*/