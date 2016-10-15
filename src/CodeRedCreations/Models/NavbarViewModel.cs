using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations.Models
{
    public class NavbarViewModel
    {
        public IEnumerable<BrandModel> Brand { get; set; }
        public IEnumerable<CarModel> Cars { get; set; }
    }
}
