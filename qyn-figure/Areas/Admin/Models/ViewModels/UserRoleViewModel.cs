namespace qyn_figure.Areas.Admin.Models.ViewModels
{
    public class UserRoleViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string RoleId { get; set; }  // Thêm property này để lưu trữ RoleId
        public List<string> Roles { get; set; } // Giữ nguyên nếu cần hiển thị nhiều role
    }
}
