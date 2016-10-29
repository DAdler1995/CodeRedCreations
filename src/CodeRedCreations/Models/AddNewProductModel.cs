using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CodeRedCreations.Models
{
    public class AddNewProductModel
    {
        public BrandModel Brand { get; set; }
        public IList<BrandModel> Brands { get; set; }
        public CarModel NewCar { get; set; }
        public IList<CarModel> Cars { get; set; }
        public ProductModel Part { get; set; }
    }
}
