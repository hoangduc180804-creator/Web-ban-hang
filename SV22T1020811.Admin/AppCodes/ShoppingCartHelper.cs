using SV22T1020811.Models.Sales;

namespace SV22T1020811.Admin
{
    /// <summary>
    /// Lớp cung cấp các chưcs năng xử lý trên giỏ hàng
    /// (
    /// </summary>
    public class ShoppingCartHelper
    {
        public const string CART = "Shoppingcart";
        /// <summary>
        /// Lấy giỏ hàng từ session
        /// </summary>
        public static List<OrderDetailViewInfo> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetailViewInfo>>("CART");
            if (cart == null)
            {
                cart = new List<OrderDetailViewInfo>();
                ApplicationContext.SetSessionData("CART", cart);
            }
            return cart;
        }

        // Lấy thông tin 1 mặt hàng từ giỏ hàng
        public static OrderDetailViewInfo? GetCartItem(int productID)
        {
            var cart = GetShoppingCart();
            var item = cart.FirstOrDefault(m => m.ProductID == productID);
            return item;
        }

        /// <summary>
        /// Thêm hàng vào giỏ hàng
        /// </summary>
        /// <param name="item"></param>
        public static void AddToCart(OrderDetailViewInfo item)
        {
            var cart = GetShoppingCart();
            var existingItem = cart.Find(m => m.ProductID == item.ProductID);
            if (existingItem != null)
            {
                cart.Add(item);
            }
            else
            {
                existingItem.Quantity += item.Quantity;
                existingItem.SalePrice += item.SalePrice;
            }
            ApplicationContext.SetSessionData("CART", cart);
        }

        /// <summary>
        /// Xóa mặt hàng ra khỏi giỏ
        /// </summary>
        /// <param name="productID"></param>
        public static void RemoveItemFromCart(int productID)
        {
            var cart = GetShoppingCart();
            int index = cart.FindIndex(m => m.ProductID == productID);
            if (index >= 0)
            {
                cart.RemoveAt(index);
                ApplicationContext.SetSessionData("CART", cart);
            }
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        public static void ClearCart()
        {
            var newCart = new List<OrderDetailViewInfo>();
            ApplicationContext.SetSessionData("CART", newCart);
        }
    }
}
