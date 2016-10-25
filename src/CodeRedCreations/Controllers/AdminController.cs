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

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CodeRedCreations.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;
        private CodeRedContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ILoggerFactory loggerFactory,
            CodeRedContext context)
        {
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _context = context;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            var allUsers = _context.Users.ToList();

            return View(allUsers);
        }

        [HttpGet]
        public IActionResult ManageUsers()
        {
            var allUsers = _context.Users.ToList();

            return View(allUsers);
        }

        [HttpGet]
        public IActionResult ManageProducts()
        {
            var allProducts = _context.Brand.Include(x => x.Parts).ToList();

            return View(allProducts);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct(int? id)
        {
            var AddNewProduct = new AddNewProductModel();
            AddNewProduct.Cars = await _context.Car.ToListAsync();
            AddNewProduct.Brands = await _context.Brand.ToListAsync();

            if (id != null)
            {
                AddNewProduct.Part = await _context.Part.Include(x => x.Brand)
                    .Include(x => x.Images)
                    .Include(x => x.CompatibleCars)
                    .FirstOrDefaultAsync(x => x.PartId == id);

                AddNewProduct.Brand = AddNewProduct.Part.Brand;
            }

            return View(AddNewProduct);
        }

        public async Task<IActionResult> AddBrand(AddNewProductModel model)
        {
            var newBrand = model.Brand;
            var existing = _context.Brand.FirstOrDefault(x => x.Name == newBrand.Name);

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

            return RedirectToAction("AddProduct", new { id = newBrand.BrandId });
        }

        public async Task<IActionResult> AddCar(AddNewProductModel model)
        {
            var newCar = model.NewCar;
            bool brandExists = _context.Car.FirstOrDefault(x => x.Make == newCar.Make && x.Model == newCar.Model) != null;

            if (brandExists)
            {
                TempData["SuccessMessage"] = $"Updated {newCar.Make} {newCar.Model}.";
                _context.Car.Update(newCar);
            }
            else
            {
                TempData["SuccessMessage"] = $"Added {newCar.Make} {newCar.Model}.";
                _context.Car.Add(newCar);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("AddProduct");
        }

        public async Task<IActionResult> AddPart(AddNewProductModel model)
        {
            int imgCount = 0;
            var images = new List<ImageModel>();
            var newPart = model.Part;

            var existing = await _context.Part.Include(x => x.Brand)
                .Include(x => x.Images).Include(x => x.CompatibleCars)
                .FirstOrDefaultAsync(x => (x.Brand == newPart.Brand && x.Name == newPart.Name) || x.PartId == newPart.PartId);

            model.Part.CompatibleCars = await _context.Car.FirstOrDefaultAsync(x => x.CarId == model.Part.CompatibleCars.CarId);
            model.Part.Brand = await _context.Brand.Include(x => x.Parts).FirstOrDefaultAsync(x => x.BrandId == model.Part.Brand.BrandId);

            foreach (var file in Request.Form.Files)
            {
                if (file.Length > 0)
                {
                    imgCount++;

                    images.Add(new ImageModel
                    {
                        Name = $"{newPart.Name} ({imgCount})",
                        Description = file.FileName,
                        Bytes = ConvertToBytes(file)
                    });
                }
            }
            newPart.Images = images;

            if (existing != null)
            {
                existing.Name = newPart.Name;
                existing.Description = newPart.Description;
                existing.Brand = newPart.Brand;
                existing.PartType = newPart.PartType;
                existing.CompatibleCars = newPart.CompatibleCars;
                existing.Price = newPart.Price;
                existing.Shipping = newPart.Shipping;
                existing.Stock = newPart.Stock;
                existing.OnSale = newPart.OnSale;
                existing.Images = (newPart.Images.Count() > 0) ? newPart.Images : existing.Images;
                TempData["SuccessMessage"] = $"Successfully updated {existing.Name} ({existing.PartId})";
            }
            else
            {
                TempData["SuccessMessage"] = $"Added {newPart.Brand} {newPart.Name}.";
                _context.Part.Add(newPart);
            }
            _context.Brand.Update(newPart.Brand);
            await _context.SaveChangesAsync();

            return RedirectToAction("AddProduct", new { id = newPart.PartId });
        }

        public async Task<IActionResult> DeletePart(int id)
        {
            var partFound = await _context.Part.Include(x => x.Images).FirstOrDefaultAsync(x => x.PartId == id);
            if (partFound != null)
            {
                _context.Images.RemoveRange(partFound.Images);
                var remove = _context.Part.Remove(partFound);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageProducts");
        }

        // Called via ajax
        [HttpPost]
        public async Task<IActionResult> UpdateRole(string email, string newRole)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(newRole))
            {
                var user = await _userManager.FindByEmailAsync(email);
                var currentRole = await _userManager.GetRolesAsync(user);

                if (currentRole.Count > 0)
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRole);
                    if (removeResult.Succeeded)
                    {
                        _logger.LogInformation($"{user.Email} has had their roles removed.");
                    }
                }

                var addResult = await _userManager.AddToRoleAsync(user, newRole);
                if (addResult.Succeeded)
                {
                    _logger.LogInformation($"{user.Email} was given {newRole} roles.");
                }

            }

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

        private byte[] ConvertToBytes(IFormFile file)
        {
            Stream stream = file.OpenReadStream();
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
