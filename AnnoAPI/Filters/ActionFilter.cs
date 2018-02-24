using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Collections.Generic;
using System.Linq;
using AnnoAPI.Core;

namespace AnnoAPI.Filters
{
    public class ActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            LogRequest(actionContext);
            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
        }

        private void LogRequest(HttpActionContext actionContext)
        {
            if(Config.LogRequests)
            {
                string logMessage = "Request: ({0}) {1}";
                Log.Info(logMessage, actionContext.Request.Method.ToString(), actionContext.Request.RequestUri.ToString());
            }
        }
    }
}