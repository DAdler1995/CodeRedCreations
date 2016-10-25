using System.Collections.Generic;

namespace CodeRedCreations.Models
{
    public class ProductDetailsView
    {
        public PartModel PartModel { get; set; }
        public IList<ImageModel> Images { get; set; }
        public int Quantity { get; set; }
    }
}
