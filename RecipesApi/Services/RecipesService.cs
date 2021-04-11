using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecipesApi.DTOs;
using RecipesApi.DTOs.Recipes;
using RecipesApi.Models;
using RecipesApi.Services;
using RecipesApi.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi
{
    /// <summary>
    /// Recipe service
    /// Handles relaying requests between controller and DB + perform logic when necessary
    /// </summary>
    /// Recipe Service needs its own logic. Removing EntityService
    public class RecipesService : IRecipesService<RecipeDto>
    {
        protected readonly RecipesContext Context;
        protected readonly IMediaLogicHelper MediaHelper;
        protected readonly DbSet<Recipe> Entities;
        protected readonly ILogger _logger;
        protected readonly IMapper _mapper;

        public RecipesService(RecipesContext context, ILogger<RecipesService> logger, IMediaLogicHelper mediaHelper, IMapper mapper)
        {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
            this.MediaHelper = mediaHelper;
            this.Entities = this.Context.Set<Recipe>() ?? throw new ArgumentNullException(nameof(context));
            this._logger = logger;
            this._mapper = mapper;

            try
            {
                context.Database.EnsureCreated();

            }
            catch (Exception e)
            {
                this._logger.LogError($"Something happened: {e.InnerException}", e);
                throw e;
            }
        }

        // TODO: This should be paged
        public IEnumerable<RecipeDto> GetAll()
        {
            // TODO: Use of RecipeDto will break for Media. Fix it
            var recipes = this.Entities.Include(r => r.Medias).Include(r => r.RecipeCategories).ThenInclude(i => i.Category).Include(r => r.Instructions).Include(r => r.Ingredients).ThenInclude(i => i.Unit);
            var recipeDtos = new List<RecipeDto>();
            foreach (var recipe in recipes)
            {
                var recipeDto = ConvertRecipeToDto(recipe);
                recipeDtos.Add(recipeDto);
            }
            return recipeDtos;
        }

        public async Task<RecipeDto> GetOne(int id)
        {
            var recipe = await this.GetRecipeFromDb(id);
            var recipeDto = ConvertRecipeToDto(recipe);
            return recipeDto;
        }

        private async Task<Recipe> GetRecipeFromDb(int id)
        {
            var recipe = await this.Entities.Include(r => r.Medias).Include(r => r.RecipeCategories).ThenInclude(i => i.Category).Include(r => r.Instructions).Include(r => r.Ingredients).ThenInclude(i => i.Unit).AsNoTracking().SingleOrDefaultAsync(r => r.Id == id);
            return recipe;
        }

        public async Task<ServiceResponse<RecipeDto>> AddOne(RecipeDto input)
        {
            var response = new ServiceResponse<RecipeDto>();
            Recipe recipeToSave = null;
            try
            {
                recipeToSave = PrepareRecipeDtoForDatabase(input);
                await this.Entities.AddAsync(recipeToSave); // NO
                var result = this.Context.SaveChanges();
                _logger.LogInformation($"Entity succesfully saved to DB: Id:{recipeToSave.Id}");
                // TODO: return created object in future
                if (result > 0)
                {
                    response.Success = true;
                    response.Message = $"Object created. Id:{recipeToSave.Id}";
                    response.Content = input;
                    return response;
                }
                response.Message = "Object could not be created";
            }
            catch (Exception e)
            {
                this.Entities.Remove(recipeToSave);
                var errorMessage = $"Error while saving entity. Rolling back changes: ENTITYTYPE:[{input.GetType().Name}] ENTITY:[{input}] ERROR:[{e}]";
                _logger.LogError(errorMessage);
                response.Message = errorMessage;
            }
            return response;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Update a single recipe
        /// </summary>
        /// <param name="updatedRecipeDto">The recipe with updates</param>
        /// <returns></returns>
        public async Task<bool> UpdateOne(RecipeDto updatedRecipeDto)
        {
            // TODO: AsNoTracking is no longer needed. We changed default behavior to NoTracking in RecipeContext
            //var dbRecipe = this.Entities.Include(r => r.Media).Include(r => r.Ingredients).AsNoTracking().FirstOrDefault(r => r.Id == recipe.Id); // TODO: make async
            //var dbRecipe = await this.GetOne(updatedRecipe.Id);
            var dbRecipe = await this.GetRecipeFromDb(updatedRecipeDto.Id);

            //// Convert updated recipeDto to (DB)recipe, !!! only MEDIAS will be ommited as they need to be handled separately
            var updatedRecipe = this.ConvertDtoToRecipe(updatedRecipeDto);
       

            int result = 0;
            if (dbRecipe != null)
            {
                // Disconnected scenario                  
                try
                {
                    //this.Entities.Attach(recipe);           

                    // Update Ingredients
                    ScanAndApplyChanges<Ingredient>(updatedRecipe.Ingredients, dbRecipe.Ingredients);

                    // Update Instrutions
                    ScanAndApplyChanges<Instruction>(updatedRecipe.Instructions, dbRecipe.Instructions);

                    // Update Media
                    // Get DB Medias that match recipe ID, we need medias not mediaDtos for Existing medias
                    ScanForImageUpdates(updatedRecipeDto.Medias, dbRecipe.Medias);

                    // Update recipe itself
                    // TODO: (1) test with just update instead of below. (2) check if we shouldn't do below when updating related entities (rather than using update)
                    // TODO: !!! this always overrides Recipe props. Check if it's different first
                    this.Context.Entry(dbRecipe).CurrentValues.SetValues(updatedRecipeDto);  // setting all values
                    this.Context.Entry(dbRecipe).State = EntityState.Modified;

                    // UPDATE REGULAR PROPS: Go through each --matching-- properties between the 2 objects (media vs mediaDto)
                    // Using reflection here to not have to update this code if we add more properties in the future (that are easy to handle)
                    //var receivedRecipeProps = receivedMedia.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0); // Removing Indexed Property (Item) to prevent circular get
                    // TODO: Implement
                    //foreach (var prop in receivedMediaProps)
                    //{
                    //    object mediaVal;
                    //    try
                    //    {
                    //        mediaVal = media[prop.Name];
                    //    }
                    //    catch (NullReferenceException e)
                    //    {
                    //        // Expected. Property doesn't exist on Media. Skip check
                    //        continue;
                    //    }
                    //    var mediaDtoVal = receivedMedia[prop.Name];
                    //    // IF property existed on both objects, and value differ. Override dbMedia with updated value
                    //    // Have to be repetitive here because we're using "objects" and can't use !=. But Equals won't work on nullref objects
                    //    // If prop is null on one of the media/mediaDto but not both, update
                    //    if ((mediaVal == null ^ mediaDtoVal == null))
                    //    {
                    //        media[prop.Name] = mediaDtoVal;
                    //        this.Context.Entry(media).Property(prop.Name).IsModified = true;
                    //    }
                    //    else if (!(mediaVal == null && mediaDtoVal == null) && !mediaVal.Equals(mediaDtoVal))
                    //    {
                    //        media[prop.Name] = mediaDtoVal;
                    //        this.Context.Entry(media).Property(prop.Name).IsModified = true;
                    //    }
                    //}

                    // TODO: finish below
                    //var receivedRecipeProps = receivedMedia.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0); // Removing Indexed Property (Item) to prevent circular get
                    //foreach (var prop in receivedMediaProps)
                    //{

                    var entries = this.Context.ChangeTracker.Entries(); // FOR DEBUGGING (add to watch to see what changes were detectdd on context side)

                    result = this.Context.SaveChanges();
                }
                catch (Exception e)
                {
                    //this.Context.Entry(dbRecipe).State = EntityState.Unchanged;
                    var errorMessage = $"Error while updating entity. Rolling back changes: Entity={JsonConvert.SerializeObject(dbRecipe)} ErrorMessage ={JsonConvert.SerializeObject(e)}";
                    _logger.LogError(errorMessage);
                    return false;
                }

                return result > 0;
            }
            return false;
        }


        /// <summary>
        /// Delete single recipe by Id
        /// </summary>
        /// <param name="id">Recipe Id</param>
        /// <returns>Status of action performed</returns>
        public async Task<ServiceResponse> DeleteOne(int id)
        {
            //TODO: verify if need GetOne here (if not, get rid of dbmediasToDelete)
            //var dbRecipeDto = await this.GetOne(id);
            var dbRecipe = await this.GetRecipeFromDb(id);
            if (dbRecipe != null)
            {
                // DELETE Medias

                // get dbMedias (with actual paths) from DB
                //var recipeMediasId = dbRecipe.Medias.Select(m => m.Id);
                //if (recipeDtoMediasId.Any())
                //{
                //var dbMediasToDelete = this.Context.Set<Media>().Where(m => recipeDtoMediasId.Contains(m.Id));
                var deleteStatus = HandleDeletingPhysicalMedia(dbRecipe.Medias);
                if (deleteStatus.Success == false)
                {
                    // TODO: return full message on what happened
                    return deleteStatus;
                }
                //}

                // Turn RecipeDto to Recipe (with medias ignored since already handled separately) & delete with standard method
                //var recipe = this.ConvertDtoToRecipe(dbRecipeDto);

                // TODO: verify cascade delete works here             
                this.Entities.Remove(dbRecipe);
                return new ServiceResponse { Success = true };
            }
            return new ServiceResponse { Message = $"Recipe [ID:{id}] does not exist" };
        }

        /// <summary>
        /// Cleanse the object of any unwanted parameters before pushing to DB
        /// </summary>
        /// <param name="recipe">The recipe</param>
        /// <returns></returns>
        protected void prepareInputForCreateOrUpdate(RecipeDto input, bool isCreation)
        {

            // TODO: separate these back into post/put 
            if (isCreation)
            {
                //// CREATION
                // Make sure Id is 0 -- DB generates it
                input.Id = 0;

                if (input.Ingredients != null)
                {
                    foreach (var ingredient in input.Ingredients)
                    {
                        // clear Unid, recipeId and Id to be safe
                        ingredient.Id = 0;
                        ingredient.Recipe_Id = 0;
                    }
                }
                if (input.Medias != null)
                {
                    foreach (var media in input.Medias)
                    {
                        media.Id = 0;
                        media.Recipe_Id = 0;
                    }
                }

                // TODO: keep reseting other nested data here

            }
            else
            {
                //// UPDATE
                this._logger.LogInformation($"Updating Recipe w/ ID: {input.Id}");
                //input.AuditDate = DateTime.Now;
                // getting read only value of this recipe (thus the no tracking)
                // TODO: AsNoTracking is no longer needed. We changed default behavior to NoTracking in RecipeContext
                var dbRecipe = this.Entities.Include(r => r.Medias).Include(r => r.Ingredients).AsNoTracking().FirstOrDefault(r => r.Id == input.Id); // TODO: make async

                ///// INGREDIENTS
                // Check for possible duplicate ingredients  
                var updatedRecipeIngredients = new HashSet<int>(input.Ingredients.Select(i => i.Id));
                if (updatedRecipeIngredients.Count() < input.Ingredients.Count())
                {
                    throw new DuplicateNameException($"You provided a recipe with duplicate ingredients ID"); // TODO: pick up throw in nicer way 
                }

                //// Clear unit object if it was provided so it doesn't create new one TODO: handle this with AutoMapper ? (if separate RecipeDto)
                //foreach (var ingredient in input.Ingredients)
                //{
                //    ingredient.Unit = null;
                //}

                // Check for removed ingredients and delete them
                var dbRecipeIngredients = new HashSet<int>(dbRecipe.Ingredients.Select(i => i.Id));
                var ingMissingInUpdatedRecipe = dbRecipeIngredients.Where(i => !updatedRecipeIngredients.Contains(i)); // list of ingredients IDs missing from updated Recipe and need to remove
                foreach (var id in ingMissingInUpdatedRecipe)
                {
                    var dbIng = this.Context.Set<Ingredient>().Find(id); // there should not be any doubt that it's there
                    this.Context.Set<Ingredient>().Remove(dbIng);
                    this._logger.LogInformation($"Deleting Ingredient with Id:{dbIng.Id} and Name:{dbIng.Name} on Recipe ID: {input.Id}");
                }

                ///// MEDIA
                // Check for possible duplicate media  
                var updatedRecipeMedia = new HashSet<int>(input.Medias.Select(i => i.Id));
                if (updatedRecipeMedia.Count() < input.Medias.Count())
                {
                    throw new DuplicateNameException($"You provided a recipe with duplicate media ID"); // TODO: pick up throw in nicer way 
                }

                // Check for removed media and delete them
                var dbRecipeMedias = dbRecipe.Medias == null ? null : new HashSet<int>(dbRecipe.Medias.Select(i => i.Id));
                var ingMissingInUpdatedMedia = updatedRecipeMedia == null ? dbRecipeMedias : dbRecipeMedias.Where(i => !updatedRecipeMedia.Contains(i)); // list of ingredients IDs missing from updated Recipe and need to remove
                foreach (var id in ingMissingInUpdatedMedia)
                {
                    // TODO: Here use remove range: context.Children.RemoveRange(parent.Children)
                    // to avoid slow performance because of tracking
                    var dbMedia = this.Context.Set<Media>().Find(id); // there should not be any doubt that it's there
                    this.Context.Set<Media>().Remove(dbMedia);
                    this._logger.LogInformation($"Deleting Ingredient with Id:{dbMedia.Id} and Name:{dbMedia.Title} on Recipe ID: {input.Id}");
                }
            }
        }


        #region Update Related Entities

        /// <summary>
        /// Scan received recipe's related entities/children for additions, removals and updates. And apply them
        /// CANNOT BE USED WITH MEDIAS
        /// </summary>
        /// <typeparam name="TBase">The Base type for that model (aka: IngredientBase/InstructionBase) </typeparam>
        /// <typeparam name="TEntity">The Context Entity type for that model  (aka: Ingredient/Instruction)</typeparam>
        /// <param name="updatedRecipeItems">The list of related entities on Received/Updated recipe</param>
        /// <param name="existingRecipeItems">The list of related entities on matching recipe in DB</param>
        //private void ScanAndApplyChanges<TBase,TEntity>(IList<TBase> updatedRecipeItems, IList<TBase> existingRecipeItems) where TBase : class, ICustomModel<TEntity>, new()
                                                                                                                           //where TEntity : class, new()

        private void ScanAndApplyChanges<TEntity>(IList<TEntity> updatedRecipeItems, IList<TEntity> existingRecipeItems) where TEntity : class, ICustomModel<TEntity>, new()
        {
            if (!existingRecipeItems.Any() && !updatedRecipeItems.Any()) { return; }
            var existingRecipeItemsIds = existingRecipeItems.Select(i => i.Id);  // items ids from DB recipe
            var updatedRecipeItemsIds = updatedRecipeItems.Select(i => i.Id);      // items ids from updated recipe


            // ADD ALL items if DB recipe doesn't have any yet and we receive some to add
            if (!existingRecipeItemsIds.Any() && updatedRecipeItems.Any())
            {
                // reset Ids to ensure DB will apply its own
                foreach (var item in updatedRecipeItems)
                {
                    item.Id = 0;
                }
                Context.AddRange(updatedRecipeItems);
            }
            else
            {
                var itemsIdsToDelete = existingRecipeItemsIds.Except(updatedRecipeItemsIds).ToList();  // get items Ids in existing recipe missing from updated recipe
                var itemsIdsToAdd = updatedRecipeItemsIds.Except(existingRecipeItemsIds).ToList();  // get items Ids in updated recipe missing from existing recipe
                var itemsIdsInCommon = existingRecipeItemsIds.Intersect(updatedRecipeItemsIds).ToList(); // get items IDs that are common to DB recipe and update. Freezing with toList()

                // ADD items not present in DB recipe
                // Looking for all additional items from what's in DB (rather than all with ID = 0), to cover cases where ID isn't 0 by mistake
                if (itemsIdsToAdd.Count() > 0)
                {
                    var itemsToAdd = updatedRecipeItems.Where(i => itemsIdsToAdd.Contains(i.Id)).ToList();
                    // reset Ids to ensure DB will apply its own
                    itemsToAdd.ForEach(i => i.Id = 0);
                    //foreach (var item in itemsToAdd)
                    //{
                    //    item.Id = 0;
                    //}

                    // Map Base type to Entity type.
                    // TODO: waste of time if no base type...
                    //var itemsToAddConverted = this._mapper.Map<IList<TBase>, IList<TEntity>>(itemsToAdd);
                    this.Context.Set<TEntity>().AddRange(itemsToAdd);
                }

                // DELETE items no longer in updated recipe
                if (itemsIdsToDelete.Count() > 0)
                {
                    // create new list of items with just Id and delete (rather than using item from DB Recipe that is tracked)
                    // TODO: !!!! waste of time, we could directly convert to TEntity, but it doesn't have Id because doesn't implement ICustomModel
                    var itemsToDelete = existingRecipeItems.Where(i => itemsIdsToDelete.Contains(i.Id)).Select(i => new TEntity { Id = i.Id });
                    // Map Base type to Entity type.        
                    //var itemsToDeleteConverted = this._mapper.Map<IEnumerable<TBase>, IEnumerable<TEntity>>(itemsToDelete);
                    this.Context.Set<TEntity>().RemoveRange(itemsToDelete);
                }

                // UPDATE matching items (between Existing and Updated recipe) if difference in properties is registered                       
                // and retrieve actual item from existing items
                var itemsInCommon = existingRecipeItems.Where(i => itemsIdsInCommon.Contains(i.Id));

                // Run through items on updated recipe and DB recipe, If they contain the same info skip. Otherwise update
                foreach (var existingItem in itemsInCommon)
                {
                    // locate item with same ID in updated recipe
                    var receivedItem = updatedRecipeItems.SingleOrDefault(i => i.Id == existingItem.Id);
                    if (receivedItem != null && !existingItem.Equals(receivedItem))
                    {
                        // TODO: GOOD TO KNOW: here we can use a DTO that's not translated if needed in update, as long as updated object has same properties as existing
                        //this.Context.Entry(existingIng).CurrentValues.SetValues(receivedIng);  // TODO: BUG values are not set to modified 
                        // var x = this.Context.Entry(existingIng).State;
                        //var entries = this.Context.ChangeTracker.Entries(); // FOR DEBUGGING (add to watch to see what changes were detectdd on context side)


                        // Map Base type to Entity type.        
                        //var receivedItemConverted = this._mapper.Map<TBase, TEntity>(receivedItem);
      
                        // We  entire object. It's easier for now
                        this.Context.Set<TEntity>().Update(receivedItem); // TODO: go through each prop like in Media Update to only update prop necessary
                    }
                }

            }
        }

        /// <summary>
        /// Scan received Recipe's MEDIAS for additions, removals and updates. And apply them
        /// </summary>
        /// <param name="updatedRecipeMediasDtos">The list of medias (Dtos) on received recipe</param>
        /// <param name="existingRecipeMedias">The list of medias on matching recipe in DB</param>
        private void ScanForImageUpdates(IEnumerable<MediaDto> updatedRecipeMediasDtos, IEnumerable<Media> existingRecipeMedias)
        {
            if (!updatedRecipeMediasDtos.Any() && !existingRecipeMedias.Any()) { return; }

            var existingRecipeMediasIds = existingRecipeMedias.Select(i => i.Id);  // items ids from DB recipe
            var updatedRecipeMediasIds = updatedRecipeMediasDtos.Select(i => i.Id);      // items ids from updated recipe

            // ADD ALL items if DB recipe doesn't have any yet and we receive some to add
            if (!existingRecipeMediasIds.Any() && updatedRecipeMediasIds.Any())
            {
                var response = HandleSavingNewMediaDtos(updatedRecipeMediasDtos);
            }
            else
            {
                var mediasIdsToDelete = existingRecipeMediasIds.Except(updatedRecipeMediasIds).ToList();  // get items Ids in existing recipe missing from updated recipe
                var mediasIdsToAdd = updatedRecipeMediasIds.Except(existingRecipeMediasIds).ToList();  // get items Ids in updated recipe missing from existing recipe
                var mediasIdsInCommon = existingRecipeMediasIds.Intersect(updatedRecipeMediasIds).ToList(); // get items IDs that are common to DB recipe and update. Freezing with toList()

                // ADD medias not present in DB recipe
                // Looking for all additional medias from what's in DB (rather than all with ID = 0), to cover cases where ID isn't 0 by mistake
                if (mediasIdsToAdd.Count() > 0)
                {
                    var mediasToAdd = updatedRecipeMediasDtos.Where(i => mediasIdsToAdd.Contains(i.Id));
                    // reset Ids to ensure DB will apply its own
                    foreach (var item in mediasToAdd)
                    {
                        item.Id = 0;
                    }
                    var response = HandleSavingNewMediaDtos(mediasToAdd);
                }

                // DELETE medias no longer in updated recipe
                if (mediasIdsToDelete.Count() > 0)
                {
                    // Get medias from DB (medias with actual paths) that need to be deleted
                    var dbMediasToPhysicallyDelete = existingRecipeMedias.Where(m => mediasIdsToDelete.Contains(m.Id));
                    var deleteStatus = HandleDeletingPhysicalMedia(dbMediasToPhysicallyDelete);
                    // Then delete medias from DB
                    // !!! create new list of medias with just Id and delete (to avoid deleting entity with tracked dependencies -- aka children with loop ref to Recipe or Unit)
                    var dbMediasToDelete = dbMediasToPhysicallyDelete.Select(i => new Media { Id = i.Id, Recipe_Id = i.Id });
                    this.Context.Set<Media>().RemoveRange(dbMediasToDelete);
                }

                // UPDATE matching medias (between Existing and Updated recipe) if difference in properties is registered          
                // Retrieve received Media(Dto) that match db Media (that has path). Don't keep related entities. Freezeresult with toList()
                var mediasToUpdate = existingRecipeMedias.Where(m => mediasIdsInCommon.Contains(m.Id)).ToList();
                // Run through items on updated recipe and DB recipe, If they contain the same info skip. Otherwise update
                foreach (var media in mediasToUpdate)
                {
                    // locate item with same ID in updated recipe
                    var receivedMedia = updatedRecipeMediasDtos.SingleOrDefault(i => i.Id == media.Id);
                    if (receivedMedia != null)
                    {
                        // Remove related entities from Media to avoid self referencing loop
                        media.Recipe = null;
                        // Attach media to start tracking updates/modif
                        this.Context.Set<Media>().Attach(media);

                        // UPDATE REGULAR PROPS: Go through each --matching-- properties between the 2 objects (media vs mediaDto)
                        // Using reflection here to not have to update this code if we add more properties in the future (that are easy to handle)
                        var receivedMediaProps = receivedMedia.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0).ToList(); // Removing Indexed Property (Item) to prevent circular get
                        foreach (var prop in receivedMediaProps)
                        {
                            object mediaVal;
                            try
                            {
                                mediaVal = media[prop.Name];
                            }
                            catch (NullReferenceException e)
                            {
                                // Expected. Property doesn't exist on Media. Skip check
                                continue;
                            }
                            var mediaDtoVal = receivedMedia[prop.Name];
                            // IF property existed on both objects, and value differ. Override dbMedia with updated value
                            // Have to be repetitive here because we're using "objects" and can't use !=. But Equals won't work on nullref objects
                            // If prop is null on one of the media/mediaDto but not both, update
                            if ((mediaVal == null ^ mediaDtoVal == null))
                            {
                                media[prop.Name] = mediaDtoVal;
                                this.Context.Entry(media).Property(prop.Name).IsModified = true;
                            }
                            else if (!(mediaVal == null && mediaDtoVal == null) && !mediaVal.Equals(mediaDtoVal))
                            {
                                media[prop.Name] = mediaDtoVal;
                                this.Context.Entry(media).Property(prop.Name).IsModified = true;
                            }
                        }

                        // UPDATE IMAGE PROP: Now handle bytes (mediaDto) vs image path (db media). If they differ, replace with content from received bytes
                        // STEP 1 - Load media from local storage into mediaDto  [quicker than duplicating code to load path into Image]
                        var mediaDto = MediaHelper.LocateAndLoadMedias(new List<Media> { media }).SingleOrDefault();
                        // STEP 2 - Compare bytes from received media and saved media
                        if (!mediaDto.MediaBytes.SequenceEqual(receivedMedia.MediaBytes))
                        {
                            // TODO: LOG
                            // I prefer to delete media entirely and save new one, in case Media path changed at application level (so override will not work and new image would be saved in new location without proper cleanup)
                            // Delete media from storage
                            File.Delete(media.MediaPath);
                            ServiceResponse savingResult; // TODO: do something with the response
                                                          // Save new media (will use same path)
                            var mediasWithUpdatedImagePath = MediaHelper.SaveImagesLocally(new List<MediaDto> { receivedMedia }, out savingResult);
                            var mediaWithUpdatedImagePath = mediasWithUpdatedImagePath.SingleOrDefault();

                            // override our DB media path value 
                            if (mediaWithUpdatedImagePath != null)
                            {
                                this._logger.LogInformation($"Media [ID:{media.Id}] - New image provided. Deleted image at path:[{media.MediaPath}]. Saved new image at path:[{mediaWithUpdatedImagePath.MediaPath}]");
                                media.MediaPath = mediaWithUpdatedImagePath.MediaPath;
                            }

                        }
                
                        //// STEP 3 - Attach media for tracking (automatically identify updated properties)
                        //this.Context.Set<Media>().Attach(media); // Now updates done to it will be registed by Context and saved on SaveChanges()
                        //this.Context.Entity(media).State = En
                        // TODO: Add logging


                        // TODO: TEST UPDATE / DELETE / ADD when media already exist in recipe


                    }
                }

            }
        }
        #endregion

        #region Helper methods

        //private void UpdateModifiedProperties(object entityFromClient, object entityFromDB)
        //{
        //    var receivedMediaProps = entityFromClient.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0).ToList(); // Removing Indexed Property (Item) to prevent circular get
        //    foreach (var prop in receivedMediaProps)
        //    {
        //        object mediaVal;
        //        try
        //        {
        //            mediaVal = entityFromDB[prop.Name];
        //        }
        //        catch (NullReferenceException e)
        //        {
        //            // Expected. Property doesn't exist on Media. Skip check
        //            continue;
        //        }
        //        var mediaDtoVal = entityFromClient[prop.Name];
        //        // IF property existed on both objects, and value differ. Override dbMedia with updated value
        //        // Have to be repetitive here because we're using "objects" and can't use !=. But Equals won't work on nullref objects
        //        // If prop is null on one of the media/mediaDto but not both, update
        //        if ((mediaVal == null ^ mediaDtoVal == null))
        //        {
        //            entityFromDB[prop.Name] = mediaDtoVal;
        //            this.Context.Entry(entityFromDB).Property(prop.Name).IsModified = true;
        //        }
        //        else if (!(mediaVal == null && mediaDtoVal == null) && !mediaVal.Equals(mediaDtoVal))
        //        {
        //            entityFromDB[prop.Name] = mediaDtoVal;
        //            this.Context.Entry(entityFromDB).Property(prop.Name).IsModified = true;
        //        }
        //    }
        //}

        private ServiceResponse HandleDeletingPhysicalMedia(IEnumerable<Media> mediasToDelete)
        {
            if (mediasToDelete == null || !mediasToDelete.Any()) { return new ServiceResponse { Success = true, Message = "No Media to Delete" }; }
            this._logger.LogInformation($"About to DELETE {mediasToDelete.Count()} medias for Recipe [Id:{mediasToDelete.First().Id}]");
            // Delete all medias at path
            var deleteStatus = new ServiceResponse { Success = true };
            string msg = "";
            foreach (var media in mediasToDelete)
            {
                if (File.Exists(media.MediaPath))
                {
                    try
                    {
                        File.Delete(media.MediaPath);
                        this._logger.LogInformation($"DELETED Media on local storage [ID:{media.Id}, PATH:{media.MediaPath}]");
                    }
                    catch (Exception e)
                    {
                        msg = $"Something happened when trying to DELETE Media on local storage [ID:{media.Id}, PATH:{media.MediaPath}]";
                        this._logger.LogError(msg + ", Error:", e);
                        deleteStatus.Success = false;
                        // I decide to return by default and prevent further deletion. Behavior to rethink in future
                        deleteStatus.Message = deleteStatus.Message == null ? msg : deleteStatus.Message.Concat(msg).ToString();
                        return deleteStatus;
                    }
                    // TODO: Make sure file no longer exist?
                }
                // File was not found. Specify it.
                msg = $"The following file does not exist [ID:{media.Id}, PATH:{media.MediaPath}]. Will simply remove from DB";
                this._logger.LogWarning(msg);
            }
            return deleteStatus;
        }

        private ServiceResponse HandleSavingNewMediaDtos(IEnumerable<MediaDto> mediasToSave)
        {
            if (mediasToSave == null || !mediasToSave.Any()) { return new ServiceResponse { Success = true, Message = "No Media to Save" }; }
            // Keep a reference of MediaDto and its affiliated Media to save the path to the correct Media instance after IDs have been assigned by DB to Media
            // (since there is no other way to keep track until then: without IDs multiple mediaDto/media could have no way of distinguishing them)
            // The pair will be saved to an array of length 2 with [0] = MediaDto and [1] = Media

            //List<(MediaDto Dto, Media Media)> keepInOrder = new List<(MediaDto, Media)>();
            var mediasPairTracker = new List<IMedia[]>();  // cannot use tuple since items will be immutable
            // TODO: consider setting signature to IList rather than IEnum
            mediasToSave = mediasToSave.ToList(); // "freeze" to list to prevent mediaDtos address to be reassigned when assigning ID => resulting in empty mediasToSave
            foreach (var mediaDto in mediasToSave)
            {
                // Step 1 - Create "dummy" media (copy of dto) with empty path
                var media = new Media { Id = 0, MediaPath = String.Empty, Recipe_Id = mediaDto.Recipe_Id, Tag = mediaDto.Tag, Title = mediaDto.Title };
                //var dto = MediaDto.Copy(mediaDto);
                mediasPairTracker.Add(new IMedia[2] { mediaDto, media });
            }

            // Step 2 - Add new medias without paths to Recipe and save to DB to get IDs assigned
            var medias = mediasPairTracker.Select(pair => pair[1]).Cast<Media>().ToList();
            this.Context.Set<Media>().AddRange(medias);
            // TODO: Add try and catch here and below. Add logs
            this.Context.SaveChanges();

            // Step 3 - Assign Id from media to matching mediaDto
            // [No error handling] All media should have Ids, otherwise exception would have been thrown earlier
            mediasPairTracker.ForEach(pair => pair[0].Id = pair[1].Id);   // TODO: Understand why after this line mediasToSave becomes empty when saving one new media      

            // Step 4 - Pass mediaDtos to the helper and save them to local storage
            ServiceResponse savingResult;
            // Assigning mediaDto ID above reassigned object pointer ? mediasToSave[] is now empty and needs to be reset
            //mediasToSave = mediasPairTracker.Select(pair => pair[0]).Cast<MediaDto>().ToList();
            // TODO: cleaner method could be to pass medias too rather than just DTO? 
            var resultMedias = this.MediaHelper.SaveImagesLocally(mediasToSave, out savingResult);
            // Medias are still being tracked from earlier Save to DB, we can simply update path for each and save changes
            medias.ForEach(m => m.MediaPath = resultMedias.Where(x => x.Id == m.Id).First().MediaPath);   // TODO: add error check? If matching media isn't present from failure to save?
            this.Context.SaveChanges();

            return savingResult;
        }

        /// <summary>
        /// Convert Recipe (DB) to Recipe DTO
        /// Retrieves Media from storage and converst them to bytes for DTO
        /// </summary>
        /// <param name="recipe">DB Recipe</param>
        /// <returns>Recipe DTO</returns>
        // TODO: do this with AutoMapper? -- I prefer having logic here though rather than in separate automapper dedicated logic class
        private RecipeDto ConvertRecipeToDto(Recipe recipe)
        {
            var recipeMedias = this.MediaHelper.LocateAndLoadMedias(recipe.Medias);
            return new RecipeDto
            {
                Id = recipe.Id,
                TitleShort = recipe.TitleShort,
                TitleLong = recipe.TitleLong,
                Description = recipe.Description,
                OriginalLink = recipe.OriginalLink,
                LastModifier = recipe.LastModifier,
                AuditDate = recipe.AuditDate,
                CreationDate = recipe.CreationDate,
                Ingredients = this._mapper.Map<IList<Ingredient>, IList<IngredientBase>>(recipe.Ingredients),
                Instructions = recipe.Instructions,
                RecipeCategories = recipe.RecipeCategories,
                Medias = recipeMedias,
            };
        }

        private Recipe ConvertDtoToRecipe(RecipeDto dto)
        {
            // Medias remain null in this case because they will usually be handled separately
            // TODO: BUT Consider adding Medias here?
            var recipe = new Recipe
            {
                Id = dto.Id,
                TitleShort = dto.TitleShort,
                TitleLong = dto.TitleLong,
                Description = dto.Description,
                OriginalLink = dto.OriginalLink,
                LastModifier = dto.LastModifier,
                AuditDate = dto.AuditDate,
                CreationDate = dto.CreationDate,
                Ingredients = this._mapper.Map<IList<IngredientBase>, IList<Ingredient>>(dto.Ingredients),
                Instructions = dto.Instructions,
                RecipeCategories = dto.RecipeCategories,
                Medias = null
            };
            return recipe;
        }

        /// <summary>
        /// Prepares a recipe DTO to be saved into DB, by saving Media separately and returning a recipe entity (with rest of related entities)
        /// ready to be saved/
        /// </summary>
        /// <param name="dto">The recipe received from front end</param>
        /// <returns>The recipe to be saved into DB</returns>
        private Recipe PrepareRecipeDtoForDatabase(RecipeDto dto)
        {
            // save media separately to DB / local storage
            // TODO: do something with response. If anything bad happened?
            var response = this.HandleSavingNewMediaDtos(dto.Medias);

            // Resetting IDs of related entities to 0 in case they're not (to avoid unexpected conflicts/overriding)
            dto.Ingredients.ToList().ForEach(i => i.Id = 0);
            dto.Instructions.ToList().ForEach(i => i.Id = 0);
            dto.RecipeCategories.ToList().ForEach(c => c.Id = 0);
            // Convert recipe with rest of properties to save automatically in DB - Ignore medias
            var recipe = new Recipe
            {
                Id = 0,
                TitleShort = dto.TitleShort,
                TitleLong = dto.TitleLong,
                Description = dto.Description,
                OriginalLink = dto.OriginalLink,
                LastModifier = dto.LastModifier,
                AuditDate = dto.AuditDate,
                CreationDate = dto.CreationDate,
                Ingredients = this._mapper.Map<IList<IngredientBase>, IList<Ingredient>>(dto.Ingredients),
                Instructions = dto.Instructions,
                RecipeCategories = dto.RecipeCategories,
            };
            return recipe;
        }

        #endregion

        public void Dispose()
        {

        }
    }
}
