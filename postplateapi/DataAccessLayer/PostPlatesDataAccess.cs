using Modles;
using System;
using System.Collections.Generic;
using System.Data;

using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Azure;
using System.Linq;
using System.Net.NetworkInformation;

namespace DataAccessLayer
{
    public class PostPlatesDataAccess
    {

        private readonly ConnictionString _connectionString;

        public PostPlatesDataAccess(ConnictionString connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> InsertIngredientAsync(Ingredient ingredient)
        {
            using (var conn = new SqlConnection(_connectionString.GetConnectionString()))
            using (var cmd = new SqlCommand("InsertIngredient", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", ingredient.Id);
                cmd.Parameters.AddWithValue("@name", ingredient.Name);
                cmd.Parameters.AddWithValue("@type", ingredient.IngredientType);
                cmd.Parameters.AddWithValue("@imageUrl", ingredient.ImageUrl);
                cmd.Parameters.AddWithValue("@date", ingredient.Date);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }


        public async Task<List<Ingredient>> GetAllIngredientsDataOrderedByDateAsync()
        {
            var list = new List<Ingredient>();
            using var conn = new SqlConnection(_connectionString.GetConnectionString());

            const string query = @"SELECT Id, Name, IngredientType, ImageUrl, _Date 
                                   FROM Ingredients
                                   ORDER BY _Date ASC;";

            using var cmd = new SqlCommand(query, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Ingredient
                {
                    Id = reader["Id"].ToString(), // assuming your DB column is 'Id'
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    IngredientType = reader["IngredientType"] != DBNull.Value ? reader["IngredientType"].ToString() : string.Empty,
                    ImageUrl = reader["ImageUrl"] != DBNull.Value ? reader["ImageUrl"].ToString() : string.Empty,
                    Date = reader.GetDateTime(reader.GetOrdinal("_Date"))
                });
            }

            return list;
        }

        public async Task<List<Ingredient>> GetPagedIngredientsSinceDateAsync(DateTime dateTimePaging, int pageSize)
        {
            var list = new List<Ingredient>();
            using var conn = new SqlConnection(_connectionString.GetConnectionString());

            const string query = @"
                                SELECT TOP (@PageSize) Id, Name, IngredientType, ImageUrl, _Date 
                                FROM Ingredients
                                WHERE _Date > @Date
                                ORDER BY _Date ASC;
                            ";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Date", dateTimePaging);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Ingredient
                {
                    Id = reader["Id"].ToString(),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    IngredientType = reader["IngredientType"] != DBNull.Value ? reader["IngredientType"].ToString() : string.Empty,
                    ImageUrl = reader["ImageUrl"] != DBNull.Value ? reader["ImageUrl"].ToString() : string.Empty,
                    Date = reader.GetDateTime(reader.GetOrdinal("_Date"))
                });
            }

            return list;
        }


