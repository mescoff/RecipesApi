using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace RecipesApi.Controllers
{
    /// <summary>
    /// Units Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UnitsController : ControllerBase
    {
        private RecipesContext _context;
        private IMapper _mapper;

        /// <summary>
        /// Units controller constructor
        /// </summary>
        /// <param name="context"></param>
        public UnitsController(RecipesContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
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
            var units = this._context.Units.ToList(); ;
            if (units.Count == 0)
            {
                return NoContent();
            }
            this.AddCountToHeader(units);
            return Ok(units);
        }

        /// <summary>
        /// Get unit by id
        /// </summary>
        /// <param name="id">Unit id</param>
        /// <returns>unit</returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/Units/5
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("{id}", Name = "GetUnit")]
        public IActionResult Get(int id)
        {
            var unit = this._context.Units.Find(id);
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
        // POST: api/Units
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Post([FromBody] Unit input)
        {
            input.Unit_Id = 0;
            this._context.Units.Add(input);
            var result = this._context.SaveChanges();
            if (result != 1)
            {
                return UnprocessableEntity(input);
            }
            return Ok();
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
        public IActionResult Put([FromBody] Unit input)
        {
            var unit = this._context.Units.Find(input.Unit_Id);
            //making sure both ids match
            //if (id != input.Unit_Id || unit == null)
            //{
            //    return UnprocessableEntity(input);
            //}
            // TODO: check if below is bad practice. Context is not scoped here so if object you try to modify is still attached it creates a conflict
            this._context.Entry<Unit>(unit).State = EntityState.Detached;
            // TODO: create generic method shared among controllers that will update original object and only modified fields (except creationDate/Audit)
            this._context.Units.Update(input);
            var result = this._context.SaveChanges();
            if (result != 1)
            {
                return UnprocessableEntity(input);
            }
            return Ok();
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
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            this._context.Units.Remove(this._context.Units.Find(id));
            var result = this._context.SaveChanges();
            if (result != 1)
            {
                return UnprocessableEntity(id);
            }
            return Ok();
        }
    }
}
