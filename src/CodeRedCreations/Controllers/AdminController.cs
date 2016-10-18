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
        public IActionResult AddProduct()
        {
            var AddNewProduct = new AddNewProductModel();
            AddNewProduct.Cars = _context.Car.ToList();
            AddNewProduct.Brands = _context.Brand.ToList();

            return View(AddNewProduct);
        }

        public async Task<IActionResult> AddBrand(AddNewProductModel model)
        {
            var newBrand = model.Brand;
            bool brandExists = _context.Brand.FirstOrDefault(x => x.Name == newBrand.Name) != null;

            if (brandExists)
            {
                ViewData["SuccessMessage"] = $"Updated {newBrand.Name}.";
                _context.Brand.Update(newBrand);
            }
            else
            {
                ViewData["SuccessMessage"] = $"Added {newBrand.Name}.";
                _context.Brand.Add(newBrand);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("AddProduct");
        }

        public async Task<IActionResult> AddCar(AddNewProductModel model)
        {
            var newCar = model.NewCar;
            bool brandExists = _context.Car.FirstOrDefault(x => x.Make == newCar.Make && x.Model == newCar.Model) != null;

            if (brandExists)
            {
                ViewData["SuccessMessage"] = $"Updated {newCar.Make} {newCar.Model}.";
                _context.Car.Update(newCar);
            }
            else
            {
                ViewData["SuccessMessage"] = $"Added {newCar.Make} {newCar.Model}.";
                _context.Car.Add(newCar);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("AddProduct");
        }

        public async Task<IActionResult> AddPart(AddNewProductModel model)
        {
            model.Part.CompatibleCars = _context.Car.FirstOrDefault(x => x.CarId == model.Part.CompatibleCars.CarId);
            model.Part.Brand = _context.Brand.Include(x => x.Parts).FirstOrDefault(x => x.BrandId == model.Part.Brand.BrandId);
            var newPart = model.Part;
            foreach (var image in model.Images)
            {
                var bytes = ConvertToBytes(image);
                newPart.ImageStrings += $",{Convert.ToBase64String(bytes)}";
            }

            bool partExists = _context.Part.FirstOrDefault(x => x.Brand == newPart.Brand && x.Name == newPart.Name) != null;
            if (partExists)
            {
                ViewData["SuccessMessage"] = $"Updated {newPart.Brand} {newPart.Name}.";
                _context.Part.Update(newPart);
            }
            else
            {
                ViewData["SuccessMessage"] = $"Added {newPart.Brand} {newPart.Name}.";
                _context.Part.Add(newPart);
            }
            _context.Brand.Update(newPart.Brand);
            await _context.SaveChangesAsync();

            return RedirectToAction("AddProduct");
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
