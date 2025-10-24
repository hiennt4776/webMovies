using CustomerService.Service;
using CustomerService.Utils;
using helperMovies.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace CustomerService.Utils
{
    public class FileSettings
    {
        public string FilesPath { get; set; } = string.Empty;
    }
}

