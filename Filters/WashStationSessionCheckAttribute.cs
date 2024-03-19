using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

public class WashStationSessionCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        int? washStationId = context.HttpContext.Session.GetInt32("washStationId");
        if (washStationId == null)
        {
            context.Result = new RedirectToActionResult("LogRegWash", "WashStation", null);
        }
    }
}

