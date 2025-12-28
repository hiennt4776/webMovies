namespace BlazorWebAppCustomer.Services
{

    public interface IPaymentClientService
    {
        Task PayAsync(int invoiceId, bool success);
        Task SimulateAsync(int invoiceId, bool success);
    }

    public class PaymentClientService : IPaymentClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiClient _apiClient;

        public PaymentClientService(HttpClient httpClient, ApiClient apiClient)

        {
            _httpClient = httpClient;
            _apiClient = apiClient;

        }



        public async Task PayAsync(int invoiceId, bool success)
        {
            await _apiClient.PostFormAsync(
                $"api/payment/simulate?invoiceId={invoiceId}&success={success}", null);
        }

        public async Task SimulateAsync(int invoiceId, bool success)
        {
            var url = $"api/payment/simulate?invoiceId={invoiceId}&success={success}";
            var resp = await _apiClient.PostJsonAsync(url, new { });
            resp.EnsureSuccessStatusCode();
        }

    }
}
