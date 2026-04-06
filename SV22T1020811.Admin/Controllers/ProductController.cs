using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models.Catalog;
using SV22T1020811.Models.Common;

namespace SV22T1020811.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý hàng hóa: thêm, sửa, xóa sản phẩm,
    /// quản lý thuộc tính (Attribute) và hình ảnh (Photo) của từng sản phẩm.
    /// </summary>
    [Authorize]
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 20;
        /// <summary>
        /// Hiển thị danh sách tất cả hàng hóa.
        /// </summary>
        /// <returns>View danh sách hàng hóa.</returns>
        public async Task<IActionResult> Index(int page = 1, string searchValue = "",
                                       int categoryID = 0, int supplierID = 0,
                                       decimal minPrice = 0, decimal maxPrice = 0)
        {
            // 1. Tạo đối tượng đầu vào cho hàm tìm kiếm
            var input = new ProductSearchInput()
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue ?? "",
                CategoryID = categoryID,
                SupplierID = supplierID,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            // 2. Lưu lại các giá trị lọc vào ViewBag
            ViewBag.SearchValue = searchValue;
            ViewBag.CategoryID = categoryID;
            ViewBag.SupplierID = supplierID;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            // 3. Lấy dữ liệu cho các Dropdown (Loại hàng và Nhà cung cấp)
            // Lưu ý: Để PageSize = 0 hoặc một số rất lớn để lấy toàn bộ danh sách
            var categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput { Page = 1, PageSize = 0, SearchValue = "" });
            var suppliers = await CommonDataService.ListSuppliersAsync(new PaginationSearchInput { Page = 1, PageSize = 0, SearchValue = "" });

            ViewBag.Categories = categories.DataItems;
            ViewBag.Suppliers = suppliers.DataItems;

            // 4. Gọi Service để lấy danh sách mặt hàng (Phần bảng chính)
            var data = await CatalogDataService.ListProductsAsync(input);

            return View(data);
        }

        /// <summary>
        /// Hiển thị form thêm mới hàng hóa (dùng lại view Edit).
        /// </summary>
        /// <returns>View Edit với tiêu đề "Thêm hàng hóa".</returns>
        public async Task<IActionResult> Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";

            // 1. Khởi tạo đối tượng Product với các giá trị mặc định
            var data = new Product()
            {
                ProductID = 0,
                IsSelling = true,
                Photo = "nophoto.png"
            };

            // 2. Lấy danh sách Loại hàng cho Dropdown (Dùng PageSize = 0 để lấy tất cả)
            var categoriesResult = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput { Page = 1, PageSize = 0, SearchValue = "" });
            ViewBag.Categories = categoriesResult.DataItems;

            // 3. Lấy danh sách Nhà cung cấp cho Dropdown
            var suppliersResult = await CommonDataService.ListSuppliersAsync(new PaginationSearchInput { Page = 1, PageSize = 0, SearchValue = "" });
            ViewBag.Suppliers = suppliersResult.DataItems;

            // 4. Trả về View "Edit" dùng chung cho cả Thêm và Sửa
            return View("Edit", data);
        }

        [HttpPost]
        public async Task<IActionResult> Save(Product data, IFormFile? uploadPhoto, string Price)
        {
            // 1. Xử lý hình ảnh nếu có upload
            if (uploadPhoto != null)
            {
                // Tạo tên file độc nhất để tránh trùng lặp
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                // Đường dẫn vật lý đến thư mục lưu trữ
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", fileName);

                // Lưu file vào thư mục
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }

                // Cập nhật tên file mới cho đối tượng data
                data.Photo = fileName;
            }

            // 2. Kiểm tra dữ liệu đầu vào (Validation đơn giản)
            if (string.IsNullOrWhiteSpace(data.ProductName))
            {
                ModelState.AddModelError(nameof(data.ProductName), "Tên mặt hàng không được để trống");
            }
            if (data.CategoryID <= 0)
            {
                ModelState.AddModelError(nameof(data.CategoryID), "Vui lòng chọn loại hàng");
            }
            if (data.SupplierID <= 0)
            {
                ModelState.AddModelError(nameof(data.SupplierID), "Vui lòng chọn nhà cung cấp");
            }

            // Nếu có lỗi thì quay lại View Edit để hiển thị thông báo lỗi
            if (decimal.TryParse(Price, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal p))
            {
                data.Price = p;
            }
            else
            {
                // Thử parse theo kiểu tiếng Việt nếu Invariant thất bại
                decimal.TryParse(Price, new System.Globalization.CultureInfo("vi-VN"), out p);
                data.Price = p;
            }

            // Xóa lỗi mặc định của Price nếu có để mình tự check
            ModelState.Remove("Price");

            if (data.Price < 0)
                ModelState.AddModelError(nameof(data.Price), "Giá tiền không hợp lệ");

            // ... các phần check ProductName, CategoryID giữ nguyên ...

            if (!ModelState.IsValid)
            {
                // Nạp lại ViewBag và trả về View
                // ...
                return View("Edit", data);
            }

            // 3. Thực hiện lưu vào CSDL qua Service
            if (data.ProductID <= 0)
            {
                // Thêm mới
                int id = await CatalogDataService.AddProductAsync(data);
                if (id <= 0)
                {
                    ModelState.AddModelError("", "Không thêm được mặt hàng. Có thể tên mặt hàng bị trùng.");
                    // Nạp lại Dropdown và trả về View tương tự như trên...
                    return View("Edit", data);
                }
            }
            else
            {
                // Cập nhật
                bool result = await CatalogDataService.UpdateProductAsync(data);
                if (!result)
                {
                    ModelState.AddModelError("", "Không cập nhật được dữ liệu.");
                    return View("Edit", data);
                }
            }

            // 4. Lưu thành công thì quay về trang danh sách
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa thông tin hàng hóa theo ID.
        /// </summary>
        /// <param name="id">ID của hàng hóa cần chỉnh sửa.</param>
        /// <returns>View Edit với tiêu đề "Cập nhật hàng hóa".</returns>
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật mặt hàng";

            // 1. Lấy thông tin mặt hàng từ CSDL theo ID
            var data = await CatalogDataService.GetProductAsync(id);

            // Nếu không tìm thấy mặt hàng, quay lại trang danh sách
            if (data == null)
            {
                return RedirectToAction("Index");
            }

            // 2. Nạp dữ liệu cho các Dropdown (giống như hàm Create)
            var categoriesResult = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput { Page = 1, PageSize = 0, SearchValue = "" });
            ViewBag.Categories = categoriesResult.DataItems;

            var suppliersResult = await CommonDataService.ListSuppliersAsync(new PaginationSearchInput { Page = 1, PageSize = 0, SearchValue = "" });
            ViewBag.Suppliers = suppliersResult.DataItems;

            // 3. Trả về View "Edit" cùng với dữ liệu của mặt hàng đó
            return View("Edit", data);
        }

        /// <summary>
        /// Hiển thị trang xác nhận xóa hàng hóa theo ID.
        /// </summary>
        /// <param name="id">ID của hàng hóa cần xóa.</param>
        /// <returns>View xác nhận xóa hàng hóa.</returns>
        // Giao diện xác nhận xóa
        public async Task<IActionResult> Delete(int id = 0)
        {
            var data = await CatalogDataService.GetProductAsync(id);
            if (data == null) return RedirectToAction("Index");

            // 1. Tìm tên Loại hàng
            var categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput { Page = 1, PageSize = 0 });
            ViewBag.CategoryName = categories.DataItems.FirstOrDefault(x => x.CategoryID == data.CategoryID)?.CategoryName ?? "Chưa xác định";

            // 2. Tìm tên Nhà cung cấp
            var suppliers = await CommonDataService.ListSuppliersAsync(new PaginationSearchInput { Page = 1, PageSize = 0 });
            ViewBag.SupplierName = suppliers.DataItems.FirstOrDefault(x => x.SupplierID == data.SupplierID)?.SupplierName ?? "Chưa xác định";

            // 3. Logic kiểm tra quyền xóa
            bool isUsed = await CatalogDataService.IsUsedProductAsync(id);
            bool isAdmin = User.IsInRole("admin"); // Kiểm tra role của User hiện tại

            ViewBag.CanDelete = !isUsed; // Vẫn gửi cái này để View biết có ràng buộc hay không
            ViewBag.IsAdmin = isAdmin;
            ViewBag.Title = "Xóa mặt hàng";

            return View(data);
        }

        // Thực hiện xóa
        [HttpPost]
        public async Task<IActionResult> DoDelete(int id)
        {
            bool isAdmin = User.IsInRole("admin");

            // Gọi Service: Nếu là Admin thì truyền true để xóa bằng mọi giá
            bool result = await CatalogDataService.DeleteProductAsync(id, forceDelete: isAdmin);

            if (!result)
            {
                TempData["ErrorMessage"] = "Không thể xóa mặt hàng này (Dữ liệu đang được sử dụng)";
                return RedirectToAction("Delete", new { id = id });
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Hiển thị danh sách thuộc tính của một hàng hóa theo ID sản phẩm.
        /// </summary>
        public async Task<IActionResult> ListAttributes(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null) return RedirectToAction("Index");

            ViewBag.ProductID = id;
            ViewBag.Title = $"Thuộc tính của: {product.ProductName}";

            // Gọi Service lấy danh sách thuộc tính (trả về IEnumerable<ProductAttribute>)
            var data = await CatalogDataService.ListAttributesAsync(id);
            return View(data);
        }

        /// <summary>
        /// Hiển thị form thêm mới thuộc tính cho hàng hóa.
        /// </summary>
        public async Task<IActionResult> CreateAttribute(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null) return RedirectToAction("Index");

            ViewBag.Title = "Thêm thuộc tính hàng hóa";

            // Tạo đối tượng mới với ProductID có sẵn
            var data = new ProductAttribute()
            {
                AttributeID = 0,
                ProductID = id,
                DisplayOrder = 1
            };

            return View("EditAttribute", data);
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa thuộc tính của hàng hóa theo ID thuộc tính.
        /// </summary>
        /// <param name="id">Đây là AttributeID cần sửa</param>
        public async Task<IActionResult> EditAttribute(int id, long attributeId) 
{
    ViewBag.Title = "Cập nhật thuộc tính hàng hóa";

    // Phải dùng tham số attributeId để tìm đúng thuộc tính cần sửa
    var data = await CatalogDataService.GetAttributeAsync(attributeId);
    
    if (data == null) 
    {
        // Nếu không tìm thấy thuộc tính, nó sẽ nhảy về Index. 
        // Đây chính là lý do bạn bị văng ra ngoài.
        return RedirectToAction("Index"); 
    }

    return View(data);
}

        /// <summary>
        /// Xử lý lưu dữ liệu thuộc tính (Thêm mới hoặc Cập nhật)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveAttribute(ProductAttribute data)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(data.AttributeName))
                ModelState.AddModelError(nameof(data.AttributeName), "Tên thuộc tính không được để trống");

            if (string.IsNullOrWhiteSpace(data.AttributeValue))
                ModelState.AddModelError(nameof(data.AttributeValue), "Giá trị không được để trống");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.AttributeID <= 0 ? "Thêm thuộc tính hàng hóa" : "Cập nhật thuộc tính hàng hóa";
                return View("EditAttribute", data);
            }

            // Thực hiện lưu
            if (data.AttributeID <= 0)
            {
                await CatalogDataService.AddAttributeAsync(data);
            }
            else
            {
                await CatalogDataService.UpdateAttributeAsync(data);
            }

            // Sau khi lưu xong, quay lại trang Sửa Mặt Hàng (nơi chứa danh sách thuộc tính)
            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        /// <summary>
        /// Thực hiện xóa thuộc tính của hàng hóa.
        /// </summary>
        /// <param name="id">ID của mặt hàng (ProductID) để quay về trang Edit</param>
        /// <param name="attributeId">ID của thuộc tính cần xóa</param>
        public async Task<IActionResult> DeleteAttribute(int id, long attributeId)
        {
            // Thực hiện xóa cứng thuộc tính
            await CatalogDataService.DeleteAttributeAsync(attributeId);

            // Quay lại trang Edit của mặt hàng
            return RedirectToAction("Edit", new { id = id });
        }

        /// <summary>
        /// Hiển thị danh sách hình ảnh của một hàng hóa theo ID sản phẩm.
        /// </summary>
        /// <param name="id">ID của hàng hóa cần xem danh sách ảnh.</param>
        /// <returns>View danh sách hình ảnh hàng hóa.</returns>
       /// <summary>
