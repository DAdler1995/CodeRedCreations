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
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;

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


            var foundParts = await _context.Products.Include(x => x.Images).Include(x => x.Brand).Include(x => x.CompatibleCars).ToListAsync();
            if (string.IsNullOrEmpty(part) || string.IsNullOrEmpty(brand) || string.IsNullOrEmpty(car))
            {
                ViewData["partCount"] = foundParts.Count;
                return View(foundParts);
            }
            else
            {
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
            }
            ViewData["partCount"] = foundParts.Count;

            return View(foundParts);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = new ProductDetailsView();
            var product = await _context.Products.Include(x => x.Images).Include(x => x.Brand).FirstOrDefaultAsync(x => x.PartId == id);
            viewModel.ProductModel = product;
            viewModel.Images = product.Images;

            if (TempData["Promo"] != null)
            {
                int promoId = int.Parse(TempData["Promo"].ToString());
                viewModel.PromoModel = await _context.Promos.Include(x => x.ApplicableParts).FirstOrDefaultAsync(x => x.Id == promoId);
                if (viewModel.ProductModel.Price != ApplyPromoCode(viewModel))
                {
                    ViewData["OldPrice"] = viewModel.ProductModel.Price;
                    viewModel.ProductModel.Price = ApplyPromoCode(viewModel);
                }
            }

            viewModel.ProductModel.Price = Math.Round(viewModel.ProductModel.Price, 2);

            return View(viewModel);
        }

        [HttpGet]
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public IActionResult Request()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        {
            return View(new ProductRequestModel());
        }

        [HttpPost]
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public async Task<IActionResult> Request(ProductRequestModel request)
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        {
            if (ModelState.IsValid)
            {
                var email = new MimeMessage();

                var body = $"<p>New part request from: {request.FromEmail}</p><dl><dt>Brand Name</dt><dd>{request.Part.Brand.Name}</dd><dt>Part Name</dt><dd>{request.Part.Name}</dd><dt>Part Type</dt><dd>{request.Part.PartType}</dd></dl>";

                email.From.Add(new MailboxAddress(request.FromEmail, request.FromEmail));
                email.To.Add(new MailboxAddress("Dakota", "Zeketiki@gmail.com"));
                email.Subject = "New Part Request";
                email.Body = new TextPart(TextFormat.Html) { Text = body };

                using (var smtp = new SmtpClient())
                {
                    smtp.Connect("smtp.gmail.com", 587);
                    await smtp.SendAsync(email);
                    smtp.Disconnect(true);
                }

                TempData["Message"] = "Request sent!";
            }

            return View(request);
        }

        public async Task<IActionResult> BuyNow(ProductDetailsView model)
        {
            model.ProductModel = await _context.Products.Include(x => x.Brand).FirstOrDefaultAsync(x => x.PartId == model.ProductModel.PartId);
            if (model.PromoModel != null)
            {
                model.PromoModel = await _context.Promos.Include(x => x.ApplicableParts).FirstOrDefaultAsync(x => x.Id == model.PromoModel.Id);
                model.ProductModel.Price = ApplyPromoCode(model);
            }

            if (model.ProductModel != null)
            {


                var url = (HttpContext.Request.Host.Host.Normalize().Contains("LOCALHOST")) ?
                    "https://www.sandbox.paypal.com/us/cgi-bin/webscr" : "https://www.paypal.com/us/cgi-bin/webscr";

                var builder = new StringBuilder();
                builder.Append(url);

                builder.Append($"?cmd=_xclick&business={UrlEncoder.Default.Encode("zeketiki@gmail.com")}");
                builder.Append($"&lc=US&no_note=0&currency_code=USD");
                builder.Append($"&item_name={UrlEncoder.Default.Encode($"{model.ProductModel.Brand.Name} - {model.ProductModel.Name}")}");
                builder.Append($"&amount={UrlEncoder.Default.Encode(model.ProductModel.Price.ToString())}");
                builder.Append($"&return={UrlEncoder.Default.Encode($"https://{HttpContext.Request.Host.Value}/Parts/Details?id={model.ProductModel.PartId}")}");
                builder.Append($"&cancel_return={UrlEncoder.Default.Encode($"https://{HttpContext.Request.Host.Value}/Parts/Details?id={model.ProductModel.PartId}")}");
                builder.Append($"&quantity={UrlEncoder.Default.Encode(model.Quantity.ToString())}");
                builder.Append($"&shipping={UrlEncoder.Default.Encode((model.Quantity * model.ProductModel.Shipping).ToString())}");
                builder.Append($"&item_number={UrlEncoder.Default.Encode(model.ProductModel.PartId.ToString())}");

                return Redirect(builder.ToString());
            }

            return RedirectToAction("Details", model.ProductModel.PartId);
        }

        public async Task<IActionResult> PromoCode(ProductDetailsView model)
        {
            if (!string.IsNullOrEmpty(model.PromoModel.Code))
            {
                var promo = await _context.Promos.FirstOrDefaultAsync(x => x.Code.ToUpper() == model.PromoModel.Code.ToUpper().Replace(" ", ""));
                if (promo != null)
                {
                    TempData["Promo"] = promo.Id;
                }
                else
                {
                    TempData["Message"] = "Invalid promo code.";
                }
            }

            return RedirectToAction("Details", new { id = model.ProductModel.PartId });
        }

        public decimal ApplyPromoCode(ProductDetailsView model)
        {
            var promo = model.PromoModel;
            var part = model.ProductModel;
            var price = part.Price;

            if (promo != null && promo.Enabled)
            {
                if (promo.ExpirationDate == null || promo.ExpirationDate > DateTime.UtcNow)
                {
                    if (promo.ApplicableParts.Count() == 0 || promo.ApplicableParts.FirstOrDefault(x => x.PartId == part.PartId) != null)
                    {
                        if (promo.DiscountAmount != null)
                        {
                            price = Math.Round((price - (decimal)promo.DiscountAmount), 2);
                        }
                        else
                        {
                            var percent = ((decimal)promo.DiscountPercentage / 100);
                            var discount = Math.Round((price * percent), 2);

                            price = (price - discount);
                        }

                        TempData["Message"] = "The promo code has been successfully applied.";
                        return price;
                    }
                }
                TempData["Message"] = "This promo code has expired.";
                return price;
            }

            TempData["Message"] = "This promo code is invalid.";
            return price;
        }
    }
}
