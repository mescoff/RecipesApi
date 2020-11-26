namespace RecipesApi.Utils
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; } = false;
        public T Content { get; set; }
        public string Message { get; set; }
    }
}
