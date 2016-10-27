using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations.Models
{
    public class PromoModel
    {
        [Key]
        public int Id { get; set; }
        [Required, Display(Name = "Promo Code")]
        public string Code { get; set; }
        public bool Enabled { get; set; }
        public virtual IList<PartModel> ApplicableParts { get; set; }
        [Range(1, 100)]
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}
