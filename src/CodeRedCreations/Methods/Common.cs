using CodeRedCreations.Data;
using CodeRedCreations.Models;
using CodeRedCreations.Models.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations.Methods
{
    public class Common
    {
        private IMemoryCache _cache;
        private readonly CodeRedContext _context;
        public Common(IMemoryCache cache,
            CodeRedContext context)
        {
            _cache = cache;
            _context = context;
        }

        public async Task<IList<string>> GetAllBrandNamesAsync()
        {
            string key = "brandNames";
            var brandNames = _cache.Get<IList<string>>(key);
            if (brandNames == null)
            {
                var brands = await GetAllBrandsAsync();
                brandNames = brands.Select(x => x.Name).ToList();
                _cache.Set(key, brandNames, TimeSpan.FromDays(7));
            }

            return brandNames;
        }

        public async Task<IList<BrandModel>> GetAllBrandsAsync()
        {
            string key = "brands";
            var brands = _cache.Get<IList<BrandModel>>(key);
            if (brands == null)
            {
                brands = await _context.Brand.Include(x => x.Products).Where(x => x.Products.Count > 0).OrderBy(x => x.Name).ToListAsync();
                _cache.Set(key, brands, TimeSpan.FromDays(7));
            }

            return brands;
        }

        public async Task<IList<CarModel>> GetAllCarsAsync()
        {
            string key = "cars";
            var cars = _cache.Get<IList<CarModel>>(key);
            if (cars == null)
            {
                cars = await _context.Car.Include(x => x.CarProducts).ThenInclude(x => x.Car).Where(x => x.ProductCount > 0).OrderBy(x => x.Make).ThenBy(x => x.Model).ToListAsync();
                _cache.Set(key, cars, TimeSpan.FromDays(7));
            }

            return cars;
        }

        public async Task<IList<BrandModel>> GetAllProductsAsync()
        {
            return await _context.Brand.Include(x => x.Products)
                .Include(x => x.Products).ThenInclude(x => x.CarProducts).ThenInclude(x => x.Car).ToListAsync();
        }

        public async Task<BrandModel> GetAllProductsAsync(int brandId)
        {
            return (await GetAllProductsAsync()).FirstOrDefault(x => x.BrandId == brandId);
        }


        public async Task<PromoModel> GetPromoAsync(int promoId)
        {
            string key = $"promo-{promoId}";
            var promo = _cache.Get<PromoModel>(key);

            if (promo == null)
            {
                promo = await _context.Promos.FirstOrDefaultAsync(x => x.Id == promoId);
                _cache.Set(key, promo, TimeSpan.FromHours(1));
            }

            return promo;
        }

        public async Task<PromoModel> GetPromoAsync(UserReferral userReferral)
        {
            var refPromo = await _context.Promos.FirstOrDefaultAsync(x => x.Code == userReferral.ReferralCode);
            PromoModel promo = null;

            if (refPromo != null)
            {
                promo = await GetPromoAsync(refPromo.Id);
            }

            return promo;
        }
    }
}
