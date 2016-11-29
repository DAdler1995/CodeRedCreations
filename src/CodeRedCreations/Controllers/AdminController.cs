using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeRedCreations.Data;
using Microsoft.AspNetCore.Identity;
using CodeRedCreations.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.IO.Compression;
using CodeRedCreations.Models.Account;
using Microsoft.Extensions.Options;
using CodeRedCreations.Services;
using System.Text.Encodings.Web;
using System.Text;
using Newtonsoft.Json;

namespace CodeRedCreations.Controllers
{
    [Authorize(Roles = "Admin"), ResponseCache(CacheProfileName = "Default")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly CodeRedContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ILoggerFactory loggerFactory,
            CodeRedContext context,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _context = context;
            _emailSender = emailSender;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            var allUsers = _context.Users.ToList();

            return View(allUsers);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var allUsers = await _context.Users.OrderBy(x => x.UserName).ToListAsync();
            var allUserRefs = await _context.UserReferral.ToListAsync();
            ViewData["AllUserRefs"] = allUserRefs;

            return View(allUsers);
        }

        public async Task<IActionResult> ToggleReferral(string id)
        {
            var userRef = await _context.UserReferral.FirstOrDefaultAsync(x => x.UserId == id);
            if (userRef != null)
            {
                userRef.Enabled = !userRef.Enabled;
                _context.UserReferral.Update(userRef);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        public async Task<IActionResult> ManageProducts()
        {
            var allProducts = await _context.Brand.Include(x => x.Products).ThenInclude(x => x.Images)
                .Include(x => x.Products).ThenInclude(x => x.CarProducts).ThenInclude(x => x.Car).ToListAsync();

            return View(allProducts);
        }

        [HttpGet]
        public async Task<IActionResult> ManageCars()
        {
            IList<CarModel> cars = await _context.Car.Include(x => x.CarProducts).ThenInclude(x => x.Product).ToListAsync();

            return View(cars);
        }

        [HttpGet]
        public async Task<IActionResult> ManagePromos()
        {
            var promoCodes = await _context.Promos.Include(x => x.ApplicableParts).ToListAsync();

            return View(promoCodes);
        }

        [HttpGet]
        public async Task<IActionResult> ManagePayouts(int? id)
        {
            if (id != null)
            {
                var userRef = await _context.UserReferral.FirstOrDefaultAsync(x => x.Id == id);
                var user = await _userManager.FindByIdAsync(userRef.UserId);
                var payoutAmount = Math.Round((userRef.Earnings / 3), 2);
                ViewData["Message"] = $"{user.NormalizedEmail} has been successfully paid: {payoutAmount.ToString("C2")}.";

                userRef.Earnings = 0m;
                userRef.RequestedPayout = false;
                await _context.SaveChangesAsync();
            }

            var allReferrals = await _context.UserReferral.Where(x => x.Enabled).OrderBy(x => x.RequestedPayout).ToListAsync();
            ViewData["AllUsers"] = await _context.Users.ToListAsync();
            return View(allReferrals);
        }

        [HttpGet]
        public async Task<IActionResult> SendPayment(int id)
        {
            var userRef = await _context.UserReferral.FirstOrDefaultAsync(x => x.Id == id);
            var user = await _userManager.FindByIdAsync(userRef.UserId);
            var payoutAmount = Math.Round(userRef.Earnings * (userRef.PayoutPercent / 100), 2);

            var url = (HttpContext.Request.Host.Host.ToUpper().Contains("LOCALHOST")) ?
                    "https://www.sandbox.paypal.com/us/cgi-bin/webscr" : "https://www.paypal.com/us/cgi-bin/webscr";

            var builder = new StringBuilder();
            builder.Append(url);

            builder.Append($"?cmd=_xclick&business={UrlEncoder.Default.Encode(userRef.PayPalAccount)}");
            builder.Append($"&lc=US&no_note=0&currency_code=USD");
            builder.Append($"&custom={UrlEncoder.Default.Encode(userRef.Id.ToString())}");
            builder.Append($"&item_name={UrlEncoder.Default.Encode($"Referral Payment - {user.NormalizedEmail}")}");
            builder.Append($"&amount={UrlEncoder.Default.Encode(payoutAmount.ToString())}");
            builder.Append($"&return={UrlEncoder.Default.Encode($"https://{HttpContext.Request.Host.Value}/Admin/ManagePayouts?id={userRef.Id}")}");
            builder.Append($"&cancel_return={UrlEncoder.Default.Encode($"https://{HttpContext.Request.Host.Value}/Admin/ManagePayouts")}");
            builder.Append($"&quantity=1");
            builder.Append($"&shipping=0");
            builder.Append($"&item_number={UrlEncoder.Default.Encode(userRef.Id.ToString())}");

            return Redirect(builder.ToString());
        }

        [HttpGet]
        public async Task<IActionResult> UpsertPromo(int? id)
        {
            PromoModel promo;

            if (id == null)
            {
                promo = new PromoModel();
            }
            else
            {
                promo = await _context.Promos.Include(x => x.ApplicableParts).FirstOrDefaultAsync(x => x.Id == id);
            }
            ViewData["AllParts"] = await _context.Products.ToListAsync();

            return View(promo);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertPromo(PromoModel promo, IEnumerable<int> PartIds)
        {
            promo.Code = promo.Code.ToUpper();
            promo.UsageLimit = (promo.UsageLimit != null && promo.UsageLimit == 0) ? null : promo.UsageLimit;
            if (promo.ExpirationDate.HasValue)
            {
                promo.ExpirationDate = promo.ExpirationDate.Value.ToUniversalTime();
            }
            promo.ApplicableParts = new List<ProductModel>();
            if (PartIds.Count() > 0)
            {
                foreach (var id in PartIds)
                {
                    var part = await _context.Products.Include(x => x.Brand)
                        .Include(x => x.CarProducts).Include(x => x.Images)
                        .FirstOrDefaultAsync(x => x.PartId == id);
                    promo.ApplicableParts.Add(part);
                }
            }

            var existing = await _context.Promos.Include(x => x.ApplicableParts).FirstOrDefaultAsync(x => x.Id == promo.Id);

            if (existing != null)
            {
                existing.Code = promo.Code;
                existing.Enabled = promo.Enabled;
                existing.ExpirationDate = promo.ExpirationDate;
                existing.ApplicableParts = promo.ApplicableParts;
                existing.DiscountPercentage = promo.DiscountPercentage;
                existing.DiscountAmount = promo.DiscountAmount;
                existing.UsageLimit = promo.UsageLimit;
                TempData["Message"] = "Promo successfully updated.";
            }
            else
            {
                if (await _context.Promos.AnyAsync(x => x.Code.ToUpper() == promo.Code))
                {
                    TempData["Message"] = $"A promo already exists with the code: {promo.Code.ToUpper()}";
                    ModelState.AddModelError("Promo Code", "Promo code already exists.");
                    return RedirectToAction("UpsertPromo");
                }

                _context.Promos.Add(promo);
                TempData["Message"] = "Promo successfully added.";
            }
            await _context.SaveChangesAsync();

            if (promo.Id == 0)
            {
                return RedirectToAction("ManagePromos", "Admin");
            }
            return RedirectToAction("UpsertPromo", new { id = promo.Id });

        }

        [HttpGet]
        public async Task<IActionResult> AddProduct(int? id, string section, bool newSimilarProduct = false)
        {
            id = (id == 0) ? null : id;
            var AddNewProduct = new AddNewProductModel();
            AddNewProduct.Cars = await _context.Car.ToListAsync();
            AddNewProduct.Brands = await _context.Brand.ToListAsync();
            AddNewProduct.Part = new ProductModel();

            if (id != null)
            {
                if (string.IsNullOrEmpty(section) || section.Normalize() == "PART")
                {
                    AddNewProduct.Part = await _context.Products.Include(x => x.Brand)
                        .Include(x => x.Images)
                        .Include(x => x.CarProducts)
                        .FirstOrDefaultAsync(x => x.PartId == id);

                    if (newSimilarProduct)
                    {
                        AddNewProduct.Part.PartNumber = null;
                        AddNewProduct.Part.PartId = 0;
                        AddNewProduct.Part.Images = null;
                        AddNewProduct.Part.Years = null;
                        AddNewProduct.Part.CarProducts = null;
                        AddNewProduct.Part.Shipping = 0m;
                    }

                    AddNewProduct.Brand = AddNewProduct.Part.Brand;
                }
                if (section.Normalize() == "CAR")
                {
                    AddNewProduct.NewCar = await _context.Car.FirstOrDefaultAsync(x => x.CarId == id);
                }
                else if (section.Normalize() == "BRAND")
                {
                    AddNewProduct.Brand = await _context.Brand.FirstOrDefaultAsync(x => x.BrandId == id);
                }
            }

            return View(AddNewProduct);
        }

        public async Task<IActionResult> AddBrand(AddNewProductModel model)
        {
            var newBrand = model.Brand;
            var existing = _context.Brand.FirstOrDefault(x => x.Name == newBrand.Name || x.BrandId == newBrand.BrandId);

            if (existing != null)
            {
                existing.Name = newBrand.Name;
                existing.Description = newBrand.Description;
                TempData["SuccessMessage"] = $"Updated {newBrand.Name} ({existing.BrandId}).";
            }
            else
            {
                TempData["SuccessMessage"] = $"Added {newBrand.Name}.";
                _context.Brand.Add(newBrand);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("AddProduct", new { id = newBrand.BrandId, section = "BRAND", newSimilarProduct = false });
        }

        public async Task<IActionResult> AddCar(AddNewProductModel model)
        {
            var newCar = model.NewCar;
            newCar.Make = newCar.Make.ToUpper();
            newCar.Model = newCar.Model.ToUpper();

            var existing = await _context.Car.Include(x => x.CarProducts).ThenInclude(x => x.Car)
                .FirstOrDefaultAsync(x => (x.Make == newCar.Make && x.Model == newCar.Model) || x.CarId == newCar.CarId);

            if (existing != null)
            {
                existing.Make = newCar.Make;
                existing.Model = newCar.Model;
                TempData["SuccessMessage"] = $"Updated {newCar.Make} {newCar.Model}.";
            }
            else
            {
                _context.Car.Add(newCar);
                TempData["SuccessMessage"] = $"Added {newCar.Make} {newCar.Model}.";
            }
            await _context.SaveChangesAsync();
            
            return RedirectToAction("AddProduct", new { section = "CAR", newSimilarProduct = false });
        }

        public async Task<IActionResult> AddPart(AddNewProductModel model, ICollection<int> CompatibleCarIds)
        {
            int imgCount = 0;
            var images = new List<ImageModel>();
            var carProducts = new List<CarProduct>();
            var newProduct = model.Part;
            newProduct.Brand = await _context.Brand.Include(x => x.Products).FirstOrDefaultAsync(x => x.BrandId == model.Part.Brand.BrandId);
            newProduct.PartNumber = (string.IsNullOrEmpty(newProduct.PartNumber)) ? DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") : newProduct.PartNumber;
            newProduct.Years = (!string.IsNullOrEmpty(model.Part.Years)) ? model.Part.Years.Replace(" ", "").Replace("-", " - ") : null;
            newProduct.Shipping = (newProduct.Shipping == 0m) ? await CalculateShippingAsync(newProduct.Price) : newProduct.Shipping;

            foreach (var file in Request.Form.Files)
            {
                if (file.Length > 0)
                {
                    imgCount++;

                    images.Add(new ImageModel
                    {
                        Name = $"{newProduct.Name} ({imgCount})",
                        Description = file.FileName,
                        Bytes = ConvertToBytes(file)
                    });
                }
            }
            newProduct.Images = images;

            foreach (var carId in CompatibleCarIds)
            {
                var car = await _context.Car.Include(x => x.CarProducts).FirstOrDefaultAsync(x => x.CarId == carId);
                carProducts.Add(new CarProduct
                {
                    CarId = car.CarId,
                    Car = car,
                    ProductId = newProduct.PartId,
                    Product = newProduct
                });
            }
            newProduct.CarProducts = carProducts;

            var existing = await _context.Products
                                            .Include(x => x.Brand).Include(x => x.Images).Include(x => x.CarProducts).ThenInclude(x => x.Car)
                                            .FirstOrDefaultAsync(x => x.PartId == newProduct.PartId);
            if (existing != null)
            {
                foreach (var existingCarProduct in existing.CarProducts)
                {
                    _context.CarProduct.Remove(existingCarProduct);
                }
                await _context.SaveChangesAsync();
                foreach (var newCarProduct in newProduct.CarProducts)
                {
                    existing.CarProducts = newProduct.CarProducts;
                    await _context.SaveChangesAsync();
                }

                existing.PartNumber = newProduct.PartNumber;
                existing.Years = newProduct.Years;
                existing.Name = newProduct.Name;
                existing.Description = newProduct.Description;
                existing.Brand = newProduct.Brand;
                existing.PartType = newProduct.PartType;
                existing.Price = newProduct.Price;
                existing.Shipping = newProduct.Shipping;
                existing.Images = (newProduct.Images.Count() > 0) ? newProduct.Images : existing.Images;
                
                TempData["SuccessMessage"] = $"Successfully updated {existing.Name} ({existing.PartNumber})";
            }
            else
            {
                _context.Products.Add(newProduct);

                TempData["SuccessMessage"] = $"Added {newProduct.Brand.Name} {newProduct.Name} ({newProduct.PartNumber}).";
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("AddProduct", new { id = newProduct.PartId, section = "PART", newSimilarProduct = false });
        }

        public IActionResult NewSimilarProduct(int id)
        {
            return RedirectToAction("AddProduct", new { id = id, section = "PART", newSimilarProduct = true });
        }

        public async Task<IActionResult> DeletePart(int id)
        {
            var partFound = await _context.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.PartId == id);
            if (partFound != null)
            {
                _context.Images.RemoveRange(partFound.Images);
                var remove = _context.Products.Remove(partFound);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageProducts");
        }
        public async Task<IActionResult> DeleteCar(int id)
        {
            var carFound = await _context.Car.FirstOrDefaultAsync(x => x.CarId == id);
            if (carFound != null)
            {
                _context.Car.Remove(carFound);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageCars");
        }

        public async Task<IActionResult> DeleteBrand(int id)
        {
            var brand = await _context.Brand.FirstOrDefaultAsync(x => x.BrandId == id);
            var brandName = brand.Name;
            if (brand != null)
            {
                _context.Brand.Remove(brand);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Successfully deleted: {brandName}.";
            }
            return RedirectToAction("ManageProducts");
        }

        public async Task<IActionResult> AdjustShipping()
        {
            var allProducts = _context.Products;
            foreach (var product in allProducts)
            {
                product.Shipping = await CalculateShippingAsync(product.Price);
            }
            _context.Products.UpdateRange(allProducts);
            await _context.SaveChangesAsync();
            TempData["Message"] = "All product shipping prices were adjusted.";
            return RedirectToAction("ManageProducts");
        }

        [HttpPost]
        public async Task<IActionResult> SetRefPercent(string UserId, string RefPercent)
        {
            var userRef = await _context.UserReferral.FirstOrDefaultAsync(x => x.UserId == UserId);
            var percent = int.Parse(RefPercent);
            if (userRef != null)
            {
                userRef.PayoutPercent = percent;
                _context.UserReferral.Update(userRef);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Payout percentage successfully set to: {percent} %.";
            }

            return RedirectToAction("ManageUsers", "Admin");
        }

        // Called via ajax
        [HttpPost]
        public async Task<IActionResult> UpdateRole(string email, string newRole)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(newRole))
            {
                var user = await _userManager.FindByEmailAsync(email);
                var currentRole = await _userManager.GetRolesAsync(user);
                var userRef = await _context.UserReferral.FirstOrDefaultAsync(x => x.UserId == user.Id);
                var refPromo = await _context.Promos.FirstOrDefaultAsync(x => x.Code == user.Email.Split('@')[0]);

                if (currentRole.Count > 0)
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRole);
                    if (removeResult.Succeeded)
                    {
                        if (userRef != null && currentRole.Contains(UserRoles.Sponsor.ToString()))
                        {
                            userRef.Enabled = false;

                            if (refPromo != null)
                            {
                                refPromo.Enabled = false;
                            }
                        }
                        _logger.LogInformation($"{user.Email} has had their roles removed.");
                    }
                }

                var addResult = await _userManager.AddToRoleAsync(user, newRole);
                if (addResult.Succeeded)
                {
                    _logger.LogInformation($"{user.Email} was given {newRole} roles.");
                    if (await _userManager.IsInRoleAsync(user, UserRoles.Sponsor.ToString()))
                    {
                        if (refPromo == null)
                        {
                            _context.Promos.Add(new PromoModel
                            {
                                Code = userRef.ReferralCode,
                                DiscountPercentage = 5,
                                Enabled = true
                            });
                        }
                        else
                        {
                            refPromo.Enabled = true;
                        }

                        if (userRef != null)
                        {
                            userRef.Enabled = true;
                            userRef.PayoutPercent = 100;
                            _context.UserReferral.Update(userRef);
                        }
                        else
                        {
                            _context.UserReferral.Add(new UserReferral
                            {
                                UserId = user.Id,
                                Enabled = true,
                                ReferralCode = user.Email.Split('@')[0]
                            });
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ManageUsers", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userManager.FindByEmailAsync(email);
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"{user.Email} has been deleted.");
                }
            }

            return RedirectToAction("ManageUsers", "Admin");
        }

        public async Task<IActionResult> TogglePromo(int id)
        {
            var promo = await _context.Promos.FirstOrDefaultAsync(x => x.Id == id);
            promo.Enabled = !promo.Enabled;
            await _context.SaveChangesAsync();

            return RedirectToAction("ManagePromos", "Admin");
        }

        public async Task<IActionResult> DeletePromo(int id)
        {
            var promo = await _context.Promos.FirstOrDefaultAsync(x => x.Id == id);
            _context.Promos.Remove(promo);
            await _context.SaveChangesAsync();

            return RedirectToAction("ManagePromos", "Admin");
        }

        public async Task<IActionResult> CapAll()
        {
            var cars = await _context.Car.ToListAsync();
            foreach (var car in cars)
            {
                car.Make = car.Make.ToUpper();
                car.Model = car.Model.ToUpper();

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageCars");
        }

        private byte[] ConvertToBytes(IFormFile file)
        {
            Stream stream = file.OpenReadStream();
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("String is empty or null.");
            }
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        private async Task<decimal> CalculateShippingAsync(decimal productPrice)
        {
            decimal shipping = 0m;

            await Task.Run(() =>
            {
                if (productPrice <= 750m)
                {
                    shipping = (Math.Round(productPrice * 0.10m, 2)) + 5m;
                }
                else
                {
                    shipping = Math.Round(productPrice * 0.08m, 2);
                }
            });

            return shipping;
        }
    }
}
