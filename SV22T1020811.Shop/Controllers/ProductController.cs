using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;

namespace SV22T1020811.Shop.Controllers
{
    public class ProductController : Controller
    {

        public async Task<IActionResult> Detail(int id)
        {
            // Lấy thông tin chính sản phẩm
            var product = await ProductDataService.GetProductAsync(id);
            if (product == null) return NotFound();

            // Lấy danh sách ảnh phụ (gọi hàm ListPhotosAsync bạn vừa viết SQL)
            ViewBag.Photos = await ProductDataService.ListPhotosAsync(id);

            // Lấy danh sách thuộc tính (gọi hàm ListAttributesAsync bạn vừa viết SQL)
            ViewBag.Attributes = await ProductDataService.ListAttributesAsync(id);

            return View(product);
        }
    }
}