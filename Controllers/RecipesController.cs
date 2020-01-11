using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipesApi.Repositories;

namespace RecipesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipesRepository _repo;
        public RecipesController(IRecipesRepository repo)
        {
            this._repo = repo;
        }

        // GET: api/Recipes
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetRecipes() { 
            var recipes = await this._repo.GetAllRecipes();
            if (recipes.ToList().Count == 0)
            {
                return NoContent();
            }
            return Ok(recipes);
        }

        // GET: api/Recipes/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Recipes
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Recipes/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
