using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using RecipesApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.Repositories
{
    /// <summary>
    /// NO Longer used. Replace by Context + Service
    /// </summary>
    public class RecipesRepository : IRecipesRepository, IDisposable
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private const string DB = "recipes";

        public RecipesRepository(ILogger<IRecipesRepository> logger, string connectionString)
        {
            this._connectionString = connectionString;
            this._logger = logger;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(this._connectionString);
        }

        public async Task<IEnumerable<RecipeBase>> GetAllRecipes()
        {
            var recipes = new List<RecipeBase>();
            using (var conn = this.GetConnection())
            {
                try
                {
                    conn.Open();
                }
                catch (MySqlException ex)
                {
                    this._logger.LogError($"SQL Exception. Code: {ex.Code} | Message: {ex.Message}");
                }
                var cmdText = $"SELECT * from {DB}"; 
                var command = new MySqlCommand() { CommandText = cmdText, Connection = conn };

                using (var reader = await command.ExecuteReaderAsync())
                {

                    while ( reader.Read())
                    {
                        //for (var i = 0; i < reader.FieldCount; i++)
                        //{
                            recipes.Add(new RecipeBase()
                            {
                                Recipe_Id = Convert.ToInt32(reader["Recipe_Id"]),
                                TitleLong = reader["TitleLong"].ToString(),
                                TitleShort = reader["TitleShort"].ToString(),
                                Description = reader["Description"].ToString(),
                                OriginalLink = reader["OriginalLink"].ToString(),
                                LastModifier = reader["LastModifier"].ToString(),
                                AuditDate = Convert.ToDateTime(reader["AuditDate"]),
                                CreationDate = Convert.ToDateTime(reader["CreationDate"])
                            });
                        //}
                    }
                }
                conn.Close();
            }
            return recipes;
        }

        public void Dispose()
        {
            this._logger.LogInformation("Disposing");
        }
    }
}
