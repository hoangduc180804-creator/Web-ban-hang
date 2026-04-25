using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.Sales;
using Svg; // Cần cài NuGet: Svg
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
        // Lấy dữ liệu và in đơn hàng ra PDF bằng Rotativa
        public async Task<IActionResult> ExportToPdf(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return RedirectToAction("Index");

            var details = await SalesDataService.ListDetailsAsync(id);
            var model = new OrderDetailModel { Order = order, Details = details };

            return new ViewAsPdf("Print", model)
            {
                FileName = $"HoaDon_{id}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"Trang [page]/[toPage]\" --footer-font-size \"10\""
            };
        }

        public async Task<IActionResult> ExportToExcel(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            var details = await SalesDataService.ListDetailsAsync(id);
            if (order == null) return RedirectToAction("Index");

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Hóa Đơn");

                // --- 1. SETUP KHÔNG GIAN ---
                ws.ShowGridLines = false;
                ws.PageSetup.CenterHorizontally = true;
                ws.Cells("A1:Z200").Style.Fill.BackgroundColor = XLColor.FromHtml("#E5E7EB");
                ws.Range("A1:H200").Style.Fill.BackgroundColor = XLColor.White;

                // Cấu hình độ rộng cột
                ws.Column(1).Width = 5;   // Đệm trái
                ws.Column(2).Width = 7;   // CỘT B: Chừa chỗ cho LOGO
                ws.Column(3).Width = 43;  // CỘT C: Tên SP (giảm lại chút để bù cho cột B)
                ws.Column(4).Width = 10;
                ws.Column(5).Width = 12;
                ws.Column(6).Width = 18;
                ws.Column(7).Width = 20;
                ws.Column(8).Width = 5;   // Đệm phải

                // --- 2. HEADER SECTION ---
                var headerBg = ws.Range("A1:H4"); // Tăng độ cao header lên dòng 4
                headerBg.Style.Fill.BackgroundColor = XLColor.FromHtml("#f2f3f5");

                // --- CHÈN LOGO SVG (MỚI) ---
                string svgContent = @"<svg width='55' height='55' viewBox='0 0 100 100' fill='none' xmlns='http://www.w3.org/2000/svg'>
            <rect x='20' y='45' width='40' height='35' stroke='#2a3d66' stroke-width='5' fill='none'/>
            <rect x='50' y='20' width='30' height='30' stroke='#2a3d66' stroke-width='5' fill='none'/>
            <line x1='10' y1='80' x2='90' y2='80' stroke='#2a3d66' stroke-width='5'/>
        </svg>";

                try
                {
                    var svgDocument = SvgDocument.FromSvg<SvgDocument>(svgContent);
                    using (var bitmap = svgDocument.Draw())
                    {
                        using (var ms = new MemoryStream())
                        {
                            bitmap.Save(ms, ImageFormat.Png);
                            var picture = ws.AddPicture(ms)
                                            .MoveTo(ws.Cell("B2")) // Đặt logo tại ô B2
                                            .WithSize(45, 45);     // Chỉnh kích thước vừa vặn
                        }
                    }
                }
                catch { /* Xử lý nếu không có thư viện Svg */ }

                // Tên thương hiệu (Dịch sang cột C để không đè lên Logo)
                var brandCell = ws.Cell("C2");
                brandCell.Value = "DUC STEEL";
                brandCell.Style.Font.FontColor = XLColor.FromHtml("#2a3d66");
                brandCell.Style.Font.Bold = true;
                brandCell.Style.Font.FontSize = 20;
                brandCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                var webCell = ws.Cell("G2");
                webCell.Value = "Hệ thống quản lý trực tuyến";
                webCell.Style.Font.FontColor = XLColor.FromHtml("#2a3d66");
                webCell.Style.Font.Bold = true;
                webCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                webCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // --- 3. THÔNG TIN KHÁCH HÀNG (Giữ nguyên logic cũ nhưng dịch cột) ---
                ws.Cell("B6").Value = $"TÊN: {order.CustomerName.ToUpper()}";
                ws.Cell("B6").Style.Font.Bold = true;
                ws.Cell("B6").Style.Font.FontSize = 13;
                ws.Cell("B6").Style.Font.FontColor = XLColor.FromHtml("#2a3d66");

                ws.Cell("B7").Value = $"SĐT: {order.CustomerPhone}";
                ws.Cell("B8").Value = order.CustomerAddress;

                var invoiceTitle = ws.Cell("G6");
                invoiceTitle.Value = "HÓA ĐƠN";
                invoiceTitle.Style.Font.Bold = true;
                invoiceTitle.Style.Font.FontSize = 26;
                invoiceTitle.Style.Font.FontColor = XLColor.FromHtml("#d15c42");
                invoiceTitle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                ws.Cell("G7").Value = $"Hóa đơn #{order.OrderID:D6}";
                ws.Cell("G7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                ws.Cell("G8").Value = $"Ngày {order.OrderTime:dd/MM/yyyy}";
                ws.Cell("G8").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // --- 4. BẢNG DỮ LIỆU (Phong cách Minimalist) ---
                int headerRow = 10; // Chỉnh lại dòng 10 cho sát với mẫu
                var tableHeader = ws.Range(headerRow, 2, headerRow, 7);
                tableHeader.Style.Font.Bold = true;
                tableHeader.Style.Font.FontSize = 12;
                tableHeader.Style.Font.FontColor = XLColor.Black;

                // Chỉ sử dụng đường kẻ ngang thanh mảnh phía trên và dưới tiêu đề
                tableHeader.Style.Border.TopBorder = XLBorderStyleValues.Medium;
                tableHeader.Style.Border.TopBorderColor = XLColor.Black;
                tableHeader.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                tableHeader.Style.Border.BottomBorderColor = XLColor.Black;

                // Căn lề tiêu đề cho khớp với dữ liệu bên dưới
                ws.Cell(headerRow, 2).Value = "Mục";
                ws.Cell(headerRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(headerRow, 3).Value = "Tên sản phẩm";
                ws.Cell(headerRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                ws.Cell(headerRow, 4).Value = "ĐVT";
                ws.Cell(headerRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(headerRow, 5).Value = "Số lượng";
                ws.Cell(headerRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(headerRow, 6).Value = "Đơn giá";
                ws.Cell(headerRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                ws.Cell(headerRow, 7).Value = "Thành tiền";
                ws.Cell(headerRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // Tăng chiều cao dòng tiêu đề
                ws.Row(headerRow).Height = 33;
                tableHeader.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                int currentRow = headerRow + 1;
                decimal totalAmount = 0;
                int stt = 1;

                foreach (var item in details)
                {
                    var rowRange = ws.Range(currentRow, 2, currentRow, 7);

                    // Đường kẻ ngang mờ giữa các dòng sản phẩm
                    rowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    rowRange.Style.Border.BottomBorderColor = XLColor.FromHtml("#EAEAEA");

                    // Tăng chiều cao dòng cực rộng để giống mẫu ảnh 2
                    ws.Row(currentRow).Height = 30;
                    rowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rowRange.Style.Font.FontColor = XLColor.FromHtml("#4A4A4A"); // Màu chữ xám đậm tinh tế

                    decimal thanhTien = item.Quantity * item.SalePrice;
                    totalAmount += thanhTien;

                    // Cột 2: Mục (Căn giữa)
                    ws.Cell(currentRow, 2).Value = stt++;
                    ws.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Cột 3: Tên sản phẩm (Căn trái + Thêm khoảng trống lề)
                    ws.Cell(currentRow, 3).Value = item.ProductName;
                    ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(currentRow, 3).Style.Alignment.Indent = 1;

                    // Cột 4: ĐVT (Căn giữa)
                    ws.Cell(currentRow, 4).Value = item.Unit;
                    ws.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Cột 5: Số lượng (Căn giữa)
                    ws.Cell(currentRow, 5).Value = item.Quantity;
                    ws.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Cột 6: Đơn giá (Căn phải)
                    ws.Cell(currentRow, 6).Value = item.SalePrice;
                    ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0\"đ\"";

                    // Cột 7: Thành tiền (Căn phải)
                    ws.Cell(currentRow, 7).Value = thanhTien;
                    ws.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0\"đ\"";

                    currentRow++;
                }
                // --- 5. TỔNG TIỀN ---
                decimal tax = totalAmount * 0.1m;
                decimal grandTotal = totalAmount + tax;

                currentRow += 1;
                ws.Cell(currentRow, 6).Value = "Tổng cộng:";
                ws.Cell(currentRow, 7).Value = totalAmount;
                ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0\"đ\"";

                currentRow += 1;
                ws.Cell(currentRow, 6).Value = "Thuế (10%):";
                ws.Cell(currentRow, 7).Value = tax;
                ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0\"đ\"";

                currentRow += 1;
                ws.Cell(currentRow, 6).Value = "TỔNG TIỀN:";
                ws.Cell(currentRow, 6).Style.Font.Bold = true;
                ws.Cell(currentRow, 7).Value = grandTotal;
                ws.Cell(currentRow, 7).Style.Font.Bold = true;
                ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0\"Đ\"";
                ws.Range(currentRow, 6, currentRow, 7).Style.Border.TopBorder = XLBorderStyleValues.Thin;

                // --- 6. FOOTER SECTION ---
                currentRow += 3;

                // Vẽ vạch dọc màu cam tạo điểm nhấn
                var footerLine = ws.Range(currentRow, 2, currentRow + 3, 2);
                footerLine.Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                footerLine.Style.Border.LeftBorderColor = XLColor.FromHtml("#d15c42");

                // Cột trái: Thông tin giao dịch
                ws.Cell(currentRow, 2).Value = "    THÔNG TIN GIAO DỊCH";
                ws.Cell(currentRow, 2).Style.Font.Bold = true;
                ws.Cell(currentRow, 2).Style.Font.FontColor = XLColor.FromHtml("#2a3d66");

                ws.Cell(currentRow + 1, 2).Value = $"    Nhân viên lập: {order.EmployeeName}";

                // Ánh xạ trạng thái từ Enum sang Tiếng Việt theo yêu cầu của bạn
                string statusText = order.Status switch
                {
                    OrderStatusEnum.Rejected => "bị từ chối",
                    OrderStatusEnum.Cancelled => "đã bị hủy",
                    OrderStatusEnum.New => "vừa tạo",
                    OrderStatusEnum.Accepted => "đang duyệt",
                    OrderStatusEnum.Shipping => "đang vận chuyển",
                    OrderStatusEnum.Completed => "đã hoàn tất",
                    _ => "Không xác định"
                };
                ws.Cell(currentRow + 2, 2).Value = $"    Trạng thái đơn: {statusText}";

                ws.Cell(currentRow + 3, 2).Value = $"    Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}";

                // Đảm bảo các dòng này được căn giữa theo chiều dọc so với vạch cam
                ws.Range(currentRow, 2, currentRow + 3, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Cột phải: Khối xác nhận của khách hàng
                var customerConfirmHeader = ws.Range(currentRow, 6, currentRow, 7).Merge();
                customerConfirmHeader.Value = "XÁC NHẬN CỦA KHÁCH HÀNG";
                customerConfirmHeader.Style.Font.Bold = true;
                customerConfirmHeader.Style.Font.FontColor = XLColor.FromHtml("#2a3d66"); // Đã sửa màu đúng ô
                customerConfirmHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Dòng hướng dẫn ký tên
                var signatureInstruction = ws.Range(currentRow + 3, 6, currentRow + 3, 7).Merge();
                signatureInstruction.Value = "(Ký và ghi rõ họ tên)";
                signatureInstruction.Style.Font.Italic = true; // Cho in nghiêng cho đúng phong cách hóa đơn
                signatureInstruction.Style.Font.FontSize = 10; // Giảm nhẹ font size một chút cho tinh tế
                signatureInstruction.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Căn giữa theo tiêu đề trên

                // --- 7. ĐÓNG KHUNG CAM DÀY ---
                int lastRow = currentRow + 5;
                if (lastRow < 200) ws.Range(lastRow + 1, 1, 200, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#E5E7EB");

                var a4Page = ws.Range(1, 1, lastRow, 8);
                a4Page.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                a4Page.Style.Border.OutsideBorderColor = XLColor.FromHtml("#d15c42");

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DUC_STEEL_Invoice_{id}.xlsx");
                }
            }
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
        public async Task<IActionResult> UpdateDetail(int orderID, int productID, int quantity, decimal salePrice)
        {
            // Tạo object mới từ tham số để truyền vào Service
            var data = new OrderDetail
            {
                OrderID = orderID,
                ProductID = productID,
                Quantity = quantity,
                SalePrice = salePrice
            };
            // 1. Kiểm tra dữ liệu đầu vào cơ bản
            if (data.Quantity <= 0)
                return Json(new { success = false, message = "Số lượng phải lớn hơn 0." });
            if (data.SalePrice < 0)
                return Json(new { success = false, message = "Giá bán không được nhỏ hơn 0." });

            // 2. Gọi Service để cập nhật vào Database
            // data sẽ tự động mapping các trường OrderID, ProductID, Quantity, SalePrice từ Form gửi lên
            bool result = await SalesDataService.UpdateDetailAsync(data);

            if (result)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Không thể cập nhật mặt hàng. Có thể đơn hàng đã thay đổi trạng thái." });
            }
        }

        [HttpPost]
        public IActionResult AddToCart(CartItem item)
        {
            if (item.Quantity <= 0) return Json("Số lượng không hợp lệ");

            var cart = GetCart();
            var existsItem = cart.FirstOrDefault(m => m.ProductID == item.ProductID);

            if (existsItem == null)
            {
                cart.Add(item);
            }
            else
            {
                // Thay vì cộng dồn (+=), ta gán trực tiếp giá trị mới từ Modal gửi về
                existsItem.Quantity = item.Quantity;
                existsItem.SalePrice = item.SalePrice;
            }

            SaveCart(cart);
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

        // 1. Trả về Modal xác nhận xóa
        public IActionResult ConfirmClearCart()
        {
            return PartialView("ClearCart");
        }

        // 2. Xử lý xóa sạch giỏ hàng (Gọi qua AJAX)
        public IActionResult ClearCart()
        {
            // Xóa Session giỏ hàng theo đúng Key "ShoppingCart"
            HttpContext.Session.Remove(SHOPPING_CART);

            // QUAN TRỌNG: Nạp lại ViewBag để các Select Khách hàng/Tỉnh thành không bị trống
            LoadDataToViewBag();

            // Trả về PartialView giỏ hàng với danh sách rỗng
            return PartialView("ShowCart", new List<CartItem>());
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