using qyn_figure.Models.Momo;
using qyn_figure.Models.OrderInfo;

namespace qyn_figure.Services
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
