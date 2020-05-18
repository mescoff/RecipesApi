using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipesApi.DTOs;
using RecipesApi.Models;

namespace RecipesApi.Controllers
{

    /// <summary>
    /// medias controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {

        private readonly IEntityService<Media> _mediaService;
        private readonly IMapper _mapper;

        /// <summary>
        /// All recipe media (photos, videos, logo) Controller
        /// </summary>
        /// <param name="mediaService">The medias service</param>
        /// <param name="mapper">The mapper</param>
        public MediaController(IEntityService<Media> mediaService, IMapper mapper)
        {
            this._mediaService = mediaService;
            this._mapper = mapper;
        }


        /// <summary>
        /// Get medias
        /// </summary>    
        /// <returns></returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        // GET: api/medias
        [HttpGet(Name = "GetAllRecipeMedia")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            //TODO: Get set in more generic way (https://stackoverflow.com/questions/21533506/find-a-specified-generic-dbset-in-a-dbcontext-dynamically-when-i-have-an-entity)
            var medias = this._mediaService.GetAll().ToList();
            if (medias.Count == 0)
            {
                return NoContent();
            }
            this.AddCountToHeader(medias);
            return Ok(this._mapper.Map<List<Media>, List<MediaDto>>(medias));
        }

        /// <summary>
        /// Get 1 media by id
        /// </summary>
        /// <param name="id">media id</param>
        /// <returns>unit</returns>
        /// <response code="200">Returns found item</response>
        /// <response code="204">No items found</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // GET: api/medias/5
        [HttpGet("{id}", Name = "GetRecipeMedia")]
        public async Task<ActionResult> Get(int id)
        {
            var media = await this._mediaService.GetOne(id);
            if (media == null)
            {
                return NoContent();
            }
            return Ok(this._mapper.Map<Media, MediaDto>(media));
        }

        /// <summary>
        /// Add media
        /// </summary>
        /// <remarks>

        /// </remarks> 
        /// <param name="input">media</param>
        /// <returns></returns>
        /// <response code="200">media was created</response>
        /// <response code="422">Input cannot be processed</response> 
        // POST: api/medias
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Post([FromBody] MediaDto input)
        {
            //TODO: add standard on naming
            var media = this._mapper.Map<MediaDto, Media>(input);
            var createdEntityId = await this._mediaService.AddOne(media);
            // this._context.medias.Add(media);
            // FIXME: Bug on saving new media..
            // var result = this._context.SaveChanges();
            if (createdEntityId == 0)
            {
                return UnprocessableEntity(input);
            }
            return Ok($"Object added. ID:{createdEntityId}");
        }

        /// <summary>
        /// Update media
        /// </summary>
        /// <param name="input">media to update</param>
        /// <returns></returns>
        /// <response code="200">media was updated</response>
        /// <response code="422">Input cannot be processed</response> 
        // PUT: api/medias/5
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Put([FromBody] MediaDto input)
        {
            var media = this._mapper.Map<MediaDto, Media>(input);
            var isSuccess = await this._mediaService.UpdateOne(media);
            if (!isSuccess)
            {
                return UnprocessableEntity(input);
            }
            return Ok();
        }

        /// <summary>
        /// Delete media
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">media was deleted</response>
        /// <response code="422">Input cannot be processed</response> 
        //// DELETE: api/medias/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var isSuccess = await this._mediaService.DeleteOne(id);
            if (!isSuccess)
            {
                // TODO: this should return diff error depending on if couldn't remove or couldn't find
                return UnprocessableEntity(id);
            }
            return Ok();
        }

    }
}
