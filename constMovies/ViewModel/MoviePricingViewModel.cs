using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helperMovies.ViewModel
{
    public class MoviePricingViewModel
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public bool? IsPaid { get; set; }
        public string PricingType { get; set; }
        public decimal Price { get; set; }
        public int? RentalDurationDays { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
    }
    public class CreateUpdateMoviePricingViewModel
    {
        public int Id { get; set; }
        public int? MovieId { get; set; }
        public bool? IsPaid { get; set; } = true;
        public string PricingType { get; set; } = "Buy";
        public decimal? Price { get; set; }
        public int? RentalDurationDays { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; } = true;
    }
}
