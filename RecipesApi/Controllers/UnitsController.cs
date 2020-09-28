using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecipesApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.Controllers
{
    /// <summary>
    /// Units Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UnitsController : ControllerBase
    {
      
        private IEntityService<Unit> _unitsService;
        private ILogger _logger;

        /// <summary>
        /// Units controller constructor
        /// </summary>
        /// <param name="unitsService">The units service</param>
        /// <param name="mapper">The mapper</param>
        public UnitsController(IEntityService<Unit> unitsService, IMapper mapper, ILogger<UnitsController> logger)
        {
            this._logger = logger;
            this._unitsService = unitsService;
        }

        /// <summary>
        /// Get units
        /// </summary>
        /// <returns>List of units</returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/Units
        [HttpGet(Name = "GetUnits")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var units = this._unitsService.GetAll().ToList();
            if (units.Count == 0)
            {
                return NoContent();
            }
            this.AddCountToHeader(units);
            return Ok(units);
        }

        /// <summary>
        /// Get 1 unit by id
        /// </summary>
        /// <param name="id">Unit id</param>
        /// <returns>unit</returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/Units/5
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("{id}", Name = "GetUnit")]
        public async Task<ActionResult> Get(int id)
        {
            var unit = await this._unitsService.GetOne(id);
            if (unit == null)
            {
                return NoContent();
            }
            return Ok(unit);
        }

        /// <summary>
        /// Add unit (ex: Oz, Tbsp, ml, cl)
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Unit
        ///     {
        ///        "name": "tablespoon",
        ///        "symbol": "Tbsp"
        ///     }
        ///     
        ///     POST /Unit
        ///     {
        ///        "name": "ounce",
        ///        "symbol": "oz"
        ///     }
        ///
        /// </remarks>
        /// <param name="input">A unit</param>
        /// <returns></returns>
        /// <response code="200">Unit was created</response>
        /// <response code="422">Input cannot be processed</response> 
        /// <response code="409">Input already exists</response> 
        /// <response code="500">Issue on server side</response> 
        // POST: api/Units
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Post([FromBody] Unit input)
        {
            try
            {
                var response = await this._unitsService.AddOne(input);
                if (!response.Success)
                {
                    return UnprocessableEntity(response);
                }
                //return Ok($"Object added. ID:{createdEntityId}");
                return Ok(response);
            }
            catch (Exception e)
            {
                // Unit has a constraint in DB on having unique name
                if (e.InnerException.ToString().Contains("Duplicate"))
                {
                    this._logger.LogError($"Exception on input: {JsonConvert.SerializeObject(input)}. Error: {e.InnerException.ToString()}");
                    return Conflict(input);
                }
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Update unit
        /// </summary>
        /// <param name="id">Unit id</param>
        /// <param name="input">Unit to update</param>
        /// <response code="200">Unit was updated</response>
        /// <response code="422">Input cannot be processed</response> 
        // PUT: api/Units/5
        //[HttpPut("{id}")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Put([FromBody] Unit input)
        {
            var isSuccess = await this._unitsService.UpdateOne(input);
            if (!isSuccess)
            {
                return UnprocessableEntity(input);
            }
            return Ok();

            //var unit = this._context.Units.Find(input.Unit_Id);     
            // TODO: check if below is bad practice. Context is not scoped here so if object you try to modify is still attached it creates a conflict

            // this._context.Entry<Unit>(unit).State = EntityState.Detached;
            // this._context.Units.Update(input);
            //  var result = this._context.SaveChanges();          
        }

        /// <summary>
        /// Delete unit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Unit was deleted</response>
        /// <response code="422">Input cannot be processed</response> 
        // DELETE: api/ApiWithActions/5
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            // TODO: Consider potentially dangerous cascade delete here. If Unit is deleted it could go delete all ingredients connected to it
            var isSuccess = await this._unitsService.DeleteOne(id);
            if (!isSuccess)
            {
                return UnprocessableEntity(id);
            }
            return Ok();
        }
    }
}
