using System.Net.Http;
using System.Web.Http;
using Anno.Api.Core;
using Anno.Api.Core.Const;

namespace Anno.Api.Controllers
{
    public class AppController : ApiController
    {
        [HttpGet]
        [Route("status")]
        public HttpResponseMessage GetStatus()
        {
            //Send response
            return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Success, ResponseMessages.Success));
        }
    }
}
