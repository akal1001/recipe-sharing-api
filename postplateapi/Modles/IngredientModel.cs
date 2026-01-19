using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Modles
{
    public class IngredientModel
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("quantity")]
        public string? Quantity { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }
        
        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

      
    }
}
