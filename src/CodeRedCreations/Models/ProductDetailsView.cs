using System.Collections.Generic;

namespace CodeRedCreations.Models
{
    public class ProductDetailsView
    {
        public ProductModel ProductModel { get; set; }
        public PromoModel PromoModel { get; set; }
        public IList<ImageModel> Images { get; set; }
        public int Quantity { get; set; }
    }
}
