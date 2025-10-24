using AdminService.Service;
using AdminService.Utils;
using helperMovies.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace AdminService.Utils
{
    public class FileSettings
    {
        public string FilesPath { get; set; } = string.Empty;
    }

    public interface IFileService
    {
        public Task<FileDTO> SaveFileAsync(IFormFile file, string subDirectory);

            //Task<bool> DeleteFileAsync(int fileId, bool deletePhysical = false);
      
    }
    public class FileService : IFileService
    {
        private readonly FileSettings _settings;

        public FileService(IOptions<FileSettings> settings)
        {
            _settings = settings.Value;
        }
        public async Task<FileDTO> SaveFileAsync(IFormFile file, string subDirectory)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            // 🔹 Đường dẫn gốc lấy từ appsettings.json
            var basePath = _settings.FilesPath;

            // 🔹 Tạo thư mục con
            var uploadPath = Path.Combine(basePath, subDirectory);
            Directory.CreateDirectory(uploadPath);

            // 🔹 Giữ lại phần đuôi file (extension)
            var extension = Path.GetExtension(file.FileName);

            // 🔹 Tạo tên file duy nhất có phần đuôi
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadPath, uniqueFileName);

            // 🔹 Lưu file
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            Console.WriteLine($"[FileService] File saved: {fullPath}");

            // 🔹 Trả lại thông tin file
            return new FileDTO
            {
                OriginalFileName = Path.GetFileName(file.FileName),
                UniqueFileName = uniqueFileName,
                FullPath = Path.Combine(subDirectory, uniqueFileName)
            };
        }

    }
}

