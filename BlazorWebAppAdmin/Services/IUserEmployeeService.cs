using Azure;
using Blazored.LocalStorage;
using helperMovies.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace BlazorWebAppAdmin.Services
{
    public interface IUserEmployeeService
    {
        public Task<LoginEmployeeResponseViewModel> LoginAsync(LoginEmployeeRequestViewModel model);
        public Task AddAuthHeaderAsync();
        public Task<EmployeeViewModel> GetCurrentUserNameAsync();
        public Task<PagedResult<UserEmployeeViewModel>> GetPagedAsync(int page, int pageSize, string keyword, int? roleId, bool? isLocked);
        public Task<UserEmployeeCreateViewModel> AddUserEmployeeAsync([FromBody] UserEmployeeCreateViewModel userEmployeeCreateViewModel);
        public Task<UserEmployeeViewModel> UpdateUserEmployeeAsync([FromBody] UserEmployeeEditViewModel userEmployeeEditViewModel);
        public Task<ChangePasswordViewModel> ChangePasswordAsync(ChangePasswordViewModel changePasswordViewModel);

        public Task<UserEmployeeEditViewModel> GetEditByIdAsync(int id);
        public Task<UserEmployeeViewModel> GetByIdAsync(int id);

        public Task<UserEmployeeResetPasswordViewModel?> GetResetPasswordInfoAsync(int id);
        public Task<UserEmployeeResetPasswordViewModel> UserEmployeeResetPasswordAsync(UserEmployeeResetPasswordViewModel userEmployeeResetPasswordViewModel);


    }


    public class UserEmployeeService : IUserEmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiClient _apiClient;
        private readonly ILocalStorageService _localStorage;


        public UserEmployeeService(HttpClient httpClient, ILocalStorageService localStorage, ApiClient apiClient)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _apiClient = apiClient;
        }

        public async Task<LoginEmployeeResponseViewModel> LoginAsync(LoginEmployeeRequestViewModel model)
        {


            var response = await _httpClient.PostAsJsonAsync("UserEmployee/Login", model);


            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<LoginEmployeeResponseViewModel>();
            //if (response.IsSuccessStatusCode)
            //{
            //    var token = await response.Content.ReadAsStringAsync();
            //    var cleanToken = token.Replace("\"", "").Trim(); // loại bỏ dấu "
            //    await _localStorage.SetItemAsStringAsync("authToken", cleanToken);
            //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cleanToken);

            //    Console.WriteLine($"[DEBUG] Token saved: {cleanToken.Substring(0, 20)}...");
            //    return cleanToken;
            //}

            //return null;


        }

        public async Task<EmployeeViewModel> GetCurrentUserNameAsync()
        {


            var token = await _localStorage.GetItemAsStringAsync("authToken");
            //mình xóa bớt log nha nhìn hơi rối 
            if (token != null)
            {
                Console.WriteLine($"Authorization zzzzzzz:", token.ToString());
            }
            else { Console.WriteLine("token is null"); } 
            if (string.IsNullOrEmpty(token)) throw new UnauthorizedAccessException("Please Login");
            // Gắn token vào header Authorization
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Log chi tiết
            // bạn chạy backend chưa ta 
            // roi 1 man hinh front -1 man hinh back 
            Console.WriteLine("==== [CLIENT DEBUG HTTP REQUEST] ====");
            Console.WriteLine($"BaseAddress: {_httpClient.BaseAddress}");
            Console.WriteLine($"Request URL: {_httpClient.BaseAddress}UserEmployee/profile");
            Console.WriteLine($"Authorization: {_httpClient.DefaultRequestHeaders.Authorization}");

            Console.WriteLine("====================================");

            var response = await _httpClient.GetAsync("Employee/profile");




            Console.WriteLine("==== [CLIENT DEBUG RESPONSE] ====");
            Console.WriteLine($"StatusCode: {response.StatusCode}");
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Body: {body}");
            Console.WriteLine("=================================");

            //
            if (response.IsSuccessStatusCode)
            {
                var pf = await response.Content.ReadFromJsonAsync<EmployeeViewModel>();
                if (pf is null) throw new Exception("Không đọc được dữ liệu người dùng.");
                return pf;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn hoặc token không hợp lệ.");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập tài nguyên này.");

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new Exception("Không tìm thấy hồ sơ người dùng.");



            response.EnsureSuccessStatusCode();
            var profile = await response.Content.ReadFromJsonAsync<EmployeeViewModel>();
            return profile;


        }
        public async Task AddAuthHeaderAsync()
        {
            var token = await _localStorage.GetItemAsStringAsync("authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        public async Task<UserEmployeeViewModel> GetByIdAsync(int id)
        {

            string url = $"UserEmployees/{id}";
            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<UserEmployeeViewModel>();

        
        }

        public async Task<UserEmployeeEditViewModel> GetEditByIdAsync(int id)
        {


            string url = $"UserEmployee/getEdit/{id}";
            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<UserEmployeeEditViewModel>();


        }

        public async Task<PagedResult<UserEmployeeViewModel>> GetPagedAsync(int page, int pageSize, string keyword, int? roleId, bool? isLocked)
        {
            string url = $"UserEmployee/paged?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrEmpty(keyword))
                url += $"&keyword={keyword}";
            if (roleId.HasValue)
                url += $"&roleId={roleId}";
            if (isLocked.HasValue)
                url += $"&isLocked={isLocked.Value}";

            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) throw new Exception($"Error: {response.StatusCode}");
            // Deserialize JSON về object
            var result = await response.Content.ReadFromJsonAsync<PagedResult<UserEmployeeViewModel>>();
            return result;

        }

        public async Task<UserEmployeeCreateViewModel> AddUserEmployeeAsync([FromBody] UserEmployeeCreateViewModel userEmployeeCreateViewModel)
        {
     
            var response = await _apiClient.PostJsonAsync("UserEmployee/create", userEmployeeCreateViewModel);
            return await response.Content.ReadFromJsonAsync<UserEmployeeCreateViewModel>();
        }

        public async Task<UserEmployeeViewModel> UpdateUserEmployeeAsync(UserEmployeeEditViewModel userEmployeeEditViewModel)
        {

            var response = await _apiClient.PutJsonAsync($"UserEmployee/update/{userEmployeeEditViewModel.Id}", userEmployeeEditViewModel);
            return await response.Content.ReadFromJsonAsync<UserEmployeeViewModel>();
          
        }

        public async Task<UserEmployeeResetPasswordViewModel> UserEmployeeResetPasswordAsync(UserEmployeeResetPasswordViewModel userEmployeeResetPasswordViewModel)
        {

            var response = await _apiClient.PutJsonAsync($"UserEmployee/resetpassword/{userEmployeeResetPasswordViewModel.Id}", userEmployeeResetPasswordViewModel);
            return await response.Content.ReadFromJsonAsync<UserEmployeeResetPasswordViewModel>();

        }

        public async Task<ChangePasswordViewModel> ChangePasswordAsync(ChangePasswordViewModel changePasswordViewModel)
        {

            var response = await _apiClient.PutJsonAsync($"UserEmployee/changepassword/{changePasswordViewModel.UserId}", changePasswordViewModel);
            return await response.Content.ReadFromJsonAsync<ChangePasswordViewModel>();

        }
        public async Task<UserEmployeeResetPasswordViewModel> GetResetPasswordInfoAsync(int id)
        {

            string url = $"UserEmployee/getEdit/{id}";
            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<UserEmployeeResetPasswordViewModel>();

          
        }

        //public async Task<UserEmployeeResetPasswordViewModel> ResetPasswordAsync(UserEmployeeResetPasswordViewModel model)
        //{
        //    var response = await _apiClient.PutJsonAsync("UserEmployees/resetpassword", model);
        //    return await response.Content.ReadFromJsonAsync<UserEmployeeResetPasswordViewModel>();
        //}
    }
}
