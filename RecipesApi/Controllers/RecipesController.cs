using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using RecipesApi.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using RecipesApi.DTOs.Recipes;
using System.Collections.Generic;
using RecipesApi.Validate;

namespace RecipesApi.Controllers
{
    /// <summary>
    /// Recipes Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    //[ValidateModel]
    public class RecipesController : ControllerBase
    {
        private readonly IEntityService<Recipe> _recipesService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Recipes Controller constructor
        /// </summary>
        /// <param name="recipeService"></param>
        public RecipesController(IEntityService<Recipe> recipeService, IMapper mapper)
        {
            this._recipesService = recipeService;
            this._mapper = mapper;
        }

        /// <summary>
        /// Get Recipes
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns found items</response>
        /// <response code="204">No items found</response>
        // GET: api/Recipes
        [HttpGet(Name = "GetRecipes")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAll() {
            var recipes = this._recipesService.GetAll();
            if (recipes.Count() == 0)
            {
                return NoContent();
            }
            var recipesMapped = this._mapper.Map<IEnumerable<Recipe>, IEnumerable<GetRecipeDto>>(recipes);
            this.AddCountToHeader(recipes);
            //return Ok(this._mapper.Map<List<RecipeBase>, List<RecipeBaseDto>>(recipes));
            return Ok(recipesMapped);
        }

        /// <summary>
        /// Get 1 recipe by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/Recipes/5
        [HttpGet("{id}", Name = "GetRecipe")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Get(int id)
        {
            var recipe = await this._recipesService.GetOne(id);
            if (recipe == null)
            {
                return NoContent();
            }
            var recipeMapped = this._mapper.Map<Recipe, GetRecipeDto>(recipe);
            //return Ok(this._mapper.Map<RecipeBase,RecipeBaseDto>(recipe));
            return Ok(recipeMapped);
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
        ///     
        /// </remarks> 
        /// <param name="input">Recipe</param>
        /// <returns></returns>
        /// <response code="200">Recipe was created</response>
        /// <response code="422">Input cannot be processed</response> 
        //// POST: api/Recipes
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Post([FromBody] Recipe input)
        {
            //var recipe = this._mapper.Map<RecipeBaseDto, Recipe>(input);
            var response = await this._recipesService.AddOne(input);
            if (!response.Success)
            {
                return UnprocessableEntity(response);
            }
            //return Ok($"Object added. ID:{createdEntityId}");
            return Ok(response);
        }

        /// <summary>
        /// Update recipe
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Recipe was updated</response>
        /// <response code="422">Input cannot be processed</response>
        //// PUT: api/Recipes/5
        // [HttpPut("{id}")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
      
        public async Task<ActionResult> Put([FromBody] PushRecipeDto input)
        {
         

            var recipe = this._mapper.Map<PushRecipeDto, Recipe>(input);
            // validate model after transformed into Recipe
            if (!TryValidateModel(recipe, nameof(Recipe)))
            {
                var state = ModelState;
                return new BadRequestObjectResult(ModelState);           
            }
            var isSuccess = await this._recipesService.UpdateOne(recipe);
            if (!isSuccess)
            {
                return UnprocessableEntity(input);
            }
            return Ok();
        }

        /// <summary>
        /// Delete recipe
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Recipe was deleted</response>
        /// <response code="422">Input cannot be processed</response>
        //// DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Delete(int id)
        {
            var isSuccess = await this._recipesService.DeleteOne(id);
            if (!isSuccess)
            {
                // TODO: this should return diff error depending on if couldn't remove or couldn't find
                return UnprocessableEntity($"Deletion cannot be processed for Recipe with ID {id}. Recipe might not exist");
            }
            return Ok();
        }
    }
}
