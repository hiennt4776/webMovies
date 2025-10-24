
using helperMovies.DTO;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace helperMovies.ViewModel
{
    public class ContractCreateViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Partner name is required")]
        [JsonPropertyName("partnerId")]
        public int PartnerId { get; set; }

        [JsonPropertyName("contractNumber")]
        public string ContractNumber { get; set; }
        [JsonPropertyName("contractDate")]
        public DateOnly? ContractDate { get; set; }
        [JsonPropertyName("totalValue")]
        public decimal? TotalValue { get; set; }
        [JsonPropertyName("paymentTerms")]
        public string PaymentTerms { get; set; }
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        [JsonPropertyName("movieInContracts")]
        public List<MovieInContractViewModel> MovieInContract { get; set; } = new();
        public IBrowserFile? ContractFile { get; set; }
    }

    public class ContractEditViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Partner name is required")]
        [JsonPropertyName("partnerId")]
        public int PartnerId { get; set; }

        [JsonPropertyName("contractNumber")]
        public string ContractNumber { get; set; }
        [JsonPropertyName("contractDate")]
        public DateOnly? ContractDate { get; set; }
        [JsonPropertyName("totalValue")]
        public decimal? TotalValue { get; set; }
        [JsonPropertyName("paymentTerms")]
        public string PaymentTerms { get; set; }
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        [JsonPropertyName("movieInContracts")]
        public List<MovieInContractViewModel> MovieInContract { get; set; } = new();

        public IBrowserFile? ContractFile { get; set; }

        // 🔹 File hiện tại (để hiển thị link tải)
        public string? ExistingFileName { get; set; }
        public string? ExistingFileUrl { get; set; }


    }

    public class MovieInContractViewModel
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        [JsonPropertyName("notes")]
        public string Notes { get; set; }

    }
    public class ContractViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; }

        [JsonPropertyName("partner")]
        public PartnerViewModel Partner { get; set; }
        [JsonPropertyName("contractNumber")]
        public string ContractNumber { get; set; }
        [JsonPropertyName("contractDate")]
        public DateOnly? ContractDate { get; set; }
        [JsonPropertyName("totalValue")]
        public decimal? TotalValue { get; set; }
        [JsonPropertyName("paymentTerms")]
        public string PaymentTerms { get; set; }
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        [JsonPropertyName("movieInContracts")]
        public List<MovieInContractViewModel> MovieInContract { get; set; } = new();

    }

    public class ContractViewModelDetail
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; }

        [JsonPropertyName("partner")]
        public PartnerViewModel Partner { get; set; }
        [JsonPropertyName("contractNumber")]
        public string ContractNumber { get; set; }
        [JsonPropertyName("contractDate")]
        public DateOnly? ContractDate { get; set; }
        [JsonPropertyName("totalValue")]
        public decimal? TotalValue { get; set; }
        [JsonPropertyName("paymentTerms")]
        public string PaymentTerms { get; set; }
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        [JsonPropertyName("movieInContracts")]
        public List<MovieInContractViewModel> MovieInContract { get; set; } = new();

        [JsonPropertyName("contractFilePath")]
        public string? ContractFilePath { get; set; }

        [JsonPropertyName("contractFileName")]
        public string? ContractFileName { get; set; }
    }

   

}
