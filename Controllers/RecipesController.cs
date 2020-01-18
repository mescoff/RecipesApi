using Microsoft.AspNetCore.Mvc;
using RecipesApi.Models;

namespace RecipesApi.Controllers
{
    /// <summary>
    /// Recipes Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipesService _recipesService;

        /// <summary>
        /// Recipes Controller constructor
        /// </summary>
        /// <param name="recipeService"></param>
        public RecipesController(IRecipesService recipeService)
        {
            this._recipesService = recipeService;
        }

        // GET: api/Recipes
        [HttpGet(Name = "GetRecipes")]
        //[Route("[action]")]
        public IActionResult GetAll() {
            var recipes = this._recipesService.GetAllRecipes();
            return this.GetAllFormatedResp(recipes);
        }

        // GET: api/Recipes/5
        [HttpGet("{id}", Name = "GetRecipe")]
        public IActionResult Get(int id)
        {
            var recipe = this._recipesService.GetRecipe(id);
            if (recipe == null)
            {
                return NoContent();
            }
            return Ok(recipe);
        }

        //// POST: api/Recipes
        [HttpPost]
        public IActionResult Post([FromBody] RecipeBase input)
        {
            var isSuccess = this._recipesService.AddRecipe(input);
            if (!isSuccess)
            {
                return UnprocessableEntity(input);
            }
            return Ok();
        }

        //// PUT: api/Recipes/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var isSuccess = this._recipesService.DeleteRecipe(id);
            if (!isSuccess)
            {
                return UnprocessableEntity(id);
            }
            return Ok();
        }
    }
}
