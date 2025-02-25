CREATE PROCEDURE dbo.SearchPluginsByHash
    @PluginHash NVARCHAR(128)
AS
BEGIN
SELECT
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.ImageId AS ProductImageId,
    p.Price AS ProductPrice,
    p.IsEnabled AS IsEnabled,
    p.IsLoaderEnabled AS IsLoaderRequired,
    (SELECT SUM(v.DownloadsCount) FROM dbo.Versions v JOIN dbo.Branches b ON v.BranchId = b.Id WHERE b.ProductId = p.Id) AS TotalProductDownloads,
    u.Id AS SellerId,
    u.Name AS SellerName,
    u.AvatarImageId AS SellerImageId,
    b.Id AS BranchId,
    b.Name AS BranchName,
    v.Id AS VersionId,
    v.Name AS Version,
    v.DownloadsCount AS Downloads
FROM
    dbo.Versions v
        JOIN
    dbo.Branches b ON v.BranchId = b.Id
        JOIN
    dbo.Products p ON b.ProductId = p.Id
        JOIN
    dbo.Users u ON p.SellerId = u.Id
WHERE
    v.PluginHash = @PluginHash

    RETURN 0
END