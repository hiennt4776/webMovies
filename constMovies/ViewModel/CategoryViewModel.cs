using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace helperMovies.ViewModel
{
    public class CategoryViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Color is required")]
        [JsonPropertyName("color")]
        public string Color { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [JsonPropertyName("description")]
        public string Description { get; set; }
        
        
      
    }
}
