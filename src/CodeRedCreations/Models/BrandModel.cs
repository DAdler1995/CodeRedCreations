using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CodeRedCreations.Models
{
    public class BrandModel
    {
        [Key]
        public int BrandId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<PartModel> Parts { get; set; }
    }
}
