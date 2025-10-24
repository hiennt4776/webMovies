using Blazored.LocalStorage;
using dbMovies.Models;
using helperMovies.ViewModel;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace BlazorWebAppAdmin.Services
{

    public interface IContractService
    {
        Task<bool> CreateContractAsync(ContractCreateViewModel contract);
        Task<bool> UpdateContractAsync(int id, ContractEditViewModel contract);
        Task<ContractEditViewModel> GetEditByIdAsync(int id);
        Task<ContractViewModelDetail> GetByIdAsync(int id);
        Task<PagedResult<ContractViewModel>> GetPagedSearchSortAsync(int pageNumber = 1,int pageSize = 10,string? search = null,string? sortField = null,bool ascending = true);
        Task<bool> DeleteAsync(int id);
        Task DownloadFileAsync(int id);
    }

    public class ContractService : IContractService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly IUserEmployeeService _userService;
        private readonly ApiClient _apiClient;
        private readonly IJSRuntime _js;

        public ContractService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage, IUserEmployeeService userService, IJSRuntime js)
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;
            _userService = userService;
            _js = js;
        }

        // Lấy theo ID
        public async Task<ContractEditViewModel> GetEditByIdAsync(int id)
        {
            var response = await _apiClient.GetAsync($"Contract/get/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            var contractViewModelDetail = await response.Content.ReadFromJsonAsync<ContractViewModelDetail>();
            if (contractViewModelDetail == null) return null;

            var viewModel = new ContractEditViewModel
            {
                Id = contractViewModelDetail.Id,
                PartnerId = contractViewModelDetail.Partner?.Id ?? 0,
                ContractNumber = contractViewModelDetail.ContractNumber,
                ContractDate = contractViewModelDetail.ContractDate,
                TotalValue = contractViewModelDetail.TotalValue,
                PaymentTerms = contractViewModelDetail.PaymentTerms,
                Notes = contractViewModelDetail.Notes,
                MovieInContract = contractViewModelDetail.MovieInContract?.Select(m => new MovieInContractViewModel
                {
                    Title = m.Title,
                    Price = m.Price,
                    Notes = m.Notes
                }).ToList() ?? new()
            };

            // 🔹 Nếu có file cũ thì thêm vào
            if (contractViewModelDetail.ContractFileName != null)
            {
                viewModel.ExistingFileName = contractViewModelDetail.ContractFileName;
                viewModel.ExistingFileUrl = contractViewModelDetail.ContractFilePath;
            }

            return viewModel;
        }

        public async Task<PagedResult<ContractViewModel>> GetPagedSearchSortAsync(
              int pageNumber,
              int pageSize,
              string? search,
              string? sortColumn,
              bool ascending)
        {
            var query = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}",
                $"ascending={ascending.ToString().ToLower()}"
            };

            if (!string.IsNullOrWhiteSpace(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrWhiteSpace(sortColumn))
                query.Add($"sortColumn={Uri.EscapeDataString(sortColumn)}");

            string url = $"Contract/getPageSortSearchPartners?{string.Join("&", query)}";

            // Gọi qua ApiClient
            var response = await _apiClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Lỗi khi gọi API: {response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<PagedResult<ContractViewModel>>();
            if (result == null)
                throw new Exception("Không thể đọc dữ liệu trả về từ API.");

            return result;
        }

        public async Task<bool> CreateContractAsync(ContractCreateViewModel contract)
        {

            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(contract.PartnerId.ToString()), "PartnerId");
            content.Add(new StringContent(contract.ContractNumber), "ContractNumber");
            content.Add(new StringContent(contract.ContractDate.ToString()), "ContractDate");
            content.Add(new StringContent(contract.TotalValue.ToString()), "TotalValue");

            if (!string.IsNullOrEmpty(contract.PaymentTerms))
                content.Add(new StringContent(contract.PaymentTerms), "PaymentTerms");

            if (!string.IsNullOrEmpty(contract.Notes))
                content.Add(new StringContent(contract.Notes), "Notes");

            // Danh sách phim (serialize JSON)
            var moviesJson = JsonSerializer.Serialize(contract.MovieInContract);
            content.Add(new StringContent(moviesJson), "MovieInContract");

            // Thêm file upload
            if (contract.ContractFile != null)
            {
                var stream = contract.ContractFile.OpenReadStream(); // 10MB
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(contract.ContractFile.ContentType ?? "application/octet-stream");
                content.Add(fileContent, "ContractFile", contract.ContractFile.Name);
            }

            var response = await _apiClient.PostFormAsync("Contract/create", content);
            return response.IsSuccessStatusCode;
        }

  
        public async Task<bool> UpdateContractAsync(int id, ContractEditViewModel contract)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(contract.Id.ToString()), "Id");
            content.Add(new StringContent(contract.PartnerId.ToString()), "PartnerId");
            content.Add(new StringContent(contract.ContractNumber ?? ""), "ContractNumber");
            content.Add(new StringContent(contract.ContractDate?.ToString() ?? ""), "ContractDate");
            content.Add(new StringContent(contract.TotalValue?.ToString() ?? "0"), "TotalValue");
            content.Add(new StringContent(contract.PaymentTerms ?? ""), "PaymentTerms");
            content.Add(new StringContent(contract.Notes ?? ""), "Notes");

            var moviesJson = JsonSerializer.Serialize(contract.MovieInContract);
            content.Add(new StringContent(moviesJson), "MovieInContract");


            if (contract.ContractFile != null)
            {
                var stream = contract.ContractFile.OpenReadStream();
                var fileContent = new StreamContent(stream);
                content.Add(fileContent, "ContractFile", contract.ContractFile.Name);
            }

            var response = await _apiClient.PutFormAsync($"Contract/update/{contract.Id}", content);
            return response.IsSuccessStatusCode;
        }



        public async Task<bool> DeleteAsync(int id)
        {
            string url = $"Contract/delete/{id}";
            var response = await _apiClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }

        public async Task<ContractViewModelDetail> GetByIdAsync(int id)
        {
            string url = $"Contract/get/{id}";
            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<ContractViewModelDetail>();
        }

        public async Task DownloadFileAsync(int id)
        {
            var response = await _apiClient.GetAsync($"Contract/download/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await _js.InvokeVoidAsync("alert", "Không thể tải file hợp đồng!");
                return;
            }

            var fileName = GetFileNameFromHeader(response.Content.Headers);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var base64 = Convert.ToBase64String(bytes);
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

            // 🔹 Tạo URL blob và tải xuống
            var fileUrl = $"data:{contentType};base64,{base64}";
            await _js.InvokeVoidAsync("downloadFile", fileUrl, fileName);
        }

        private string GetFileNameFromHeader(HttpContentHeaders headers)
        {
            if (headers.ContentDisposition != null)
                return headers.ContentDisposition.FileName?.Trim('"') ?? "contract_file";

            return "contract_file";
        }
    }
}
