using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helperMovies.ViewModel
{
    public class MovieAccessViewModel
    {
        public int MovieId { get; set; }
        public bool IsFree { get; set; }

        public bool CanWatch { get; set; }

        public bool ShowBuy { get; set; }
        public bool ShowRent { get; set; }

        public decimal? BuyPrice { get; set; }
        public decimal? RentPrice { get; set; }
        public int? RentalDurationDays { get; set; }
    }
}
