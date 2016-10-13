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
            return View();
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
    }
}
