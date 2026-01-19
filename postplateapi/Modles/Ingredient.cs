using System;
using System.Collections.Generic;
using System.Text;

namespace Modles
{
    public class Ingredient
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IngredientType { get; set; }
        public string ImageUrl { get; set; }
        public DateTime Date { get; set; }
    }

    public class IngredientDetail
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IngredientType { get; set; }
        public string ImageUrl { get; set; }
        public DateTime Date { get; set; }

        public HealthProfile HealthProfile { get; set; }
        public List<IngredientProperty> IngredientProperty { get; set; }
    }
}
