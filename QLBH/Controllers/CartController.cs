using Microsoft.AspNetCore.Mvc;
using MyWebApp.Models;
using MyWebApp.Models.ViewModels;
using MyWebApp.Repository;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MyWebApp.Controllers
{
  public class CartController : Controller
  {
    private readonly DataContext _dataContext;
    private readonly IConfiguration _config;

    public CartController(DataContext context, IConfiguration config)
    {
      _dataContext = context;
      _config = config;
    }

    public async Task<IActionResult> Index(string payment, long? payosOrderCode)
    {
      if (payment == "cancel" && payosOrderCode.HasValue)
      {
        try
        {
            var clientId = _config["PayOS:ClientId"];
            var apiKey = _config["PayOS:ApiKey"];

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-client-id", clientId);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            
            var content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                $"https://api-merchant.payos.vn/v2/payment-requests/{payosOrderCode.Value}/cancel",
                content
            );

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(jsonResponse).RootElement;
            var code = root.GetProperty("code").GetString();
            
            
            if (code == "00" || root.GetProperty("desc").GetString()?.ToUpper().Contains("ALREADY CANCELED") == true)
            {
                var order = _dataContext.Orders.FirstOrDefault(o => o.PayOSOrderCode == payosOrderCode.Value);
                if (order != null)
                {
                    if (order.Status != 2)
                    {
                        order.Status = 4; 
                        await _dataContext.SaveChangesAsync();
                        TempData["success"] = "Giao dịch thanh toán đã được hủy thành công.";
                    }
                    else
                    {
                        TempData["info"] = "Đơn hàng này đã được thanh toán và không thể hủy.";
                    }
                }
            }
            else
            {
                var errorDesc = root.GetProperty("desc").GetString();
                TempData["error"] = $"Hủy thanh toán thất bại: {errorDesc} (Mã lỗi: {code})";
                Console.WriteLine($"ERROR (Cart Cancel): Failed to cancel order for PayOS code {payosOrderCode}. Reason: {errorDesc}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION (Cart Cancel): An error occurred while cancelling order for PayOS code {payosOrderCode}. Exception: {ex.Message}");
            TempData["error"] = "Có lỗi nghiêm trọng xảy ra khi cố gắng hủy thanh toán.";
        }
      }
      else if (payment == "cancel")
      {
           TempData["info"] = "Giao dịch thanh toán đã được hủy.";
      }

      List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
      CheckoutViewModel vm = new()
      {
        CartItems = cartItems,
        GrandTotal = cartItems.Sum(x => x.Quantity * x.Price)
      };
      
      var userEmail = User.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
      if (userEmail != null)
      {
          vm.Address.Email = userEmail;
      }

      return View(vm);
    }
    public IActionResult Checkout()
    {
      return RedirectToAction("Index", "Checkout");
    }
    public async Task<IActionResult> Add(long Id, int quantity = 1)
    {
        ProductModel product = await _dataContext.Products.FindAsync(Id);
        List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
        CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

        if (cartItem == null)
        {
            cart.Add(new CartItemModel(product) { Quantity = quantity });
        }
        else
        {
            cartItem.Quantity += quantity;
        }
        HttpContext.Session.SetJson("Cart", cart);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { 
                success = true, 
                message = "Item has been added to cart successfully", 
                totalItems = cart.Sum(x => x.Quantity) 
            });
        }

        TempData["success"] = " Item has been added to cart successfully";
        return Redirect(Request.Headers["referer"].ToString());
    }

    public async Task<IActionResult> BuyNow(long Id, int quantity = 1)
    {
        ProductModel product = await _dataContext.Products.FindAsync(Id);
        List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
        CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

        if (cartItem == null)
        {
            cart.Add(new CartItemModel(product) { Quantity = quantity });
        }
        else
        {
            cartItem.Quantity += quantity;
        }
        HttpContext.Session.SetJson("Cart", cart);
        
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Increase(long Id)
    {
      var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
      var cartItem = cart.FirstOrDefault(c => c.ProductId == Id);
      if (cartItem == null)
      {
        return Json(new { success = false, message = "Item not found" });
      }
      cartItem.Quantity += 1;
      if (cart.Count == 0)
      {
        HttpContext.Session.Remove("Cart");

      }
      else
      {
        HttpContext.Session.SetJson("Cart", cart);
      }
      var itemTotal = cartItem.Quantity * cartItem.Price;
      var grandTotal = cart.Sum(x => x.Quantity * x.Price);
      return Json(new
      {
        success = true,
        quantity = cartItem.Quantity,
        itemTotal,
        grandTotal,
        removed = false
      });

    }
    [HttpPost]
    public IActionResult Decrease(long Id)
    {
      var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
      var cartItem = cart.FirstOrDefault(c => c.ProductId == Id);
      if (cartItem == null)
      {
        return Json(new { success = false, message = "Item not found" });
      }
      if (cartItem.Quantity > 1)
      {
        cartItem.Quantity -= 1;
      }
      else
      {
        cart.RemoveAll(p => p.ProductId == Id);
      }

      if (cart.Count == 0)
      {
        HttpContext.Session.Remove("Cart");
      }
      else
      {
        HttpContext.Session.SetJson("Cart", cart);
      }
      var removed = cart.All(c => c.ProductId != Id);
      var itemTotal = removed ? 0 : cart.First(c => c.ProductId == Id).Quantity * cart.First(c => c.ProductId == Id).Price;
      var grandTotal = cart.Sum(x => x.Quantity * x.Price);

      return Json(new
      {
        success = true,
        quantity = removed ? 0 : cart.First(c => c.ProductId == Id).Quantity,
        itemTotal,
        grandTotal,
        removed
      });


    }
    [HttpPost]
    public IActionResult Remove(long Id)
    {
      var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
      cart.RemoveAll(p => p.ProductId == Id);
      if (cart.Count == 0)
      {
        HttpContext.Session.Remove("Cart");
      }
      else
      {
        HttpContext.Session.SetJson("Cart", cart);

      }

      var grandTotal = cart.Sum(x => x.Quantity * x.Price);
      return Json(new
      {
        success = true,
        grandTotal,
        removed = true
      });
    }
    public async Task<IActionResult> Clear(long Id)
    {
      HttpContext.Session.Remove("Cart");
      TempData["success"] = "Clear all Item of cart Successfully";

      return RedirectToAction("Index");
    }
  }
}
