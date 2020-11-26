namespace RecipesApi.Utils
{
    public static class Utils
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
