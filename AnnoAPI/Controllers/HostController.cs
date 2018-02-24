using System.Net.Http;
using System.Web.Http;
using AnnoAPI.Core.Enum;
using AnnoAPI.Core;
using AnnoAPI.Models;
using AnnoAPI.Core.Services;
using System;

namespace AnnoAPI.Controllers
{
    public class HostController : ApiController
    {
        //TODO: This endpoint should not be exposed. Remove after CoZ contest.
        [HttpPost]
        [Route("hosts/create")]
        public HttpResponseMessage CreateHost(CreateHostRequest request)
        {
            CreateHostResponse responseData = null;

            string newAPIKey = null;

            try
            {
                //Validate input
                if (request == null ||
                    string.IsNullOrEmpty(request.Name))
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, ResponseMessages.InvalidParam));
                }

                //Perform transaction
                HostServices hostService = new HostServices();
                hostService.CreateHost(request, out newAPIKey);

                responseData = new CreateHostResponse();
                responseData.APIKey = newAPIKey;

                //Send response
                return Request.CreateResponse(new GenericResponse(responseData, ResponseCodes.Success, ResponseMessages.Success));
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error));
            }
        }

        [HttpGet]
        [Route("hosts/info")]
        public HttpResponseMessage GetInfo()
        {
            HostInfoResponse responseData = null;

            try
            {
                //Authenticate API key
                long? hostId = HostServices.GetCallerHostId();
                if (!hostId.HasValue)
                {
                    Log.Warn(ResponseMessages.InvalidAPIKey);
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidAPIKey, ResponseMessages.InvalidAPIKey));
                }

                //Perform transaction
                HostServices hostService = new HostServices();
                var hostInfo = hostService.GetHostInfoByAPIKey(RequestHeaders.API_KEY);

                responseData = new HostInfoResponse();
                responseData.Info = hostInfo;

                //Send response
                return Request.CreateResponse(new GenericResponse(responseData, ResponseCodes.Success, ResponseMessages.Success));
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error));
            }
        }
    }
}
