using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipesApi.DTOs;
using RecipesApi.DTOs.Recipes;
using RecipesApi.Models;

namespace RecipesApi.Controllers
{

    /// <summary>
    /// recipeCategorys controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RecipeCategoriesController : ControllerBase
    {

        private readonly IEntityService<RecipeCategory> _recipeCategoryService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Recipe Categories Controller
        /// </summary>
        /// <param name="recipeCategoryService">The recipeCategorys service</param>
        /// <param name="mapper">The mapper</param>
        public RecipeCategoriesController(IEntityService<RecipeCategory> recipeCategoryService, IMapper mapper)
        {
            this._recipeCategoryService = recipeCategoryService;
            this._mapper = mapper;
        }


        /// <summary>
        /// Get recipeCategories
        /// </summary>    
        /// <returns></returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/recipeCategorys
        //[HttpGet(Name = "GetAllRecipeRecipeCategory")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public IActionResult GetAll()
        //{
        //    //TODO: Get set in more generic way (https://stackoverflow.com/questions/21533506/find-a-specified-generic-dbset-in-a-dbcontext-dynamically-when-i-have-an-entity)
        //    var recipeCategorys = this._recipeCategoryService.GetAll().ToList();
        //    if (recipeCategorys.Count == 0)
        //    {
        //        return NoContent();
        //    }
        //    this.AddCountToHeader(recipeCategorys);
        //    return Ok(this._mapper.Map<List<RecipeCategory>, List<RecipeCategoryDto>>(recipeCategorys));
        //}

        /// <summary>
        /// Get 1 recipeCategory by id
        /// </summary>
        /// <param name="id">recipeCategory id</param>
        /// <returns>unit</returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //// GET: api/recipeCategorys/5
        //[HttpGet("{id}", Name = "GetRecipeRecipeCategory")]
        //public async Task<ActionResult> Get(int id)
        //{
        //    var recipeCategory = await this._recipeCategoryService.GetOne(id);
        //    if (recipeCategory == null)
        //    {
        //        return NoContent();
        //    }
        //    return Ok(this._mapper.Map<RecipeCategory, RecipeCategoryDto>(recipeCategory));
        //}

        /// <summary>
        /// Add recipeCategory
        /// </summary>
        /// <remarks>

        /// </remarks> 
        /// <param name="input">recipeCategory</param>
        /// <returns></returns>
        /// <response code="200">recipeCategory was created</response>
        /// <response code="422">Input cannot be processed</response> 
        // POST: api/recipeCategorys
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Post([FromBody] AddRecipeCategoryDto input)
        {
            //TODO: add standard on naming
            var recipeCategory = this._mapper.Map<AddRecipeCategoryDto, RecipeCategory>(input);
            var response = await this._recipeCategoryService.AddOne(recipeCategory);
            // this._context.recipeCategorys.Add(recipeCategory);
            // FIXME: Bug on saving new recipeCategory..
            // var result = this._context.SaveChanges();
            if (!response.Success)
            {
                return UnprocessableEntity(response);
            }
            //return Ok($"Object added. ID:{createdEntityId}");
            return Ok(response);
        }

        /// <summary>
        /// Update recipeCategory
        /// </summary>
        /// <param name="input">recipeCategory to update</param>
        /// <returns></returns>
        /// <response code="200">recipeCategory was updated</response>
        /// <response code="422">Input cannot be processed</response> 
        // PUT: api/recipeCategorys/5
        //[HttpPut]
        //[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public async Task<ActionResult> Put([FromBody] RecipeCategoryDto input)
        //{
        //    var recipeCategory = this._mapper.Map<RecipeCategoryDto, RecipeCategory>(input);
        //    var isSuccess = await this._recipeCategoryService.UpdateOne(recipeCategory);
        //    if (!isSuccess)
        //    {
        //        return UnprocessableEntity(input);
        //    }
        //    return Ok();
        //}

        /// <summary>
        /// Delete recipeCategory
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">recipeCategory was deleted</response>
        /// <response code="422">Input cannot be processed</response> 
        //// DELETE: api/recipeCategorys/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var isSuccess = await this._recipeCategoryService.DeleteOne(id);
            if (!isSuccess)
            {
                // TODO: this should return diff error depending on if couldn't remove or couldn't find
                return UnprocessableEntity(id);
            }
            return Ok();
        }

    }
}
