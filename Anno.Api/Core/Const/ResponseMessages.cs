using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Anno.Api.Core.Const
{
    public static class ResponseMessages
    {
        public const string Success = "Success";
        public const string Error = "The server is unable to process your request at the moment.";

        public const string InvalidAPIKey = "The specified API key is invalid.";
        public const string InvalidParam = "The parameters specified is invalid or incomplete. Please refer to the API documentation for usage.";
        public const string RecordNotFound = "Record not found.";
        public const string DuplicateRefId = "The reference ID specified already exists. Reference ID must be unique.";
    }
}