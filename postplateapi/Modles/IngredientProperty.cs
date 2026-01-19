using System;
using System.Collections.Generic;
using System.Text;

namespace Modles
{
    public class IngredientProperty
    {
        public int Id { get; set; }
        public string IngredientPropertiesId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Unit { get; set; }
    }
    public class HealthProfile
    {
        public string IngredientId { get; set; }    
        public string AntiInflammatoryLevel { get; set; } = string.Empty;
        public string AntioxidantLevel { get; set; } = string.Empty;
        public Antioxidants Antioxidants { get; set; } 
    }

    public class Antioxidants
    {
        public double? VitaminC { get; set; }
        public double? BetaCarotene { get; set; }
        public string? Polyphenols { get; set; }
    }
    public class IngredientAnalysisResponse
    {
        public ServingSize ServingSize { get; set; }
        public List<IngredientProperty> Nutrients { get; set; } 
        public HealthProfile HealthProfile { get; set; } 
    }
    public class ServingSize
    {
        public int Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }


   

}
