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

    }
}