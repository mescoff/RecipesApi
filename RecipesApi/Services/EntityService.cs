using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using RecipesApi.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RecipesApi.Services
{
    /// <summary>
    /// Generic entry service
    /// </summary>
    /// <typeparam name="T">The type/object handled by the service</typeparam>
    public abstract class EntityService<T> : IEntityService<T> where T: class, ICustomModel
    {

        protected readonly RecipesContext Context;
        protected readonly DbSet<T> Entities;
        protected readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">The DB context</param>
        /// <param name="logger">The logger</param>
        public EntityService(RecipesContext context, ILogger<EntityService<T>> logger)
        {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
            this.Entities = this.Context.Set<T>() ?? throw new ArgumentNullException(nameof(context));
            this._logger = logger;

            try
            {
                context.Database.EnsureCreated();

            }
            catch (Exception e)
            {
                this._logger.LogError($"Something happened: {e.InnerException}", e.InnerException);
                throw e;
            }
        }

        /// <summary>
        /// Get all object in DB set
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<T> GetAll()
        {
            return this.Entities;
        }

        /// <summary>
        /// Get a single object in DB set
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async virtual Task<T> GetOne(int id)
        {
            var result = await this.Entities.FindAsync(id);
            return result;
        }

        /// <summary>
        /// Add a single object to DB set
        /// </summary>
        /// <param name="input">The input</param>
        /// <returns></returns>
        public async virtual Task<ServiceResponse<T>> AddOne(T input)
        {
            var response = new ServiceResponse<T>();
            try
            {
                this.prepareInputForCreateOrUpdate(input, true);
                await this.Entities.AddAsync(input);
                //this._context.Set<T>().Add(entityToUpdate);
                // TODO: [Response handling]. If duplicate excetion thrown: handle properly and return conflict
                var result = this.Context.SaveChanges();
                _logger.LogInformation($"Entity succesfully saved to DB: Entity={input}");
                // TODO: return created object in future
                if (result > 0)
                {
                    input.Id = input.Id;
                    response.Success = true;
                    response.Message = "Object created";
                    response.Content = input;
                    return response;
                }
                response.Message = "Object could not be created";
            }
            catch(Exception e)
            {             
                this.Entities.Remove(input);
                var errorMessage = $"Error while saving entity. Rolling back changes: Entity={input} ErrorMessage={e.InnerException?.Message}";
                _logger.LogError(errorMessage);
                response.Message = errorMessage;
            }
            return response;
        }

        /// <summary>
        /// Update a single object in DB set
        /// </summary>
        /// <param name="input"></para
        /// <returns></returns>
        public async virtual Task<bool> UpdateOne(T input)
        {
            try
            {
                //var entityToUpdate = await this.Entities.FindAsync(input.Id);
                //if (entityToUpdate == null) { return false; } // TODO: Return meaningful response
                //this.Context.Entry<T>(entityToUpdate).State = EntityState.Detached;
                //this.prepareInputForCreateOrUpdate(input, false);
                //this.Entities.Update(input);
                //var result = this.Context.SaveChanges();

                // TODO: CHECK that removing the prepareInputForCreateOrUpdate doesn't break everything for Recipe Service...
           


                // Find it (by id), go through every nested prop and set the ones that were modified or add new ones.
                // SaveChanges
                // TODO: forgot to Attach... + modify state... OR just call Update(input). See disconnected update in https://code-maze.com/efcore-modifying-data/
                // BUUUUT, with both methods above the whole object will be set as modified even though only a few prop might have been updated.... Better to go the longer way but more precise
                // where we look for object in Entities, By Id and then one by one update what needs to be updated from object received

                // Here we'd do (instead of everything above
                this.Context.Entry(input).State = EntityState.Modified;
                // Then save
                var result = this.Context.SaveChanges();
                return result > 0;
            }
            catch(Exception e)
            {
                this.Context.Entry(input).State = EntityState.Unchanged;
                var errorMessage = $"Error while updating entity. Rolling back changes: Entity={input} ErrorMessage={e.InnerException?.Message}";
                _logger.LogError(errorMessage);
                return false;
            }
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
            var entityToDelete = await this.Entities.FindAsync(id);
            if (entityToDelete != null)
            {
                this.Entities.Remove(entityToDelete);
                var result = this.Context.SaveChanges();
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
        protected abstract void prepareInputForCreateOrUpdate(T input, bool isCreation); // TODO: rename to ValidateInput

        public void Dispose()
        {

        }
    }
}
