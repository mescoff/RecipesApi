﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipesApi.DTOs;
using RecipesApi.Models;

namespace RecipesApi.Controllers
{
    /// <summary>
    /// Ingredients controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientsController : ControllerBase
    {

        private readonly IEntityService<Ingredient> _ingredientsService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Ingredients Controllers
        /// </summary>
        /// <param name="ingredientsService">The ingredients service</param>
        /// <param name="mapper">The mapper</param>
        public IngredientsController(IEntityService<Ingredient> ingredientsService, IMapper mapper)
        {
            this._ingredientsService = ingredientsService;
            this._mapper = mapper;
        }


        /// <summary>
        /// Get Ingredients
        /// </summary>    
        /// <returns></returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/Ingredients
        [HttpGet(Name = "GetIngredients")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            //TODO: Get set in more generic way (https://stackoverflow.com/questions/21533506/find-a-specified-generic-dbset-in-a-dbcontext-dynamically-when-i-have-an-entity)
            var ingredients = this._ingredientsService.GetAll().ToList();
            if (ingredients.Count == 0)
            {
                return NoContent();
            }
            this.AddCountToHeader(ingredients);
            return Ok(this._mapper.Map<List<Ingredient>, List<IngredientDto>>(ingredients));
        }

        /// <summary>
        /// Get 1 ingredient by id
        /// </summary>
        /// <param name="id">Ingredient id</param>
        /// <returns>unit</returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/Units/5
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // GET: api/Ingredients/5
        [HttpGet("{id}", Name = "GetIngredient")]
        public async Task<ActionResult> Get(int id)
        {
            var ingredient = await this._ingredientsService.GetOne(id);
            if (ingredient == null)
            {
                return NoContent();
            }
            return Ok(this._mapper.Map<Ingredient,IngredientDto>(ingredient));
        }

        /// <summary>
        /// Add Ingredient
        /// </summary>
        /// <remarks>
        /// {
        ///     "recipeIng_Id": 0,
        ///     "name": "Olive oil",
        ///     "quantity": 10,
        ///     "recipe_Id": 3,
        ///     "unit_Id": 2
        /// }
        /// </remarks> 
        /// <param name="input">Ingredient</param>
        /// <returns></returns>
        /// <response code="200">Ingredient was created</response>
        /// <response code="422">Input cannot be processed</response> 
        // POST: api/Ingredients
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Post([FromBody] IngredientDto input)
        {
            //TODO: add standard on naming
            var ingredient = this._mapper.Map<IngredientDto, Ingredient>(input);
            var isSuccess = this._ingredientsService.AddOne(ingredient);
            // this._context.Ingredients.Add(ingredient);
            // FIXME: Bug on saving new ingredient..
            // var result = this._context.SaveChanges();
            if (!isSuccess)
            {
                return UnprocessableEntity(input);
            }
            return Ok();
        }

        /// <summary>
        /// Update ingredient
        /// </summary>
        /// <param name="input">Ingredient to update</param>
        /// <returns></returns>
        /// <response code="200">Ingredient was updated</response>
        /// <response code="422">Input cannot be processed</response> 
        // PUT: api/Ingredients/5
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Put([FromBody] IngredientDto input)
        {
            var ingredient = this._mapper.Map<IngredientDto, Ingredient>(input);
            var isSuccess = this._ingredientsService.UpdateOne(ingredient);
            if (!isSuccess)
            {
                return UnprocessableEntity(input);
            }
            return Ok();
        }

        /// <summary>
        /// Delete ingredient
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Ingredient was deleted</response>
        /// <response code="422">Input cannot be processed</response> 
        //// DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var isSuccess = this._ingredientsService.DeleteOne(id);
            if (!isSuccess)
            {
                // TODO: this should return diff error depending on if couldn't remove or couldn't find
                return UnprocessableEntity(id);
            }
            return Ok();
        }
    }
}