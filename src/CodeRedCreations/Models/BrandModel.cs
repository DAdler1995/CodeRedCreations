using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CodeRedCreations.Models
{
    public class BrandModel
    {
        [Key]
        public int BrandId { get; set; }
        [Required, Display(Name = "Brand Name")]
        public string Name { get; set; }

        [Required, Display(Name = "Brand Description")]
        public string Description { get; set; }
        public virtual ICollection<PartModel> Parts { get; set; }
    }
}
