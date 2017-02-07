using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CodeRedCreations.Models;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using CodeRedCreations.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using CodeRedCreations.Models.Account;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CodeRedCreations.Controllers
{
    public class ShoppingCartController : Controller
    {
        const string sessionKey = "ShoppingCart";
        private CodeRedContext _context;
        private readonly AppSettings _settings;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(
            CodeRedContext context,
            IOptions<AppSettings> settings,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _settings = settings.Value;
            _userManager = userManager;
        }


        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var session = HttpContext.Session.GetString(sessionKey);
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (session == null)
            {
                return View(new ShoppingCartViewModel());
            }
            else
            {
                var cart = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShoppingCartViewModel>(session));

                UserReferral referralCookie = null;
                PromoModel refPromo = null;
                if (HttpContext.Request.Cookies["Referral"] != null)
                {
                    referralCookie = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<UserReferral>(HttpContext.Request.Cookies["Referral"]));
                    if (referralCookie != null)
                    {
                        refPromo = await _context.Promos.FirstOrDefaultAsync(x => x.Code == referralCookie.ReferralCode);
                    }
                }

                if (user != null)
                {
                    var userRef = await _context.UserReferral.FirstOrDefaultAsync(x => x.UserId == user.Id);
                    cart.UserReferral = userRef;
                }

                if (TempData["StoreCredit"] != null)
                {
                    decimal StoreCredit = decimal.Parse(TempData["StoreCredit"].ToString());
                    decimal total = 0m;
                    foreach (var product in cart.Products)
                    {
                        total += product.Price;
                    }

                    decimal newTotal = ((total - StoreCredit) <= 0) ? 0.01m : (total - StoreCredit);
                    cart.NewTotal = newTotal;
                }
                else if (TempData["RefPromo"] == null && refPromo != null)
                {
                    if ((user != null && referralCookie.UserId != user.Id) || user == null)
                    {
                        return RedirectToAction("PromoCode", "ShoppingCart", new { promoCode = refPromo.Code });
                    }
                }

                return View(cart);
            }

        }

        [HttpGet]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var session = HttpContext.Session.GetString(sessionKey);
            var part = await _context.Products
                    .Include(x => x.Brand).Include(x => x.CarProducts).ThenInclude(x => x.Car).Include(x => x.Images)
                    .FirstOrDefaultAsync(x => x.PartId == id);

            if (session == null)
            {
                var newCart = new ShoppingCartViewModel();
                newCart.Products = new List<ProductModel>();
                newCart.PromoModel = null;
                newCart.ShoppingCartStarted = DateTime.Now;

                for (int i = 0; i < quantity; i++)
                {
                    newCart.Products.Add(part);
                }

                var serializedCart = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(newCart, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }));
                HttpContext.Session.SetString(sessionKey, serializedCart);
            }
            else
            {
                var cart = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShoppingCartViewModel>(session));

                for (int i = 0; i < quantity; i++)
                {
                    cart.Products.Add(part);
                }

                var serializedCart = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(cart, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }));
                HttpContext.Session.SetString(sessionKey, serializedCart);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var session = HttpContext.Session.GetString(sessionKey);

            if (session == null)
            {
                TempData["Message"] = "The shopping cart doesn't seem to exist.";
            }
            else
            {
                var cart = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShoppingCartViewModel>(session));
                cart.Products.Remove(cart.Products.FirstOrDefault(x => x.PartId == id));

                if (cart.Products == null || cart.Products.Count() == 0)
                {
                    HttpContext.Session.Remove(sessionKey);
                }

                var serializedCart = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(cart, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }));
                HttpContext.Session.SetString(sessionKey, serializedCart);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ClearCart()
        {
            var session = HttpContext.Session.GetString(sessionKey);
            if (session != null)
            {
                HttpContext.Session.Remove(sessionKey);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(string id)
        {
            var session = HttpContext.Session.GetString(sessionKey);
            var url = "https://www.paypal.com/us/cgi-bin/webscr";
            var paypalBusiness = _settings.PaypalBusiness;

            if (session != null)
            {
                int productCount = 1;
                var cart = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShoppingCartViewModel>(session));

                if (cart.PromoModel != null)
                {
                    cart.PromoModel.TimesUsed++;
                    await _context.SaveChangesAsync();
                }

                var builder = new StringBuilder();
                builder.Append(url);
                builder.Append($"?cmd=_cart&upload=1&business={UrlEncoder.Default.Encode(paypalBusiness)}");
                builder.Append($"&lc=US&no_note=0&currency_code=USD");
                if (cart.PromoModel != null)
                {
                    builder.Append($"&custom=PromoCode:{cart.PromoModel.Code}");
                }

                var totalValue = 0m;
                decimal taxRate = Math.Round((decimal)8 / 100, 2);
                foreach (var product in cart.Products)
                {
                    totalValue += product.SalePrice;
                    builder.Append($"&item_name_{productCount}={UrlEncoder.Default.Encode($"{product.Brand.Name} - {product.Name}: #{product.PartNumber}")}");
                    builder.Append($"&item_number_{productCount}={UrlEncoder.Default.Encode(product.PartNumber)}");
                    builder.Append($"&amount_{productCount}={UrlEncoder.Default.Encode(product.SalePrice.ToString())}");
                    builder.Append($"&shipping_{productCount}={UrlEncoder.Default.Encode(product.Shipping.ToString())}");

                    productCount++;
                }
                decimal originalTotal = totalValue;

                if (!string.IsNullOrEmpty(id))
                {
                    var amount = decimal.Parse(id);

                    var user = await _userManager.GetUserAsync(HttpContext.User);
                    if (user != null)
                    {
                        var userRef = await _context.UserReferral.FirstOrDefaultAsync(x => x.UserId == user.Id);
                        amount = (amount > userRef.Earnings) ? userRef.Earnings : amount;
                        amount = (amount > originalTotal) ? (originalTotal - 0.01m) : amount;
                        TempData["RefAmount"] = amount.ToString();

                        builder.Append($"&discount_amount_cart={amount.ToString()}");
                    }
                }
                decimal tax = Math.Round((totalValue * taxRate), 2);
                builder.Append($"&tax_cart={UrlEncoder.Default.Encode(tax.ToString())}");


                builder.Append($"&return={UrlEncoder.Default.Encode($"https://{HttpContext.Request.Host.Value}/Home/ThankYou")}");
                builder.Append($"&cancel_return={UrlEncoder.Default.Encode($"https://{HttpContext.Request.Host.Value}/ShoppingCart")}");

                TempData["ThankYouValidation"] = true;
                return Redirect(builder.ToString());
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> PromoCode(string promoCode)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userRef = await _context.UserReferral.FirstOrDefaultAsync(x => x.UserId == user.Id);
            var promo = await _context.Promos.FirstOrDefaultAsync(x => x.Code.ToUpper().Replace(" ", "") == promoCode.ToUpper().Replace(" ", "")
                                                                    && x.Code.ToUpper() != userRef.ReferralCode);
            var session = HttpContext.Session.GetString(sessionKey);
            TempData["RefPromo"] = true;

            if (promo != null && session != null)
            {
                if (promo.Enabled)
                {
                    if (promo.UsageLimit == null || promo.TimesUsed <= promo.UsageLimit)
                    {
                        if (promo.ExpirationDate == null || promo.ExpirationDate > DateTime.UtcNow)
                        {
                            var cart = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShoppingCartViewModel>(session));

                            if (cart.PromoModel != null)
                            {
                                foreach (var product in cart.Products)
                                {
                                    product.Price = (await _context.Products.FirstOrDefaultAsync(x => x.PartId == product.PartId)).Price;
                                }
                            }

                            cart.PromoModel = promo;
                            foreach (var product in cart.Products)
                            {
                                if (promo.ApplicableParts != null)
                                {
                                    if (promo.ApplicableParts.FirstOrDefault(x => x.PartId == product.PartId) != null)
                                    {
                                        if (promo.DiscountAmount != null)
                                        {
                                            product.Price = (product.Price - (decimal)promo.DiscountAmount);
                                        }
                                        else
                                        {
                                            var percent = ((decimal)promo.DiscountPercentage / 100);
                                            var discount = (product.Price * percent);

                                            product.Price = Math.Round((product.Price - discount), 2);
                                        }
                                    }
                                }
                                else
                                {
                                    if (promo.DiscountAmount != null)
                                    {
                                        product.Price = (product.Price - (decimal)promo.DiscountAmount);
                                    }
                                    else
                                    {
                                        var percent = ((decimal)promo.DiscountPercentage / 100);
                                        var discount = (product.Price * percent);

                                        product.Price = Math.Round((product.Price - discount), 2);
                                    }
                                }
                            }

                            var serializedCart = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(cart, Formatting.Indented,
                                new JsonSerializerSettings
                                {
                                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                }));
                            HttpContext.Session.SetString(sessionKey, serializedCart);

                            TempData["PromoCode"] = promoCode;
                            TempData["Message"] = "This promo code has been successfully applied.";
                            return RedirectToAction("Index");
                        }
                        TempData["Message"] = "This promo code has expired.";
                        return RedirectToAction("Index");
                    }
                    TempData["Message"] = "This promo code has expired.";
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ApplyStoreCredit(ShoppingCartViewModel model)
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

            return RedirectToAction("Index");
        }
    }
}
