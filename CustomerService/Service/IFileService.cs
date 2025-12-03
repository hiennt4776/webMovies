using System;
using Confluent.Kafka;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace CustomerService.Service
{
    public interface IFileService
    {
        byte[] GetFileBytes(string relativePath);
    }
    public class FileService : IFileService
    {
        private readonly string _filesPath;
        private readonly IFileService _fileService;
        public FileService(IConfiguration configuration)
        {
            _filesPath = configuration["FileSettings:FilesPath"];
        }

        public string GetFullPath(string relativePath)
        {
            // Kết hợp đường dẫn cấu hình + đường dẫn file trong DB
            return Path.Combine(_filesPath, relativePath);
        }

        public byte[] GetFileBytes(string relativePath)
        {
            var fullPath = GetFullPath(relativePath);
            if (File.Exists(fullPath))
            {
                return File.ReadAllBytes(fullPath);
            }
            return null;
        }



    }
}
