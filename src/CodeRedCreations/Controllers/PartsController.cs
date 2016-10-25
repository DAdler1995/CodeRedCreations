using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CodeRedCreations.Models;
using CodeRedCreations.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Encodings.Web;

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
        public async Task<IActionResult> Index(string part, string brand, string car)
        {
            var carDict = new Dictionary<string, string>();
            foreach (var c in _context.Car.OrderBy(x => x.Make))
            {
                carDict.Add(c.Model, c.Make);
            }

            ViewData["allBrands"] = await _context.Brand.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();
            ViewData["allCars"] = carDict;


            var foundParts = await _context.Part.Include(x => x.Images).Include(x => x.Brand).Include(x => x.CompatibleCars).ToListAsync();
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
            ViewData["partCount"] = foundParts.Count;

            return View(foundParts);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = new ProductDetailsView();
            var product = await _context.Part.Include(x => x.Images).Include(x => x.Brand).FirstOrDefaultAsync(x => x.PartId == id);
            viewModel.PartModel = product;
            viewModel.Images = product.Images;

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Request()
        {
            return View(new PartModel());
        }

        [HttpPost]
        public async Task<IActionResult> Request(PartModel part)
        {
            return View();
        }

        public async Task<IActionResult> BuyNow(ProductDetailsView model)
        {
            var part = await _context.Part.Include(x => x.Brand).FirstOrDefaultAsync(x => x.PartId == model.PartModel.PartId);

            if (part != null)
            {
                var url = (HttpContext.Request.Host.Host.Normalize().Contains("LOCALHOST")) ?
                    "https://www.sandbox.paypal.com/us/cgi-bin/webscr" : "https://www.paypal.com/us/cgi-bin/webscr";

                var builder = new StringBuilder();
                builder.Append(url);

                builder.Append($"?cmd=_xclick&business={UrlEncoder.Default.Encode("zeketiki@gmail.com")}");
                builder.Append($"&lc=US&no_note=0&currency_code=USD");
                builder.Append($"&item_name={UrlEncoder.Default.Encode($"{part.Brand.Name} - {part.Name}")}");
                builder.Append($"&amount={UrlEncoder.Default.Encode(part.Price.ToString())}");
                builder.Append($"&return={UrlEncoder.Default.Encode($"https://{HttpContext.Request.Host.Value}/Parts/Details?id={part.PartId}")}");
                builder.Append($"&cancel_return={UrlEncoder.Default.Encode($"https://{HttpContext.Request.Host.Value}/Parts/Details?id={part.PartId}")}");
                builder.Append($"&quantity={UrlEncoder.Default.Encode(model.Quantity.ToString())}");
                builder.Append($"&shipping={UrlEncoder.Default.Encode((model.Quantity * part.Shipping).ToString())}");
                builder.Append($"&item_number={UrlEncoder.Default.Encode(part.PartId.ToString())}");

                return Redirect(builder.ToString());
            }

            return RedirectToAction("Details", model.PartModel.PartId);
        }
    }
}
