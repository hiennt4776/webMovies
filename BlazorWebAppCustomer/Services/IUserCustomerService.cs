using Azure;
using Blazored.LocalStorage;
using BlazorWebAppCustomer.Data;
using BlazorWebAppCustomer.Services;
using helperMovies.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace BlazorWebAppCustomer.Services
{
    public interface IUserCustomerService
    {
        Task<(bool IsSuccess, string Message)> RegisterAsync(CustomerRegisterViewModel dto);
       Task<LoginResponseViewModel> LoginAsync(LoginRequestViewModel LoginRequestViewModel);
    }


    public class UserCustomerService : IUserCustomerService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiClient _apiClient;
        private readonly ApiSettings _settings;
        private readonly ILocalStorageService _localStorage;


        public UserCustomerService(HttpClient httpClient, ILocalStorageService localStorage, ApiClient apiClient, IOptions<ApiSettings> settings)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _apiClient = apiClient;
            _settings = settings.Value;
        }

        public async Task<(bool IsSuccess, string Message)> RegisterAsync(CustomerRegisterViewModel dto)
        {
         
               


            try
            {
                var url = $"{_settings.BaseUrl}UserCustomer/register";
                var response = await _httpClient.PostAsJsonAsync(url, dto);

                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

                if (response.IsSuccessStatusCode)
                {
                    return (true, result?.Message ?? "Registration successful!");
                }
                else
                {
                    return (false, result?.Message ?? "Registration failed!");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }



        }

        public async Task<LoginResponseViewModel> LoginAsync(LoginRequestViewModel loginRequestViewModel)
        {
           
                var url = $"{_settings.BaseUrl}UserCustomer/login";

                //string url = "Category/create";
                //var response = await _apiClient.PostJsonAsync(url, dto);


                //return await response.Content.ReadFromJsonAsync<CategoryViewModel>();

                var response = await _httpClient.PostAsJsonAsync(url, loginRequestViewModel);


                if (!response.IsSuccessStatusCode)
                    return null;

            return await response.Content.ReadFromJsonAsync<LoginResponseViewModel>();

              
        }

   


    }




}
