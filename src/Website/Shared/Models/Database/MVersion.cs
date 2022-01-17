using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models.Database
{
    public class MVersion
    {
        public int Id { get; set; }
        [Required]
        public int BranchId { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [Required]
        [MaxLength(2000)]
        public string Changelog { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
        public int DownloadsCount { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreateDate { get; set; }

        public MBranch Branch { get; set; }
    }
}
