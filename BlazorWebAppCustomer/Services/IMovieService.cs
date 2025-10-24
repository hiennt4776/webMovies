using Blazored.LocalStorage;
using helperMovies.ViewModel;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using BlazorWebAppCustomer.Data;

namespace BlazorWebAppCustomer.Services
{

    public interface IMovieService
    {
        Task<List<MovieViewModel>> GetRandomTop5MoviesAsync();
    }
    


    public class MovieService : IMovieService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiClient _apiClient;
        private readonly ApiSettings _settings;
        private readonly ILocalStorageService _localStorage;

       

        public MovieService(HttpClient httpClient, ILocalStorageService localStorage, ApiClient apiClient, IOptions<ApiSettings> settings)
        {
                _httpClient = httpClient;
                _localStorage = localStorage;
                _apiClient = apiClient;
            _settings = settings.Value;
        }


            public async Task<List<MovieViewModel>> GetRandomTop5MoviesAsync()
        {

            var url = $"{_settings.BaseUrl}/UserCustomer/random-top5";
            return await _httpClient.GetFromJsonAsync<List<MovieViewModel>>(url) ?? new List<MovieViewModel>();

            //return await _httpClient.GetFromJsonAsync<List<MovieViewModel>>("/api/Movie/random-top5")
            //       ?? new List<MovieViewModel>();



            //string url = $"Movie/random-top5";
            //var response = await _apiClient.GetAsync(url);
            //if (!response.IsSuccessStatusCode) return new List<MovieViewModel>();
            //return await response.Content.ReadFromJsonAsync<List<MovieViewModel>>() ?? new List<MovieViewModel>();
        }

    
    }
}