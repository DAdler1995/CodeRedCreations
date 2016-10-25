using System.Collections.Generic;

namespace CodeRedCreations.Models
{
    public class NavbarViewModel
    {
        public IEnumerable<BrandModel> Brand { get; set; }
        public IEnumerable<CarModel> Cars { get; set; }
    }
}
