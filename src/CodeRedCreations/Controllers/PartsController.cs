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
using CodeRedCreations.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using CodeRedCreations.Models.Account;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using CodeRedCreations.Methods;

namespace CodeRedCreations.Controllers
{
    public class PartsController : Controller
    {
        private CodeRedContext _context;
        private readonly AppSettings _settings;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        private IMemoryCache _cache;
        private Common _common;

        public PartsController(
            CodeRedContext context,
            IOptions<AppSettings> settings,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IMemoryCache cache)
        {
            _context = context;
            _settings = settings.Value;
            _userManager = userManager;
            _emailSender = emailSender;
            _cache = cache;
            _common = new Common(cache, context);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string part = "All", string brand = "All", string car = "All", string search = null)
        {
            ViewData["allBrands"] = await _common.GetAllBrandNamesAsync();
            ViewData["allCars"] = await _common.GetAllCarsAsync();
            ViewData["Search"] = search;
            var products = await GetProductsAsync(part, brand, car, search);

            await Task.Run(() =>
            {
                foreach (var product in products.Where(x => x.OnSale == true))
                {
                    if (product.SaleExpiration <= DateTime.UtcNow)
                    {
                        product.OnSale = false;
                        _context.Products.Update(product);
                        _context.SaveChangesAsync();
                    }
                }
            });

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var viewModel = new ProductDetailsView();

            viewModel.ProductModel = await _context.Products.Include(x => x.Images).Include(x => x.Brand).Include(x => x.CarProducts).ThenInclude(x => x.Car).FirstOrDefaultAsync(x => x.PartId == id);
            viewModel.Images = viewModel.ProductModel.Images;

            if (user != null)
            {
                viewModel.UserReferral = await _context.UserReferral.FirstOrDefaultAsync(x => x.UserId == user.Id);
            }

            // Refferal Check
            if (HttpContext.Request.Cookies["Referral"] != null)
            {
                var referralCookie = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<UserReferral>(HttpContext.Request.Cookies["Referral"]));
                if (referralCookie != null && (user != null && referralCookie.UserId != user.Id) || user == null)
                {
                    viewModel.PromoModel = await _common.GetPromoAsync(referralCookie);
                }
            }

            // Promo
            if (viewModel.PromoModel != null)
            {
                var promoPrice = await ApplyPromoCode(viewModel);

                if (viewModel.ProductModel.Price != promoPrice)
                {
                    viewModel.NewPrice = promoPrice;
                }
            }

            // Store Credit
            if (TempData["StoreCredit"] != null)
            {
                var refAmount = decimal.Parse(TempData["StoreCredit"].ToString());

                viewModel.NewPrice = ((viewModel.ProductModel.Price - refAmount) <= 0 ? 0.01m : (viewModel.ProductModel.Price - refAmount));
            }

            // Sale
            if (viewModel.ProductModel.OnSale)
            {
                if (viewModel.ProductModel.SaleExpiration == null || viewModel.ProductModel.SaleExpiration >= DateTime.UtcNow)
                {
                    if (viewModel.ProductModel.SaleAmount != null)
                    {
                        viewModel.NewPrice = viewModel.ProductModel.Price - (decimal)viewModel.ProductModel.SaleAmount;
                    }
                    else
                    {
                        var discount = viewModel.ProductModel.Price * (decimal)(viewModel.ProductModel.SalePercent / 100m);
                        viewModel.NewPrice = viewModel.ProductModel.Price - discount;
                    }
                }
                else
                {
                    var product = viewModel.ProductModel;
                    product.OnSale = false;

                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                }
            }

            return View(viewModel);
        }

        [HttpGet]
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public IActionResult Request()
        {
            return View(new ProductRequestModel());
        }

