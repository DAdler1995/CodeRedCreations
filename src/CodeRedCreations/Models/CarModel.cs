using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeRedCreations.Models
{
    public class CarModel
    {

        [Key, Column(Order = 1)]
        public int CarId { get; set; }
        [Required, Display(Name = "Make")]
        public string Make { get; set; }
        [Required, Display(Name = "Model")]
        public string Model { get; set; }
        public virtual ICollection<CarProduct> CarProducts { get; set; }
        public int ProductCount
        {
            get
            {
                if (CarProducts != null)
                {
                    return CarProducts.Count;
                }
                return 0;
            }
        }
    }
}
