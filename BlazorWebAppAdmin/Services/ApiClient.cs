using Blazored.LocalStorage;
using helperMovies.DTO;
using helperMovies.ViewModel;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;



namespace BlazorWebAppAdmin.Services
{


    
    public interface IApiClient
    {
        public Task AddJwtHeaderAsync();
        public  Task<HttpResponseMessage> GetAsync(string relativeUrl);
        public  Task<HttpResponseMessage> PostJsonAsync<T>(string url, T data);

        public  Task<HttpResponseMessage> PostFormAsync(string url, HttpContent content);
        public  Task<HttpResponseMessage> PutJsonAsync<T>(string relativeUrl, T data);

        public Task<HttpResponseMessage> DeleteAsync(string relativeUrl);
    }
    public class ApiClient: IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public ApiClient(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;        // HttpClient đã có BaseAddress
            _localStorage = localStorage;
        }

        public async Task AddJwtHeaderAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string relativeUrl)
        {
            await AddJwtHeaderAsync();

            Console.WriteLine($"BaseAddress: {_httpClient.BaseAddress}");
            Console.WriteLine($"Request URL: {_httpClient.BaseAddress}{relativeUrl}");
            Console.WriteLine($"Authorization: {_httpClient.DefaultRequestHeaders.Authorization}");



            return await _httpClient.GetAsync(relativeUrl); // dùng BaseAddress + relativeUrl
        }

        public async Task<HttpResponseMessage> PostJsonAsync<T>(string url, T data)
        {
            await AddJwtHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();
            return response;
        }

        public async Task<HttpResponseMessage> PostFormAsync(string url, HttpContent content)
        {
            await AddJwtHeaderAsync();
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            return response;
        }
        public async Task<HttpResponseMessage> PutJsonAsync<T>(string relativeUrl, T data)
        {
            await AddJwtHeaderAsync();
            return await _httpClient.PutAsJsonAsync(relativeUrl, data);
        }

        public async Task<HttpResponseMessage> PutFormAsync(string url, HttpContent content)
        {
            await AddJwtHeaderAsync();
            var response = await _httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
            return response;
        }

        public async Task<HttpResponseMessage> DeleteAsync(string relativeUrl)
        {
            await AddJwtHeaderAsync();
            return await _httpClient.DeleteAsync(relativeUrl);
        }
    }

}
