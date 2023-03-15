CREATE PROCEDURE dbo.GetProduct
	@ProductId INT
AS
BEGIN
	WITH CTE_ProductSales AS (
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
		p.*,
		(SELECT SUM(v.DownloadsCount) FROM dbo.Versions v LEFT JOIN dbo.Branches b ON b.Id = v.BranchId WHERE b.ProductId = @ProductId) AS TotalDownloadsCount,
		(SELECT COUNT(*) FROM dbo.CustomerServers cs JOIN dbo.ProductCustomers pc ON pc.Id = cs.CustomerId WHERE pc.ProductId = @ProductId) AS ServersCount,
		s.*,
		ps.*
	FROM 
		dbo.Products p
	JOIN
		dbo.Users s ON s.Id = p.SellerId
	LEFT JOIN 
		CTE_ProductSales ps ON ps.ProductId = p.Id AND ps.ROWNUM = 1
	WHERE
		p.Id = @ProductId;

	RETURN 0
END