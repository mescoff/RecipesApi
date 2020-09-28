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
    /// Instructions Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class InstructionsController : ControllerBase
    {
      
        private IEntityService<Instruction> _instructionsService;
        private ILogger _logger;

        /// <summary>
        /// Instructions controller constructor
        /// </summary>
        /// <param name="instructionsService">The instructions service</param>
        /// <param name="mapper">The mapper</param>
        public InstructionsController(IEntityService<Instruction> instructionsService, IMapper mapper, ILogger<InstructionsController> logger)
        {
            this._logger = logger;
            this._instructionsService = instructionsService;
        }

        /// <summary>
        /// Get instructions
        /// </summary>
        /// <returns>List of instructions</returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/Instructions
        [HttpGet(Name = "GetInstructions")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var instructions = this._instructionsService.GetAll().ToList();
            if (instructions.Count == 0)
            {
                return NoContent();
            }
            this.AddCountToHeader(instructions);
            return Ok(instructions);
        }

        /// <summary>
        /// Get 1 instruction by id
        /// </summary>
        /// <param name="id">Instruction id</param>
        /// <returns>instruction</returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/Instructions/5
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("{id}", Name = "GetInstruction")]
        public async Task<ActionResult> Get(int id)
        {
            var instruction = await this._instructionsService.GetOne(id);
            if (instruction == null)
            {
                return NoContent();
            }
            return Ok(instruction);
        }

        /// <summary>
        /// Add instruction 
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Instruction
        ///     {
        ///        "stepNum": 2,
        ///        "description": "Poor the milk into a bowl",
        ///        "recipe_id": 3
        ///     }
        ///     
        ///     POST /Instruction
        ///     {
        ///        "stepNum": 1,
        ///        "description": "start melting the chocolate",
        ///        "recipe_id": 1
        ///     }
        ///
        /// </remarks>
        /// <param name="input">A instruction</param>
        /// <returns></returns>
        /// <response code="200">Instruction was created</response>
        /// <response code="422">Input cannot be processed</response> 
        /// <response code="409">Input already exists</response> 
        /// <response code="500">Issue on server side</response> 
        // POST: api/Instructions
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Post([FromBody] Instruction input)
        {
            try
            {
                var response = await this._instructionsService.AddOne(input);
                if (!response.Success)
                {
                    return UnprocessableEntity(response);
                }
                //return Ok($"Object added. ID:{createdEntityId}");
                return Ok(response);
            }
            catch (Exception e)
            {
                // Instruction has a constraint in DB on having unique name
                if (e.InnerException.ToString().Contains("Duplicate"))
                {
                    this._logger.LogError($"Exception on input: {JsonConvert.SerializeObject(input)}. Error: {e.InnerException.ToString()}");
                    return Conflict(input);
                }
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Update instruction
        /// </summary>
        /// <param name="id">Instruction id</param>
        /// <param name="input">Instruction to update</param>
        /// <response code="200">Instruction was updated</response>
        /// <response code="422">Input cannot be processed</response> 
        // PUT: api/Instructions/5
        //[HttpPut("{id}")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Put([FromBody] Instruction input)
        {
            var isSuccess = await this._instructionsService.UpdateOne(input);
            if (!isSuccess)
            {
                return UnprocessableEntity(input);
            }
            return Ok();

            //var instruction = this._context.Instructions.Find(input.Instruction_Id);     
            // TODO: check if below is bad practice. Context is not scoped here so if object you try to modify is still attached it creates a conflict

            // this._context.Entry<Instruction>(instruction).State = EntityState.Detached;
            // this._context.Instructions.Update(input);
            //  var result = this._context.SaveChanges();          
        }

        /// <summary>
        /// Delete instruction
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Instruction was deleted</response>
        /// <response code="422">Input cannot be processed</response> 
        // DELETE: api/ApiWithActions/5
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            
            var isSuccess = await this._instructionsService.DeleteOne(id);
            if (!isSuccess)
            {
                return UnprocessableEntity(id);
            }
            return Ok();
        }
    }
}
