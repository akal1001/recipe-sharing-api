using BusinessLogicLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Modles;
using Newtonsoft.Json;
using postplateapi.OpenAiManagers;
using postplateapi.ResponseHelper;
using postplateapi.S3Managers;
using postplateapi.UnsplashManager;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace postplateapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodController : ControllerBase
    {
        //private readonly ILogger<FoodController> _logger;
       
        private readonly BusinessLogicLayer.PostPlateManager _postPlateManager;
        private readonly S3Manager _s3Manager;
        private readonly OpenAiManager _openaiManager;
        private readonly UnsplashService _unsplashService;
        private readonly ActionResultResponseHelper _responseHelper;    
        public FoodController(ILogger<FoodController> logger, BusinessLogicLayer.PostPlateManager postPlateManager, OpenAiManager openAiManager, UnsplashService unsplashService) 
        {
           // _logger = logger;
            _s3Manager = new S3Manager(); // If you use DI, inject it instead
            _responseHelper= new ActionResultResponseHelper();
            _postPlateManager= postPlateManager;
            _openaiManager= openAiManager;
            _unsplashService= unsplashService;
        }

        //retun full recipe based on the posted/send ingredients
        [HttpPost("postIngredients")]
        public async Task<IActionResult> PostFoodAsync([FromBody] PostRequest postRequest)
        {
            

            if (postRequest?.ingredients == null || postRequest.ingredients.Length == 0)
            {
                return _responseHelper.CreateResponse(false, 400, "Please provide at least one ingredient.", null);
            }

            try
            {
                var ingredientList = string.Join(", ", postRequest.ingredients);

                var prompt = $"Generate a complete food recipe using ONLY the following **main ingredients**: {ingredientList}. " +
           "You may include common supporting ingredients (like oil, salt, pepper, water, etc.) if necessary, " +
           "but do not add any additional main ingredients. " +
           "Respond strictly in JSON format using this structure: " +
           "{\\\"title\\\": \\\"string\\\", " +
                            "\\\"ingredients\\\": [{\\\"name\\\": \\\"string\\\", \\\"quantity\\\": \\\"string\\\", \\\"unit\\\": \\\"string\\\", \\\"notes\\\": \\\"string\\\"}], " +
                            "\\\"cooking_time\\\": \\\"string\\\", " +
                            "\\\"servings\\\": number, " +
                            "\\\"servingTime\\\": \\\"string\\\", " +
                            "\\\"difficulty\\\": \\\"string\\\", " +
                            "\\\"cuisine\\\": \\\"string\\\", " +
                            "\\\"tags\\\": [\\\"string\\\"], " +
                            "\\\"nutrition\\\": {\\\"Calories\\\": \\\"string\\\", \\\"Protein\\\": \\\"string\\\", " +
                            "\\\"Carbohydrates\\\": \\\"string\\\", \\\"Fat\\\": \\\"string\\\", \\\"Cholesterol\\\": \\\"string\\\", " +
                            "\\\"Saturated Fat\\\": \\\"string\\\", \\\"Unsaturated Fat\\\": \\\"string\\\", \\\"Iron\\\": \\\"string\\\", " +
                            "\\\"magnesium\\\": \\\"string\\\", \\\"Calcium\\\": \\\"string\\\", \\\"Fiber\\\": \\\"string\\\", " +
                            "\\\"Sugar\\\": \\\"string\\\", \\\"Sodium\\\": \\\"string\\\"}, " +
                            "\\\"Vitamins\\\": {\\\"A\\\": \\\"string\\\", \\\"B1\\\": \\\"string\\\", \\\"B2\\\": \\\"string\\\", " +
                            "\\\"B3\\\": \\\"string\\\", \\\"B5\\\": \\\"string\\\", \\\"B6\\\": \\\"string\\\", \\\"B9\\\": \\\"string\\\", " +
                            "\\\"B12\\\": \\\"string\\\", \\\"C\\\": \\\"string\\\", \\\"D\\\": \\\"string\\\", \\\"E\\\": \\\"string\\\", " +
                            "\\\"K\\\": \\\"string\\\"}, " +
                            "\\\"nutritionalCategories\\\": [\\\"string\\\"], " +
                            "\\\"preparation_steps\\\": {\\\"step_1\\\": \\\"string\\\", \\\"step_2\\\": \\\"string\\\", ...}}. " +
                            "Each preparation step should be labeled as step_1, step_2, etc., and be a plain instruction and must be in order from beginning to end. " +
                            "The \\\"servingTime\\\" must match one of these exactly: [\\\"Breakfast\\\", \\\"Lunch\\\", \\\"Dinner\\\", \\\"Lunch or Dinner\\\", \\\"Snack\\\", \\\"Anytime\\\", \\\"N/A\\\"]. " +
                            "The \\\"nutritionalCategories\\\" must be an array containing all applicable categories from the following list, based on the nutrition data: " +
                            "[\\\"High-Protein\\\", \\\"High-Fat\\\", \\\"High-Sodium\\\", \\\"Low-Sugar\\\", \\\"Low-Carb\\\", \\\"High-Carb\\\", \\\"High-Fiber\\\", \\\"Low-Fat\\\", \\\"Low-Sodium\\\", \\\"High-Sugar\\\", \\\"High-Magnesium\\\"]. Include multiple values if applicable. " +
                            "Respond with ONLY the JSON. Do not include explanations or extra text.";


               // ["High-Protein", "High-Fat", "High-Sodium", "Low-Sugar", "Low-Carb"]

                var response = await _openaiManager.GetChatCompletionAsync(prompt);

                if (response?.choices == null || response.choices.Length == 0)
                {
                    return _responseHelper.CreateResponse(false, 404, "No recipe generated.", null);
                }

                var recipe = JsonConvert.DeserializeObject<RecipeModel>(response.choices[0].message.content);
                //if (recipe != null)
                //{
                //    recipe.Tags = await ModifyTags(recipe.Tags, recipe.NutritionalCategories);
                //}

                //return Ok(recipe);
                return _responseHelper.CreateResponse(true, 200, "success", recipe);
            }
            catch (Exception ex)
            {
                // Optionally log the exception details here
                return _responseHelper.CreateResponse(false, 500, ex.Message, null);
            }
        }

        string[] testInputs = new string[]
        {
            // ✅ Valid food-related inputs
            "Spaghetti Bolognese",
            "Grilled chicken with vegetables",
            "Vegan tofu stir fry",
            "Cheeseburger",
            "Fish curry",
            "Avocado toast",

            // ❌ Invalid or nonsense inputs
            "Plastic soup",
            "Wooden sandwich",
            "Metal pasta",
            "Low carb plastic meal",
            "Imaginary food delight",
            "Energy from rocks",

            // ⚠️ Edge cases (depending on strictness)
            "Low carb meal", // vague
            "Healthy dinner", // vague, no ingredients
            "Fruit", // valid ingredient but could be too broad
            "Salt and pepper", // supporting ingredients
            "Carbonated beverage" // borderline
        };

        //retun full recipe based on the posted/send ingredients
        //[HttpPost("postIngredients/byName")]
        //public async Task<IActionResult> PostBynameFoodAsync([FromBody] DishNameRequest request)
        //{

        //    string firstPrompt =
        //                    $"Evaluate whether the following user input is a valid request for a food dish or recipe.\n" +
        //                    $"Ignore general phrases like 'give me', 'based on', 'a tasty dish'. Focus on extracting:\n\n" +
        //                    $"1. Up to 10 **real edible food items** or ingredients mentioned\n" +
        //                    $"2. A **core summary** of what the user is asking for (like 'a quick lunch', 'healthy dinner')\n\n" +
        //                    $"Respond only in the following JSON format:\n\n" +
        //                    $"{{\n" +
        //                    $"  \"isFoodRelated\": true or false,\n" +
        //                    $"  \"recognizedFoodItems\": [\"food1\", \"food2\", ...],\n" +
        //                    $"  \"coreRequest\": \"summary of user intent\"\n" +
        //                    $"}}\n\n" +
        //                    $"Input: \"{request.DishName}\"";




        //    var firetResponse = await _openaiManager.GetChatCompletionAsync(firstPrompt);

        //    var result = JsonConvert.DeserializeObject<FoodRelatedCheckResponse>(firetResponse.choices[0].message.content);


        //    if (result?.IsFoodRelated == false)
        //    {
        //        return _responseHelper.CreateResponse(false, 400, "Please enter the name of a real dish or food item.", request.DishName);
        //    }
        //    if(result?.RecognizedFoodItems.Count > 5)
        //    {
        //        return _responseHelper.CreateResponse(false, 400, "Too many items .", result.RecognizedFoodItems);

        //    }





        //    if (string.IsNullOrEmpty(request.DishName))
        //    {
        //        return _responseHelper.CreateResponse(false, 400, "Please provide dish name.", null);
        //    }

        //    try
        //    {
        //        string finalRequest = "";
        //        if (!String.IsNullOrEmpty(result.CoreRequest))
        //        {
        //            finalRequest = result.CoreRequest+" ";
        //        }

        //         finalRequest += string.Join(", ", result.RecognizedFoodItems);


        //        var prompt = $"Generate a complete food recipe for the dish \"{finalRequest}\". " +
        //            "Base the recipe on how this dish is commonly prepared. " +
        //            "Respond strictly in JSON format using the following structure: " +
        //            "{\\\"title\\\": \\\"string\\\", " +
        //            "\\\"ingredients\\\": [{\\\"name\\\": \\\"string\\\", \\\"quantity\\\": \\\"string\\\", \\\"unit\\\": \\\"string\\\", \\\"notes\\\": \\\"string\\\"}], " +
        //            "\\\"cooking_time\\\": \\\"string\\\", " +
        //            "\\\"servings\\\": number, " +
        //            "\\\"servingTime\\\": \\\"string\\\", " +
        //            "\\\"difficulty\\\": \\\"string\\\", " +
        //            "\\\"cuisine\\\": \\\"string\\\", " +
        //            "\\\"tags\\\": [\\\"string\\\"], " +
        //            "\\\"nutrition\\\": {\\\"Calories\\\": \\\"string\\\", \\\"Protein\\\": \\\"string\\\", " +
        //            "\\\"Carbohydrates\\\": \\\"string\\\", \\\"Fat\\\": \\\"string\\\", " +
        //            "\\\"Fiber\\\": \\\"string\\\", \\\"Sugar\\\": \\\"string\\\", \\\"Sodium\\\": \\\"string\\\"}, " +
        //            "\\\"preparation_steps\\\": {\\\"step_1\\\": \\\"string\\\", \\\"step_2\\\": \\\"string\\\", ...}}. " +
        //            "Each preparation step should be labeled as step_1, step_2, etc., and be a plain instruction. " +
        //            "Respond with ONLY the JSON. Do not include explanations or extra text.";


        //        var response = await _openaiManager.GetChatCompletionAsync(prompt);

        //        if (response?.choices == null || response.choices.Length == 0)
        //        {
        //            return _responseHelper.CreateResponse(false, 404, "No recipe generated.", null);
        //        }

        //        var recipe = JsonConvert.DeserializeObject<RecipeModel>(response.choices[0].message.content);

        //        //return Ok(recipe);
        //        return _responseHelper.CreateResponse(true, 200, "success", recipe);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Optionally log the exception details here
        //        return _responseHelper.CreateResponse(false, 500, ex.Message, null);
        //    }
        //}


        //return the image based on the food recipe steps 

        [HttpPost("postIngredients/byName")]
        public async Task<IActionResult> PostBynameFoodAsync([FromBody] DishNameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DishName))
            {
                return _responseHelper.CreateResponse(false, 400, "Please provide dish name.", null);
            }

            string analysisPrompt =
                "Evaluate whether the following user input is a valid request for a food dish or recipe.\n" +
                "Ignore general phrases like 'give me', 'based on', 'a tasty dish'. Focus on extracting:\n\n" +
                "1. Up to 10 **real edible food items** or ingredients mentioned\n" +
                "2. A **core summary** of what the user is asking for (like 'a quick lunch', 'healthy dinner')\n\n" +
                "Respond only in the following JSON format:\n\n" +
                "{\n" +
                "  \"isFoodRelated\": true or false,\n" +
                "  \"recognizedFoodItems\": [\"food1\", \"food2\", ...],\n" +
                "  \"coreRequest\": \"summary of user intent\"\n" +
                "}\n\n" +
                $"Input: \"{request.DishName}\"";

            var analysisResponse = await _openaiManager.GetChatCompletionAsync(analysisPrompt);

            var foodAnalysis = JsonConvert.DeserializeObject<FoodRelatedCheckResponse>(
                analysisResponse.choices[0].message.content);

            if (foodAnalysis == null || foodAnalysis.IsFoodRelated == false)
            {
                return _responseHelper.CreateResponse(false, 400, "Please enter the name of a real dish or food item.", request.DishName);
            }

            if (foodAnalysis.RecognizedFoodItems?.Count > 5)
            {
                return _responseHelper.CreateResponse(false, 400, "Too many items provided.", foodAnalysis.RecognizedFoodItems);
            }

            // Build the final request string (e.g. "healthy dinner egg, carrot")
            string formattedDishRequest = string.IsNullOrWhiteSpace(foodAnalysis.CoreRequest)
                ? string.Join(", ", foodAnalysis.RecognizedFoodItems)
                : $"{foodAnalysis.CoreRequest} {string.Join(", ", foodAnalysis.RecognizedFoodItems)}";


            
            string recipePrompt =
                            $"Generate a complete food recipe for the dish \"{formattedDishRequest}\". " +
                            "Base the recipe on how this dish is commonly prepared. " +
                            "Respond strictly in JSON format using the following structure: " +
                            "{\\\"title\\\": \\\"string\\\", " +
                            "\\\"ingredients\\\": [{\\\"name\\\": \\\"string\\\", \\\"quantity\\\": \\\"string\\\", \\\"unit\\\": \\\"string\\\", \\\"notes\\\": \\\"string\\\"}], " +
                            "\\\"cooking_time\\\": \\\"string\\\", " +
                            "\\\"servings\\\": number, " +
                            "\\\"servingTime\\\": \\\"string\\\", " +
                            "\\\"difficulty\\\": \\\"string\\\", " +
                            "\\\"cuisine\\\": \\\"string\\\", " +
                            "\\\"tags\\\": [\\\"string\\\"], " +
                            "\\\"nutrition\\\": {\\\"Calories\\\": \\\"string\\\", \\\"Protein\\\": \\\"string\\\", " +
                            "\\\"Carbohydrates\\\": \\\"string\\\", \\\"Fat\\\": \\\"string\\\", \\\"Cholesterol\\\": \\\"string\\\", " +
                            "\\\"Saturated Fat\\\": \\\"string\\\", \\\"Unsaturated Fat\\\": \\\"string\\\", \\\"Iron\\\": \\\"string\\\", " +
                            "\\\"magnesium\\\": \\\"string\\\", \\\"Calcium\\\": \\\"string\\\", \\\"Fiber\\\": \\\"string\\\", " +
                            "\\\"Sugar\\\": \\\"string\\\", \\\"Sodium\\\": \\\"string\\\"}, " +
                            "\\\"Vitamins\\\": {\\\"A\\\": \\\"string\\\", \\\"B1\\\": \\\"string\\\", \\\"B2\\\": \\\"string\\\", " +
                            "\\\"B3\\\": \\\"string\\\", \\\"B5\\\": \\\"string\\\", \\\"B6\\\": \\\"string\\\", \\\"B9\\\": \\\"string\\\", " +
                            "\\\"B12\\\": \\\"string\\\", \\\"C\\\": \\\"string\\\", \\\"D\\\": \\\"string\\\", \\\"E\\\": \\\"string\\\", " +
                            "\\\"K\\\": \\\"string\\\"}, " +
                            "\\\"nutritionalCategories\\\": [\\\"string\\\"], " +
                            "\\\"preparation_steps\\\": {\\\"step_1\\\": \\\"string\\\", \\\"step_2\\\": \\\"string\\\", ...}}. " +
                            "Each preparation step should be labeled as step_1, step_2, etc., and be a plain instruction and must be in order from beginning to end. " +
                            "The \\\"servingTime\\\" must match one of these exactly: [\\\"Breakfast\\\", \\\"Lunch\\\", \\\"Dinner\\\", \\\"Lunch or Dinner\\\", \\\"Snack\\\", \\\"Anytime\\\", \\\"N/A\\\"]. " +
                            "The \\\"nutritionalCategories\\\" must be an array containing all applicable categories from the following list, based on the nutrition data: " +
                            "[\\\"High-Protein\\\", \\\"High-Fat\\\", \\\"High-Sodium\\\", \\\"Low-Sugar\\\", \\\"Low-Carb\\\", \\\"High-Carb\\\", \\\"High-Fiber\\\", \\\"Low-Fat\\\", \\\"Low-Sodium\\\", \\\"High-Sugar\\\", \\\"High-Magnesium\\\"]. Include multiple values if applicable. " +
                            "Respond with ONLY the JSON. Do not include explanations or extra text.";


            try
            {
                var recipeResponse = await _openaiManager.GetChatCompletionAsync(recipePrompt);

                if (recipeResponse?.choices == null || recipeResponse.choices.Length == 0)
                {
                    return _responseHelper.CreateResponse(false, 404, "No recipe generated.", null);
                }

                var recipe = JsonConvert.DeserializeObject<RecipeModel>(recipeResponse.choices[0].message.content);

                //if (recipe != null)
                //{
                //    recipe.Tags = await ModifyTags(recipe.Tags, recipe.NutritionalCategories);
                //}
                return _responseHelper.CreateResponse(true, 200, "success", recipe);
            }
            catch (Exception ex)
            {
                // Log exception details here if needed
                return _responseHelper.CreateResponse(false, 500, ex.Message, null);
            }
        }

        [HttpPost("postImage")]
        public async Task<IActionResult> PostImageAsync([FromBody] string recipe)
        {
            var x = JsonConvert.DeserializeObject<RecipeModel>(recipe);
            try
            {
                var imagePrompt = GenerateImagePromptFromRecipe(x);
                var imageUrl = await _openaiManager.GenerateImageAsync(imagePrompt);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    return _responseHelper.CreateResponse(false, 500, "Image generation failed.", null);
                }

                var uploadResponse = await _s3Manager.UploadImageFromUrlAsync(imageUrl);

                var result = new
                {
                    imageUrl = uploadResponse.FileUrl,
                    key = uploadResponse.Key,
                };

                return _responseHelper.CreateResponse(true, 200, "successfull", result);
            }
            catch (Exception ex)
            {
                // Optionally log the exception details here
                return _responseHelper.CreateResponse(false, 500, "An error occurred while processing the image.", null);
            }
        }

        [HttpPost("postIngredientImage")]
        public async Task<IActionResult> PostIngredientImageAsync(string name)  
        {

            var ings = await _postPlateManager.GetAllIngredientsAsync();
            
            foreach(var ing in ings)
            {
                //ing.Name = name;
                
                if(ing.ImageUrl == "imageurl.com" || ing.ImageUrl == "https://example.com/images/tomato.png" || ing.Date == DateTime.Parse("2025-05-23 19:20:49.873"))
                {
                    var imageUrl = await _openaiManager.GenerateImageAsync(ing.Name + " with wahite background");

                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        return _responseHelper.CreateResponse(false, 500, "Image generation failed.", null);
                    }

                    var uploadResponse = await _s3Manager.UploadImageFromUrlAsync(imageUrl);
                   
                    ing.ImageUrl = uploadResponse.FileUrl;

                    await _postPlateManager.UpdateIngredien(ing);


                }
            }
            return _responseHelper.CreateResponse(false, 500, "An error occurred while processing the image.", null);

        
        }

        //does both the above action method at once 
        [HttpPost("postFoodWithImage")]
        public async Task<IActionResult> PostFoodWithImageAsync([FromBody] PostRequest postRequest)
        {
            

            try
            {
                if (postRequest?.ingredients == null || postRequest.ingredients.Length == 0)
                {
                    return _responseHelper.CreateResponse(false, 400, "Please provide at least one ingredient.", null);
                }

                var ingredientList = string.Join(", ", postRequest.ingredients);

                var prompt = $"Generate a food recipe using the ingredients: {ingredientList}. " +
                             "Respond strictly in JSON format using this structure: " +
                             "{\\\"title\\\": \\\"string\\\", " +
                             "\\\"ingredients\\\": [{\\\"name\\\": \\\"string\\\", \\\"quantity\\\": \\\"string\\\", \\\"unit\\\": \\\"string\\\", \\\"notes\\\": \\\"string\\\"}], " +
                             "\\\"cooking_time\\\": \\\"string\\\", " +
                             "\\\"servings\\\": number, " +
                             "\\\"difficulty\\\": \\\"string\\\", " +
                             "\\\"cuisine\\\": \\\"string\\\", " +
                             "\\\"tags\\\": [\\\"string\\\"], " +
                             "\\\"nutrition\\\": {\\\"Calories\\\": \\\"string\\\", \\\"Protein\\\": \\\"string\\\", " +
                             "\\\"Carbohydrates\\\": \\\"string\\\", \\\"Fat\\\": \\\"string\\\", " +
                             "\\\"Fiber\\\": \\\"string\\\", \\\"Sugar\\\": \\\"string\\\", \\\"Sodium\\\": \\\"string\\\"}, " +
                             "\\\"preparation_steps\\\": {\\\"step_1\\\": \\\"string\\\", \\\"step_2\\\": \\\"string\\\", ...}}. " +
                             "Each preparation step should be labeled as step_1, step_2, etc., and should be a plain instruction.";

               
                var response = await _openaiManager.GetChatCompletionAsync(prompt);

                if (response?.choices == null || response.choices.Length <= 0)
                {
                    return _responseHelper.CreateResponse(false, 404, "No recipe generated.", null);
                }

                var recipe = JsonConvert.DeserializeObject<RecipeModel>(response.choices[0].message.content);

                var imagePrompt = GenerateImagePromptFromRecipe(recipe);




                var imageUrl = await _openaiManager.GenerateImageAsync(imagePrompt);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    return _responseHelper.CreateResponse(false, 500, "Image generation failed.", null);
                }

                var _uploadResponse = await _s3Manager.UploadImageFromUrlAsync(imageUrl);

                var result = new
                {
                    Recipe = recipe,
                    ImageUrl = _uploadResponse.FileUrl,
                    S3Key = _uploadResponse.Key
                };

                return _responseHelper.CreateResponse(true, 200, "Recipe and image generated successfully.", result);
            }
            catch (Exception ex)
            {
                // Optional: log the exception
                return _responseHelper.CreateResponse(false, 500, $"An error occurred: {ex.Message}", null);
            }
        }

        //donwload image from url and upload it to s3 bucket
        [HttpPost("UploadImageFromUrl")]
        public async Task<IActionResult> TestUploadFromUrl([FromBody] string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return _responseHelper.CreateResponse(
                        success: false,
                        status: 400,
                        message: "Image URL is required.",
                        data: null
                    ) ;
                }

                var result = await _s3Manager.UploadImageFromUrlAsync(imageUrl);

               

                if (string.IsNullOrEmpty(result.FileUrl))
                {
                    return _responseHelper.CreateResponse(
                        success: false,
                        status: 500,
                        message: "failed.",
                        data: null

                    ); ;
                }
                
                return _responseHelper.CreateResponse(
                    success: true,
                    status: 200,
                    message: "successful.",
                    data:  new {result.FileUrl, result.Key} 
                ) ;
            }
            catch (Exception ex)
            {
                return _responseHelper.CreateResponse(
                    success: false,
                    status: 500,
                    message: $"Unexpected error: {ex.Message}",
                    data:null
                  
                );
            }
        }

        //rturn all the inredient from the database 
        [HttpGet("Ingredients/Details")]
        public async Task<IActionResult> GetIngredientsDetailAsync()
        {
            try
            {
                var result = await _postPlateManager.GetAllIngredientsDetailsAsync();
                 
                return _responseHelper.CreateResponse(true, 200, "Success", result);
                
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Failed to fetch ingredients." + ex.Message);
                return _responseHelper.CreateResponse(false, 500, "An error occurred while fetching ingredients.", null);
            }
        }

        [HttpGet("Ingredients")]
        public async Task<IActionResult> GetIngredientsAsync()
        {
            try
            {
                var result = await _postPlateManager.GetAllIngredientsAsync();
                foreach (var item in result)
                {
                    await inertIngridientDetails(item);
                }

                return _responseHelper.CreateResponse(true, 200, "Success", result);

            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Failed to fetch ingredients." + ex.Message);
                return _responseHelper.CreateResponse(false, 500, "An error occurred while fetching ingredients.", null);
            }
        }




        //return all the ingredients by catagorized them by type
        [HttpGet("Ingredients/ByType")]
        public async Task<IActionResult> GetIngredientsGroupedByTypeAsync()
        {
            try
            {
                //var ingredients = await _postPlateManager.GetAllIngredientsAsync();
                var ingredients = await _postPlateManager.GetAllIngredientsDetailsAsync();

                var grouped = ingredients
                    .GroupBy(i => i.IngredientType)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return _responseHelper.CreateResponse(true, 200, "Grouped by type", grouped);
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Failed to group ingredients by type. " + ex.Message);
                return _responseHelper.CreateResponse(false, 500, "An error occurred while grouping ingredients.", null);
            }
        }

        private static string GenerateImagePromptFromRecipe(RecipeModel recipe)
        {
            var ingredientList = string.Join(", ", recipe.Ingredients.Select(i =>
                $"{i.Quantity} {i.Unit} {i.Name}" // Removed notes for brevity
            ));

            //var description = $"A realistic photo of {recipe.Title}, made using the following ingredients: {ingredientList}. " +
            //                  $"The food is served on a white circular plate, centered on a white background.";
            var description = $"A realistic photo of {recipe.Title}, made using the following ingredients: {ingredientList}. " +
                  $"The dish is served on a white circular plate, placed on a clean, bright white background with no distractions.";


            return description;
        }


        //post the full recipe and its image  to the database 
        [HttpPost("Add")]
        public async Task<IActionResult> AddRecipeAsync([FromBody] RecipeModel model)
        {
            
            model.UserId = "6C39DEF4-1A8C-4C0A-8EEE-1E553C6D2A84";
            string jsonString = System.Text.Json.JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });

            var result = await _postPlateManager.InsertRecipeAsync(model);

            if (model == null)

                return BadRequest("Invalid recipe data.");


            return Ok(new { message = "Recipe received successfully!"  });
        }

        [HttpGet("Recipes")]
        public async Task<IActionResult> GetAllRecipes()
        {
            var recipes = await _postPlateManager.GetAllAsync();
            return Ok(recipes);
        }

      
        [HttpPost("testUnsplash")]
        public async Task<IActionResult> GetRecipe(string name)
        {
            //var result = await _unsplashService.GetPhotosAsync(name);
           var result = await _openaiManager.GenerateImageAsync(name);
            return Ok(result);
        }


        //for ingredient to genereate properties
        [HttpPost("GenerateIngredientProperties")]
        public async Task<IActionResult> GenerateIngredientPropertiesFromName([FromBody] Ingredient ingredient)
        {
            if (!await _openaiManager.IsFoodRelatedRequestAsync(ingredient.Name))
                return _responseHelper.CreateResponse(false, 400, "The input does not appear to be a valid food ingredient.");


            var prompt =
                        $"Provide the nutritional composition and health profile of the ingredient \"{ingredient.Name}\" **per 100 grams**.\n\n" +
                        "Respond ONLY in valid raw JSON format with the following structure:\n\n" +
                        "{\n" +
                        "  \"servingSize\": {\n" +
                        "    \"amount\": 100,\n" +
                        "    \"unit\": \"g\",\n" +
                        "    \"description\": \"Nutritional values are based on a 100-gram serving of the ingredient.\"\n" +
                        "  },\n" +
                        "  \"nutrients\": [\n" +
                        "    { \"Name\": \"string\", \"Value\": \"string\", \"Unit\": \"string (or null)\" },\n" +
                        "    ... up to 25 items for key nutrients\n" +
                        "  ],\n" +
                        "  \"healthProfile\": {\n" +
                        "    \"antiInflammatoryLevel\": \"low | medium | high\",\n" +
                        "    \"antioxidantLevel\": \"low | medium | high\",\n" +
                        "    \"antioxidants\": {\n" +
                        "      \"vitaminC\": number (mg),\n" +
                        "      \"betaCarotene\": number (μg),\n" +
                        "      \"polyphenols\": \"low | medium | high\"\n" +
                        "    }\n" +
                        "  }\n" +
                        "}\n\n" +
                        "Notes:\n" +
                        "- Do not include any text or explanation outside the JSON.\n" +
                        "- Use null for units where not applicable.\n" +
                        "- Base estimations on typical food science knowledge.\n" +
                        "- If betaCarotene or polyphenols cannot be determined, omit them from antioxidants.\n";



            var resultOpenai = await _openaiManager.GetChatCompletionAsync(prompt);

            try
            {
                var properties = JsonConvert.DeserializeObject<IngredientAnalysisResponse>(resultOpenai.choices[0].message.content);

                //var ing = properties.Nutrients[0];

                
                
                foreach(var p in properties.Nutrients)
                {
                    p.IngredientPropertiesId = ingredient.Id;
                    var result = await _postPlateManager.InsertIngredientPropertyAsync(p);
                }
               
                properties.HealthProfile.IngredientId= ingredient.Id;
                var preut = await _postPlateManager.InsertHealthProfile(properties.HealthProfile);

                return _responseHelper.CreateResponse(true, 200, "Ingredient properties generated successfully.", properties);
            }
            catch (Exception ex)
            {
                // Optionally log `ex.Message`
                return _responseHelper.CreateResponse(false, 500, "Failed to parse response from AI.");
            }
        }

        //for ingredient to genereate image
        [HttpPost("postIngredientImage")]
        public async Task<IActionResult> PostIngImageAsync([FromBody] Ingredient ingredient)    
        {
            var ing = JsonConvert.DeserializeObject<RecipeModel>(ingredient.Name);
            try
            {
                var imagePrompt = GenerateImagePromptFromRecipe(ing);   
                var imageUrl = await _openaiManager.GenerateImageAsync(imagePrompt);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    return _responseHelper.CreateResponse(false, 500, "Image generation failed.", null);
                }

                var uploadResponse = await _s3Manager.UploadImageFromUrlAsync(imageUrl);

                var result = new
                {
                    imageUrl = uploadResponse.FileUrl,
                    key = uploadResponse.Key,
                };

                return _responseHelper.CreateResponse(true, 200, "successfull", result);
            }
            catch (Exception ex)
            {
                // Optionally log the exception details here
                return _responseHelper.CreateResponse(false, 500, "An error occurred while processing the image.", null);
            }
        }



        private async Task<bool> inertIngridientDetails(Ingredient ingredient)
        {
            var v1 = await _postPlateManager.GetIngredientPropertyAsync(ingredient.Id);
            //var p1 = await _postPlateManager.GetHealthProfileAsync(ingredient.Id);
            if (v1.Unit != null)
            {
                return false;
            }
            //if (p1 != null)
            //{
            //    return false;
            //}
            if (!await _openaiManager.IsFoodRelatedRequestAsync(ingredient.Name))
                return false;
            
            var prompt =
                        $"Provide the nutritional composition and health profile of the ingredient \"{ingredient.Name}\" **per 100 grams**.\n\n" +
                        "Respond ONLY in valid raw JSON format with the following structure:\n\n" +
                        "{\n" +
                        "  \"servingSize\": {\n" +
                        "    \"amount\": 100,\n" +
                        "    \"unit\": \"g\",\n" +
                        "    \"description\": \"Nutritional values are based on a 100-gram serving of the ingredient.\"\n" +
                        "  },\n" +
                        "  \"nutrients\": [\n" +
                        "    { \"Name\": \"string\", \"Value\": \"string\", \"Unit\": \"string (or null)\" },\n" +
                        "    ... up to 25 items for key nutrients\n" +
                        "  ],\n" +
                        "  \"healthProfile\": {\n" +
                        "    \"antiInflammatoryLevel\": \"low | medium | high\",\n" +
                        "    \"antioxidantLevel\": \"low | medium | high\",\n" +
                        "    \"antioxidants\": {\n" +
                        "      \"vitaminC\": number (mg),\n" +
                        "      \"betaCarotene\": number (μg),\n" +
                        "      \"polyphenols\": \"low | medium | high\"\n" +
                        "    }\n" +
                        "  }\n" +
                        "}\n\n" +
                        "Notes:\n" +
                        "- Do not include any text or explanation outside the JSON.\n" +
                        "- Use null for units where not applicable.\n" +
                        "- Base estimations on typical food science knowledge.\n" +
                        "- If betaCarotene or polyphenols cannot be determined, omit them from antioxidants.\n";

          

            var resultOpenai = await _openaiManager.GetChatCompletionAsync(prompt);
            if(resultOpenai.choices == null)
            {
                return false;
            }

            try
            {
                var properties = JsonConvert.DeserializeObject<IngredientAnalysisResponse>(resultOpenai.choices[0].message.content);

                for(var i = 0; i < properties.Nutrients.Count; i++)
                {
                    properties.Nutrients[i].IngredientPropertiesId = ingredient.Id;
                   
                    var v = await _postPlateManager.GetIngredientPropertyAsync(ingredient.Id, properties.Nutrients[i].Name);
                   
                    if (v.Unit == null)
                    {
                        await _postPlateManager.InsertIngredientPropertyAsync(properties.Nutrients[i]);
                    }
                }

                var p = await _postPlateManager.GetHealthProfileAsync(ingredient.Id);
                if(p.AntioxidantLevel == "")
                {
                    properties.HealthProfile.IngredientId = ingredient.Id;
                    var preut = await _postPlateManager.InsertHealthProfile(properties.HealthProfile);
                   // var result2 = await _postPlateManager.InsertIngredientPropertyAsync(ing);

                }

                //var result = await _postPlateManager.InsertIngredientPropertyAsync(ing);
                //properties.HealthProfile.IngredientId = ingredient.Id;
                return true;
            }
            catch (Exception ex)
            {
                // Optionally log `ex.Message`
                return false;
            }
        }


        private static async Task<string[]> ModifyTags(string[] existingTages, string[] newTages)  
        {
            var list = new List<string>();

            for (int i = 0; i < existingTages.Length; i++)
            {
                list.Add(existingTages[i]);
            }

          
            for (int i = 0; i < newTages.Length; i++)
            {
                list.Add(newTages[i]);
            }

          return list.ToArray();
        }


    }
    //var result = await _unsplashService.GetPhotosAsync(name);
    // var result = await _postPlateManager.InsertIngredientPropertyAsync(ingredientProperty);


    public class DishNameRequest
    {
        public string DishName { get; set; }
    }











    //public class PostRequest
    //{
    //    public string[] ingredients { get; set; }   
    //}
    //public class ImageRequest
    //{
    //    public string description { get; set; }
    //}

    //public class IngredientModel
    //{
    //    [JsonPropertyName("name")]
    //    public string Name { get; set; }

    //    [JsonPropertyName("quantity")]
    //    public string Quantity { get; set; }

    //    [JsonPropertyName("unit")]
    //    public string Unit { get; set; }

    //    [JsonPropertyName("notes")]
    //    public string Notes { get; set; }
    //}

    //public class RecipeModel
    //{
    //    [JsonPropertyName("title")]
    //    public string Title { get; set; }

    //    [JsonPropertyName("ingredients")]
    //    public List<IngredientModel> Ingredients { get; set; }

    //    [JsonPropertyName("cooking_time")]
    //    public string cooking_time { get; set; }

    //    [JsonPropertyName("servings")]
    //    public int Servings { get; set; }

    //    [JsonPropertyName("difficulty")]
    //    public string Difficulty { get; set; }

    //    [JsonPropertyName("cuisine")]
    //    public string Cuisine { get; set; }

    //    [JsonPropertyName("tags")]
    //    public string[] Tags { get; set; }

    //    [JsonPropertyName("nutrition")]
    //    public Dictionary<string, string> Nutrition { get; set; }

    //    [JsonPropertyName("preparation_steps")]
    //    public Dictionary<string, string> preparation_steps { get; set; }   
    //}




}
