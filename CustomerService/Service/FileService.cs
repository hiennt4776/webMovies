namespace CustomerService.Service
{
    public class FileService
    {
        private readonly string _filesPath;

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
