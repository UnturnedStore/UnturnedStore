using System;

namespace Website.Shared.Params
{
    public class GetPluginParams
    {
        public Guid LicenseKey { get; set; }
        public string ProductName { get; set; }
        public string BranchName { get; set; }
        public string VersionName { get; set; }
    }
}
