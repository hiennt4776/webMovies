using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace helperMovies.DTO
{
    public class ContractCreateDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
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
        public List<MovieInContractDTO> MovieInContract { get; set; } = new();
        public IFormFile? ContractFile { get; set; }



    }
    public class MovieInContractDTO
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        [JsonPropertyName("notes")]
        public string Notes { get; set; }

    }
    public class ContractViewDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("partnerId")]
        public int? PartnerId { get; set; }

        [JsonPropertyName("partner")]
        public PartnerDTO Partner { get; set; }
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
        public List<MovieInContractDTO> MovieInContract { get; set; } = new();

        [JsonPropertyName("contractFilePath")]
        public string? ContractFilePath { get; set; }

        [JsonPropertyName("contractFileName")]
        public string? ContractFileName { get; set; }


    }

    public class ContractEditDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("partnerId")]
        public int PartnerId { get; set; }

        [JsonPropertyName("contractNumber")]
        public string ContractNumber { get; set; }

        [JsonPropertyName("contractDate")]
        public DateOnly? ContractDate { get; set; }

        [JsonPropertyName("paymentTerms")]
        public string PaymentTerms { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        [JsonPropertyName("movieInContracts")]
        public List<MovieInContractDTO> MovieInContract { get; set; } = new();

        public IFormFile? ContractFile { get; set; }
    }
}
