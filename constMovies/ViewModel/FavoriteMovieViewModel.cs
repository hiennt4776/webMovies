using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helperMovies.ViewModel
{
    public class FavoriteMovieViewModel
    {
        public int? MovieId { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public DateTime? AddedDate { get; set; }
    }
}
