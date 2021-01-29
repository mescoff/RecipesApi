using Microsoft.EntityFrameworkCore;
using System;

namespace RecipesApi.Tests.Services
{
    public static class RecipeServiceTestsHelper
    {
        public static void EnsureCreated(RecipesContext context)
        {
            if (context.Database.EnsureCreated())
            {
                using var viewCommand = context.Database.GetDbConnection().CreateCommand();
                viewCommand.CommandText = @"
                        CREATE VIEW AllResources AS
                        SELECT *
                        FROM Recipes;";
                viewCommand.ExecuteNonQuery();
            }
        }

        public static string GenerateRandomString(this Random rdm, int length, string allowedChars = null)
        {
            allowedChars = allowedChars??  "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            char[] chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[rdm.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }
    }
}
