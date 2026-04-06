using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.Sales;
using SV22T1020811.Shop.Models;

namespace SV22T1020811.Shop.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        public async Task<IActionResult> Profile()
        {
            // 1. Lấy ID từ Claim
            var customerIdClaim = User.FindFirst("CustomerID")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim)) return RedirectToAction("Login", "Account");
            int customerId = int.Parse(customerIdClaim);

            // 2. Lấy thông tin khách hàng (Model chính)
            var customer = await PartnerDataService.GetCustomerAsync(customerId);
            if (customer == null) return NotFound();

            // 3. Lấy 5 đơn hàng mới nhất truyền qua ViewBag (giống cách làm trang Product)
            var orderInput = new OrderSearchInput { Page = 1, PageSize = 5, CustomerID = customerId };
            var orderResult = await SalesDataService.ListOrdersAsync(orderInput);
            ViewBag.RecentOrders = orderResult.DataItems;

            return View(customer); // Truyền Model Customer vào View
        }
    }
}