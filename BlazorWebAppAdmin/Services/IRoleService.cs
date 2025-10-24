using Blazored.LocalStorage;
using dbMovies.Models;
using helperMovies.ViewModel;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace BlazorWebAppAdmin.Services
{

    public interface IRoleService
    {
        Task<List<RoleViewModel>> GetAllRolesAsync();
    }

    public class RoleService : IRoleService
    {

        private readonly ApiClient _apiClient;
        public RoleService(ApiClient apiClient) { 

            _apiClient = apiClient;

        }

        public async Task<List<RoleViewModel>> GetAllRolesAsync()
        {
            string url = $"Role/getAllRoles";
            var response = await _apiClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<RoleViewModel>();

            return await response.Content.ReadFromJsonAsync<List<RoleViewModel>>() ?? new List<RoleViewModel>();
        }



    }
}