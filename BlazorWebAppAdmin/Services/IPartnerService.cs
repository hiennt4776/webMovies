using BlazorWebAppAdmin.Services;
using helperMovies.ViewModel;
using System.Net.Http.Json;
using System.Web;

namespace BlazorWebAppAdmin.Services
{
    public interface IPartnerService
    {
        Task<PagedResult<PartnerViewModel>> GetPagedAsync(int pageIndex, int pageSize);

        
        Task<PagedResult<PartnerViewModel>> GetPagedSearchSortAsync(
            int pageNumber,
            int pageSize,
            string? status = null,
            string? search = null,
            string? sortColumn = null,
            bool ascending = true);

        Task<List<PartnerViewModel>> GetAllPartnersAsync();
        Task<List<PartnerViewModel>> GetAllPartnersByStatusAsync(string status);
        Task<PartnerViewModel> GetByIdAsync(int id);
        Task<PartnerViewModel> AddAsync(PartnerViewModel dto);
        Task<PartnerViewModel> UpdateAsync(int id, PartnerViewModel dto);
        Task<bool> DeleteAsync(int id);
    }

    public class PartnerService : IPartnerService
    {
        private readonly ApiClient _apiClient;

        public PartnerService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PagedResult<PartnerViewModel>> GetPagedAsync(int pageNumber, int pageSize)
        {
            string url = $"Partner/getPagePartners?pageNumber={pageNumber}&pageSize={pageSize}";
            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<PagedResult<PartnerViewModel>>();
        }

        public async Task<List<PartnerViewModel>> GetAllPartnersByStatusAsync(string status)
        {
            string url = $"Partner/getAllPartnersByStatus?partnerStatusConstant={status}";
            var response = await _apiClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<PartnerViewModel>();

            return await response.Content.ReadFromJsonAsync<List<PartnerViewModel>>() ?? new List<PartnerViewModel>();
        }

        public async Task<List<PartnerViewModel>> GetAllPartnersAsync()
        {
            string url = $"Partner/getAllPartners";
            var response = await _apiClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<PartnerViewModel>();

            return await response.Content.ReadFromJsonAsync<List<PartnerViewModel>>() ?? new List<PartnerViewModel>();
        }


        public async Task<PagedResult<PartnerViewModel>> GetPagedSearchSortAsync(
            int pageNumber,
            int pageSize,
            string? status = null,
            string? search = null,
            string? sortColumn = null,
            bool ascending = true)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["pageNumber"] = pageNumber.ToString();
            query["pageSize"] = pageSize.ToString();
            if (!string.IsNullOrWhiteSpace(status)) query["status"] = status;
            if (!string.IsNullOrWhiteSpace(search)) query["search"] = search;
            if (!string.IsNullOrWhiteSpace(sortColumn)) query["sortColumn"] = sortColumn;
            query["ascending"] = ascending.ToString().ToLower();

            string url = $"Partner/getPageSortSearchPartners?{query}";

            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<PagedResult<PartnerViewModel>>();
        }

        public async Task<PartnerViewModel> GetByIdAsync(int id)
        {
            string url = $"Partner/get/{id}";
            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<PartnerViewModel>();
        }

        public async Task<PartnerViewModel> AddAsync(PartnerViewModel dto)
        {
            string url = "Partner/create";
            var response = await _apiClient.PostJsonAsync(url, dto);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<PartnerViewModel>();
        }

        public async Task<PartnerViewModel> UpdateAsync(int id, PartnerViewModel dto)
        {
            string url = $"Partner/update/{id}";
            var response = await _apiClient.PutJsonAsync(url, dto);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<PartnerViewModel>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            string url = $"Partner/delete/{id}";
            var response = await _apiClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
    }
}
