using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CodeRedCreations.Models
{
    public class ProductModel
    {

        [Key]
        public int PartId { get; set; }
        [Display(Name = "Part Number")]
        public string PartNumber { get; set; }

        [Required, Display(Name = "Product Name")]
        public string Name { get; set; }

        [Required, Display(Name = "Product Description")]
        public string Description { get; set; }

        [Required]
        public virtual BrandModel Brand { get; set; }

        [Required, Display(Name = "Part Type")]
        public PartTypeEnum PartType { get; set; }

        [Required, Display(Name = "Price"), Range(1, double.MaxValue, ErrorMessage = "Not a valid price.")]
        public decimal Price { get; set; }

        [Required, Display(Name = "Shipping Price")]
        public decimal Shipping { get; set; }

        public virtual ICollection<ImageModel> Images { get; set; }
        
        [Display(Name = "Compatible Cars")]
        public virtual ICollection<CarProduct> CarProducts { get; set; }
        public string Years { get; set; }

        public DateTime DateAdded { get; set; }

        public bool OnSale { get; set; }
        public int? SalePercent { get; set; }
        public int? SaleAmount { get; set; }
        public DateTime? SaleExpiration { get; set; }
        public decimal SalePrice
        {
            get
            {
                if (OnSale)
                {
                    if (SalePercent != null)
                    {
                        var discount = Price * (decimal)(SalePercent / 100m);
                        return Math.Round(Price - discount, 2);
                    }
                    else if (SaleAmount != null)
                    {
                        return Math.Round(Price - (decimal)SaleAmount, 2);
                    }
                }

                return Math.Round(Price, 2);
            }
        }

    }
}
