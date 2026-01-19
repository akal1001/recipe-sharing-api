using DataAccessLayer;
using Modles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer
{
    public class PostPlateManager
    {
        private readonly InsertHealthProfileDataAccess _InsertHealthProfileDataAccess;
        private readonly PostPlatesDataAccess _postPlatesDataAccess;
        private readonly IngredientPropertyDataAccess _IngredientPropertyDataAccess;    
        public PostPlateManager(PostPlatesDataAccess postPlatesDataAccess, IngredientPropertyDataAccess IngredientPropertyDataAccess, InsertHealthProfileDataAccess insertHealthProfileDataAccess)
        {
            _postPlatesDataAccess = postPlatesDataAccess;
            _IngredientPropertyDataAccess = IngredientPropertyDataAccess;
            _InsertHealthProfileDataAccess= insertHealthProfileDataAccess;
        }
       
        public async Task<List<Ingredient>> GetAllIngredientsAsync()
        {
            return await _postPlatesDataAccess.GetAllIngredientsDataOrderedByDateAsync();
        }

        public async Task<List<IngredientDetail>> GetAllIngredientsDetailsAsync() 
        {
            var ingDetailList = new List<IngredientDetail>();

            var ings = await _postPlatesDataAccess.GetAllIngredientsDataOrderedByDateAsync();
            
            foreach ( var ing in ings )
            {
                ingDetailList.Add(new IngredientDetail
                {
                    Id = ing.Id,
                    Name = ing.Name,
                    Date = ing.Date,
                    ImageUrl = ing.ImageUrl,
                    IngredientType = ing.IngredientType,
                    IngredientProperty = await _IngredientPropertyDataAccess.GetByIngredientPropertyIdAsync(ing.Id),
                    HealthProfile = await _InsertHealthProfileDataAccess.GetHealthProfileByIngredientIdAsync(ing.Id)
                });
                
                
            }
            return ingDetailList;
        }



        public async Task<bool> UpdateIngredien(Ingredient ingredient)
        {
           return await _postPlatesDataAccess.UpdateIngredientByIdAsync(ingredient);
        }

        public async Task<bool> InsertRecipeAsync(RecipeModel recipeModel)
        {

            //var list = new List<string>();

            //for(int i = 0; i< recipeModel.Tags.Length; i++)
            //{
            //    list.Add(recipeModel.Tags[i]);
            //}

            //var tag2 = GetNutritionalCategories(recipeModel.Nutrition);
            //foreach(var t in tag2)
            //{
            //    list.Add(t);
            //}

            //recipeModel.Tags = list.ToArray();


            var rowAffect = await _postPlatesDataAccess.AddRecipeAsync(recipeModel);
            if(rowAffect > 0)
            {
                return true;
            }
            else { return false; }
        }

        public async Task<List<RecipeModel>> GetAllAsync()
        {
            var recipes = new List<RecipeModel>();

            var res = await _postPlatesDataAccess.GetAllRecipes();
            foreach (var recipe in res)
            {
                var count = recipe.Servings;
             
                var val = recipe.Nutrition;
              
                
                recipes.Add(recipe);
            }

            return recipes;
        }

        private List<string> GetNutritionalCategories1(Dictionary<string, string> nutritionDict)
        {
            var categories = new List<string>();

            // Helper to parse values safely, with case-insensitive key lookup
            float TryParse(string key)
            {
                var match = nutritionDict
                    .FirstOrDefault(kvp => string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase));

                if (match.Key == null)
                {
                    Console.WriteLine($"Key '{key}' not found.");
                    return 0f;
                }

                Console.WriteLine($"Found key '{match.Key}' with value '{match.Value}'.");

                //string valueStr = match.Value.Replace("g", "").Replace("mg", "").Trim();
                string valueStr = match.Value.Replace("mg", "").Replace("g", "").Trim();

                if (float.TryParse(valueStr, out var result))
                    return result;

                Console.WriteLine($"Failed to parse '{valueStr}' as float.");
                return 0f;
            }



            float protein = TryParse("Protein");
            float carbohydrates = TryParse("Carbohydrates");
            float fiber = TryParse("Fiber");
            float sugar = TryParse("Sugar");
            float fat = TryParse("Fat");
            float sodium = TryParse("Sodium");
            float magnesium = TryParse("Magnesium"); // fixed casing here

           

            // Protein
            if (protein >= 20)
                categories.Add("High-Protein");

            // Carbohydrates
            if (carbohydrates >= 40)
                categories.Add("High-Carb");
            else if (carbohydrates <= 25)
                categories.Add("Low-Carb");

            // Fiber
            if (fiber >= 5)
                categories.Add("High-Fiber");

            // Sugar
            if (sugar >= 10)
                categories.Add("High-Sugar");
            else if (sugar <= 5)
                categories.Add("Low-Sugar");

            // Fat
            if (fat >= 17)
                categories.Add("High-Fat");
            else if (fat <= 3)
                categories.Add("Low-Fat");

            // Sodium
            if (sodium >= 600)
                categories.Add("High-Sodium");
            else if (sodium <= 140)
                categories.Add("Low-Sodium");

            // Magnesium
            if (magnesium >= 100)
                categories.Add("High-Magnesium");

            // You can now assign or return this list
            // recipe.NutritionalCategories = categories;


            return categories;
        }


        //for ingridents propertis (IngredientProperty)
        public async Task<bool> InsertIngredientPropertyAsync(IngredientProperty ingredientProperty)      
        {
            var rowAffect = await _IngredientPropertyDataAccess.InsertIngredientPropertyAsync(ingredientProperty);
            if (rowAffect > 0)
            {
                return true;
            }
            else { return false; }
        }
        public async Task<IngredientProperty> GetIngredientPropertyAsync(string ingredientPropertyId)
        {
            var _ingProperity = new IngredientProperty();

            var ingProperity = await _IngredientPropertyDataAccess.GetByIngredientPropertyIdAsync(ingredientPropertyId);
           
            if(ingProperity.Count() > 0)
            {
                _ingProperity = ingProperity.FirstOrDefault(h => h.IngredientPropertiesId == ingredientPropertyId);
            }

            return _ingProperity;
        }
        public async Task<IngredientProperty> GetIngredientPropertyAsync(string ingredientPropertyId, string propertyName)
        {
            var _ingProperity = new IngredientProperty();

            var ingProperity = await _IngredientPropertyDataAccess.GetByIngredientPropertyIdAsync(ingredientPropertyId, propertyName);

            if (ingProperity.Count() > 0)
            {
                _ingProperity = ingProperity.FirstOrDefault(h => h.IngredientPropertiesId == ingredientPropertyId);
            }

            return _ingProperity;
        }


        //for (HealthProfile)
        public async Task<bool> InsertHealthProfile(HealthProfile healthProfile)
        {
            var result = await _InsertHealthProfileDataAccess.InsertHealthProfileAsync(healthProfile);
            if(result > 0)
            {
                return true;
                
            }
            return false;
        }

        public async Task<HealthProfile> GetHealthProfileAsync(string ingredientId)
        {
            var _healthProfile = await _InsertHealthProfileDataAccess.GetHealthProfileByIngredientIdAsync(ingredientId);
            return _healthProfile;
        }





    }
    
}
