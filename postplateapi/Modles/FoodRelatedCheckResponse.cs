using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Modles
{

    public class FoodRelatedCheckResponse
    {
        [JsonPropertyName("isFoodRelated")]
        public bool IsFoodRelated { get; set; }

        [JsonPropertyName("recognizedFoodItems")]
        public List<string>? RecognizedFoodItems { get; set; }

        [JsonPropertyName("coreRequest")]
        public string CoreRequest { get; set; } // E.g. "a tasty dish", "simple breakfast", etc.

    }

}
