using System;
using System.Net.Http;
using System.Web.Http;
using AnnoAPI.Models;
using AnnoAPI.Core.Services;
using AnnoAPI.Core;
using AnnoAPI.Core.Const;

namespace AnnoAPI.Controllers
{
    public class BookingsController : ApiController
    {
        [HttpPost]
        [Route("bookings/book-event")]
        public HttpResponseMessage BookEvent(BookEventRequest request)
        {
            BookEventResponse responseData;

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
                    string.IsNullOrEmpty(request.EventReferenceId) ||
                    string.IsNullOrEmpty(request.CustomerReferenceId) ||
                    request.Tickets == null ||
                    request.Tickets.Count == 0)
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, ResponseMessages.InvalidParam));
                }

                //Perform transaction
                BookingServices bookingService = new BookingServices();
                string status = null;
                responseData = bookingService.BookEvent(hostId.Value, request, out status);
                
                if (status == BookEventStatuses.CustomerNotFound)
                {
                    return Request.CreateResponse(new GenericResponse(null, BookEventStatuses.CustomerNotFound, "CustomerNotFound"));
                }
                else if (status == BookEventStatuses.EventNotFound)
                {
                    return Request.CreateResponse(new GenericResponse(null, BookEventStatuses.EventNotFound, "EventNotFound"));
                }
                else if (status == BookEventStatuses.TierNotFound)
                {
                    return Request.CreateResponse(new GenericResponse(null, BookEventStatuses.TierNotFound, "TierNotFound"));
                }
                else if (status == BookEventStatuses.EventNotActive)
                {
                    return Request.CreateResponse(new GenericResponse(null, BookEventStatuses.EventNotActive, "Event is already cancelled or closed."));
                }
                else if (status == BookEventStatuses.EventHasAlreadyStarted)
                {
                    return Request.CreateResponse(new GenericResponse(null, BookEventStatuses.EventHasAlreadyStarted, "EventHasAlreadyStarted"));
                }
                else if (status == BookEventStatuses.InsufficientTickets)
                {
                    return Request.CreateResponse(new GenericResponse(null, BookEventStatuses.InsufficientTickets, "InsufficientTickets"));
                }
                else if (status == BookEventStatuses.CustomerInsufficientFunds)
                {
                    return Request.CreateResponse(new GenericResponse(null, BookEventStatuses.CustomerInsufficientFunds, "CustomerInsufficientFunds"));
                }
                else if (status == BookEventStatuses.InvalidTicketQuantity)
                {
                    return Request.CreateResponse(new GenericResponse(null, BookEventStatuses.InvalidTicketQuantity, "InvalidTicketQuantity"));
                }
                else if (status == BookEventStatuses.Success)
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

        [HttpGet]
        [Route("bookings/summary")]
        public HttpResponseMessage GetBookingsSummary()
        {
            GetBookingsSummaryResponse responseData = null;

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
                BookingServices bookingService = new BookingServices();
                var bookingsSummary = bookingService.GetBookingsSummary(hostId.Value);

                responseData = new GetBookingsSummaryResponse();
                responseData.Bookings = bookingsSummary;

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
        [Route("bookings/event/{referenceId}")]
        public HttpResponseMessage GetBookingsByEvent(string referenceId)
        {
            GetBookingsByEventResponse responseData = null;

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
                BookingServices bookingService = new BookingServices();
                var bookings = bookingService.GetBookingsByEvent(hostId.Value, referenceId);

                responseData = new GetBookingsByEventResponse();
                responseData.Bookings = bookings;

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
        [Route("bookings/customer/{referenceId}")]
        public HttpResponseMessage GetBookingsByCustomer(string referenceId)
        {
            GetBookingsByCustomerResponse responseData = null;

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
                BookingServices bookingService = new BookingServices();
                var bookings = bookingService.GetBookingsByCustomer(hostId.Value, referenceId);

                responseData = new GetBookingsByCustomerResponse();
                responseData.Bookings = bookings;

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
        [Route("bookings/confirmation/{confirmationNo}")]
        public HttpResponseMessage GetBookingByConfirmationNo(string confirmationNo)
        {
            GetBookingByConfirmationNoResponse responseData = null;

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
                BookingServices bookingService = new BookingServices();
                var bookingDetails = bookingService.GetBookingByConfirmationNo(hostId.Value, confirmationNo);

                if(bookingDetails == null)
                {
                    return Request.CreateResponse(new GenericResponse(responseData, ResponseCodes.RecordNotFound, ResponseMessages.RecordNotFound));
                }
                else
                {
                    responseData = new GetBookingByConfirmationNoResponse();
                    responseData.BookingDetails = bookingDetails;
                    
                    return Request.CreateResponse(new GenericResponse(responseData, ResponseCodes.Success, ResponseMessages.Success));
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error));
            }
        }

        [HttpGet]
        [Route("bookings/ticket/{ticketNo}")]
        public HttpResponseMessage GetTicketByTicketNo(string ticketNo)
        {
            GetTicketsResponse responseData = null;

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
                AdmissionServices admissionService = new AdmissionServices();
                var ticket = admissionService.GetTicketByTicketNo(hostId.Value, ticketNo);

                if(ticket== null)
                {
                    return Request.CreateResponse(new GenericResponse(responseData, ResponseCodes.RecordNotFound, ResponseMessages.RecordNotFound));
                }
                else
                {
                    responseData = new GetTicketsResponse();
                    responseData.Ticket = ticket;
                    
                    return Request.CreateResponse(new GenericResponse(responseData, ResponseCodes.Success, ResponseMessages.Success));
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error));
            }
        }

        [HttpPost]
        [Route("bookings/redeem-ticket")]
        public HttpResponseMessage RedeemTicket(RedeemTicketRequest request)
        {
            RedeemTicketReponse responseData = null;

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
                    string.IsNullOrEmpty(request.TicketNumber))
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, ResponseMessages.InvalidParam));
                }

                //Perform transaction
                AdmissionServices admissionService = new AdmissionServices();
                string status = null;
                responseData = admissionService.UseTicket(hostId.Value, request.TicketNumber, out status);
                
                if (status == RedeemTicketStatuses.TicketNotFound)
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Failed, "TicketNotFound"));
                }
                else if (status == RedeemTicketStatuses.TicketAlreadyUsed)
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Failed, "TicketAlreadyUsed"));
                }
                else if (status == RedeemTicketStatuses.TicketIsInactive)
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.Failed, "TicketIsInactive"));
                }
                else if (status == RedeemTicketStatuses.Success)
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

        [HttpPost]
        [Route("bookings/cancel-ticket")]
        public HttpResponseMessage CancelTicket(CancelTicketRequest request)
        {
            CancelTicketResponse responseData = null;

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
                    string.IsNullOrEmpty(request.TicketNumber))
                {
                    return Request.CreateResponse(new GenericResponse(null, ResponseCodes.InvalidParam, ResponseMessages.InvalidParam));
                }

                //Perform transaction
                EventsServices eventsService = new EventsServices();
                string status = null;
                responseData = eventsService.CancelTicket(hostId.Value, request, out status);
                
                if (status == CancelTicketStatuses.TicketNotFound)
                {
                    return Request.CreateResponse(new GenericResponse(null, CancelTicketStatuses.TicketNotFound, "TicketNotFound"));
                }
                else if (status == CancelTicketStatuses.TicketAlreadyCancelled)
                {
                    return Request.CreateResponse(new GenericResponse(null, CancelTicketStatuses.TicketAlreadyCancelled, "TicketAlreadyCancelled"));
                }
                else if (status == CancelTicketStatuses.TicketAlreadyUsed)
                {
                    return Request.CreateResponse(new GenericResponse(null, CancelTicketStatuses.TicketAlreadyUsed, "TicketAlreadyUsed"));
                }
                else if (status == CancelTicketStatuses.EventNotFound)
                {
                    return Request.CreateResponse(new GenericResponse(null, CancelTicketStatuses.EventNotFound, "EventNotFound"));
                }
                else if (status == CancelTicketStatuses.EventNotActive)
                {
                    return Request.CreateResponse(new GenericResponse(null, CancelTicketStatuses.EventNotActive, "Event is already cancelled or closed."));
                }
                else if (status == CancelTicketStatuses.EventHasAlreadyStarted)
                {
                    return Request.CreateResponse(new GenericResponse(null, CancelTicketStatuses.EventHasAlreadyStarted, "EventHasAlreadyStarted"));
                }
                else if (status == CancelTicketStatuses.CustomerNotFound)
                {
                    return Request.CreateResponse(new GenericResponse(null, CancelTicketStatuses.CustomerNotFound, "CustomerNotFound"));
                }
                else if (status == CancelTicketStatuses.Success)
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
