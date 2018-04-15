using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Anno.Api.Core.Const
{
    public static class ResponseCodes
    {
        public const string Success = "1";
        public const string SuccessWithoutBlockchain = "2";
        public const string Failed = "5";

        public const string InvalidAPIKey = "20";
        public const string InvalidParam = "21";
        public const string DuplicateRefId = "25";
        public const string RecordNotFound = "33";
        public const string Error = "500";
    }
}