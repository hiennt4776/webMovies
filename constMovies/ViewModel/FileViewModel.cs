using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace helperMovies.DTO
{
    public class FileViewModel
    {




        [JsonPropertyName("originalFileName")]
        public string OriginalFileName { get; set; }


        [JsonPropertyName("fullPath")]
        public string FullPath { get; set; }

        [JsonPropertyName("uniqueFileName")]
        public string UniqueFileName { get; set; }
    }
}
