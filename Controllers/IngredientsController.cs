using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.DTOs;
using RecipesApi.Models;

namespace RecipesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientsController : ControllerBase
    {
        // TODO: Set parent class with base or all actions (same in unit and ingredients)

        private readonly RecipesContext _context;
        private readonly IEntityService<Ingredient> _ingredientsService;
        private readonly IMapper _mapper;

        public IngredientsController(IEntityService<Ingredient> ingredientsService, IMapper mapper)
        {
            this._ingredientsService = ingredientsService;
            this._mapper = mapper;
        }


        /// <summary>
        /// Get Ingredients
        /// </summary>    
        /// <returns></returns>
        // GET: api/Ingredients
        [HttpGet(Name = "GetIngredients")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            //TODO: Get set in more generic way (https://stackoverflow.com/questions/21533506/find-a-specified-generic-dbset-in-a-dbcontext-dynamically-when-i-have-an-entity)
            var ingredients = (this._ingredientsService.GetAll()).ToList();
            if (ingredients.Count == 0)
            {
                return NoContent();
            }
            this.AddCountToHeader(ingredients);
            return Ok(this._mapper.Map<List<Ingredient>, List<IngredientDto>>(ingredients));
        }

        // GET: api/Ingredients/5
        //[HttpGet("{id}", Name = "GetIngredient")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

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
