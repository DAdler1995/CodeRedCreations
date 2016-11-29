using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations.Models
{
    public class CarProduct
    {
        public int ProductId { get; set; }
        public ProductModel Product { get; set; }


        public int CarId { get; set; }
        public CarModel Car { get; set; }
    }
}
