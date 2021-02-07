namespace RecipesApi.Utils
{
    public class ServiceResponse<T>: IServiceResponse
    {
        public bool Success { get; set; } = false;
        public T Content { get; set; }
        public string Message { get; set; }
    }

    public class ServiceResponse: IServiceResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; }
    }

    public interface IServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
