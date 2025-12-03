using Blazored.LocalStorage;
using BlazorWebAppAdmin.Services;
using helperMovies.ViewModel;
using helperMovies.ViewModel;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace BlazorWebAppAdmin.Services
{


    public interface IMoviePricingService
    {
        Task<IEnumerable<CreateUpdateMoviePricingViewModel>> GetByMovieAsync(int movieId);
        Task<CreateUpdateMoviePricingViewModel> GetByIdAsync(int id);
        Task<CreateUpdateMoviePricingViewModel> CreateAsync(CreateUpdateMoviePricingViewModel ViewModel);
    
     Task<CreateUpdateMoviePricingViewModel> UpdateAsync(int id, CreateUpdateMoviePricingViewModel ViewModel);
        Task<bool> DeleteAsync(int id);
        Task<DateTime?> RentAsync(int id);
    }

    public class MoviePricingService : IMoviePricingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ApiClient _apiClient;

        public MoviePricingService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;
        
        }

        //    public async Task<IEnumerable<MoviePricingViewModel>> GetByMovieAsync(int movieId)
        //    {
        //        var res = await _apiClient.GetAsync($"MoviePricing/movie/{movieId}");
        //        res.EnsureSuccessStatusCode();
        //        return await res.Content.ReadFromJsonAsync<IEnumerable<MoviePricingViewModel>>();
        //    }

        //    public async Task<MoviePricingViewModel> GetActiveAsync(int movieId)
        //    {
        //        var res = await _apiClient.GetAsync($"MoviePricing/movie/{movieId}/active");
        //        res.EnsureSuccessStatusCode();
        //        return await res.Content.ReadFromJsonAsync<MoviePricingViewModel>();
        //    }
        //    public async Task<CreateUpdateMoviePricingViewModel> GetByIdAsync(int id)
        //=> await _http.GetFromJsonAsync<CreateUpdateMoviePricingViewModel>($"MoviePricings/{id}");
        //    public async Task<MoviePricingViewModel> CreateAsync(CreateUpdateMoviePricingViewModel ViewModel)
        //    {
        //        var res = await _apiClient.PostJsonAsync("MoviePricing", ViewModel);
        //        return await res.Content.ReadFromJsonAsync<MoviePricingViewModel>();
        //    }

        //    public async Task<MoviePricingViewModel> UpdateAsync(int id, CreateUpdateMoviePricingViewModel ViewModel)
        //    {
        //        var res = await _apiClient.PutJsonAsync($"MoviePricing/{id}", ViewModel);
        //        res.EnsureSuccessStatusCode();
        //        return await res.Content.ReadFromJsonAsync<MoviePricingViewModel>();
        //    }

        //    public async Task<bool> DeleteAsync(int id)
        //    {
        //        var res = await _apiClient.DeleteAsync($"MoviePricing/{id}");
        //        return res.IsSuccessStatusCode;
        //    }


        public async Task<IEnumerable<CreateUpdateMoviePricingViewModel>> GetByMovieAsync(int movieId)
        {
            var response = await _apiClient.GetAsync($"MoviePricing/movie/{movieId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<CreateUpdateMoviePricingViewModel>>();
        }

        public async Task<CreateUpdateMoviePricingViewModel> GetByIdAsync(int id)
        {
            var response = await _apiClient.GetAsync($"MoviePricing/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CreateUpdateMoviePricingViewModel>();
        }

        public async Task<CreateUpdateMoviePricingViewModel> CreateAsync(CreateUpdateMoviePricingViewModel ViewModel)
        {
            var response = await _apiClient.PostJsonAsync("MoviePricing", ViewModel);
            return await response.Content.ReadFromJsonAsync<CreateUpdateMoviePricingViewModel>();
        }

        public async Task<CreateUpdateMoviePricingViewModel> UpdateAsync(int id, CreateUpdateMoviePricingViewModel ViewModel)
        {
            var response = await _apiClient.PutJsonAsync($"MoviePricing/{id}", ViewModel);
            return await response.Content.ReadFromJsonAsync<CreateUpdateMoviePricingViewModel>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _apiClient.DeleteAsync($"MoviePricings/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<DateTime?> RentAsync(int id)
        {
            var res = await _apiClient.PostFormAsync($"MoviePricings/{id}/rent", null);
            if (!res.IsSuccessStatusCode)
                return null;

            var json = await res.Content.ReadFromJsonAsync<JsonElement>();

            if (json.TryGetProperty("rentalExpiryUtc", out var v) &&
                v.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(v.GetString(), out var dt))
            {
                return dt;
            }

            return null;
        }
    }


}
