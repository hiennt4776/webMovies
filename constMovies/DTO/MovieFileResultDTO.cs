using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helperMovies.DTO
{
    public class MovieFileResultDTO
    {
        public Stream Stream { get; set; } = default!;
        public string ContentType { get; set; } = default!;
    }
}
