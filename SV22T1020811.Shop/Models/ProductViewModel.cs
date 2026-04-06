using SV22T1020811.Models.Common;  // Chứa PagedResult
using SV22T1020811.Models.Catalog;  // Chứa class Product
namespace SV22T1020811.Shop.Models
{
    public class ProductViewModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12; // Shop thường hiện 12 SP (3 hoặc 4 cột)
        public string SearchValue { get; set; } = "";
        public int CategoryID { get; set; } = 0;
        public int SupplierID { get; set; } = 0;
        public decimal MinPrice { get; set; } = 0;
        public decimal MaxPrice { get; set; } = 0;

        // Kết quả trả về từ Service
        public List<Product> DataItems { get; set; } = new List<Product>();
        public int RowCount { get; set; }
        public int PageCount => (int)Math.Ceiling((double)RowCount / PageSize);
    }
}