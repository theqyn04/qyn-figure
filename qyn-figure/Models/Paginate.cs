namespace qyn_figure.Models
{
    public class Paginate
    {
        public int TotalItems { get; private set; } //tổng số items
        public int PageSize { get; private set; } //tổng số item/trang
        public int CurrentPage { get; private set; } //trang hiện tại

        public int TotalPages { get; private set; } //tổng trang
        public int StartPage { get; private set; } //trang bắt đầu
        public int EndPage { get; private set; } //trang kết thúc
        public Paginate()
        {

        }
        public Paginate(int totalItems, int page, int pageSize = 10) //10 items/trang
        {
            //làm tròn tổng items/10 items trên 1 trang VD:16 items/10 = tròn 3 trang
            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize); //33/10 = 3.3 4 trang

            int currentPage = page; //page hiện tại = 1

            int startPage = currentPage - 5; //trang bắt đầu trừ 5 button / 10-5 = 5
            int endPage = currentPage + 4; //trang cuối sẽ cộng thêm 4 button 5 + 4

            if (startPage <= 0)
            {
                //nếu số trang bắt đầu nhỏ hơn or = 0 thì số trang cuối sẽ bằng 
                endPage = endPage - (startPage - 1); //6 - (-3 - 1) = 10;
                startPage = 1;

            }
            if (endPage > totalPages) //nếu số page cuối > số tổng trang 
            {
                endPage = totalPages; //số page cuối = số tổng trang
                if (endPage > 10) //nếu số page cuối > 10
                {
                    startPage = endPage - 9; //trang bắt đầu = trang cuối - 9
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;
        }
    }
}
