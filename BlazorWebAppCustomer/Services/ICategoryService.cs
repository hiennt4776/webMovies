using Azure;
using Blazored.LocalStorage;
using BlazorWebAppCustomer.Data;
using dbMovies.Models;
using helperMovies.DTO;
using helperMovies.ViewModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace BlazorWebAppCustomer.Services
{

    public interface ICategoryService
    {
        Task<List<CategoryViewModel>> GetCategoriesAsync();

    }

    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ApiClient _apiClient;
        private readonly ApiSettings _settings;
        public CategoryService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage,  IOptions<ApiSettings> settings)
        
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;

            _settings = settings.Value;
        }



        public async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
            //string url = $"Category/getAllCategories";
            //var response = await _apiClient.GetAsync(url);
            //var z = await response.Content.ReadFromJsonAsync<CategoryViewModel>();
            //if (!response.IsSuccessStatusCode) return new List<CategoryViewModel>();
           
            //return await response.Content.ReadFromJsonAsync<List<CategoryViewModel>>() ?? new List<CategoryViewModel>();


            var url = $"{_settings.BaseUrl}Category/getAllCategories";
            return await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>(url);

            //return await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>() ?? new List<CategoryViewModel>();
        }


    }
}