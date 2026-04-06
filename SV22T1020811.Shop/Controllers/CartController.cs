using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models.Sales;
using SV22T1020811.Shop.AppCodes; // Nhớ dùng đúng namespace AppCodes của Đức

namespace SV22T1020811.Shop.Controllers
{
    public class CartController : Controller
    {
        private const string CART_KEY = "MyCart";

        // Xem giỏ hàng
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ (gọi từ trang Detail)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productID, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

            var item = cart.FirstOrDefault(p => p.ProductID == productID);
            if (item == null)
            {
                // Nếu chưa có trong giỏ, lấy từ DB và thêm mới
                var product = await ProductDataService.GetProductAsync(productID);
                if (product != null)
                {
                    cart.Add(new CartItem
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Photo = product.Photo,
                        Unit = product.Unit,
                        SalePrice = product.Price, // Sử dụng Price làm SalePrice
                        Quantity = quantity
                    });
                }
            }
            else
            {
                // Nếu đã có rồi thì cộng dồn số lượng
                item.Quantity += quantity;
            }

            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ
        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY);
            if (cart != null)
            {
                cart.RemoveAll(p => p.ProductID == id);
                HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
            }
            return RedirectToAction("Index");
        }

        // Xóa sạch giỏ hàng
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CART_KEY);
            return RedirectToAction("Index");
        }
    }
}