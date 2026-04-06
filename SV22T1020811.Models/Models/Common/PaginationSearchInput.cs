namespace SV22T1020811.Models.Common
{
    /// <summary>
    /// Lớp dùng để biểu diễn thông tin đầu vào của một truy vấn/tìm kiếm 
    /// dữ liệu đơn giản dưới dạng phân trang
    /// </summary>
    public class PaginationSearchInput
    {
        private const int MaxPageSize = 100; //Giới hạn tối đa 100 dòng mỗi trang
        private int _page = 1;
        private int _pageSize = 20;
        private string _searchValue = "";
        
        /// <summary>
        /// Trang cần được hiển thị (bắt đầu từ 1)
        /// </summary>
        public int Page 
        { 
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }
        /// <summary>
        /// Số dòng được hiển thị trên mỗi trang
        /// (0 có nghĩa là hiển thị tất cả các dòng trên một trang, tức là không phân trang)
        /// </summary>
        public int PageSize 
        { 
            get => _pageSize; 
            set
            {
                if (value < 0)
                    _pageSize = 0;
                else if (value > MaxPageSize)
                    _pageSize = MaxPageSize;
                else
                    _pageSize = value;
            }
        }
        /// <summary>
        /// Giá trị tìm kiếm (nếu có) được sử dụng để lọc dữ liệu 
        /// (Nếu không có giá trị tìm kiếm, thì để rỗng)
        /// </summary>
        public string SearchValue
        { 
            get => _searchValue; 
            set => _searchValue = value?.Trim() ?? ""; 
        }        
        /// <summary>
        /// Số dòng cần bỏ qua (tính từ dòng đầu tiên của tập dữ liệu) 
        /// để lấy dữ liệu cho trang hiện tại
        /// </summary>
        public int Offset => PageSize > 0 ? (Page - 1) * PageSize : 0;
    }
}
