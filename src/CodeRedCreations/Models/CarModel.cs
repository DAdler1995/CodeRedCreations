using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CodeRedCreations.Models
{
    public class CarModel
    {

        [Key]
        public int CarId { get; set; }
        [Required, Display(Name = "Make")]
        public string Make { get; set; }
        [Required, Display(Name = "Model")]
        public string Model { get; set; }

        public virtual ICollection<CarProduct> CarProducts { get; set; }
    }
}
