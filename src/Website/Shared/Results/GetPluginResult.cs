using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Website.Shared.Models.Database;

namespace Website.Shared.Results
{
    public class GetPluginResult
    {
        public int ReturnCode { get; set; }
        public string ErrorMessage { get; set; }
        [JsonIgnore]
        public MVersion Version { get; set; }
    }
}
