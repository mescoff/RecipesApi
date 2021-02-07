namespace RecipesApi.Models
{
    public interface IMedia //: ICustomModel<IMedia>
    {
        int Id { get; set; }
        object this[string propertyName] { get; set; }
        int Recipe_Id { get; set; }
        int RecipeInst_Id { get; set; }
        string Tag { get; set; }
        string Title { get; set; }
    }
}