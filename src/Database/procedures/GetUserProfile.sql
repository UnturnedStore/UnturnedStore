CREATE PROCEDURE [dbo].[GetUserProfile]
	@UserId INT
AS
	WITH CTE_ProductDownloads AS (
		SELECT 
			ProductId, 
			TotalDownloadsCount = SUM(v.DownloadsCount) 
		FROM dbo.Branches b 
		JOIN dbo.Products p ON b.ProductId = p.Id
		LEFT JOIN dbo.Versions v ON v.BranchId = b.Id 
		WHERE p.SellerId = @UserId
		GROUP BY ProductId
	), 
	CTE_ProductRating AS (
		SELECT 
			r.ProductId, 
			AverageRating = AVG(r.Rating), 
			RatingsCount = COUNT(*) 
		FROM dbo.Products p 
		LEFT JOIN dbo.ProductReviews r ON p.Id = r.ProductId 
		WHERE p.SellerId = @UserId 
		GROUP BY ProductId
	),
	CTE_UserSales AS (
		SELECT 
			u.Id AS UserId,
			Sales = COUNT(*)
		FROM dbo.Users u 
		LEFT JOIN dbo.Orders o ON u.Id = o.SellerId
		JOIN dbo.OrderItems i ON o.Id = i.OrderId
		WHERE u.Id = @UserId AND o.Status = 'Completed'
		GROUP BY u.Id
	)

	SELECT
		u.Id,
		u.Name,
		u.SteamId,
		u.Role,
		u.AvatarImageId,
		u.Biography,
		u.Color,
		u.CreateDate,
		Sales = ISNULL(us.Sales, 0),
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
		RatingsCount = ISNULL(RatingsCount, 0)	
	FROM dbo.Users u 
	LEFT JOIN dbo.Products p ON p.SellerId = u.Id
	LEFT JOIN CTE_ProductDownloads d ON d.ProductId = p.Id
	LEFT JOIN CTE_ProductRating r ON r.ProductId = p.Id
	LEFT JOIN CTE_UserSales us ON us.UserId = u.Id
	WHERE u.Id = @UserId;
RETURN 0
