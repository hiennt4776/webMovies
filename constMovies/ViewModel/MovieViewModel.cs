using Microsoft.AspNetCore.Components.Forms;

namespace helperMovies.ViewModel
{
    public class MovieViewModel
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

        public List<MovieFileViewModel> Files { get; set; } = new();
        public MovieFileConfig FileConfig { get; set; } = new();

        public bool HasTrailer { get; set; }
        public bool HasMovie { get; set; }
        public bool HasSubtitle { get; set; }

    }

    public class MovieFileViewModel
    {
        public int Id { get; set; }

        public int MovieId { get; set; }
        public string FileType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }

    public class UploadMovieFileViewModel
    {
        public int MovieId { get; set; }
        public string FileType { get; set; } = string.Empty;
        public IBrowserFile? File { get; set; }
    }

    public class MovieSearchViewModel
    {
        public string? Keyword { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }


    public class MovieFileConfig
    {
        public string? PosterUrl { get; set; }
        public string? TrailerUrl { get; set; }
        public string? MovieUrl { get; set; }
        public string? SubtitleUrl { get; set; }
    }
}