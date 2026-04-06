using SV22T1020811.Models.Catalog;

namespace SV22T1020811.DataLayers.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> ListAllAsync(); // Lấy tất cả danh mục để hiện Sidebar
        Task<Category?> GetAsync(int categoryID);
    }
}