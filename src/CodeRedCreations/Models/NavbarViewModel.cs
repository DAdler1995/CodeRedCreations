using System.Collections.Generic;

namespace CodeRedCreations.Models
{
    public class NavbarViewModel
    {
        public ICollection<BrandModel> Brand { get; set; }
        public ICollection<CarModel> Cars { get; set; }
    }
}
