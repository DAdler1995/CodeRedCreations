using CodeRedCreations.Data;
using CodeRedCreations.Methods;
using CodeRedCreations.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace CodeRedCreations.Components
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly CodeRedContext _context;
        private IMemoryCache _cache;

        public NavbarViewComponent(
            CodeRedContext context,
            IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var _common = new Common(_cache, _context);
            var NavbarModel = new NavbarViewModel();

            NavbarModel.Brand = await _common.GetAllBrandsAsync();
            NavbarModel.Cars = await _common.GetAllCarsAsync();

            return View(NavbarModel);
        }
    }
}
