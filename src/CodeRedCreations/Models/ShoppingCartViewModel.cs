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

        public ICollection<ProductModel> Products { get; set; }
        public PromoModel PromoModel { get; set; }
        public UserReferral UserReferral { get; set; }

        public decimal? NewTotal { get; set; }
        public decimal Total
        {
            get
            {
                decimal totalAmount = 0m;
                foreach (var product in Products)
                {
                    totalAmount += product.Price;
                }

                return totalAmount;
            }
        }
    }
}
