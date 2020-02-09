using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace RecipesApi.Services
{
    public abstract class EntityService<T> : IEntityService<T> where T:class
    {
        protected readonly RecipesContext _context;
        protected readonly ILogger _logger;
        public EntityService(RecipesContext context, ILogger<EntityService<T>> logger){
            this._context = context;
            this._logger = logger;
        }

        public virtual IEnumerable<T> GetAll()
        {
            return this._context.Set<T>();
        }
        public virtual T GetOne(int id)
        {
            var result = this._context.Set<T>().Find(id);
            return result;
        }
        public virtual bool AddOne(T input)
        {
            var entityToUpdate = this.prepareInputForUpdate(input, true);
            this._context.Set<T>().Add(entityToUpdate);
            var result = this._context.SaveChanges();
            return result == 1;
        }

        public virtual bool UpdateOne(T input)
        {
            //var entityToUpdate = this._context.Set<T>().Find(id);
            //making sure both ids match
            //if (id != entityToUpdate.Unit_Id || unit == input)
            //{
            //    return UnprocessableEntity(input);
            //}
            // bad check but make sure given Id the same in input (find in generic way...)
            var entityToUpdate = this.prepareInputForUpdate(input, false);
            this._context.Set<T>().Update(input);
            var result = this._context.SaveChanges();
            return result == 1;         
        }

        public virtual bool DeleteOne(int id)
        {
            var entityToDelete = this._context.Set<T>().Find(id);
            if (entityToDelete != null)
            {
                this._context.Set<T>().Remove(entityToDelete);
                var result = this._context.SaveChanges();
                return result == 1;
            }
            return false;
        }

        protected abstract T prepareInputForUpdate(T input, bool isCreation);
    }
}
