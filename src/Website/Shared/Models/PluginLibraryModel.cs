using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models
{
    public class PluginLibraryModel
    {
        public int Id { get; set; }
        public int PluginId { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }
    }
}
