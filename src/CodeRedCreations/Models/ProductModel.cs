using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CodeRedCreations.Models
{
    public class ProductModel
    {
        [Key]
        public int PartId { get; set; }
        [Required, Display(Name = "Product Name")]
        public string Name { get; set; }
        [Required, Display(Name = "Product Description")]
        public string Description { get; set; }
        [Required]
        public virtual BrandModel Brand { get; set; }
        [Required, Display(Name = "Part Type")]
        public PartTypeEnum PartType { get; set; }
        [Display(Name = "Compatible Cars")]
        public virtual CarModel CompatibleCars { get; set; }
        [Required, Display(Name = "Price")]
        public decimal Price { get; set; }
        [Required, Display(Name = "Shipping Price")]
        public decimal Shipping { get; set; }
        public virtual IList<ImageModel> Images { get; set; }
    }
}
