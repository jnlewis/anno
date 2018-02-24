using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnoAPI.Core.Enum
{
    public static class ResponseCodes
    {
        public static string Success = "1";
        public static string SuccessWithoutBlockchain = "2";
        public static string Failed = "5";

        public static string InvalidAPIKey = "20";
        public static string InvalidParam = "21";
        public static string DuplicateRefId = "25";
        public static string RecordNotFound = "33";
        public static string Error = "500";
    }
}