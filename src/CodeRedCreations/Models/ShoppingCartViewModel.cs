using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations.Models
{
    public class ShoppingCartViewModel
    {
        public DateTime ShoppingCartStarted { get; set; }
        public IList<ProductModel> Parts { get; set; }
        public int? PromoId { get; set; }
    }
}
