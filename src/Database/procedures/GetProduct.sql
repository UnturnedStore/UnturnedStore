CREATE PROCEDURE dbo.GetProduct
	@ProductId INT
AS
BEGIN

	SELECT 
		p.*,
		(SELECT SUM(v.DownloadsCount) FROM dbo.Versions v LEFT JOIN dbo.Branches b ON b.Id = v.BranchId WHERE b.ProductId = @ProductId) AS TotalDownloadsCount,
		(SELECT COUNT(*) FROM dbo.CustomerServers cs JOIN dbo.ProductCustomers pc ON pc.Id = cs.CustomerId WHERE pc.ProductId = @ProductId) AS ServersCount,
		s.*		
	FROM 
		dbo.Products p
	JOIN
		dbo.Users s ON s.Id = p.SellerId
	WHERE
		p.Id = @ProductId;

	RETURN 0
END