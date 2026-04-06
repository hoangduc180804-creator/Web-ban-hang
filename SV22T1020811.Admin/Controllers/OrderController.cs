using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.Sales;
using System.Text.Json;

namespace SV22T1020811.Admin.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private const int PAGE_SIZE = 25;
        private const string ORDER_SEARCH_CONDITION = "OrderSearchCondition";
        private const string SHOPPING_CART = "ShoppingCart";

        public IActionResult Index()
        {
            var sessionData = HttpContext.Session.GetString(ORDER_SEARCH_CONDITION);
            OrderSearchInput? condition = !string.IsNullOrEmpty(sessionData)
                ? JsonSerializer.Deserialize<OrderSearchInput>(sessionData)
                : new OrderSearchInput { Page = 1, PageSize = PAGE_SIZE, Status = (OrderStatusEnum)0 };

            return View(condition);
        }

        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            input.PageSize = PAGE_SIZE;
            HttpContext.Session.SetString(ORDER_SEARCH_CONDITION, JsonSerializer.Serialize(input));

            var model = await SalesDataService.ListOrdersAsync(input);
            return View(model);
        }

        #region Order Create (Lập đơn hàng mới)

        // 1. Sửa lại hàm Create để nạp sẵn dữ liệu ban đầu
        public IActionResult Create()
        {
            // Nạp sẵn danh sách Khách hàng và Tỉnh thành vào ViewBag
            LoadDataToViewBag();

            // Trả về khung giao diện chính (Index của trang lập đơn)
            return View();
        }

        // 2. Giữ nguyên hàm SearchProduct của Đức
        public async Task<IActionResult> SearchProduct(string searchValue = "", int page = 1)
        {
            var input = new SV22T1020811.Models.Catalog.ProductSearchInput()
            {
                Page = page,
                PageSize = 5,
                SearchValue = searchValue ?? ""
            };
            var model = await ProductDataService.ListProductsAsync(input);
            return PartialView(model);
        }

        // Sửa lại hàm nạp dữ liệu cho chuẩn xác
        private void LoadDataToViewBag()
        {
            // 1. Lấy tỉnh thành (Dùng .Result để ép Async chạy đồng bộ trong Controller)
            ViewBag.ProvinceList = CommonDataService.ListProvincesAsync().Result;

            // 2. Lấy khách hàng
            var customerInput = new PaginationSearchInput() { Page = 1, PageSize = 0, SearchValue = "" };
            var customerResult = PartnerDataService.ListCustomersAsync(customerInput).Result;
            ViewBag.CustomerList = customerResult.DataItems;
        }

        [HttpPost]
        public IActionResult AddToCart(CartItem item)
        {
            if (item.Quantity <= 0) return Json("Số lượng không hợp lệ");
            var cart = GetCart();
            var existsItem = cart.FirstOrDefault(m => m.ProductID == item.ProductID);
            if (existsItem == null) cart.Add(item);
            else { existsItem.Quantity += item.Quantity; existsItem.SalePrice = item.SalePrice; }

            SaveCart(cart);

            // QUAN TRỌNG: Phải có dòng này thì khi View nạp lại mới có danh sách Khách hàng/Tỉnh thành
            LoadDataToViewBag();
            return PartialView("ShowCart", cart);
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            cart.RemoveAll(m => m.ProductID == id);
            SaveCart(cart);

            LoadDataToViewBag(); // Thêm dòng này
            return PartialView("ShowCart", cart);
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(SHOPPING_CART);
            LoadDataToViewBag(); // Thêm dòng này
            return PartialView("ShowCart", new List<CartItem>());
        }
        // Hàm này chỉ để hiển thị cái Modal xác nhận lên màn hình
        public IActionResult ConfirmClearCart()
        {
            return PartialView("ClearCart");
        }

        [HttpPost]
        public async Task<IActionResult> InitOrder(int customerID, string deliveryProvince, string deliveryAddress)
        {
            try
            {
                var cart = GetCart();
                if (cart.Count == 0)
                    return Json(new { success = false, message = "Giỏ hàng trống." });

                // LẤY ID NHÂN VIÊN ĐANG ĐĂNG NHẬP TỪ CLAIMS
                // Trong AccountController bạn đã lưu: new Claim("UserId", userAccount.UserId)
                var userIdClaim = User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int employeeID))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại." });
                }

                // Gọi hàm lưu với employeeID thực tế từ người đang đăng nhập
                int orderID = await SalesDataService.InitOrderAsync(employeeID, customerID, deliveryProvince, deliveryAddress, cart);

                if (orderID > 0)
                {
                    HttpContext.Session.Remove(SHOPPING_CART);
                    return Json(new { success = true, orderID = orderID });
                }

                return Json(new { success = false, message = "Không thể lưu đơn hàng." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        private List<CartItem> GetCart()
        {
            var sessionData = HttpContext.Session.GetString(SHOPPING_CART);
            return string.IsNullOrEmpty(sessionData) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(sessionData)!;
        }

        private void SaveCart(List<CartItem> cart) => HttpContext.Session.SetString(SHOPPING_CART, JsonSerializer.Serialize(cart));

        // 1. Hàm hiển thị Modal Chỉnh sửa
        // 1. Hàm hiển thị Modal Sửa
        public async Task<IActionResult> EditCartItem(int id = 0, int productId = 0)
        {
            // TRƯỜNG HỢP: Sửa mặt hàng trong ĐƠN HÀNG ĐÃ LƯU (Trang Detail)
            // URL sẽ có dạng: /Order/EditCartItem/10?productId=5 (id là OrderID)
            if (productId > 0)
            {
                var model = await SalesDataService.GetDetailAsync(id, productId);
                if (model == null)
                    return Json(new { success = false, message = "Không tìm thấy chi tiết mặt hàng." });

                return PartialView("EditCartItem", model);
            }

            // TRƯỜNG HỢP: Sửa mặt hàng trong GIỎ HÀNG TẠM (Trang ShowCart)
            // URL sẽ có dạng: /Order/EditCartItem/5 (id là ProductID)
            var cart = GetCart();
            var item = cart.FirstOrDefault(m => m.ProductID == id);
            if (item == null)
            {
                return RedirectToAction("ShowCart");
            }

            return PartialView("EditCartItem", item);
        }

        // 2. Hàm hiển thị Modal xác nhận Xóa
        public async Task<IActionResult> DeleteCartItem(int id = 0, int productId = 0)
        {
            // TRƯỜNG HỢP: Xóa trong ĐƠN HÀNG ĐÃ LƯU
            if (productId > 0)
            {
                var model = await SalesDataService.GetDetailAsync(id, productId);
                if (model == null)
                    return Json(new { success = false, message = "Không tìm thấy mặt hàng để xóa." });

                return PartialView("DeleteCartItem", model);
            }

            // TRƯỜNG HỢP: Xóa trong GIỎ HÀNG TẠM
            var cart = GetCart();
            var item = cart.FirstOrDefault(m => m.ProductID == id);
            if (item == null)
            {
                return RedirectToAction("ShowCart");
            }

            return PartialView("DeleteCartItem", item);
        }
        #endregion

        public async Task<IActionResult> Detail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return RedirectToAction("Index");

            var details = await SalesDataService.ListDetailsAsync(id);
            var model = new OrderDetailModel { Order = order, Details = details };

            return View(model);
        }



        #region Order Status Transitions (Các thao tác thay đổi trạng thái)

        /// <summary>
        /// Giao diện hỏi xác nhận duyệt đơn hàng (GET)
        /// </summary>
        [HttpGet]
        public IActionResult Accept(int id)
        {
            // Chỉ trả về View (Modal) để hỏi xác nhận
            return View(id);
        }

        /// <summary>
        /// Thực hiện duyệt đơn hàng sau khi nhấn nút Chấp nhận (POST)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Accept(int id, bool _ = false)
        {
            // Lấy EmployeeID an toàn để tránh lỗi Foreign Key
            int employeeID = 1;
            var userData = User.GetUserData();
            if (userData != null && int.TryParse(userData.UserId, out int idFromUser) && idFromUser > 0)
            {
                employeeID = idFromUser;
            }

            bool result = await SalesDataService.AcceptOrderAsync(id, employeeID);

            if (!result)
            {
                TempData["Message"] = "Không thể duyệt đơn hàng này. Vui lòng kiểm tra lại trạng thái.";
            }

            return RedirectToAction("Detail", new { id = id });
        }

        /// <summary>
        /// Giao diện hỏi xác nhận chuyển giao hàng (GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Shipping(int id)
        {
            // Truyền danh sách người giao hàng sang View để hiển thị trong thẻ <select>
            var input = new PaginationSearchInput { Page = 1, PageSize = 0, SearchValue = "" };
            var result = await CommonDataService.ListShippersAsync(input);
            ViewBag.Shippers = result.DataItems;

            return View(id);
        }

        /// <summary>
        /// Thực hiện chuyển giao hàng (POST)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Shipping(int id, int shipperID)
        {
            bool result = await SalesDataService.ShipOrderAsync(id, shipperID);

            if (!result)
            {
                TempData["Message"] = "Không thể chuyển giao đơn hàng. Vui lòng kiểm tra lại.";
            }

            return RedirectToAction("Detail", new { id = id });
        }

        /// <summary>
        /// Giao diện hỏi xác nhận hoàn tất (GET)
        /// </summary>
        [HttpGet]
        public IActionResult Finish(int id)
        {
            return View(id);
        }

        /// <summary>
        /// Thực hiện hoàn tất đơn hàng (POST)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Finish(int id, bool _ = false)
        {
            bool result = await SalesDataService.CompleteOrderAsync(id);

            if (!result)
            {
                TempData["Message"] = "Không thể hoàn tất đơn hàng. Vui lòng kiểm tra lại trạng thái.";
            }

            return RedirectToAction("Detail", new { id = id });
        }

        /// <summary>
        /// Hiển thị giao diện xác nhận hủy (GET)
        /// </summary>
        [HttpGet]
        public IActionResult Cancel(int id)
        {
            return View(id);
        }

        /// <summary>
        /// Thực hiện hủy đơn hàng (POST)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Cancel(int id, bool _ = false)
        {
            bool result = await SalesDataService.CancelOrderAsync(id);
            if (!result)
            {
                TempData["Message"] = "Hệ thống không cho phép hủy đơn hàng khi đang trong trạng thái này.";
            }
            return RedirectToAction("Detail", new { id = id });
        }

        /// <summary>
        /// Hiển thị giao diện xác nhận từ chối (GET)
        /// </summary>
        [HttpGet]
        public IActionResult Reject(int id)
        {
            return View(id);
        }

        /// <summary>
        /// Thực hiện từ chối đơn hàng (POST)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Reject(int id, bool _ = false)
        {
            // Áp dụng cơ chế lấy mã nhân viên an toàn giống hàm Accept
            int employeeID = 1;
            var userData = User.GetUserData();
            if (userData != null && int.TryParse(userData.UserId, out int idFromUser) && idFromUser > 0)
            {
                employeeID = idFromUser;
            }

            bool result = await SalesDataService.RejectOrderAsync(id, employeeID);

            if (!result)
            {
                TempData["Message"] = "Không thể từ chối đơn hàng này. Vui lòng kiểm tra lại trạng thái.";
            }

            return RedirectToAction("Detail", new { id = id });
        }

        /// <summary>
        /// Hiển thị giao diện xác nhận xóa (GET)
        /// </summary>
        [HttpGet]
        public IActionResult Delete(int id)
        {
            return View(id);
        }

        /// <summary>
        /// Thực hiện xóa đơn hàng (POST)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id, bool _ = false)
        {
            bool result = await SalesDataService.DeleteOrderAsync(id);

            if (!result)
            {
                TempData["Message"] = "Không thể xóa đơn hàng này do trạng thái không hợp lệ hoặc đang có dữ liệu ràng buộc.";
                return RedirectToAction("Detail", new { id = id });
            }

            return RedirectToAction("Index");
        }

        #endregion
    }
}