        [HttpPost]
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public async Task<IActionResult> Request(ProductRequestModel request)
        {
            if (ModelState.IsValid)
            {
                var from = (request.FromEmail != null) ? request.FromEmail : "Anonymous";
                var body = $"<p>New part request from: {from}</p><dl><dt>Brand Name:</dt><dd>{request.Part.Brand.Name}</dd><dt>Part Name:</dt><dd>{request.Part.Name}</dd><dt>Part Type:</dt><dd>{request.Part.PartType}</dd></dl>";
                await _emailSender.SendEmailAsync("Dakota@CodeRedPerformance.com", "Product Request", "Part Request", body);

                TempData["Message"] = "Request sent!";
                return RedirectToAction("Request");
            }

            TempData["Message"] = "Request failed to send.";
            return View(request);

        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyNow(ProductDetailsView model, decimal? refAmount)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            model.ProductModel = await _context.Products.Include(x => x.Brand).FirstOrDefaultAsync(x => x.PartId == model.ProductModel.PartId);

            if (model.PromoModel != null)
            {
                model.PromoModel = await _context.Promos.Include(x => x.ApplicableParts).FirstOrDefaultAsync(x => x.Id == model.PromoModel.Id);
                var promo = model.PromoModel;
                var timesUsed = promo.TimesUsed;
                model.ProductModel.Price = await ApplyPromoCode(model);
                timesUsed++;
                await _context.SaveChangesAsync();
            }

            if (user != null)
            {
                var userRef = await _context.UserReferral.FirstOrDefaultAsync(x => x.Id == model.UserReferral.Id && x.UserId == user.Id);
                if (refAmount != null && userRef != null)
                {
                    if (refAmount > userRef.Earnings)
                    {
                        refAmount = userRef.Earnings;
                    }

                    decimal price = ((model.ProductModel.Price - (decimal)refAmount) <= 0 ? 0.01m : (model.ProductModel.Price - (decimal)refAmount));
                    model.ProductModel.Price = Math.Round(price, 2);
                }
            }

            if (model.ProductModel != null)
            {
                var url = (HttpContext.Request.Host.Host.ToUpper().Contains("LOCALHOST")) ?
                    "https://www.sandbox.paypal.com/us/cgi-bin/webscr" : "https://www.paypal.com/us/cgi-bin/webscr";
                var paypalBusiness = _settings.PaypalBusiness;

                var builder = new StringBuilder();
                builder.Append(url);

                builder.Append($"?cmd=_xclick&business={UrlEncoder.Default.Encode(paypalBusiness)}");
                builder.Append($"&lc=US&no_note=0&currency_code=USD&tax_rate=8");
                builder.Append($"&custom={UrlEncoder.Default.Encode(model.ProductModel.PartNumber)}");
                builder.Append($"&item_name={UrlEncoder.Default.Encode($"{model.ProductModel.Brand.Name} - {model.ProductModel.Name}: #{model.ProductModel.PartNumber}")}");
                builder.Append($"&amount={UrlEncoder.Default.Encode(model.ProductModel.SalePrice.ToString())}");
                builder.Append($"&return={UrlEncoder.Default.Encode($"https://{HttpContext.Request.Host.Value}/Home/ThankYou?id={model.ProductModel.PartId}")}");
                builder.Append($"&cancel_return={UrlEncoder.Default.Encode($"https://{HttpContext.Request.Host.Value}/Parts/Details?id={model.ProductModel.PartId}")}");
                builder.Append($"&quantity={UrlEncoder.Default.Encode(model.Quantity.ToString())}");
                builder.Append($"&shipping={UrlEncoder.Default.Encode(Math.Round((decimal)model.Quantity * model.ProductModel.Shipping, 2).ToString())}");
                builder.Append($"&item_number={UrlEncoder.Default.Encode(model.ProductModel.PartNumber)}");

                TempData["ThankYouValidation"] = true;
                TempData["RefAmount"] = refAmount.ToString();
                return Redirect(builder.ToString());
            }

            return RedirectToAction("Details", model.ProductModel.PartId);
        }

        public async Task<IActionResult> PromoCode(ProductDetailsView model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userRef = await _context.UserReferral.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (!string.IsNullOrEmpty(model.PromoModel.Code))
            {
                var promo = await _context.Promos.FirstOrDefaultAsync(x => x.Code.ToUpper() == model.PromoModel.Code.ToUpper().Replace(" ", "")
                                                                    && x.Code.ToUpper() != userRef.ReferralCode);
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

        public async Task<decimal> ApplyPromoCode(ProductDetailsView model)
        {
            if (model.PromoModel == null)
            {
                int promoId = int.Parse(TempData["Promo"].ToString());
                model.PromoModel = await _context.Promos.FirstOrDefaultAsync(x => x.Id == promoId);
            }

            var promo = model.PromoModel;
            var part = model.ProductModel;
            var price = part.Price;

            // Valid Promo Check
            if (promo != null && promo.Enabled)
            {
                // Usage Limit Check
                if (promo.UsageLimit == null || promo.TimesUsed <= promo.UsageLimit)
                {
                    // Expiration Check
                    if (promo.ExpirationDate == null || promo.ExpirationDate > DateTime.UtcNow)
                    {
                        // Applicable Part Check
                        if (promo.ApplicableParts == null || promo.ApplicableParts.Count() == 0 || promo.ApplicableParts.FirstOrDefault(x => x.PartId == part.PartId) != null)
                        {
                            // Valid Discount Amount
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
                        TempData["Message"] = $"The promo has already reached its limit of {promo.UsageLimit}";
                        return price;
                    }
                    TempData["Message"] = "This promo code has expired.";
                    return price;
                }
            }
            TempData["Message"] = "This promo code is invalid.";
            return price;
        }

        [HttpPost]
        public async Task<IActionResult> ApplyStoreCredit(ProductDetailsView model)
        {
            var amount = model.UserReferral.Earnings;

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                var userRef = await _context.UserReferral.FirstOrDefaultAsync(x => x.Id == model.UserReferral.Id && x.UserId == user.Id);
                if (userRef != null && userRef.Enabled)
                {
                    if (amount > userRef.Earnings)
                    {
                        amount = userRef.Earnings;
                    }
                    TempData["StoreCredit"] = amount.ToString();
                }
            }
            return RedirectToAction("Details", new { id = model.ProductModel.PartId });
        }

        public async Task<IQueryable<ProductModel>> SearchProductsAsync(string searchTerm, IQueryable<ProductModel> search)
        {
            string s = searchTerm.Replace(" ", "").ToUpper();
            // Brand Name
            if (await search.AnyAsync(x => x.Brand.Name.Replace(" ", "").ToUpper().Contains(s)))
            {
                search = search.Where(x => x.Brand.Name.Replace(" ", "").ToUpper().Contains(s));
            }

            // Product Name
            if (await search.AnyAsync(x => x.Name.Replace(" ", "").ToUpper().Contains(s)))
            {
                search = search.Where(x => x.Name.Replace(" ", "").ToUpper().Contains(s));
            }

            // Product Description
            if (await search.AnyAsync(x => x.Description.Replace(" ", "").ToUpper().Contains(s)))
            {
                search = search.Where(x => x.Description.Replace(" ", "").ToUpper().Contains(s));
            }

            // Car Make
            if (await search.AnyAsync(x => x.CarProducts.Any(c => c.Car.Make.Replace(" ", "").ToUpper().Contains(s))))
            {
                search = search.Where(x => x.CarProducts.Any(c => c.Car.Make.Replace(" ", "").ToUpper().Contains(s)));
            }

            // Car Model
            if (await search.AnyAsync(x => x.CarProducts.Any(c => c.Car.Model.Replace(" ", "").ToUpper().Contains(s))))
            {
                search = search.Where(x => x.CarProducts.Any(c => c.Car.Model.Replace(" ", "").ToUpper().Contains(s)));
            }

            // Product Price
            if (s.Contains("$"))
            {
                if (await search.AnyAsync(x => x.Price.ToString().Replace(" ", "").ToUpper().Contains(s.Replace("$", ""))))
                {
                    search = search.Where(x => x.Price.ToString().Replace(" ", "").ToUpper().Contains(s.Replace("$", "")));
                }
            }

            // Part Number
            if (await search.AnyAsync(x => x.PartNumber.Replace(" ", "").ToUpper() == s))
            {
                search = search.Where(x => x.PartNumber.Replace(" ", "").ToUpper() == s);
            }

            // Car Year
            int ignore = -1;
            if (int.TryParse(s, out ignore))
            {
                int year = int.Parse(s);
                search = search.Where(x => x.CarProducts.Count() > 0 &&
                    !string.IsNullOrEmpty(x.Years) &&
                    year >= int.Parse(x.Years.Replace(" ", "").Split('-')[0].Replace("-", "")) &&
                    year <= ((x.Years.Replace(" ", "").Split('-')[1].Replace("-", "").ToUpper().Contains("PRESENT")) ? 9999 : int.Parse(x.Years.Replace(" ", "").Split('-')[1].Replace("-", ""))));
            }

            return search;
        }
        public async Task<List<ProductModel>> GetProductsAsync(string part, string brand, string car, string search)
        {
            string key = $"{part}{brand}{car}{search}";
            var products = _cache.Get<List<ProductModel>>(key);
            if (products == null)
            {
                var partsQuery = _context.Products
                   .Include(x => x.Brand).Include(x => x.CarProducts).ThenInclude(x => x.Car)
                   .Where(x => x.Price > 0m);

                if (car != "All")
                {
                    partsQuery = partsQuery.Where(x => x.CarProducts.Any(c => c.Car.Model.ToUpper() == car.ToUpper()));
                }
                if (part != "All")
                {
                    PartTypeEnum partType = (PartTypeEnum)Enum.Parse(typeof(PartTypeEnum), part);
                    partsQuery = partsQuery.Where(x => x.PartType == partType);
                }
                if (brand != "All")
                {
                    partsQuery = partsQuery.Where(x => x.Brand.Name.ToUpper() == brand.ToUpper());
                }
                if (!string.IsNullOrEmpty(search))
                {
                    partsQuery = await SearchProductsAsync(search, partsQuery);
                }

                products = await partsQuery
                    .OrderBy(x => x.Name).ThenBy(x => x.Brand.Name)
                    .ThenBy(x => x.CarProducts.Select(c => c.Car).OrderBy(c => c.Make).FirstOrDefault())
                    .ThenBy(x => x.CarProducts.Select(c => c.Car).OrderBy(c => c.Model).FirstOrDefault())
                    .ThenBy(x => x.Years)
                    .ThenBy(x => x.Price)
                    .ToListAsync();

                _cache.Set(key, products, TimeSpan.FromHours(1));
            }

            ViewData["partCount"] = products.Count();
            return products;
        }

        [HttpGet]
        public async Task<string> GetProductImageAsync(int productId)
        {
            return await Static.GetImageSrcAsync(productId, _context);
        }
    }
}
