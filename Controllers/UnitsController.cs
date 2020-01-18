using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;

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

        /// <summary>
        /// Units controller constructor
        /// </summary>
        /// <param name="context"></param>
        public UnitsController(RecipesContext context)
        {
            this._context = context;
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
            var units = this._context.Units;
            return this.GetAllFormatedResp(units);
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
        //[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public IActionResult Put(int id, [FromBody] Unit input)
        //{
        //    var unit = this._context.Units.Find(id);
        //    //making sure both id match
        //    if (id != input.Unit_Id)
        //    {
        //        return UnprocessableEntity(input);
        //    }
        //    this._context.Entry<Unit>(input).State = EntityState.Detached;
        //    this._context.Units.Update(input);
        //    var result = this._context.SaveChanges();
        //    if (result != 1)
        //    {
        //        return UnprocessableEntity(id);
        //    }
        //    return Ok();
        //}

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
