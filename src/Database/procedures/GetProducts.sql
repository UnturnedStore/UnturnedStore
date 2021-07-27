CREATE PROCEDURE dbo.GetProducts
AS
BEGIN
	WITH CTE_ProductDownloads AS (
		SELECT ProductId, TotalDownloadsCount = SUM(v.DownloadsCount) FROM dbo.Branches b LEFT JOIN dbo.Versions v ON v.BranchId = b.Id GROUP BY ProductId
	), 
	CTE_ProductRating AS (
		SELECT ProductId, AverageRating = AVG(r.Rating), RatingsCount = COUNT(*) FROM dbo.Products p LEFT JOIN dbo.ProductReviews r ON p.Id = r.ProductId GROUP BY ProductId
	)
	SELECT 
		p.Id,
		p.Name,
		p.Description,
		p.Category,
		p.GithubUrl,
		p.ImageId,
		p.Price,
		p.SellerId,
		p.IsEnabled,
		p.LastUpdate,
		p.CreateDate,
		TotalDownloadsCount = ISNULL(d.TotalDownloadsCount, 0), 
		AverageRating = ISNULL(r.AverageRating, 0), 
		RatingsCount = ISNULL(RatingsCount, 0),
		u.Id, 
		u.Name, 
		u.Role, 
		u.SteamId, 
		u.CreateDate		
	FROM dbo.Products p 
	JOIN dbo.Users u ON p.SellerId = u.Id 
	LEFT JOIN CTE_ProductDownloads d ON d.ProductId = p.Id
	LEFT JOIN CTE_ProductRating r ON r.ProductId = p.Id;
END;