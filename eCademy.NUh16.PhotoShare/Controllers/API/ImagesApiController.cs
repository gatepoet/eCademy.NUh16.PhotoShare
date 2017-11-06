using eCademy.NUh16.PhotoShare.Models;
using System.Web.Http;

namespace eCademy.NUh16.PhotoShare.Controllers.API
{
    public class ImagesApiController : ApiController
    {
        // PUT api/<controller>/5
        [HttpPut]
        [Route("Images/{id:int}/rate/{rating:int}")]
        public IHttpActionResult PutRate(int id, int rating)
        {
            ///TODO: Rate photo, return new score.
            var newScore = 1.1;

            return Ok(new RateResult(newScore));
        }
    }
}