UPDATE dbo.Products 
SET 
	ReleaseDate = CreateDate
WHERE ReleaseDate IS NULL AND Status = 4;

DELETE r FROM dbo.MessageReplies r JOIN dbo.Messages m ON r.MessageId = m.Id WHERE m.ToUserId = m.FromUserId;
DELETE mr FROM dbo.MessagesRead mr JOIN dbo.Messages m ON mr.MessageId = m.Id WHERE m.ToUserId = m.FromUserId;
DELETE FROM dbo.Messages WHERE ToUserId = FromUserId;

/*
ProductStatus:
New = 0,
WaitingForApproval = 1,
Approved = 2,
Rejected = 3,
Released = 4,
Disabled = 5
*/