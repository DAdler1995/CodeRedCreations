using CodeRedCreations.Models.Account;
using System.Collections.Generic;

namespace CodeRedCreations.Models
{
    public class ProductDetailsView
    {
        public ProductModel ProductModel { get; set; }
        public PromoModel PromoModel { get; set; }
        public ICollection<ImageModel> Images { get; set; }
        public int Quantity { get; set; }
        public UserReferral UserReferral { get; set; }
    }
}
