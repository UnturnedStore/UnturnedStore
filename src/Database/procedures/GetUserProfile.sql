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
	),
	CTE_ProductSales AS (
		SELECT
			Id,
			ProductId,
			SaleName,
			SaleMultiplier,
			StartDate,
			EndDate,
			IsExpired,
			IsActive,
			ROWNUM = ROW_NUMBER() OVER (PARTITION BY ProductId ORDER BY Id)
		FROM dbo.ProductSales 
		WHERE IsExpired = 0 AND IsActive = 1
	)

	SELECT
		u.Id,
		u.Name,
		u.SteamId,
		u.Role,
		u.AvatarImageId,
		u.Biography,
		u.Color,
		u.IsVerifiedSeller,
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
		p.ReleaseDate,
		p.LastUpdate,
		p.CreateDate,
		TotalDownloadsCount = ISNULL(d.TotalDownloadsCount, 0), 
		AverageRating = ISNULL(r.AverageRating, 0), 
		RatingsCount = ISNULL(RatingsCount, 0),
		ps.Id,
		ps.ProductId,
		ps.SaleName,
		ps.SaleMultiplier,
		ps.StartDate,
		ps.EndDate,
		ps.IsActive,
		ps.IsExpired
	FROM dbo.Users u 
	LEFT JOIN dbo.Products p ON p.SellerId = u.Id AND p.IsEnabled = 1
	LEFT JOIN CTE_ProductSales ps ON ps.ProductId = p.Id AND ps.ROWNUM = 1
	LEFT JOIN CTE_ProductDownloads d ON d.ProductId = p.Id
	LEFT JOIN CTE_ProductRating r ON r.ProductId = p.Id
	LEFT JOIN CTE_UserSales us ON us.UserId = u.Id
	WHERE u.Id = @UserId;
RETURN 0
