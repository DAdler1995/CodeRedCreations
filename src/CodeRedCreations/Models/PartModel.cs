using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations.Models
{
    public class PartModel
    {
        [Key]
        public int PartId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PartTypeEnum PartType { get; set; }
        public IEnumerable<CarModel> CompatibleCars { get; set; }
        public decimal Price { get; set; }
        public decimal Shipping { get; set; }
        public int Stock { get; set; }
        public bool OnSale { get; set; }
    }
}
