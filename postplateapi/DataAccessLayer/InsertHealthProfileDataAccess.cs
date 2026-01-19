using Microsoft.Data.SqlClient;
using Modles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class InsertHealthProfileDataAccess
    {


        //private readonly string _connectionString;
        private readonly ConnictionString _connectionString;

        public InsertHealthProfileDataAccess(ConnictionString connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<int> InsertHealthProfileAsync(HealthProfile profile)
        {
            var p = new HealthProfile();
            p = profile;

            const string query = @"
                                INSERT INTO HealthProfiles 
                                (IngredientId, AntiInflammatoryLevel, AntioxidantLevel, VitaminC, BetaCarotene, Polyphenols)
                                VALUES (@IngredientId, @AntiInflammatoryLevel, @AntioxidantLevel, @VitaminC, @BetaCarotene, @Polyphenols);
                                SELECT SCOPE_IDENTITY();";

            using var connection = new SqlConnection(_connectionString.GetConnectionString());
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@IngredientId", p.IngredientId);
            command.Parameters.AddWithValue("@AntiInflammatoryLevel", p.AntiInflammatoryLevel);
            command.Parameters.AddWithValue("@AntioxidantLevel",    p.AntioxidantLevel);

            if(p.Antioxidants == null)
            {
                command.Parameters.AddWithValue("@VitaminC",  DBNull.Value);
                command.Parameters.AddWithValue("@BetaCarotene", DBNull.Value);
                command.Parameters.AddWithValue("@Polyphenols", DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue("@VitaminC", (object?)p.Antioxidants.VitaminC ?? DBNull.Value);
                command.Parameters.AddWithValue("@BetaCarotene", (object?)p.Antioxidants.BetaCarotene ?? DBNull.Value);
                command.Parameters.AddWithValue("@Polyphenols", (object?)p.Antioxidants.Polyphenols ?? DBNull.Value);

            }


            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        public async Task<HealthProfile?> GetHealthProfileByIngredientIdAsync(string ingredientId)
        {
           
            const string query = @"
                                SELECT Id, IngredientId, AntiInflammatoryLevel, AntioxidantLevel, 
                                       VitaminC, BetaCarotene, Polyphenols
                                FROM HealthProfiles
                                WHERE IngredientId = @IngredientId";

            using var connection = new SqlConnection(_connectionString.GetConnectionString());
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IngredientId", ingredientId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                return new HealthProfile
                {
                  
                    IngredientId = reader["IngredientId"].ToString(),
                    AntiInflammatoryLevel = reader["AntiInflammatoryLevel"].ToString()!,
                    AntioxidantLevel = reader["AntioxidantLevel"].ToString()!,
                    Antioxidants = new Antioxidants
                    {
                        VitaminC = ReadDoubleRounded(reader, "VitaminC"),
                        BetaCarotene = ReadDoubleRounded(reader, "BetaCarotene"),
                        Polyphenols = reader["Polyphenols"] != DBNull.Value ? reader["Polyphenols"].ToString() : null
                    }
                };
            }

            return new HealthProfile();
        }
        private double ReadDoubleRounded(IDataReader reader, string column, int decimals = 1)
        {
            return reader[column] != DBNull.Value ? Math.Round((double)reader[column], decimals) : 0;
        }
        public async Task<bool> UpdateHealthProfileAsync(HealthProfile profile)
        {
            const string query = @"
                                UPDATE HealthProfiles
                                SET AntiInflammatoryLevel = @AntiInflammatoryLevel,
                                    AntioxidantLevel = @AntioxidantLevel,
                                    VitaminC = @VitaminC,
                                    BetaCarotene = @BetaCarotene,
                                    Polyphenols = @Polyphenols
                                WHERE IngredientId = @IngredientId";

            using var connection = new SqlConnection(_connectionString.GetConnectionString());
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@IngredientId", profile.IngredientId);
            command.Parameters.AddWithValue("@AntiInflammatoryLevel", profile.AntiInflammatoryLevel);
            command.Parameters.AddWithValue("@AntioxidantLevel", profile.AntioxidantLevel);
            command.Parameters.AddWithValue("@VitaminC", (object?)profile.Antioxidants.VitaminC ?? DBNull.Value);
            command.Parameters.AddWithValue("@BetaCarotene", (object?)profile.Antioxidants.BetaCarotene ?? DBNull.Value);
            command.Parameters.AddWithValue("@Polyphenols", (object?)profile.Antioxidants.Polyphenols ?? DBNull.Value);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        public async Task<bool> DeleteHealthProfileByIngredientIdAsync(int ingredientId)
        {
            const string query = "DELETE FROM HealthProfiles WHERE IngredientId = @IngredientId";

            using var connection = new SqlConnection(_connectionString.GetConnectionString());
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IngredientId", ingredientId);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

    }
}
