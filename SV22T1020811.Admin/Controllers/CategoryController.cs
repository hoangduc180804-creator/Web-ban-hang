using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models;
using SV22T1020811.Models.Catalog;
using SV22T1020811.Models.Common;

namespace SV22T1020811.Admin.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private const int PAGE_SIZE = 10;

        /// <summary>
        /// Hiển thị danh sách loại hàng
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            var input = new PaginationSearchInput()
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue ?? ""
            };

            // Gọi hàm Async từ CatalogDataService
            var result = await CatalogDataService.ListCategoriesAsync(input);
            return View(result);
        }

        /// <summary>
        /// Form thêm mới
        /// </summary>
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm loại hàng";
            var data = new Category()
            {
                CategoryID = 0
            };
            return View("Edit", data);
        }

        /// <summary>
        /// Form cập nhật
        /// </summary>
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật loại hàng";
            var data = await CatalogDataService.GetCategoryAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        /// <summary>
        /// Xử lý lưu dữ liệu
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save(Category data)
        {
            // Kiểm tra dữ liệu thủ công tại Controller
            if (string.IsNullOrWhiteSpace(data.CategoryName))
                ModelState.AddModelError(nameof(data.CategoryName), "Tên loại hàng không được để trống");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.CategoryID <= 0 ? "Thêm loại hàng" : "Cập nhật loại hàng";
                return View("Edit", data);
            }

            if (data.CategoryID <= 0)
            {
                await CatalogDataService.AddCategoryAsync(data);
            }
            else
            {
                await CatalogDataService.UpdateCategoryAsync(data);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xác nhận và thực hiện xóa
        /// </summary>
        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                bool result = await CatalogDataService.DeleteCategoryAsync(id);
                if (!result)
                {
                    // Nếu không xóa được (do đang có hàng hóa thuộc loại này), báo lỗi
                    TempData["ErrorMessage"] = "Không thể xóa loại hàng này vì đang có dữ liệu liên quan.";
                    return RedirectToAction("Delete", new { id = id });
                }
                return RedirectToAction("Index");
            }

            var data = await CatalogDataService.GetCategoryAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }
    }
}