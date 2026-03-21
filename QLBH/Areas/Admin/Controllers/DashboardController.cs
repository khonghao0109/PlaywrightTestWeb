using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebApp.Models;
using MyWebApp.Repository;

namespace MyWebApp.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("Admin/Dashboard")]
    [Authorize(Roles = "Admin,Seller")]
    public class DashboardController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DashboardController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {

            var count_product = _dataContext.Products.Count();
            var count_order = _dataContext.Orders.Count();
            var count_category = _dataContext.Categories.Count();
            var count_user = _dataContext.Users.Count();
            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.CountUser = count_user;
            return View();
        }
        [HttpPost("GetOrderChartData")]
        public async Task<IActionResult> GetOrderChartData()
        {
            var data = _dataContext.Orders
                .GroupBy(o => o.CreatedDate.Date)
                .Select(g => new {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    count = g.Count()
                })
                .ToList();
            return Json(data);
        }

        [HttpPost("GetProductChartData")]
        public async Task<IActionResult> GetProductChartData()
        {
            var data = _dataContext.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new {
                    productId = g.Key,
                    count = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.count)
                .Take(5)
                .Join(_dataContext.Products,
                      od => od.productId,
                      p => p.Id,
                      (od, p) => new {
                          label = p.Name,
                          value = od.count
                      })
                .ToList();
            return Json(data);
        }

        [HttpPost("GetUserChartData")]
        public async Task<IActionResult> GetUserChartData()
        {
            var userRoles = _dataContext.UserRoles
                .Join(_dataContext.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { RoleName = r.Name })
                .GroupBy(x => x.RoleName)
                .Select(g => new {
                    label = g.Key,
                    value = g.Count()
                })
                .ToList();

            return Json(userRoles);
        }
    }
}
