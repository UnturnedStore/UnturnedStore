CREATE PROCEDURE dbo.GetProducts 
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

	-- CTE_ProductSales is additional safety to make sure that there is only one active non expired sale for one product

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
		p.IsLoaderEnabled,
		p.ReleaseDate,
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
		u.CreateDate,
		t.Id,
		t.Title,
		t.Color,
		t.BackgroundColor,
		ps.Id,
		ps.ProductId,
		ps.SaleName,
		ps.SaleMultiplier,
		ps.StartDate,
		ps.EndDate,
		ps.IsExpired,
		ps.IsActive
	FROM dbo.Products p 
	JOIN dbo.Users u ON p.SellerId = u.Id
	LEFT JOIN dbo.Tags t ON t.Id IN (SELECT pt.TagId FROM dbo.ProductTags pt WHERE pt.ProductId = p.Id)
	LEFT JOIN CTE_ProductSales ps ON ps.ProductId = p.Id AND ps.ROWNUM = 1
	LEFT JOIN CTE_ProductDownloads d ON d.ProductId = p.Id
	LEFT JOIN CTE_ProductRating r ON r.ProductId = p.Id
	LEFT JOIN CTE_ProductServers s ON s.ProductId = p.Id
	WHERE
		(p.Status = 4 AND p.IsEnabled = 1)		
		OR EXISTS (
			SELECT * FROM dbo.ProductCustomers c 
			WHERE c.ProductId = p.Id 
			AND c.UserId = @UserId
		);
END;