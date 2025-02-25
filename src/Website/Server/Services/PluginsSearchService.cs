using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Website.Server.Extensions;

namespace Website.Server.Services;

public class PluginsSearchService
{
    private readonly SqlConnection _connection;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public PluginsSearchService(SqlConnection connection, IHttpContextAccessor httpContextAccessor)
    {
        _connection = connection;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<IReadOnlyCollection<PluginSearchDto>> SearchByHash(string pluginHash)
    {
        if (string.IsNullOrWhiteSpace(pluginHash))
        {
            throw new ArgumentException("Cannot be null or empty", nameof(pluginHash));
        }
        
        var host = _httpContextAccessor.HttpContext?.Request.Host.Value;
        var scheme = _httpContextAccessor.HttpContext?.Request.Scheme;
        
        return (await SearchPluginsByPluginHashAsync(pluginHash))
            .Select(e =>
            {
                e.ProductImageUrl = $"{scheme}://{host}/api/images/{e.ProductImageId}";
                e.SellerImageUrl = $"{scheme}://{host}/api/images/{e.SellerImageId}";
                e.DownloadUrl = $"{scheme}://{host}/api/versions/download/{e.VersionId}";
                return e;
            }).ToList();
    }


    private async Task<IEnumerable<PluginSearchDto>> SearchPluginsByPluginHashAsync(string pluginHash)
    {
        const string sql = "dbo.SearchPluginsByHash";

        DynamicParameters p = new();
        p.Add("@PluginHash", pluginHash, DbType.String, ParameterDirection.Input, 128);

        return await _connection.QueryAsync<PluginSearchDto>(sql, p, commandType: CommandType.StoredProcedure);
    }
}

public class PluginSearchDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    [JsonIgnore] public int ProductImageId { get; set; }
    public string ProductImageUrl { get; set; }
    public decimal ProductPrice { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsLoaderRequired { get; set; }
    public int TotalProductDownloads { get; set; }
    
    public int SellerId { get; set; }
    public string SellerName { get; set; }
    [JsonIgnore] public int SellerImageId { get; set; }
    public string SellerImageUrl { get; set; }
    
    public int BranchId { get; set; }
    public string BranchName { get; set; }
    
    [JsonIgnore] public int VersionId { get; set; }
    public string Version { get; set; }
    public string DownloadUrl { get; set; }
    public int Downloads { get; set; }
}