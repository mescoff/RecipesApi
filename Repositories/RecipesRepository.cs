using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using RecipesApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.Repositories
{
    public class RecipesRepository : Controller, IRecipesRepository
    {
        private readonly string _connectionString;

        public RecipesRepository(string connectionString)
        {
            this._connectionString = connectionString;
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
                conn.Open();
                var command = new MySqlCommand("SELECT * from recipes");

                using (var reader = command.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        recipes.Add(new RecipeBase()
                        {
                            RecipeId = Convert.ToInt32(reader["Recipe_Id"]),
                            TitleLong = reader["TitleLong"].ToString(),
                            TitleShort = reader["TitleShort"].ToString(),
                            Description = reader["Description"].ToString(),
                            OriginalLink = reader["OriginalLink"].ToString(),
                            LastModifier = reader["LastModifier"].ToString(),
                            AuditDate = Convert.ToDateTime(reader["AuditDate"]),
                            CreationDate = Convert.ToDateTime(reader["CreationDate"])
                        });
                    }
                }
                conn.Close();
            }
            return recipes;
        }
    }
}
