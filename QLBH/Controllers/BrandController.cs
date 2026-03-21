using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;
using MyWebApp.Repository;

namespace MyWebApp.Controllers
{
   public class BrandController : Controller   
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index(string Slug = "")
        {
            BrandModel brand = await _dataContext.Brands.Where(c => c.Slug == Slug).FirstOrDefaultAsync();
            if (brand == null) return RedirectToAction("Index");

            var productsByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id); 

            return View(await productsByBrand.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}

