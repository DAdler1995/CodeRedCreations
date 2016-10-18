using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CodeRedCreations.Models;
using CodeRedCreations.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CodeRedCreations.Controllers
{
    public class PartsController : Controller
    {
        private CodeRedContext _context;
        public PartsController(CodeRedContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string part, string brand, string car)
        {
            var foundParts = _context.Part.Include(x => x.Brand).Include(x => x.CompatibleCars).ToList();
            if (part != "All")
            {
                PartTypeEnum partType = (PartTypeEnum)Enum.Parse(typeof(PartTypeEnum), part);
                foundParts.RemoveAll(x => x.PartType != partType);
            }
            if (brand != "All")
            {
                foundParts.RemoveAll(x => x.Brand.Name.ToUpper() != brand.ToUpper());
            }
            if (car != "All")
            {
                foundParts.RemoveAll(x => x.CompatibleCars.Model.ToUpper() != car.ToUpper());
            }

            foundParts.OrderBy(x => x.Name);
            
            return View(foundParts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var viewModel = new ProductDetailsView();
            var product = await _context.Part.Include(x => x.Brand).FirstOrDefaultAsync(x => x.PartId == id);
            viewModel.PartModel = product;

            if (!string.IsNullOrEmpty(product.ImageStrings))
            {
                viewModel.Images = product.ImageStrings.Split(',').Where(x => x.Length > 0).ToList();
            }


            return View(viewModel);
        }
    }
}
