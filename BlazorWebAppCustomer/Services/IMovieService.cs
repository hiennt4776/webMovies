using Blazored.LocalStorage;
using BlazorWebAppCustomer.Data;
using dbMovies.Models;
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

    public interface IMovieService
    {
        Task<List<MovieViewModel>> GetRandomTop5MoviesAsync();
        Task<List<MovieViewModel>> GetMoviesAsync();
        Task<MovieViewModel> GetMovieAsync(int movieId);
        string GetMovieFileUrl(int movieId, string type);
        Task<MovieAccessViewModel?> CheckAccessAsync(int movieId);
        Task<PagedResult<MovieViewModel>> SearchMoviesAsync(MovieQueryViewModel query);

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

            var url = $"{_settings.BaseUrl}UserCustomer/random-top5";
            return await _httpClient.GetFromJsonAsync<List<MovieViewModel>>(url) ?? new List<MovieViewModel>();

            //return await _httpClient.GetFromJsonAsync<List<MovieViewModel>>("/api/Movie/random-top5")
            //       ?? new List<MovieViewModel>();



            //string url = $"Movie/random-top5";
            //var response = await _apiClient.GetAsync(url);
            //if (!response.IsSuccessStatusCode) return new List<MovieViewModel>();
            //return await response.Content.ReadFromJsonAsync<List<MovieViewModel>>() ?? new List<MovieViewModel>();
        }

        public async Task<List<MovieViewModel>> GetMoviesAsync()
        {
            var url = $"{_settings.BaseUrl}movie";
            var response = _httpClient.GetFromJsonAsync<List<MovieViewModel>>(url);
        
            return await _httpClient.GetFromJsonAsync<List<MovieViewModel>>(url);
        }

        public async Task<MovieViewModel> GetMovieAsync(int movieId)
        {
            var url = $"{_settings.BaseUrl}movie/{movieId}";
            return await _httpClient.GetFromJsonAsync<MovieViewModel>(url);
        }

        public async Task<MovieAccessViewModel?> CheckAccessAsync(int movieId)
        {
            return await _apiClient.GetAsync<MovieAccessViewModel>(
                $"movie/access/{movieId}");
        }

        public string GetMovieFileUrl(int movieId, string type)
        {
            return $"{_settings.BaseUrl}movie/{movieId}/file/{type}";
        }
        public async Task<PagedResult<MovieViewModel>> SearchMoviesAsync(MovieQueryViewModel query)
        {
            var url = $"{_settings.BaseUrl}movie/search";
            var response = await _httpClient.PostAsJsonAsync(url, query);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PagedResult<MovieViewModel>>();
        }


    }
}