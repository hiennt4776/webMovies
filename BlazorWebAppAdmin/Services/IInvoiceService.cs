using Azure;
using Blazored.LocalStorage;
using dbMovies.Models;
using helperMovies.ViewModel;
using Microsoft.JSInterop;
using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace BlazorWebAppAdmin.Services
{
    public interface IInvoiceService
    {
        public Task<PagedResult<InvoiceViewModel>> GetInvoicesAsync(InvoiceQueryViewModel q);
        public Task<InvoiceViewModel?> GetDetailAsync(int id);
        public Task CancelAsync(int id, string reason);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly IUserEmployeeService _userEmployeeService;
        private readonly ApiClient _apiClient;

        public InvoiceService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage, IUserEmployeeService userEmployeeService)
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;
            _userEmployeeService = userEmployeeService;
        }

        public async Task<PagedResult<InvoiceViewModel>> GetInvoicesAsync(
       InvoiceQueryViewModel q)
        {
            var url =
                $"Invoice/getPageSearchInvoices?" +
                $"page={q.Page}&pageSize={q.PageSize}" +
                $"&keyword={q.Keyword}" +
                $"&sortBy={q.SortBy}" +
                $"&sortDesc={q.SortDesc}" +
                $"&isDeleted={q.IsDeleted}";
            //var response = await _apiClient.GetAsync(url);

            //if (!response.IsSuccessStatusCode) throw new Exception($"Error: {response.StatusCode}");
            //// Deserialize JSON về object
            //var result = await response.Content.ReadFromJsonAsync<PagedResult<InvoiceViewModel>>();
            //return result;

            return await _apiClient.GetAsync<PagedResult<InvoiceViewModel>>(url);

        }
        public async Task CancelAsync(int id, string reason)
        {
            await _apiClient.PostJsonAsync(
                $"Invoice/cancel/{id}",
                new CancelInvoiceRequestViewModel
                {
                    Reason = reason
                });
        }

        public async Task<InvoiceViewModel> GetDetailAsync(int id)
        {
            return await _apiClient.GetAsync<InvoiceViewModel>(
                $"Invoice/detail/{id}");
        }
    }

}
