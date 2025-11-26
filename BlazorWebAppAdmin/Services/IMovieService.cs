using Blazored.LocalStorage;
using helperMovies.ViewModel;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace BlazorWebAppAdmin.Services
{

    public interface IMovieService
    {
        Task<PagedResult<MovieViewModel>> GetMoviesAsync(MovieSearchViewModel movieSearchViewModel);
        Task<MovieViewModel?> GetMovieAsync(int id);
        Task<List<MovieFileViewModel>> GetFilesAsync(int movieId);
        Task<bool> UploadFileAsync(int movieId, string fileType, IBrowserFile file);
    }



public class MovieService : IMovieService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ApiClient _apiClient;

        public MovieService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;
        
        }

        public async Task<PagedResult<MovieViewModel>> GetMoviesAsync(MovieSearchViewModel movieSearchViewModel)
        {
            //return await _http.GetFromJsonAsync<PagedResult<MovieViewModel>>
            //    ($"api/movies?search={search}&page={page}&pageSize={pageSize}")
            //    ?? new PagedResult<MovieViewModel>();

            var query = $"Movie/getPageSearchMovies?Keyword={movieSearchViewModel.Keyword}&PageNumber={movieSearchViewModel.PageNumber}&PageSize={movieSearchViewModel.PageSize}";

            // Gọi qua ApiClient
            var response = await _apiClient.GetAsync(query);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Lỗi khi gọi API: {response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<PagedResult<MovieViewModel>>();
            if (result == null)
                throw new Exception("Không thể đọc dữ liệu trả về từ API.");

            return result;

        }

        public async Task<MovieViewModel?> GetMovieAsync(int id)
        {
            string url = $"Movie/get/{id}";
            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<MovieViewModel>();
        }

        public async Task<List<MovieFileViewModel>> GetFilesAsync(int movieId)
        {
            return await _httpClient.GetFromJsonAsync<List<MovieFileViewModel>>($"api/moviefiles/{movieId}") ?? new();
        }


        public async Task<bool> UploadFileAsync(int movieId, string fileType, IBrowserFile file)
        {
            var content = new MultipartFormDataContent();
            var stream = file.OpenReadStream(maxAllowedSize: 2000_000_000);
            content.Add(new StreamContent(stream), "file", file.Name);
            content.Add(new StringContent(fileType), "fileType");


            var response = await _httpClient.PostAsync($"api/moviefiles/upload/{movieId}", content);
            return response.IsSuccessStatusCode;
        }

    }


}
