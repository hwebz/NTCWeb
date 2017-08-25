using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer;
using EPiServer.Core;
using Niteco.Web.Controllers;

namespace Niteco.Web
{
    public class EPiServerApplication : EPiServer.Global
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //Tip: Want to call the EPiServer API on startup? Add an initialization module instead (Add -> New Item.. -> EPiServer -> Initialization Module)
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            if (Context.IsCustomErrorEnabled)
            {
                ShowCustomErrorPage(Server.GetLastError());
            }
            
        }     

        protected override void RegisterRoutes(RouteCollection routes)
        {
            base.RegisterRoutes(routes);
            routes.MapRoute(
              "GetSearchResult", // Route name
              "SearchPage/GetSearchResult", // URL with parameters
              new { controller = "SearchPage", action = "GetSearchResult", id = UrlParameter.Optional } // Parameter defaults
           );
           routes.MapRoute(
              "GetBlogSearch", // Route name
              "BlogListingPage/GetSearchResult", // URL with parameters
              new { controller = "SearchPage", action = "GetSearchResult", id = UrlParameter.Optional } // Parameter defaults
           );
        }
        private void ShowCustomErrorPage(Exception exception)
        {
            var httpException = exception as HttpException ?? new HttpException(500, "Internal Server Error", exception);
            Response.Clear();
            var routeData = new RouteData();
            routeData.Values.Add("controller", "Error");
            routeData.Values.Add("fromAppErrorEvent", true);
            switch (httpException.GetHttpCode())
            {
                case 500:
                    routeData.Values.Add("action", "InternalServerError");
                    break;
                default:
                    routeData.Values.Add("action", "OtherHttpStatusCode");
                    routeData.Values.Add("httpStatusCode", httpException.GetHttpCode());
                    break;
            }
            Server.ClearError();

            IController controller = new ErrorController();

            controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
        }
    }
}