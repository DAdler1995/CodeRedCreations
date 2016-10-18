using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations.Models
{
    public class ProductDetailsView
    {
        public PartModel PartModel { get; set; }
        public IList<string> Images { get; set; }
    }
}
