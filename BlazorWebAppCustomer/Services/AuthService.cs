using Blazored.LocalStorage;
using dbMovies.Models;
using helperMovies.ViewModel;
using Microsoft.JSInterop;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace BlazorWebAppCustomer.Services
{
    public class AuthService
    {
        private readonly ILocalStorageService _localStorage;

        public AuthService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<bool> IsAuthenticated()
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            if (string.IsNullOrEmpty(token)) return false;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            return jwt.ValidTo > DateTime.UtcNow;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("token");
        }
    }
}
