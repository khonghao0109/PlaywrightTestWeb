using Microsoft.AspNetCore.Mvc;
using MyWebApp.Models;
using MyWebApp.Repository;
using System.Collections.Generic;
using System.Linq;

namespace MyWebApp.Views.Components
{
    public class CartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            int itemCount = cart.Sum(x => x.Quantity);
            
            return View(itemCount);
        }
    }
}
