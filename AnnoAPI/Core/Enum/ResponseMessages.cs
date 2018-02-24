using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnoAPI.Core.Enum
{
    public static class ResponseMessages
    {
        public static string Success = "Success";
        public static string Error = "The server is unable to process your request at the moment.";

        public static string InvalidAPIKey = "The specified API key is invalid.";
        public static string InvalidParam = "The parameters specified is invalid or incomplete. Please refer to the API documentation for usage.";
        public static string RecordNotFound = "Record not found.";
        public static string DuplicateRefId = "The reference ID specified already exists. Reference ID must be unique.";
    }
}