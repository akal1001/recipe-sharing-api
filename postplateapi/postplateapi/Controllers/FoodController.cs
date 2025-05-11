using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using postplateapi.OpenAiManagers;
using postplateapi.ResponseHelper;
using postplateapi.S3Managers;
using System.Text.Json.Serialization;

namespace postplateapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodController : ControllerBase
    {
        private readonly S3Manager _s3Manager;
        private readonly ActionResultResponseHelper _responseHelper;    
        public FoodController() 
        {
            _s3Manager = new S3Manager(); // If you use DI, inject it instead
            _responseHelper= new ActionResultResponseHelper();
        }

        [HttpPost("postIngredients")]
        public async Task<IActionResult> PostFoodAsync([FromBody] PostRequest postRequest)
        {
            if (postRequest?.ingredients == null || postRequest.ingredients.Length == 0)
            {
                return BadRequest("Please provide at least one ingredient.");
            }

            var ingredientList = string.Join(", ", postRequest.ingredients);

            var prompt = $"Generate a food recipe using the ingredients: {ingredientList}. " +
                         "Respond strictly in JSON format using this structure: " +
                         "{\\\"title\\\": \\\"string\\\", \\\"ingredients\\\": [\\\"string\\\"], " +
                         "\\\"preparation_steps\\\": [\\\"string\\\"], \\\"cooking_time\\\": \\\"string\\\"}. " +
                         "Do not include any numbering or extra characters in the preparation_steps.";

            OpenAiManager openaiManager = new OpenAiManager();
            var response = await openaiManager.GetChatCompletionAsync(prompt);
            var responseData = response.choices[0].message.content;

            return Ok(responseData);
        }

        [HttpPost("postImage")]
        public async Task<IActionResult> PostImageAsync([FromBody] RecipeModel recipe)
        {

            var prompt = GenerateImagePromptFromRecipe(recipe);

            OpenAiManager openaiManager = new OpenAiManager();
            var imageUrl = await openaiManager.GenerateImageAsync(prompt);

            if (string.IsNullOrEmpty(imageUrl))
            {
                return StatusCode(500, "Image generation failed.");
            }

            return Ok(new { imageUrl });
        }

        [HttpPost("postFoodWithImage")]
        public async Task<IActionResult> PostFoodWithImageAsync([FromBody] PostRequest postRequest)
        {
            if (postRequest?.ingredients == null || postRequest.ingredients.Length == 0)
            {
                return BadRequest("Please provide at least one ingredient.");
            }

            var ingredientList = string.Join(", ", postRequest.ingredients);

            var prompt = $"Generate a food recipe using the ingredients: {ingredientList}. " +
                         "Respond strictly in JSON format using this structure: " +
                         "{\\\"title\\\": \\\"string\\\", \\\"ingredients\\\": [\\\"string\\\"], " +
                         "\\\"preparation_steps\\\": [\\\"string\\\"], \\\"cooking_time\\\": \\\"string\\\"}. " +
                         "Do not include any numbering or extra characters in the preparation_steps.";

            OpenAiManager openaiManager = new OpenAiManager();
            var response = await openaiManager.GetChatCompletionAsync(prompt);
            var recipe = JsonConvert.DeserializeObject<RecipeModel>(response.choices[0].message.content);

            // Generate the image description from the recipe data
            var imagePrompt = GenerateImagePromptFromRecipe(recipe);

           

            // Now call the image generation API with the generated description
            var imageUrl = await openaiManager.GenerateImageAsync(imagePrompt);

            //upload to s3 bucket
            var _uplodResposne = await _s3Manager.UploadImageFromUrlAsync(imageUrl);

            if (string.IsNullOrEmpty(imageUrl))
            {
                return StatusCode(500, "Image generation failed.");
            }

            // Return both the recipe and image URL
            return Ok(new { recipe, _uplodResposne.FileUrl, _uplodResposne.Key });
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




        private string GenerateImagePromptFromRecipe(RecipeModel recipe)
        {
            var description = $"A realistic photo of {recipe.title} made with {string.Join(", ", recipe.ingredients)}. ";
            description += "The food is served on a white circular plate, fully visible and placed in the center of the plate with white background. ";
            //description += "The background is plain white with soft natural lighting.";

            return description;
        }


    }
    public class PostRequest
    {
        public string[] ingredients { get; set; }   
    }
    public class ImageRequest
    {
        public string description { get; set; }
    }
    public class RecipeModel
    {
        [JsonPropertyName("title")]
        public string title { get; set; }

        [JsonPropertyName("ingredients")]
        public string[] ingredients { get; set; }

        [JsonPropertyName("preparation_steps")]
        public string[] preparation_steps { get; set; }

        [JsonPropertyName("cooking_time")]
        public string cookingTime { get; set; }
    }
    
 

}
