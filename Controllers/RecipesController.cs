using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RecipesApi.DTOs;
using System.Linq;
using RecipesApi.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace RecipesApi.Controllers
{
    /// <summary>
    /// Recipes Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IEntityService<RecipeBase> _recipesService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Recipes Controller constructor
        /// </summary>
        /// <param name="recipeService"></param>
        public RecipesController(IEntityService<RecipeBase> recipeService, IMapper mapper)
        {
            this._recipesService = recipeService;
            this._mapper = mapper;
        }

        // GET: api/Recipes
        [HttpGet(Name = "GetRecipes")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll() {
            var recipes = (this._recipesService.GetAll()).ToList(); 
            if (recipes.Count == 0)
            {
                return NoContent();
            }
            this.AddCountToHeader(recipes);
            return Ok(this._mapper.Map<List<RecipeBase>, List<RecipeBaseDto>>(recipes));
        }

        // GET: api/Recipes/5
        [HttpGet("{id}", Name = "GetRecipe")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get(int id)
        {
            var recipe = this._recipesService.GetOne(id);
            if (recipe == null)
            {
                return NoContent();
            }
            return Ok(this._mapper.Map<RecipeBase,RecipeBaseDto>(recipe));
        }

        /// <summary>
        /// Add Recipe
        /// </summary>
        /// <remarks>
        ///  Sample request:
        ///
        ///     POST /Recipe
        ///     {
        ///         "recipe_Id": 0,
        ///         "titleShort": "Guac",
        ///         "titleLong": "Spicy Guacamole",
        ///         "description": "Create an tasty guacamole in 10 min",
        ///         "originalLink": "https://recipesSomething/",
        ///         "lastModifier": "xxx",
        ///         "auditDate": null,
        ///         "creationDate": null
        ///     }
        /// </remarks> 
        /// <param name="input">Recipe</param>
        /// <returns></returns>
        //// POST: api/Recipes
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Post([FromBody] RecipeBaseDto input)
        {
            var recipe = this._mapper.Map<RecipeBaseDto, RecipeBase>(input);
            var isSuccess = this._recipesService.AddOne(recipe);
            if (!isSuccess)
            {
                return UnprocessableEntity(input);
            }
            return Ok();
        }

        //// PUT: api/Recipes/5
        // [HttpPut("{id}")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Put([FromBody] RecipeBaseDto input)
        {
            var recipe = this._mapper.Map<RecipeBaseDto, RecipeBase>(input);
            var isSuccess = this._recipesService.UpdateOne(recipe);
            if (!isSuccess)
            {
                return UnprocessableEntity(input);
            }
            return Ok();

        }

        //// DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Delete(int id)
        {
            var isSuccess = this._recipesService.DeleteOne(id);
            if (!isSuccess)
            {
                // TODO: this should return diff error depending on if couldn't remove or couldn't find
                return UnprocessableEntity(id);
            }
            return Ok();
        }
    }
}
