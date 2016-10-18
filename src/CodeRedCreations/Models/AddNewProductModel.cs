using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations.Models
{
    public class AddNewProductModel
    {
        public BrandModel Brand { get; set; }
        public IList<BrandModel> Brands { get; set; }
        public CarModel NewCar { get; set; }
        public IList<CarModel> Cars { get; set; }
        public PartModel Part { get; set; }
        public IList<IFormFile> Images { get; set; }
    }
}
