using Microsoft.Data.SqlClient;
using Modles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class IngredientPropertyDataAccess
    {
        //private readonly string _connectionString;
        private readonly ConnictionString _connectionString;

        public IngredientPropertyDataAccess(ConnictionString connectionString)    
        {
            _connectionString = connectionString;
        }

        public async Task<int> InsertIngredientPropertyAsync(IngredientProperty prop)
        {
            const string query = @"
            INSERT INTO IngredientProperties (IngredientPropertiesId, Name, Value, Unit)
            VALUES (@IngredientPropertiesId, @Name, @Value, @Unit);
        ";

            using SqlConnection conn = new SqlConnection(_connectionString.GetConnectionString());
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@IngredientPropertiesId", prop.IngredientPropertiesId);
            cmd.Parameters.AddWithValue("@Name", prop.Name);
            cmd.Parameters.AddWithValue("@Value", prop.Value);
            cmd.Parameters.AddWithValue("@Unit", (object?)prop.Unit ?? DBNull.Value);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<IngredientProperty>> GetByIngredientPropertyIdAsync(string ingredientPropertiesId)
        {
            const string query = "SELECT * FROM IngredientProperties WHERE IngredientPropertiesId = @IngredientPropertiesId";

            var results = new List<IngredientProperty>();

            using SqlConnection conn = new SqlConnection(_connectionString.GetConnectionString());
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@IngredientPropertiesId", ingredientPropertiesId);

            await conn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new IngredientProperty
                {
                    Id = reader.GetInt32(0),
                    IngredientPropertiesId = reader.GetString(1),
                    Name = reader.GetString(2),
                    Value = reader.GetString(3),
                    Unit = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }

            return results;
        }
        public async Task<List<IngredientProperty>> GetByIngredientPropertyIdAsync(string ingredientPropertiesId, string Name)  
        {
            const string query = "SELECT * FROM IngredientProperties WHERE IngredientPropertiesId = @IngredientPropertiesId AND Name = @Name";


            var results = new List<IngredientProperty>();

            using SqlConnection conn = new SqlConnection(_connectionString.GetConnectionString());
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@IngredientPropertiesId", ingredientPropertiesId);
            cmd.Parameters.AddWithValue("@Name", Name);

            await conn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new IngredientProperty
                {
                    Id = reader.GetInt32(0),
                    IngredientPropertiesId = reader.GetString(1),
                    Name = reader.GetString(2),
                    Value = reader.GetString(3),
                    Unit = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }

            return results;
        }

        public async Task<int> UpdateIngredientPropertyAsync(IngredientProperty prop)
        {
            const string query = @"
            UPDATE IngredientProperties
            SET Name = @Name, Value = @Value, Unit = @Unit
            WHERE Id = @Id;
        ";

            using SqlConnection conn = new SqlConnection(_connectionString.GetConnectionString());
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", prop.Id);
            cmd.Parameters.AddWithValue("@Name", prop.Name);
            cmd.Parameters.AddWithValue("@Value", prop.Value);
            cmd.Parameters.AddWithValue("@Unit", (object?)prop.Unit ?? DBNull.Value);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteIngredientPropertyAsync(int id)
        {
            const string query = "DELETE FROM IngredientProperties WHERE Id = @Id";

            using SqlConnection conn = new SqlConnection(_connectionString.GetConnectionString());
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }
    }
}
