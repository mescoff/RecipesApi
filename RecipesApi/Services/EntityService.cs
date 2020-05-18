using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.Services
{
    /// <summary>
    /// Generic entry service
    /// </summary>
    /// <typeparam name="T">The type/object handled by the service</typeparam>
    public abstract class EntityService<T> : IEntityService<T> where T: class, ICustomModel
    {
        protected readonly DbContext _context;
        protected readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">The DB context</param>
        /// <param name="logger">The logger</param>
        public EntityService(DbContext context, ILogger<EntityService<T>> logger)
        {
            this._context = context;
            this._logger = logger;

            try
            {
                context.Database.EnsureCreated();

            }
            catch (Exception e)
            {
                this._logger.LogError("Something happened", e);
                throw e;
            }
        }

        /// <summary>
        /// Get all object in DB set
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<T> GetAll()
        {
            return this._context.Set<T>();
        }

        /// <summary>
        /// Get a single object in DB set
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async virtual Task<T> GetOne(int id)
        {
            var result = await this._context.Set<T>().FindAsync(id);
            return result;
        }

        /// <summary>
        /// Add a single object to DB set
        /// </summary>
        /// <param name="input">The input</param>
        /// <returns></returns>
        public async virtual Task<int> AddOne(T input)
        {
            this.prepareInputForCreateOrUpdate(input, true);
            await this._context.Set<T>().AddAsync(input);
            //this._context.Set<T>().Add(entityToUpdate);
            // TODO: [Response handling]. If duplicate excetion thrown: handle properly and return conflict
            var result = this._context.SaveChanges();
            // TODO: return created object in future
            if (result > 0)
            {
                return input.Id;
            }
            return 0;
        }

        /// <summary>
        /// Update a single object in DB set
        /// </summary>
        /// <param name="input"></para
        /// <returns></returns>
        // TODO: make async
        public async virtual Task<bool> UpdateOne(T input)
        {
            // TODO: return error message for below
            var entityToUpdate = await this._context.Set<T>().FindAsync(input.Id);
            if (entityToUpdate == null) { return false; } // TODO: Return meaningful response
            this._context.Entry<T>(entityToUpdate).State = EntityState.Detached;
            //if ( ! this._context.Set<T>().ToList().Any(r => r.Id == input.Id)) { return false; } // TODO: Return meaningful response
            this.prepareInputForCreateOrUpdate(input, false);
            this._context.Set<T>().Update(input); 
            var result = this._context.SaveChanges();
            return result > 0;         
        }

        /// <summary>
        /// Delete single object in DB set
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async virtual Task<bool> DeleteOne(int id)
        {
            // TODO: Consider disabling Cascade delete on DB side and deleting all depenencies manually
            // to allow logging of potential exceptions on the way
            var entityToDelete = await this._context.Set<T>().FindAsync(id);
            if (entityToDelete != null)
            {
                this._context.Set<T>().Remove(entityToDelete);
                var result = this._context.SaveChanges();
                return result > 0;
            }
            return false;
        }

        /// <summary>
        /// Prepare/Cleanup input before sending it to DB
        /// </summary>
        /// <param name="input">The input</param>
        /// <param name="isCreation">Is it a creation or update</param>
        /// <returns></returns>
        protected abstract void prepareInputForCreateOrUpdate(T input, bool isCreation);

        public void Dispose()
        {

        }
    }
}
