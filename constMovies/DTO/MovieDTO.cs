using Microsoft.AspNetCore.Http;

namespace helperMovies.DTO
{
    public class MovieDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string Director { get; set; }
        public string Cast { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string LanguageList { get; set; }
        public string Country { get; set; }
        public string AgeLimit { get; set; }
        public decimal? Rating { get; set; }
        public decimal? Budget { get; set; }
        public decimal? BoxOffice { get; set; }
        public string Status { get; set; }
        public string FilePath { get; set; }


        public List<MovieFileDTO> Files { get; set; } = new();

        public bool HasTrailer { get; set; }
        public bool HasMovie { get; set; }
        public bool HasSubtitle { get; set; }

    }



    public class MovieFileDTO
    {
        public int Id { get; set; }

        public int? MovieId { get; set; }

        public string FileType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public bool IsDeleted { get; set; }
    }


    public class UploadMovieFileDTO
    {
        public int MovieId { get; set; }
        public string FileType { get; set; }
        public IFormFile File { get; set; }
    }

    public class MovieSearchDTO
    {
        public string? Keyword { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}