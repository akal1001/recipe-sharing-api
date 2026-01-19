using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
namespace Modles
{
    public class RecipeModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }  

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("ingredients")]
        public List<IngredientModel> Ingredients { get; set; }

        [JsonPropertyName("cooking_time")]
        public string cooking_time { get; set; }

        [JsonPropertyName("servings")]
        public int Servings { get; set; }

        [JsonPropertyName("servingTime")]
        public string servingTime { get; set; }

        [JsonPropertyName("difficulty")]
        public string Difficulty { get; set; }

        [JsonPropertyName("cuisine")]
        public string Cuisine { get; set; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }

        [JsonPropertyName("nutrition")]
        public Dictionary<string, string> Nutrition { get; set; }

        [JsonPropertyName("preparation_steps")]
        public Dictionary<string, string> preparation_steps { get; set; }

        //newly add to test
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("UserId")]
        public string UserId { get; set; }

        [JsonPropertyName("NutritionalCategories")]
        public string[] NutritionalCategories { get; set; } = Array.Empty<string>();

        [JsonPropertyName("Vitamins")]
        public Dictionary<string, string> Vitamins { get; set; } 

       



    }
}
