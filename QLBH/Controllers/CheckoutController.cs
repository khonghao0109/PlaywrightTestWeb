using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MyWebApp.Models;
using MyWebApp.Models.ViewModels;
using MyWebApp.Repository;

namespace MyWebApp.Controllers
{

    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        public CheckoutController(DataContext context)
        {
            _dataContext = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Checkout(CheckoutViewModel vm)
        {
            // Khách không đăng nhập: dùng email từ form. Đã đăng nhập: dùng email từ tài khoản.
            var userEmail =  vm.Address?.Email?.Trim();

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                TempData["error"] = "Vui lòng nhập Email.";
                return RedirectToAction("Index", "Cart");
            }
            if (string.IsNullOrWhiteSpace(vm.Address?.FullName))
            {
                TempData["error"] = "Vui lòng nhập Họ và tên.";
                return RedirectToAction("Index", "Cart");
            }
            if (string.IsNullOrWhiteSpace(vm.Address?.Phone))
            {
                TempData["error"] = "Vui lòng nhập Số điện thoại.";
                return RedirectToAction("Index", "Cart");
            }

            var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            if (cartItems.Count == 0)
            {
                TempData["error"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm.";
                return RedirectToAction("Index", "Cart");
            }

            // 1. TẠO ORDER (CHƯA THANH TOÁN)
            var ordercode = Guid.NewGuid().ToString("N").Substring(0, 8);

            var orderItem = new OrderModel
            {
                OrderCode = ordercode,
                UserName = userEmail,
                Status = 0,
                CreatedDate = DateTime.Now
            };

            _dataContext.Orders.Add(orderItem);
            await _dataContext.SaveChangesAsync();

            // 2. LƯU ĐỊA CHỈ
            vm.Address ??= new OrderAddress();
            vm.Address.OrderId = orderItem.Id;
            vm.Address.Email = userEmail;
            vm.Address.FullName = vm.Address.FullName?.Trim() ?? "";
            vm.Address.Phone = vm.Address.Phone?.Trim() ?? "";
            _dataContext.OrderAddresses.Add(vm.Address);

            // 3. LƯU ORDER DETAILS


            foreach (var cart in cartItems)
            {
                _dataContext.OrderDetails.Add(new OrderDetails
                {
                    UserName = userEmail,
                    OrderCode = ordercode,
                    OrderId = orderItem.Id,
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    Quantity = cart.Quantity
                });
            }

            await _dataContext.SaveChangesAsync();


            return RedirectToAction(
                "GenerateQrCode",
                "VietQR",
                new { orderCode = orderItem.OrderCode }
            );
        }
    }
}
