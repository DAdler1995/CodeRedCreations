using CodeRedCreations.Data;
using CodeRedCreations.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations.Components
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly CodeRedContext _context;

        public NavbarViewComponent(
            CodeRedContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var NavbarModel = new NavbarViewModel();

            NavbarModel.Brand = await _context.Brand.ToListAsync();
            NavbarModel.Cars = await _context.Car.ToListAsync();

            return View(NavbarModel);
        }
    }
}
