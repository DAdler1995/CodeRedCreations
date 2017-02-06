using CodeRedCreations.Data;
using CodeRedCreations.Models;
using CodeRedCreations.Models.Account;
using CodeRedCreations.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CodeRedCreations.Controllers
{
    public class HomeController : Controller
    {
        private readonly CodeRedContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        public HomeController(CodeRedContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About(int id)
        {
            return View();
        }

        public IActionResult Contact()
        {
            TempData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Referral(string id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var referrerFound = await _context.UserReferral.FirstOrDefaultAsync(x => x.ReferralCode == id);
            if (referrerFound != null && referrerFound.Enabled)
            {
                if (user != null && referrerFound.UserId == user.Id)
                {
                    TempData["Message"] = "You cannot use your own referral link.";
                    return RedirectToAction("Index", "Home");
                }

                referrerFound.Earnings = -1;
                referrerFound.PayPalAccount = null;
                referrerFound.PayoutPercent = -1;
                referrerFound.RequestedPayout = false;
                var serializedReferral = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(referrerFound));
                HttpContext.Response.Cookies.Append("Referral", serializedReferral, new CookieOptions
                { Expires = DateTime.Now.AddMonths(3) });
                var referrer = await _userManager.FindByIdAsync(referrerFound.UserId);
                var roles = _userManager.GetRolesAsync(referrer).Result;

                if (roles.Contains(UserRoles.Sponsor.ToString()))
                {
                    TempData["Message"] = $"Referral discount is now active.";
                }
            }
            else
            {
                TempData["Message"] = "Not a valid referral link.";
            }
            
            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        public async Task<IActionResult> ThankYou(int? id)
        {
            var session = HttpContext.Session.GetString("ShoppingCart");
            var user = await _userManager.GetUserAsync(HttpContext.User);

            // Makes sure that it's a legit Home/ThankYou redirect
            // Only triggered when manually navigated to Home/ThankYou
            if (TempData["ThankYouValidation"] == null || (id == null && session == null))
            {
                return RedirectToAction("Index");
            }

            decimal refAmount = 0m;
            // Checks if the user used store credit
            if (TempData["RefAmount"] != null && !string.IsNullOrEmpty(TempData["RefAmount"].ToString()))
            {
                refAmount = decimal.Parse(TempData["RefAmount"].ToString());
                if (refAmount > 0)
                {
                    var userRef = await _context.UserReferral.FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (userRef != null && userRef.Enabled)
                    {
                        userRef.Earnings -= refAmount;
                        _context.UserReferral.Update(userRef);
                        await _context.SaveChangesAsync();
                        TempData["Message"] = $"{refAmount.ToString("C2")} has been successfully applied on your purchase.";
                    }
                }
            }
            else // if no store credit used, then checks if there's an acctive referral account
            {
                if (HttpContext.Request.Cookies["Referral"] != null)
                {
                    var referralCookie = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<UserReferral>(HttpContext.Request.Cookies["Referral"]));

                    if (user != null)
                    {
                        if (referralCookie != null && user.Id != referralCookie.UserId)
                        {
                            const decimal referralPercent = 10;
                            const decimal referralDecimal = (referralPercent / 100);
                            if (id != null)
                            {
                                var product = await _context.Products.Include(x => x.Brand).FirstOrDefaultAsync(x => x.PartId == id);

                                referralCookie.Earnings += Math.Round(product.Price * referralDecimal, 2);
                            }
                            else if (session != null)
                            {
                                var cart = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShoppingCartViewModel>(session));
                                decimal cartValue = 0;
                                foreach (var item in cart.Products)
                                {
                                    cartValue += item.Price;
                                }
                                HttpContext.Session.Remove("ShoppingCart");

                                referralCookie.Earnings += Math.Round(cartValue * referralDecimal, 2);
                            }

                            _context.Update(referralCookie);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            if (user != null)
            {
                var message = "<h2>Thank you for your order!</h2><p>Your order has been successfully placed. You'll recieve shipping information once it's made avaliable. If you have any questions please contact our Code Red Performance support team: <a href='mailto:Support@coderedperformance.com'>Support@CodeRedPerformance.com</a></p>";
                await _emailSender.SendEmailAsync(user.NormalizedEmail, "Order Confirmation", "Order Confirmation", message);
            }

            return View();
        }
    }
}
