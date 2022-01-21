using System;
using System.Text.Json.Serialization;

namespace Website.Shared.Params
{
    public class GetPluginParams
    {
        public Guid LicenseKey { get; set; }
        public string ProductName { get; set; }
        public string BranchName { get; set; }
        public string VersionName { get; set; }

        public Server ServerInfo { get; set; }

        public class Server
        {
            public string Name { get; set; }
            public int Port { get; set; }
            [JsonIgnore]
            public string Host { get; set; }
        }

    }
}
