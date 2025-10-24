
using Blazored.LocalStorage;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorWebAppAdmin.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }


        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("token");
        }


    }
}
