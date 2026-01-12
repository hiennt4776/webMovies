using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace helperMovies.DTO
{
    public class CreateInvoiceRequestDTO
    {
        public int MoviePricingId { get; set; }
    }

    public class InvoiceQuery
    {
        public string? Keyword { get; set; }
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = true;

        public bool? IsDeleted { get; set; }   // 🔥 NEW

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }


    public class InvoiceDTO
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public int? UserCustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string PaymentStatus { get; set; } 
        public string PaymentMethod { get; set; }

        public string Notes { get; set; }
        public List<InvoiceDetailDTO> InvoiceDetail { get; set; }

        public bool? IsDeleted { get; set; }
        public string Reason { get; set; }
    }
    public class InvoiceDetailDTO
    {
        public int Id { get; set; }
        public int? MovieId { get; set; }
        public string? MovieTitle { get; set; }

        public int? PackageId { get; set; }
        public string? PackageName { get; set; }
        public string PricingType { get; set; }
        public decimal? UnitPrice { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }



    public class CancelInvoiceRequestDTO
    {
        public string Reason { get; set; } = "";
    }

}
