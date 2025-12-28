using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace helperMovies.DTO
{
    public class SubscriptionPackageDTO
    {
        public int Id { get; set; } // for update

        [Required(ErrorMessage = "Package name is required")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string Name { get; set; } = null!;

        [Range(1, 120, ErrorMessage = "Time must be >= 1 month and <= 120 months")]
        public int? DurationMonths { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a number >= 0")]
        public decimal? Price { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }   
        public bool? IsActive { get; set; }
    }

    public class PurchaseSubscriptionDTO
    {
        public int SubscriptionPackageId { get; set; }
    }

}
