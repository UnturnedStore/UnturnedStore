using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Server.Helpers;
using Website.Shared.Params;
using Website.Shared.Results;

namespace Website.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PluginsController : ControllerBase
    {
        private readonly PluginsRepository loaderRepository;
        private readonly IConfiguration configuration;

        public PluginsController(PluginsRepository loaderRepository, IConfiguration configuration)
        {
            this.loaderRepository = loaderRepository;
            this.configuration = configuration;
        }

        //[HttpPost]
        //public async Task<IActionResult> PostPluginAsync([FromBody] GetPluginParams @params)
        //{
        //    if (@params.ServerInfo == null)
        //    {
        //        return BadRequest(new GetPluginResult() 
        //        { 
        //            ErrorMessage = "ServerInfo is required",
        //            ReturnCode = 101
        //        });
        //    }

        //    if (@params.ServerInfo.Port > 65535)
        //    {
        //        return BadRequest(new GetPluginResult()
        //        {
        //            ErrorMessage = "Invalid server port",
        //            ReturnCode = 102
        //        });
        //    }

        //    @params.ServerInfo.Host = Request.HttpContext.Connection.RemoteIpAddress.ToString();
        //    GetPluginResult result = await loaderRepository.GetPluginAsync(@params);

        //    if (result.ReturnCode != 0)
        //    {
        //        return BadRequest(result);            
        //    }

        //    byte[] key = Convert.FromBase64String(configuration.GetSection("PluginEncryption")["Key"]);
        //    byte[] iv = Convert.FromBase64String(configuration.GetSection("PluginEncryption")["IV"]);

        //    using Aes aes = Aes.Create();

        //    aes.Key = key;
        //    aes.IV = iv;

        //    using MemoryStream ms = new();
        //    using CryptoStream cryptoStream = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        //    await cryptoStream.WriteAsync(result.Version.Content, 0, result.Version.Content.Length);
        //    await cryptoStream.FlushFinalBlockAsync();

        //    result.Version.Content = ms.ToArray();

        //    Response.Headers.Add("PluginVersion", result.Version.Name);
        //    Response.Headers.Add("Changelog", result.Version.Changelog);
        //    return File(result.Version.Content, result.Version.ContentType, result.Version.FileName);
        //}
    }
}
