using System;
using System.Net.Http;
using System.Web.Http;
using AnnoAPI.Models;
using AnnoAPI.Core.Services;
using AnnoAPI.Core;
using AnnoAPI.Core.Const;

namespace AnnoAPI.Controllers
{
    public class CustomerController : ApiController
    {
        [HttpPost]
        [Route("customers/create")]
        public HttpResponseMessage CreateCustomer(CreateCustomerRequest request)
        {
            CreateCustomerResponse responseData = null;

            try
            {
                //Authenticate API key
                long? hostId = HostServices.GetCallerHostId();
                if(!hostId.HasValue)
                {
                    Log.Warn(ResponseMessages.InvalidAPIKey);
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidAPIKey, ResponseMessages.InvalidAPIKey));
                }

                //Validate input
                if (request == null ||
                    string.IsNullOrEmpty(request.ReferenceId))
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, ResponseMessages.InvalidParam));
                }

                //Validate input length
                if (request.ReferenceId.Length > 30)
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, "Reference ID (max 30 length) is too long."));
                }

                //Validate customer reference ID
                IdentityServices identityService = new IdentityServices();
                if (identityService.IsRefIdExists(IdentityServices.RefIdTypes.Customer, hostId.Value, request.ReferenceId))
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.DuplicateRefId, ResponseMessages.DuplicateRefId + " (Customer " + request.ReferenceId + ")"));
                }

                //Perform transaction
                CustomerServices customerServices = new CustomerServices();
                responseData = customerServices.CreateCustomer(hostId.Value, request);
                
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
        [Route("customers")]
        public HttpResponseMessage GetCustomers()
        {
            GetCustomersResponse responseData = null;

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
                CustomerServices customerServices = new CustomerServices();
                var customers = customerServices.GetCustomers(hostId.Value);
                
                responseData = new GetCustomersResponse();
                responseData.Customers = customers;

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
        [Route("customers/{refId}")]
        public HttpResponseMessage GetCustomersByRef(string refId)
        {
            GetCustomerByRefResponse responseData = null;

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
                CustomerServices customerServices = new CustomerServices();
                var customer = customerServices.GetCustomerByRef(hostId.Value, refId);

                responseData = new GetCustomerByRefResponse();
                responseData.Customer = customer;

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
