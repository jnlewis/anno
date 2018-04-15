using System;
using System.Net.Http;
using System.Web.Http;
using AnnoAPI.Models;
using AnnoAPI.Core.Services;
using AnnoAPI.Core;
using AnnoAPI.Core.Const;

namespace AnnoAPI.Controllers
{
    public class EventsController : ApiController
    {
        [HttpPost]
        [Route("events/create")]
        public HttpResponseMessage CreateEvent(CreateEventsRequest request)
        {
            try
            {
                //Authenticate API key
                long? hostId = HostServices.GetCallerHostId();
                if (!hostId.HasValue)
                {
                    Log.Warn(ResponseMessages.InvalidAPIKey);
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidAPIKey, ResponseMessages.InvalidAPIKey));
                }

                //Validate input
                if (request == null ||
                    string.IsNullOrEmpty(request.ReferenceId) ||
                    string.IsNullOrEmpty(request.Title) ||
                    !request.StartDate.HasValue ||
                    request.Tiers == null ||
                    request.Tiers.Count == 0)
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, ResponseMessages.InvalidParam));
                }

                //Validate input length
                if(request.ReferenceId.Length > 30)
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, "Reference ID (max 30 length) is too long."));
                }
                if (request.Title.Length > 200 || request.Description.Length > 500)
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, "Title (max 200 length) or Description (max 500 length) is too long."));
                }
                if (request.Tiers.Count > 10)
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, "Only a maximum of 10 tiers allowed."));
                }

                //Ensure that start date is in future
                if (DateTime.UtcNow.Date > request.StartDate.Value)
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, "Event start date must be a future date."));
                }

                //Validate event reference ID
                IdentityServices identityService = new IdentityServices();
                if (identityService.IsRefIdExists(IdentityServices.RefIdTypes.Event, hostId.Value, request.ReferenceId))
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.DuplicateRefId, ResponseMessages.DuplicateRefId + " (Event " + request.ReferenceId + ")"));
                }
                //Validate event tier reference ID and input length
                foreach(var tier in request.Tiers)
                {
                    if (tier.ReferenceId.Length > 30)
                    {
                        return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, "Reference ID (max 30 length) is too long."));
                    }
                    if (identityService.IsRefIdExists(IdentityServices.RefIdTypes.EventTier, hostId.Value, tier.ReferenceId))
                    {
                        return Request.CreateResponse(new GenericResponse(null, ResponseCodes.DuplicateRefId, ResponseMessages.DuplicateRefId + " (Tier " + tier.ReferenceId + ")"));
                    }
                    if (tier.Title.Length > 200)
                    {
                        return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, "Tier Title (max 200 length) is too long."));
                    }
                }

                //Perform transaction
                EventsServices eventsService = new EventsServices();
                eventsService.CreateEvent(hostId.Value, request);

                //Send response
                return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Success, ResponseMessages.Success));
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error));
            }
        }

        //[HttpPost]
        //[Route("events/update-status")]
        //public HttpResponseMessage UpdateEventStatus(UpdateEventStatusRequest request)
        //{
        //    try
        //    {
        //        //Authenticate API key
        //        long? hostId = HostServices.GetCallerHostId();
        //        if (!hostId.HasValue)
        //        {
        //            Log.Warn(ResponseMessages.InvalidAPIKey);
        //            return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidAPIKey, ResponseMessages.InvalidAPIKey));
        //        }

        //        //Validate input
        //        if (request == null ||
        //            string.IsNullOrEmpty(request.ReferenceId) ||
        //            string.IsNullOrEmpty(request.NewStatus))
        //        {
        //            return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, ResponseMessages.InvalidParam));
        //        }
        //        //Validate event status
        //        if (request.NewStatus != "OnSale" &&
        //            request.NewStatus != "OnHold")
        //        {
        //            return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, "Status can only be the following: OnSale, OnHold"));
        //        }

        //        //Perform transaction
        //        EventsServices eventsService = new EventsServices();
        //        eventsService.UpdateEventStatus(hostId.Value, request);

        //        //Send response
        //        return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Success, ResponseMessages.Success));
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Exception(ex);
        //        return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error));
        //    }
        //}

        [HttpGet]
        [Route("events")]
        public HttpResponseMessage GetEvents()
        {
            GetEventsResponse responseData = null;

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
                EventsServices eventsService = new EventsServices();
                var events = eventsService.GetEvents(hostId.Value);

                responseData = new GetEventsResponse();
                responseData.Events = events;

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
        [Route("events/details/{refId}")]
        public HttpResponseMessage GetEventDetailsByRef(string refId)
        {
            GetEventDetailsByRefResponse responseData = null;

            try
            {
                //Authenticate API key
                long? hostId = HostServices.GetCallerHostId();
                if (!hostId.HasValue)
                {
                    Log.Warn(ResponseMessages.InvalidAPIKey);
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidAPIKey, ResponseMessages.InvalidAPIKey));
                }

                //Get event
                EventsServices eventsService = new EventsServices();
                var eventInfo = eventsService.GetEventByRef(hostId.Value, refId);
                if(eventInfo != null)
                {
                    //Get tiers
                    var tiers = eventsService.GetEventTiersByEventId(eventInfo.EventId.Value);

                    responseData = new GetEventDetailsByRefResponse();
                    responseData.Event = eventInfo;
                    responseData.Tiers = tiers;
                }
                
                //Send response
                return Request.CreateResponse(new GenericResponse(responseData, ResponseCodes.Success, ResponseMessages.Success));
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error));
            }
        }

        [HttpPost]
        [Route("events/cancel")]
        public HttpResponseMessage CancelEvent(CancelEventRequest request)
        {
            try
            {
                //Authenticate API key
                long? hostId = HostServices.GetCallerHostId();
                if (!hostId.HasValue)
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

                //Perform transaction
                EventsServices eventsService = new EventsServices();
                string status = eventsService.CancelEvent(hostId.Value, request);
                
                if (status == CancelEventStatuses.EventNotFound)
                {
                    return Request.CreateResponse(new GenericResponse(null, CancelEventStatuses.EventNotFound, "Event Not Found"));
                }
                else if (status == CancelEventStatuses.EventNotActive)
                {
                    return Request.CreateResponse(new GenericResponse(null, CancelEventStatuses.EventNotActive, "Event is already cancelled or closed."));
                }
                else if (status == CancelEventStatuses.EventHasAlreadyStarted)
                {
                    return Request.CreateResponse(new GenericResponse(null, CancelEventStatuses.EventHasAlreadyStarted, "Event Has Already Started"));
                }
                else if (status == CancelEventStatuses.Success)
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Success, ResponseMessages.Success));
                }
                else
                {
                    Log.Error("Unrecognized status: " + status);
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error));
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error));
            }
        }

        [HttpPost]
        [Route("events/claim")]
        public HttpResponseMessage ClaimEarnings(ClaimEarningsRequest request)
        {
            ClaimEarningsResponse responseData = null;

            try
            {
                //Authenticate API key
                long? hostId = HostServices.GetCallerHostId();
                if (!hostId.HasValue)
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

                //Perform transaction
                EventsServices eventsService = new EventsServices();
                string status = null;
                responseData = eventsService.ClaimEarnings(hostId.Value, request, out status);
                
                if (status == ClaimEarningsStatuses.EventNotFound)
                {
                    return Request.CreateResponse(new GenericResponse(null, ClaimEarningsStatuses.EventNotFound, "EventNotFound"));
                }
                else if (status == ClaimEarningsStatuses.EventNotStarted)
                {
                    return Request.CreateResponse(new GenericResponse(null, ClaimEarningsStatuses.EventNotStarted, "Event has not started. Event start date must be over before earnings for the event can be claimed."));
                }
                else if (status == ClaimEarningsStatuses.EventAlreadyClaimed)
                {
                    return Request.CreateResponse(new GenericResponse(null, ClaimEarningsStatuses.EventAlreadyClaimed, "EventAlreadyClaimed"));
                }
                else if (status == ClaimEarningsStatuses.EventAlreadyCancelled)
                {
                    return Request.CreateResponse(new GenericResponse(null, ClaimEarningsStatuses.EventAlreadyCancelled, "EventAlreadyCancelled"));
                }
                else if (status == ClaimEarningsStatuses.HostNotFound)
                {
                    return Request.CreateResponse(new GenericResponse(null, ClaimEarningsStatuses.HostNotFound, "HostNotFound"));
                }
                else if (status == ClaimEarningsStatuses.Success)
                {
                    return Request.CreateResponse(new GenericResponse(responseData, ResponseCodes.Success, ResponseMessages.Success));
                }
                else
                {
                    Log.Error("Unrecognized status: " + status);
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error));
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error));
            }
        }

    }
}
