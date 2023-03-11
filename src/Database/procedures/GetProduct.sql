CREATE PROCEDURE dbo.GetProduct
	@ProductId INT
AS
BEGIN

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
		dbo.ProductSales ps ON ps.ProductId = p.Id AND ps.IsExpired = 0 AND ps.IsActive = 1
	WHERE
		p.Id = @ProductId;

	RETURN 0
END