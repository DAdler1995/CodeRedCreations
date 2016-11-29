using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        [Required(ErrorMessage = "A Promo code is required."), Display(Name = "Promo Code")]
        public string Code { get; set; }
        public bool Enabled { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public virtual ICollection<ProductModel> ApplicableParts { get; set; }
        [Range(1, 100, ErrorMessage = "The percent must be between 1 and 100%.")]
        [Display(Name = "Discount Percentage")]
        public decimal? DiscountPercentage { get; set; }
        [Display(Name = "Discount Amount")]
        public decimal? DiscountAmount { get; set; }
        public int TimesUsed { get; set; }
        [Display(Name = "How many times can the promo be used")]
        public int? UsageLimit { get; set; }
    }
}
