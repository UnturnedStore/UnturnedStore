-- THIS PROCEDURE IS DEPRECATED

CREATE PROCEDURE dbo.GetPromotedProducts
	@UserId INT
AS
BEGIN
	WITH CTE_ProductDownloads AS (
		SELECT 
			ProductId, 
			TotalDownloadsCount = SUM(v.DownloadsCount)
		FROM dbo.Branches b 
		LEFT JOIN dbo.Versions v ON v.BranchId = b.Id 
		GROUP BY ProductId
	), 
	CTE_ProductRating AS (
		SELECT 
			ProductId, 
			AverageRating = AVG(r.Rating), 
			RatingsCount = COUNT(*) 
		FROM dbo.Products p 
		LEFT JOIN dbo.ProductReviews r ON p.Id = r.ProductId 
		GROUP BY ProductId
	),
	CTE_ProductServers AS (
		SELECT
			ProductId,
			ServersCount = COUNT(*)
		FROM dbo.CustomerServers cs 
		JOIN dbo.ProductCustomers c ON c.Id = cs.CustomerId
		GROUP BY ProductId
	)

	SELECT TOP 3
		p.Id,
		p.Name,
		p.Description,
		p.Category,
		p.GithubUrl,
		p.ImageId,
		p.Price,
		p.SellerId,
		p.IsEnabled,
		p.IsLoaderEnabled,
		p.LastUpdate,
		p.CreateDate,
		TotalDownloadsCount = ISNULL(d.TotalDownloadsCount, 0), 
		AverageRating = ISNULL(r.AverageRating, 0), 
		RatingsCount = ISNULL(RatingsCount, 0),
		u.Id, 
		u.Name, 
		u.Role, 
		u.SteamId, 
		u.AvatarImageId,
		u.IsVerifiedSeller,
		u.CreateDate		
	FROM dbo.Products p 
	JOIN dbo.Users u ON p.SellerId = u.Id 
	LEFT JOIN CTE_ProductDownloads d ON d.ProductId = p.Id
	LEFT JOIN CTE_ProductRating r ON r.ProductId = p.Id
	LEFT JOIN CTE_ProductServers s ON s.ProductId = p.Id
	WHERE
		u.IsVerifiedSeller = 1 AND
		r.AverageRating > 3 AND
		(p.Status = 4 AND p.IsEnabled = 1)
	ORDER BY 
		NEWID();

RETURN 0
END