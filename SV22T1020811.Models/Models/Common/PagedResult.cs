namespace SV22T1020811.Models.Common
{
    /// <summary>
    /// Lớp dùng để biểu diễn kết quả truy vấn/tìm kiếm dữ liệu dưới dạng phân trang
    /// </summary>
    /// <typeparam name="T">Kiểu của dữ liệu truy vấn được</typeparam>
    public class PagedResult<T> where T : class
    {
        /// <summary>
        /// Trang đang được hiển thị
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// Số dòng được hiển thị trên mỗi trang (0 có nghĩa là hiển thị tất cả các dòng trên một trang/không phân trang)
        /// </summary>
        public int PageSize { get; set; }        
        /// <summary>
        /// Tổng số dòng dữ liệu được tìm thấy
        /// </summary>
        public int RowCount { get; set; }
        /// <summary>
        /// Danh sách các dòng dữ liệu được hiển thị trên trang hiện tại
        /// </summary>
        public List<T> DataItems { get; set; } = new List<T>();

        /// <summary>
        /// Tổng số trang
        /// </summary>
        public int PageCount
        {
            get
            {
                if (PageSize == 0)
                    return 1;
                return (int)Math.Ceiling((decimal)RowCount / PageSize);
            }
        }
        /// <summary>
        /// Có trang trước không?
        /// </summary>
        public bool HasPreviousPage => Page > 1;
        /// <summary>
        /// Có trang sau không?
        /// </summary>
        public bool HasNextPage => Page < PageCount;             
        /// <summary>
        /// Lấy danh sách các trang được hiển thị trên thanh phân trang
        /// </summary>
        /// <param name="n">Số lượng trang lân cận trang hiện tại cần được hiển thị</param>
        /// <returns></returns>
        public List<PageItem> GetDisplayPages(int n = 5)
        {
            var result = new List<PageItem>();

            if (PageCount == 0)
                return result;

            n = n > 0 ? n : 5; //Giá trị n không hợp lệ, đặt lại về mặc định            

            int currentPage = Page;
            if (currentPage < 1) 
                currentPage = 1;
            else if (currentPage > PageCount)
                currentPage = PageCount;

            int displayedPages = 2 * n + 1;     //Số lượng trang tối đa hiển thị trên thanh phân trang (bao gồm cả trang hiện tại)
            int startPage = currentPage - n;    //Trang bắt đầu hiển thị
            int endPage = currentPage + n;      //Trang kết thúc hiển thị

            //Nếu thiếu bên trái
            if (startPage < 1)
            {
                endPage += (1 - startPage);
                startPage = 1;
            }

            //Nếu thiếu bên phải
            if (endPage > PageCount)
            {
                startPage -= (endPage - PageCount);
                endPage = PageCount;
            }

            //Gán lại bằng 1 nếu startPage bị âm sau khi trừ
            if (startPage < 1)
                startPage = 1;

            //Đảm bảo không vượt quá displayedPages
            if (endPage - startPage + 1 > displayedPages)
                endPage = startPage + displayedPages - 1;

            //Trang đầu
            if (startPage > 1)
            {
                result.Add(new PageItem(1, currentPage == 1));
                //Thêm dấu "..." để phân cách nếu có nhiều trang ở giữa
                if (startPage > 2)
                    result.Add(new PageItem(0));
            }

            //Trang hiện tại và các trang lân cận
            for (int i = startPage; i <= endPage; i++)
            {
                result.Add(new PageItem(i, i == currentPage));
            }

            //Trang cuối
            if (endPage < PageCount)
            {
                //Thêm dấu "..." để phân cách nếu có nhiều trang ở giữa
                if (endPage < PageCount - 1)
                    result.Add(new PageItem(0));
                result.Add(new PageItem(PageCount, currentPage == PageCount));
            }

            return result;
        }
    }
}