/// Hiển thị danh sách hình ảnh của một hàng hóa.
/// </summary>
public async Task<IActionResult> ListPhotos(int id)
{
    var product = await CatalogDataService.GetProductAsync(id);
    if (product == null) return RedirectToAction("Index");

    ViewBag.ProductID = id;
    ViewBag.Title = $"Thư viện ảnh: {product.ProductName}";
    
    var data = await CatalogDataService.ListPhotosAsync(id);
    return View(data);
}

/// <summary>
/// Hiển thị form thêm ảnh mới.
/// </summary>
public async Task<IActionResult> CreatePhoto(int id)
{
    var product = await CatalogDataService.GetProductAsync(id);
    if (product == null) return RedirectToAction("Index");

    ViewBag.Title = "Thêm ảnh hàng hóa";
    
    var data = new ProductPhoto()
    {
        PhotoID = 0,
        ProductID = id,
        DisplayOrder = 1,
        IsHidden = false
    };

    return View("EditPhoto", data);
}

/// <summary>
/// Hiển thị form chỉnh sửa thông tin ảnh.
/// </summary>
public async Task<IActionResult> EditPhoto(int id, long photoId)
{
    ViewBag.Title = "Cập nhật ảnh hàng hóa";
    
    var data = await CatalogDataService.GetPhotoAsync(photoId);
    if (data == null) return RedirectToAction("Edit", new { id = id });

    return View("EditPhoto", data);
}

        /// <summary>
        /// Xử lý lưu ảnh (bao gồm cả upload file)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SavePhoto(ProductPhoto data, IFormFile? uploadPhoto)
        {
            // 1. Xử lý upload ảnh (giữ nguyên logic cũ)
            if (uploadPhoto != null)
            {
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                string folderPath = Path.Combine(ApplicationContext.WWWRootPath, "images", "products");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }
                data.Photo = fileName;
            }

            // 2. KIỂM TRA THỦ CÔNG (Thay thế cho [Required])
            // Kiểm tra mô tả
            if (string.IsNullOrWhiteSpace(data.Description))
            {
                ModelState.AddModelError(nameof(data.Description), "Vui lòng nhập mô tả cho ảnh hàng hóa");
            }

            // Kiểm tra file ảnh (nếu là thêm mới mà chưa có tên file)
            if (data.PhotoID == 0 && string.IsNullOrEmpty(data.Photo))
            {
                ModelState.AddModelError(nameof(data.Photo), "Vui lòng chọn hình ảnh để upload");
            }

            // 3. Nếu có bất kỳ lỗi nào ở trên, trả về View
            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.PhotoID <= 0 ? "Thêm ảnh hàng hóa" : "Cập nhật ảnh hàng hóa";
                return View("EditPhoto", data);
            }

            // 4. Lưu vào Database
            if (data.PhotoID <= 0)
                await CatalogDataService.AddPhotoAsync(data);
            else
                await CatalogDataService.UpdatePhotoAsync(data);

            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        /// <summary>
        /// Thực hiện xóa ảnh.
        /// </summary>
        public async Task<IActionResult> DeletePhoto(int id, long photoId)
{
    // Lưu ý: Nếu muốn kỹ hơn, bạn có thể lấy thông tin ảnh để xóa file vật lý trong wwwroot trước
    await CatalogDataService.DeletePhotoAsync(photoId);
    return RedirectToAction("Edit", new { id = id });
}
    }
}
