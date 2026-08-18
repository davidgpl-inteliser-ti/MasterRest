using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace MasterRest.Helpers
{
    public class SessionExpireAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.Session["MR"] == null)
            {
                HttpContext.Current.Session.RemoveAll();

                HttpContext.Current.Session.Abandon();

                FormsAuthentication.SignOut();

                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "Index" }, { "controller", "Sesion" } });

                return;
            }
        }
    }
}