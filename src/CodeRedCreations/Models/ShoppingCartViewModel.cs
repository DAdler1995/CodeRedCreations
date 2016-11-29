using CodeRedCreations.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations.Models
{
    public class ShoppingCartViewModel
    {
        public DateTime ShoppingCartStarted { get; set; }
        public ICollection<ProductModel> Parts { get; set; }
        public int? PromoId { get; set; }
        public UserReferral UserReferral { get; set; }
    }
}
