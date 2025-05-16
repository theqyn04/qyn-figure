namespace qyn_figure.Models.Momo
{
    public class MomoOptionModel
    {
        public string MomoApiUrl { get; set; }
        public string SecretKey { get; set; } // Đổi từ SecretKeys -> SecretKey
        public string AccessKey { get; set; }
        public string ReturnUrl { get; set; }
        public string NotifyUrl { get; set; }
        public string PartnerCode { get; set; }
        public string RequestType { get; set; }
    }
}
