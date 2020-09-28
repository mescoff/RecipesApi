using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using System.Linq;

namespace RecipesApi.Controllers
{
    /// <summary>
    /// Recipe time intervalls Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TimeIntervalsController : ControllerBase
    {
        private IEntityService<TimeInterval> _timeIntervalsService;
        private ILogger _logger;

        /// <summary>
        /// Categories controller constructor
        /// </summary>
        /// <param name="timeIntervalsService">The time intervals service</param>
        public TimeIntervalsController(IEntityService<TimeInterval> timeIntervalsService, ILogger<TimeIntervalsController> logger)
        {
            this._logger = logger;
            this._timeIntervalsService = timeIntervalsService;
        }

        /// <summary>
        /// Get time intervals
        /// </summary>
        /// <returns>List of time intervals</returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/Categories
        [HttpGet(Name = "GetRecipeTimeIntervals")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var timeIntervals = this._timeIntervalsService.GetAll();
            if (timeIntervals.Count() == 0)
            {
                return NoContent();
            }
            this.AddCountToHeader(timeIntervals);
            return Ok(timeIntervals);
        }
    }
}