        public async Task<bool> DeleteIngredientByIdAsync(string id)
        {
            using var conn = new SqlConnection(_connectionString.GetConnectionString());
            using var cmd = new SqlCommand("DeleteIngredientById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateIngredientByIdAsync(Ingredient ingredient)
        {
            using var conn = new SqlConnection(_connectionString.GetConnectionString());
            using var cmd = new SqlCommand("UpdateIngredientById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@id", ingredient.Id);
            cmd.Parameters.AddWithValue("@name", ingredient.Name);
            cmd.Parameters.AddWithValue("@type", ingredient.IngredientType);
            cmd.Parameters.AddWithValue("@imageUrl", ingredient.ImageUrl);
            cmd.Parameters.AddWithValue("@date", ingredient.Date);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }


        //for recipe 
        public async Task<int> AddRecipeAsync(RecipeModel recipe)
        {
            string newId = Guid.NewGuid().ToString();
            recipe.Id = newId;

            using (var connection = new SqlConnection(_connectionString.GetConnectionString()))
            {
                await connection.OpenAsync();

                var query = @"INSERT INTO Recipes (Id, Title, Cuisine, Difficulty, CookingTime, Servings,ServingTime, ImageUrl, CreatedAt, UserId)
                            VALUES (@Id, @Title, @Cuisine, @Difficulty, @CookingTime, @Servings, @ServingTime, @ImageUrl, @CreatedAt, @UserId);";


                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", recipe.Id);
                    command.Parameters.AddWithValue("@Title", recipe.Title);
                    command.Parameters.AddWithValue("@Cuisine", recipe.Cuisine);
                    command.Parameters.AddWithValue("@Difficulty", recipe.Difficulty);
                    command.Parameters.AddWithValue("@CookingTime", recipe.cooking_time);
                    command.Parameters.AddWithValue("@Servings", recipe.Servings);
                    //command.Parameters.AddWithValue("@ImageUrl", (object?)recipe.ImageUrl ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ServingTime", recipe.servingTime);
                    command.Parameters.AddWithValue("@ImageUrl", recipe.ImageUrl);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    command.Parameters.AddWithValue("@UserId", recipe.UserId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        await AddNutritionAsync(recipe.Nutrition, recipe.Id);
                        await AddPreparationStepsAsync(recipe.preparation_steps, recipe.Id);
                        var _IngredientModel = (List<IngredientModel>)recipe.Ingredients;

                        await  AddRecipeIngredientsAsync(_IngredientModel, recipe.Id);

                        await AddRecipeTagsAsync(recipe.Tags, recipe.Id);
                    }
                    return rowsAffected; // / Returns the new inserted Recipe Id
                }
            }
        }

        public async Task AddNutritionAsync(Dictionary<string, string> nutritionData, string recipeId)
        {
            using (var connection = new SqlConnection(_connectionString.GetConnectionString()))
            {
                await connection.OpenAsync();

                foreach (var item in nutritionData)
                {
                    var query = @"INSERT INTO Nutrition (Id, RecipeId, Name, Value)
                                VALUES (@Id, @RecipeId, @Name, @Value);";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                        command.Parameters.AddWithValue("@RecipeId", recipeId);
                        command.Parameters.AddWithValue("@Name", item.Key);
                        command.Parameters.AddWithValue("@Value", item.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task AddPreparationStepsAsync(Dictionary<string, string> steps, string recipeId)
        {
            using (var connection = new SqlConnection(_connectionString.GetConnectionString()))
            {
                await connection.OpenAsync();

                foreach (var step in steps)
                {
                    var query = @"INSERT INTO PreparationSteps (Id, RecipeId, StepKey, StepText)
                                VALUES (@Id, @RecipeId, @StepKey, @StepText);";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                        command.Parameters.AddWithValue("@RecipeId", recipeId);
                        command.Parameters.AddWithValue("@StepKey", step.Key);
                        command.Parameters.AddWithValue("@StepText", step.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        //incert recipe ingredient per post/recipe
        public async Task AddRecipeIngredientsAsync(List<IngredientModel> ingredients, string recipeId)
        {
            using (var connection = new SqlConnection(_connectionString.GetConnectionString()))
            {
                await connection.OpenAsync();

                foreach (var ingredient in ingredients)
                {
                    string ingredientId;

                    // First, check if the ingredient already exists
                    var selectQuery = "SELECT Id FROM Ingredients WHERE Name = @Name";
                    using (var selectCommand = new SqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@Name", ingredient.Name);
                        var result = await selectCommand.ExecuteScalarAsync();

                        if (result != null)
                        {
                            ingredientId = result.ToString();
                        }
                        else
                        {
                            // Create a new ID and insert the ingredient
                            ingredientId = Guid.NewGuid().ToString();

                            var ing = new Ingredient
                            {
                                Id = ingredientId,
                                Name = ingredient.Name,
                                IngredientType = "unknown",
                                ImageUrl = "imageurl.com",
                                Date = DateTime.Now,
                            };

                            await InsertIngredientAsync(ing);
                        }

                    }

                  

                    // Now insert into RecipeIngredients
                    var insertQuery = @"INSERT INTO RecipeIngredients 
                                (Id, RecipeId, Quantity, Unit, Notes, Ingredientname)
                                VALUES (@Id, @RecipeId, @Quantity, @Unit, @Notes, @Ingredientname);";

                    using (var insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                        insertCommand.Parameters.AddWithValue("@RecipeId", recipeId);
                        insertCommand.Parameters.AddWithValue("@Ingredientname", ingredient.Name ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@Quantity", ingredient.Quantity ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@Unit", ingredient.Unit ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@Notes", ingredient.Notes ?? (object)DBNull.Value);

                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }
            }
        }


        public async Task AddRecipeTagsAsync(string[] tags, string recipeId)
        {
            using (var connection = new SqlConnection(_connectionString.GetConnectionString()))
            {
                await connection.OpenAsync();

                foreach (var tag in tags)
                {
                    string tagId;

                    // Step 1: Check if tag exists
                    var checkTagQuery = "SELECT Id FROM Tags WHERE Name = @Name";
                    using (var checkTagCommand = new SqlCommand(checkTagQuery, connection))
                    {
                        checkTagCommand.Parameters.AddWithValue("@Name", tag);
                        var result = await checkTagCommand.ExecuteScalarAsync();

                        if (result != null)
                        {
                            tagId = result.ToString();
                        }
                        else
                        {
                            // Step 2: Insert new tag
                            tagId = Guid.NewGuid().ToString();

                            var insertTagQuery = "INSERT INTO Tags (Id, Name) VALUES (@Id, @Name)";
                            using (var insertTagCommand = new SqlCommand(insertTagQuery, connection))
                            {
                                insertTagCommand.Parameters.AddWithValue("@Id", tagId);
                                insertTagCommand.Parameters.AddWithValue("@Name", tag);
                                await insertTagCommand.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    // Step 3: Insert into RecipeTags
                    var insertRecipeTagQuery = @"INSERT INTO RecipeTags (RecipeId, TagId) 
                                         VALUES (@RecipeId, @TagId)";
                    using (var insertRecipeTagCommand = new SqlCommand(insertRecipeTagQuery, connection))
                    {
                        insertRecipeTagCommand.Parameters.AddWithValue("@RecipeId", recipeId);
                        insertRecipeTagCommand.Parameters.AddWithValue("@TagId", tagId);
                        await insertRecipeTagCommand.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task<List<RecipeModel>> GetAllRecipes()
        {
            var recipes = new List<RecipeModel>();
            var recipeMap = new Dictionary<string, RecipeModel>();

            using (var connection = new SqlConnection(_connectionString.GetConnectionString()))
            {
                await connection.OpenAsync();

                // 1. Get all Recipes
               // var recipeCmd = new SqlCommand("SELECT * FROM Recipes", connection);
                var recipeCmd = new SqlCommand("SELECT * FROM Recipes ORDER BY PagingId DESC", connection);
                using (var reader = await recipeCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var recipe = new RecipeModel
                        {
                            Id = reader["Id"].ToString(),
                            Title = reader["Title"].ToString(),
                            Cuisine = reader["Cuisine"].ToString(),
                            Difficulty = reader["Difficulty"].ToString(),
                            cooking_time = reader["CookingTime"].ToString(),
                            servingTime = reader["servingTime"].ToString(),
                            Servings = Convert.ToInt32(reader["Servings"]),
                            ImageUrl = reader["ImageUrl"].ToString(),
                            UserId = reader["UserId"].ToString(),
                            Ingredients = new List<IngredientModel>(),
                            preparation_steps = new Dictionary<string, string>(),
                            Nutrition = new Dictionary<string, string>()
                        };

                        recipes.Add(recipe);
                        recipeMap[recipe.Id] = recipe;
                    }
                }

                // 2. Get all Ingredients
                var ingredientCmd = new SqlCommand(@"SELECT 
                                                    r.Id AS RecipeId,
                                                    ri.IngredientName,
                                                    ri.Quantity,
	                                                ri.Unit,
	                                                ri.Id,
	                                                ri.Notes
                                                FROM 
                                                    Recipes r
                                                INNER JOIN 
                                                    RecipeIngredients ri
                                                    ON r.Id = ri.RecipeId", connection);

                using (var reader = await ingredientCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string recipeId = reader["RecipeId"].ToString();
                        if (recipeMap.ContainsKey(recipeId))
                        {
                            recipeMap[recipeId].Ingredients.Add(new IngredientModel
                            {
                                Name = reader["IngredientName"].ToString(),
                                Quantity = reader["Quantity"].ToString(),
                                Unit = reader["Unit"].ToString(),
                                Notes = reader["Notes"].ToString(),
                               
                            });
                        }
                        
                    }
                }

                // 3. Get all Tags
                var tagsCmd = new SqlCommand(@"SELECT rt.RecipeId, t.Name
                                               FROM RecipeTags rt
                                               INNER JOIN Tags t ON rt.TagId = t.Id", connection);

                using (var reader = await tagsCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string recipeId = reader["RecipeId"].ToString();
                        if (recipeMap.ContainsKey(recipeId))
                        {
                            var recipe = recipeMap[recipeId];
                            if (recipe.Tags == null)
                                recipe.Tags = new string[] { reader["Name"].ToString() };
                            else
                                recipe.Tags = recipe.Tags.Append(reader["Name"].ToString()).ToArray();
                        }
                    }
                }

                // 4. Get all Nutrition
                var nutritionCmd = new SqlCommand("SELECT RecipeId, Name, Value FROM Nutrition", connection);
                using (var reader = await nutritionCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string recipeId = reader["RecipeId"].ToString();
                        if (recipeMap.ContainsKey(recipeId))
                        {
                            recipeMap[recipeId].Nutrition[reader["Name"].ToString()] = reader["Value"].ToString();
                        }
                    }
                }

                // 5. Get all Preparation Steps
                var stepsCmd = new SqlCommand("SELECT RecipeId, StepKey, StepText FROM PreparationSteps ORDER BY CreatedAt ASC", connection);
                using (var reader = await stepsCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string recipeId = reader["RecipeId"].ToString();
                        if (recipeMap.ContainsKey(recipeId))
                        {
                            recipeMap[recipeId].preparation_steps[reader["StepKey"].ToString()] = reader["StepText"].ToString();
                        }
                    }
                }
            }

            return recipes;
        }


    }
}


