using CodeRedCreations.Data;
using CodeRedCreations.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            NavbarModel.Brand = await _context.Brand.Include(x => x.Products).Where(x => x.Products.Count > 0).OrderBy(x => x.Name).ToListAsync();
            NavbarModel.Cars = await _context.Car.Include(x => x.CarProducts).ThenInclude(x => x.Product).Where(x => x.CarProducts.Select(c => c.Product).FirstOrDefault() != null).ToListAsync();

            return View(NavbarModel);
        }
    }
}
