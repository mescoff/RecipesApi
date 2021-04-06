using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using RecipesApi.DTOs.Recipes;
using Microsoft.AspNetCore.Cors;

namespace RecipesApi.Controllers
{
    /// <summary>
    /// Recipes Controller
    /// </summary>
    [EnableCors("SameServerPolicy")] // TODO: remove in PROD
    [Route("api/[controller]")]
    [ApiController]
    //[ValidateModel]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipesService<RecipeDto> _recipesService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Recipes Controller constructor
        /// </summary>
        /// <param name="recipeService"></param>
        public RecipesController(IRecipesService<RecipeDto> recipeService, IMapper mapper)
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
            //var recipesMapped = this._mapper.Map<IEnumerable<Recipe>, IEnumerable<GetRecipeDto>>(recipes);
            this.AddCountToHeader(recipes);
            //return Ok(this._mapper.Map<List<RecipeBase>, List<RecipeBaseDto>>(recipes));
            return Ok(recipes);
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
            return Ok(recipe);
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
        public async Task<ActionResult> Post([FromBody] RecipeDto input)
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
        public async Task<ActionResult> Put([FromBody] RecipeDto recipe)
        {
         

            //var recipe = this._mapper.Map<PushRecipeDto, Recipe>(input);
            // validate model after transformed into Recipe and send specific response
            //if (!TryValidateModel(recipe, nameof(Recipe)))
            //{
            //    var state = ModelState;
            //    return new BadRequestObjectResult(ModelState);           
            //}
            var isSuccess = await this._recipesService.UpdateOne(recipe);
            if (!isSuccess)
            {
                return UnprocessableEntity(recipe);
            }
            return Ok();
        }

        /// <summary>
        /// Delete recipe
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">OK : Recipe was deleted</response>
        /// <response code="404">NOT FOUND: Recipe not found</response>
        /// <response code="500">Internal Server Error: Something happened on server side when deleting recipe</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Delete(int id)
        {
            var deleteStatus = await this._recipesService.DeleteOne(id);
            if (deleteStatus.Success == false)
            {
                if (deleteStatus.Message.Contains("not exist")){
                    return NotFound(deleteStatus.Message);
                }
                else
                {
                    return StatusCode(500, deleteStatus.Message);
                }
            }
            return Ok();
        }
    }
}
