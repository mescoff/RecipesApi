using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.Utils
{
    public static class Functions
    {
        //public static void AddCountToHeader(IEnumerable<object> result)
        //{

        //}

        public static string FirstCharToUpper(string input)
        {
            var temp = input.ToCharArray();
            temp[0] = char.ToUpper(temp[0]);
            var result = new string(temp);
            return result;
        }
    }
}
