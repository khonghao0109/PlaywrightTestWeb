using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models.ViewModels;
using MyWebApp.Models;
using MyWebApp.Repository;
using MyWebApp.Services;
using PagedList.Core;
using System.Diagnostics;
using System.Security.Claims;

namespace MyWebApp.Controllers;

public class HomeController : Controller
{
    private readonly DataContext _dataContext;
    private readonly ILogger<HomeController> _logger;
    private readonly HRecommendationService _recommendationService;

    public HomeController(ILogger<HomeController> logger, DataContext context, HRecommendationService recommendationService)
    {
        _logger = logger;
        _dataContext = context;
        _recommendationService = recommendationService;
    }

    public async Task<IActionResult> Index(int? page, string priceRange)
    {
        // Cấu hình số trang và số lượng sản phẩm
        var pageNumber = page == null || page <= 0 ? 1 : page.Value;
        var pageSize = 8;

        // 3. Tạo truy vấn (Lưu ý: Không dùng ToListAsync ở đây)
        IQueryable<ProductModel> productsQuery = _dataContext.Products
            .AsNoTracking() // Tối ưu hiệu suất cho việc đọc dữ liệu
            .Include(p => p.Category)
            .Include(p => p.Brand);

        if (!string.IsNullOrEmpty(priceRange))
        {
            var priceValues = priceRange.Split('-').Select(v => decimal.Parse(v)).ToList();
            var minPrice = priceValues[0];
            var maxPrice = priceValues[1];

            if (maxPrice == 0)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice);
            }
            else
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
            }
            ViewBag.PriceRange = priceRange;
        }

        var orderedProductsQuery = productsQuery.OrderByDescending(p => p.Id); // Bắt buộc phải sắp xếp khi phân trang

        // 4. Thực hiện phân trang
        // PagedList sẽ tự động tính toán Skip/Take dựa trên pageNumber và pageSize
        PagedList<ProductModel> models = new PagedList<ProductModel>(orderedProductsQuery, pageNumber, pageSize);

        // --- Giữ nguyên Logic Recommendation của bạn ---
        var recommendedProducts = new List<ProductModel>();
        var hasUserPurchased = false;

        // Kiểm tra user đã login
        if (User.Identity?.IsAuthenticated == true)
        {
            // Lấy email từ claims
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            _logger.LogInformation($"DEBUG: User email from claims: {userEmail}");

            if (!string.IsNullOrEmpty(userEmail))
            {
                // Kiểm tra user có lịch sử mua hàng không
                hasUserPurchased = await _dataContext.OrderDetails
                    .AsNoTracking()
                    .AnyAsync(od => od.UserName == userEmail);

                _logger.LogInformation($"DEBUG: User has purchased (by email): {hasUserPurchased}");

                // Nếu user đã mua hàng, lấy recommended products
                if (hasUserPurchased)
                {
                    _logger.LogInformation($"DEBUG: Getting recommended products for user email: {userEmail}");
                    recommendedProducts = await _recommendationService.GetRecommendedProductsByUserAsync(userEmail);
                    _logger.LogInformation($"DEBUG: Recommended products count: {recommendedProducts.Count}");
                }
            }
        }
        else
        {
            _logger.LogInformation("DEBUG: User not authenticated");
        }

        ViewBag.RecommendedProducts = recommendedProducts;
        ViewBag.HasUserPurchased = hasUserPurchased;

        // Truyền model đã phân trang sang View
        return View(models);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}