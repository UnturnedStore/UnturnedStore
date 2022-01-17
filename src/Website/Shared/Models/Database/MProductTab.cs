using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models.Database
{
    public class MProductTab
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        
        [Required]
        [MaxLength(30)]
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsEnabled { get; set; }
    }
}
