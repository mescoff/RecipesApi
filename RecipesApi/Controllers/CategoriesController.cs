using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecipesApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.Controllers
{
    /// <summary>
    /// Categories Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {

        private IEntityService<Category> _categoriesService;
        private ILogger _logger;

        /// <summary>
        /// Categories controller constructor
        /// </summary>
        /// <param name="categoriesService">The categories service</param>
        /// <param name="mapper">The mapper</param>
        public CategoriesController(IEntityService<Category> categoriesService, IMapper mapper, ILogger<CategoriesController> logger)
        {
            this._logger = logger;
            this._categoriesService = categoriesService;
        }

        /// <summary>
        /// Get categories
        /// </summary>
        /// <returns>List of categories</returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/Categories
        [HttpGet(Name = "GetCategories")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var categories = this._categoriesService.GetAll().ToList();
            if (categories.Count == 0)
            {
                return NoContent();
            }
            this.AddCountToHeader(categories);
            return Ok(categories);
        }

        /// <summary>
        /// Get 1 category by id
        /// </summary>
        /// <param name="id">Category id</param>
        /// <returns>category</returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/Categories/5
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("{id}", Name = "GetCategory")]
        public async Task<ActionResult> Get(int id)
        {
            var category = await this._categoriesService.GetOne(id);
            if (category == null)
            {
                return NoContent();
            }
            return Ok(category);
        }

        /// <summary>
        /// Add category 
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Category
        ///     {
        ///        "name": "breakfast",
        ///        "description": "bk"
        ///     }
        ///     
        ///     POST /Category
        ///     {
        ///        "name": "fancy dinner",
        ///        "description": "great for dinner with friends"
        ///     }
        ///
        /// </remarks>
        /// <param name="input">A category</param>
        /// <returns></returns>
        /// <response code="200">Category was created</response>
        /// <response code="422">Input cannot be processed</response> 
        /// <response code="409">Input already exists</response> 
        /// <response code="500">Issue on server side</response> 
        // POST: api/Categories
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Post([FromBody] Category input)
        {
            try
            {
                var createdEntityId = await this._categoriesService.AddOne(input);
                if (createdEntityId == 0)
                {
                    return UnprocessableEntity(input);
                }
                return Ok($"Object added. ID:{createdEntityId}");
            }
            catch (Exception e)
            {
                // Category has a constraint in DB on having unique name
                if (e.InnerException.ToString().Contains("Duplicate"))
                {
                    this._logger.LogError($"Exception on input: {JsonConvert.SerializeObject(input)}. Error: {e.InnerException.ToString()}");
                    return Conflict(input);
                }
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Update category
        /// </summary>
        /// <param name="id">Category id</param>
        /// <param name="input">Category to update</param>
        /// <response code="200">Category was updated</response>
        /// <response code="422">Input cannot be processed</response> 
        // PUT: api/Categories/5
        //[HttpPut("{id}")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Put([FromBody] Category input)
        {
            var isSuccess = await this._categoriesService.UpdateOne(input);
            if (!isSuccess)
            {
                return UnprocessableEntity(input);
            }
            return Ok();

            //var category = this._context.Categories.Find(input.Category_Id);     
            // TODO: check if below is bad practice. Context is not scoped here so if object you try to modify is still attached it creates a conflict

            // this._context.Entry<Category>(category).State = EntityState.Detached;
            // this._context.Categories.Update(input);
            //  var result = this._context.SaveChanges();          
        }

        /// <summary>
        /// Delete category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Category was deleted</response>
        /// <response code="422">Input cannot be processed</response> 
        // DELETE: api/ApiWithActions/5
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            // TODO: Consider potentially dangerous cascade delete here. If Category is deleted it could go delete all ingredients connected to it
            var isSuccess = await this._categoriesService.DeleteOne(id);
            if (!isSuccess)
            {
                return UnprocessableEntity(id);
            }
            return Ok();
        }
    }
}
