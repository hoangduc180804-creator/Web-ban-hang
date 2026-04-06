namespace SV22T1020811.Models.Common
{
    /// <summary>
    /// Phần tử trên thanh phân trang, có thể là một số trang hoặc dấu "..." để phân cách các nhóm trang
    /// </summary>
    public class PageItem
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pageNumber">0 nếu là phần tử dùng để thể hiện dấu "..." phân cách</param>
        /// <param name="isCurrent"></param>
        public PageItem(int pageNumber, bool isCurrent = false)
        {
            Page = pageNumber;
            IsCurrent = isCurrent;
        }
        /// <summary>
        /// Số trang (có giá trị là 0 nếu là dấu "..." để phân cách các nhóm trang)
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// Có phải là trang hiện tại hay không?
        /// </summary>
        public bool IsCurrent { get; set; }
        /// <summary>
        /// Có phải là vị trí hiển thị dấu "..." để phân cách các nhóm trang hay không?
        /// </summary>
        public bool IsEllipsis => Page == 0;
    }
}
