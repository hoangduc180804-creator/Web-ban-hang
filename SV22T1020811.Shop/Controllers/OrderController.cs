using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models.Sales;
using SV22T1020811.Shop.AppCodes;
using SV22T1020811.Models.DataDictionary;

namespace SV22T1020811.Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        /// <summary>
        /// Hiển thị lịch sử mua hàng của khách hàng
        /// </summary>
        public async Task<IActionResult> History(int page = 1, string searchValue = "")
        {
            // Lấy ID khách hàng từ Claim
            var cidClaim = User.FindFirst("CustomerID")?.Value;
            if (string.IsNullOrEmpty(cidClaim) || cidClaim == "0")
            {
                return RedirectToAction("Logout", "Account"); 
            }

            int customerId = int.Parse(cidClaim);

            var input = new OrderSearchInput
            {
                Page = page,
                PageSize = 5,
                SearchValue = searchValue ?? "", 
                CustomerID = customerId,
                Status = 0
            };

            var result = await SalesDataService.ListOrdersAsync(input);

            // Đẩy SearchValue ra để View dùng cho link phân trang
            ViewBag.SearchValue = searchValue;

            return View(result);
        }

        /// <summary>
        /// Trang xác nhận thông tin thanh toán và địa chỉ giao hàng
        /// </summary>
        [Authorize] // Đảm bảo chỉ người dùng đã đăng nhập mới vào được trang thanh toán
        public async Task<IActionResult> Checkout()
        {
            // 1. Lấy danh sách giỏ hàng từ Session
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("MyCart");
            if (cart == null || !cart.Any())
                return RedirectToAction("Index", "Cart");

            // 2. Lấy thông tin hồ sơ của Đức từ Database
            // Claims "CustomerID" đã được Đức lưu lúc đăng nhập trong AccountController
            int customerId = int.Parse(User.FindFirst("CustomerID")?.Value ?? "0");
            var profile = await PartnerDataService.GetCustomerAsync(customerId);

            // 3. Truyền dữ liệu sang View
            ViewBag.CustomerProfile = profile; // Thông tin hồ sơ để điền sẵn vào Form
            ViewBag.Provinces = await CommonDataService.ListProvincesAsync(); // Danh sách tỉnh thành

            return View(cart);
        }

        /// <summary>
        /// Xử lý lưu đơn hàng vào Database
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string deliveryProvince, string deliveryAddress)
        {
            // 1. Kiểm tra giỏ hàng
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("MyCart");
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // 2. Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(deliveryProvince) || string.IsNullOrEmpty(deliveryAddress))
            {
                ModelState.AddModelError("Error", "Vui lòng chọn Tỉnh/Thành và nhập địa chỉ giao hàng.");
                ViewBag.Provinces = await CommonDataService.ListProvincesAsync();
                return View("Checkout", cart);
            }

            try
            {
                // 3. Lấy CustomerID từ người dùng đang đăng nhập
                int customerId = int.Parse(User.FindFirst("CustomerID")?.Value ?? "0");

                // 4. Gọi hàm InitOrderAsync từ SalesDataService mà Đức đã viết
                // Tham số: employeeID = 0 (khách tự đặt), customerID, tỉnh, địa chỉ, giỏ hàng
                int orderID = await SalesDataService.InitOrderAsync(0, customerId, deliveryProvince, deliveryAddress, cart);

                if (orderID > 0)
                {
                    // Đặt hàng thành công: Xóa giỏ hàng và chuyển hướng về trang lịch sử
                    HttpContext.Session.Remove("MyCart");
                    return RedirectToAction("History");
                }
                else
                {
                    ModelState.AddModelError("Error", "Không thể khởi tạo đơn hàng. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "Đã xảy ra lỗi: " + ex.Message);
            }

            // Nếu có lỗi, quay lại trang Checkout cùng dữ liệu cũ
            ViewBag.Provinces = await CommonDataService.ListProvincesAsync();
            return View("Checkout", cart);
        }

        /// <summary>
        /// Xử lý xóa đơn hàng (Chỉ khi đơn hàng ở trạng thái Chờ duyệt - Init)
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            int customerId = int.Parse(User.FindFirst("CustomerID")?.Value ?? "0");
            var order = await SalesDataService.GetOrderAsync(id);

            // Kiểm tra: Đơn hàng tồn tại + Đúng của khách này + Trạng thái là Init (1)
            if (order != null && order.CustomerID == customerId && order.Status == OrderStatusEnum.New)
            {
                await SalesDataService.DeleteOrderAsync(id);
                TempData["Message"] = "Đã xóa đơn hàng thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể xóa đơn hàng này (có thể đơn đã được duyệt hoặc không tồn tại).";
            }

            return RedirectToAction("History");
        }

        public async Task<IActionResult> EditShipping(int id)
        {
            int customerId = int.Parse(User.FindFirst("CustomerID")?.Value ?? "0");
            var order = await SalesDataService.GetOrderAsync(id);

            // FIX: Nếu đơn KHÔNG PHẢI trạng thái New thì mới không cho sửa (Redirect)
            if (order == null || order.CustomerID != customerId || order.Status != OrderStatusEnum.New)
            {
                TempData["Error"] = "Đơn hàng đã được xử lý, không thể thay đổi địa chỉ.";
                return RedirectToAction("History");
            }

            ViewBag.Provinces = await CommonDataService.ListProvincesAsync();
            return View(order);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin giao hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateShipping(int orderID, string deliveryProvince, string deliveryAddress)
        {
            var order = await SalesDataService.GetOrderAsync(orderID);
            int customerId = int.Parse(User.FindFirst("CustomerID")?.Value ?? "0");

            if (order != null && order.CustomerID == customerId && order.Status == OrderStatusEnum.New)
            {
                order.DeliveryProvince = deliveryProvince;
                order.DeliveryAddress = deliveryAddress;

                // Gọi hàm update của SalesDataService
                await SalesDataService.UpdateOrderAsync(order);
                TempData["Message"] = "Đã cập nhật địa chỉ giao hàng mới thành công!";
            }

            return RedirectToAction("History");
        }

        public async Task<IActionResult> Edit(int id)
        {
            int customerId = int.Parse(User.FindFirst("CustomerID")?.Value ?? "0");

            // 1. Lấy thông tin đơn hàng
            var order = await SalesDataService.GetOrderAsync(id);

            // 2. Lấy danh sách sản phẩm (Dùng ListDetailsAsync đúng tên trong Service của Đức)
            var details = await SalesDataService.ListDetailsAsync(id);

            if (order != null && order.CustomerID == customerId && order.Status == OrderStatusEnum.New)
            {
                // 3. Đổ dữ liệu sang CartItem
                var cart = details.Select(d => new CartItem
                {
                    ProductID = d.ProductID,
                    ProductName = d.ProductName,
                    Photo = d.Photo,
                    Unit = d.Unit,
                    Quantity = d.Quantity,
                    SalePrice = d.SalePrice
                }).ToList();

                // 4. Lưu vào Session "MyCart" (Sử dụng extension SetObjectAsJson của Đức)
                HttpContext.Session.SetObjectAsJson("MyCart", cart);

                // 5. Xóa đơn hàng cũ (Dùng DeleteOrderAsync trong Service của Đức)
                await SalesDataService.DeleteOrderAsync(id);

                TempData["Message"] = $"Đơn hàng #{id} đã được chuyển về giỏ hàng để bạn chỉnh sửa.";
                return RedirectToAction("Index", "Cart");
            }

            TempData["Error"] = "Không thể chỉnh sửa đơn hàng này.";
            return RedirectToAction("History");
        }
    }
}