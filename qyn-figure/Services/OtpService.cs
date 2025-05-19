namespace qyn_figure.Services
{
    public class OtpService : IOtpService
    {
        private readonly Dictionary<string, (string Otp, DateTime Expiry)> _otpStorage = new();
        private readonly int _otpExpiryMinutes = 5;

        public string GenerateOtp(string email)
        {
            // Tạo OTP 6 chữ số
            var otp = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.Now.AddMinutes(_otpExpiryMinutes);

            _otpStorage[email] = (otp, expiry);

            return otp;
        }

        public bool ValidateOtp(string email, string otp)
        {
            if (!_otpStorage.TryGetValue(email, out var otpData))
                return false;

            // Kiểm tra OTP và thời gian hết hạn
            return otpData.Otp == otp && DateTime.Now <= otpData.Expiry;
        }
    }
}
