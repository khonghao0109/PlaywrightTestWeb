using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;
using MyWebApp.Repository;
using MyWebApp.Services;

namespace MyWebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IRecommendationService _recommendationService;

        public ProductController(DataContext context, IRecommendationService recommendationService)
        {
            _dataContext = context;
            _recommendationService = recommendationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return RedirectToAction("Index");

            ViewBag.Keyword = keyword;

            var products = _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Category != null && p.Brand != null)
                .ToList();

            if (!products.Any())
                return RedirectToAction("Index");

            var searchService = new ProductSearchService(products);
            var result = searchService.Search(keyword);

            // Nếu không có kết quả, vẫn hiển thị trang search (trống)
            return View(result);
        }

        public async Task<IActionResult> Detail(long Id)
        {
            if (Id == null) return RedirectToAction("Index");

            var productsById = await _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Id == Id)
                .FirstOrDefaultAsync();

            if (productsById == null) return RedirectToAction("Index");

            var relatedProducts = await _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.CategoryId == productsById.CategoryId && p.Id != productsById.Id)
                .Take(4)
                .ToListAsync();

            var recommendedProducts = await _recommendationService.GetRecommendedProductsAsync(Id);

            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.RecommendedProducts = recommendedProducts ?? new List<ProductModel>();
            return View(productsById);
        }
    }
}
