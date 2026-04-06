using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models.Catalog;
using SV22T1020811.Models.Common;
using SV22T1020811.Shop.Models;

namespace SV22T1020811.Shop.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index(ProductViewModel model)
        {
            // 1. C?u hình s? l??ng hi?n th? s?n ph?m trên m?t trang
            int pageSize = 9;

            // 2. Thi?t l?p các tham s? tìm ki?m
            var input = new ProductSearchInput()
            {
                Page = model.Page <= 0 ? 1 : model.Page,
                PageSize = pageSize,
                SearchValue = model.SearchValue ?? "",
                CategoryID = model.CategoryID,
                SupplierID = model.SupplierID,
                MinPrice = model.MinPrice,
                MaxPrice = model.MaxPrice,

                
                IsSelling = true
                
            };

            // 3. G?i Service l?y danh sách s?n ph?m
            var result = await ProductDataService.ListProductsAsync(input);

            // 4. L?Y DANH SÁCH DANH M?C CHO SIDEBAR
            ViewBag.Categories = await ProductDataService.ListCategoriesAsync();

            // 5. Gán l?i k?t qu? vào model ?? hi?n th? ? View
            model.Page = input.Page;
            model.PageSize = pageSize;
            model.DataItems = result.DataItems;
            model.RowCount = result.RowCount;

            return View(model);
        }
    }
}