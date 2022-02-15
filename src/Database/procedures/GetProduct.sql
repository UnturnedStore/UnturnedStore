CREATE PROCEDURE dbo.GetProduct
	@ProductId INT
AS
BEGIN

	SELECT 
		p.*,
		s.*,
		(SELECT SUM(DownloadsCount) FROM dbo.Versions) AS TotalDownloadsCount,
		(SELECT COUNT(*) FROM dbo.CustomerServers cs JOIN dbo.ProductCustomers pc ON pc.Id = cs.CustomerId WHERE pc.ProductId = @ProductId) AS ServersCount
	FROM 
		dbo.Products p
	JOIN
		dbo.Users s ON s.Id = p.SellerId
	WHERE
		p.Id = @ProductId;

	RETURN 0
END