using System.Globalization;

namespace SV22T1020811.Admin // Hoặc Namespace dự án của bạn
{
    public static class Converter
    {
        /// <summary>
        /// Chuyển chuỗi định dạng dd/MM/yyyy sang DateTime
        /// </summary>
        public static DateTime? ToDateTime(this string s, string format = "d/M/yyyy;dd/MM/yyyy;d/MM/yyyy;dd/M/yyyy")
        {
            try
            {
                return DateTime.ParseExact(s, format.Split(';'), CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            catch
            {
                return null;
            }
        }
    }
}