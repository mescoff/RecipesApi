using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.Models
{
    public interface ICustomModel<T>: IEquatable<T>  where T : class
    {
        int Id { get; set; }
        object this[string propertyName] { get; set; }
    }
}
