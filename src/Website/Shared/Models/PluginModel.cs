using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models
{
    public class PluginModel
    {
        public int Id { get; set; }
        [Required]
        public int BranchId { get; set; }
        [Required]
        [MaxLength(255)]
        public string Version { get; set; }
        [Required]
        [MaxLength(2000)]
        public string Changelog { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public byte[] Data { get; set; }
        public int DownloadsCount { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreateDate { get; set; }

        public BranchModel Branch { get; set; }
        public List<PluginLibraryModel> Libraries { get; set; }
    }
}
