using Blazored.LocalStorage;
using helperMovies.DTO;
using helperMovies.ViewModel;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;


namespace BlazorWebAppAdmin.Services
{


    public interface IMovieFileService
    {
        Task UploadAsync(int movieId, IBrowserFile file, string fileType);
        Task DeleteAsync(int fileId);
        Task<MovieFileViewModel> GetByIdAsync(int fileId);
    }


    public class MovieFileService : IMovieFileService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly IUserEmployeeService _userService;
        private readonly ApiClient _apiClient;
        private readonly IJSRuntime _js;

        public MovieFileService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage, IUserEmployeeService userService, IJSRuntime js)
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;
            _userService = userService;
            _js = js;
        }
        public async Task<MovieFileViewModel> GetByIdAsync(int id)
        {
            //return await _http.GetFromJsonAsync<MovieFileViewModel>($"api/movies/{id}");

            string url = $"Movie/get/{id}";
            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<MovieFileViewModel>();
        }

        public async Task UploadAsync(int movieId, IBrowserFile file, string fileType)
        {
            var content = new MultipartFormDataContent();

            // FIX: Nếu ContentType rỗng → dùng text/plain
            var mediaType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "text/plain"
                : file.ContentType;

            var stream = file.OpenReadStream(1024 * 1024 * 500); // 500MB

            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            content.Add(streamContent, "file", file.Name);
            content.Add(new StringContent(fileType), "fileType");

            var response = await _apiClient.PostFormAsync($"MovieFile/upload/{movieId}", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int fileId)
        {
            await _httpClient.DeleteAsync($"api/moviefiles/{fileId}");
        }
    }
}