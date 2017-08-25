using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.Editor;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;

namespace Niteco.Web.Business
{
    public class ExpiredContentFilter : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // don't throw 404 in edit mode
            if (!PageEditing.PageIsInEditMode)
            {
                var pageContext = ServiceLocator.Current.GetInstance<PageRouteHelper>();
                if (pageContext.Page.StopPublish <= DateTime.Now)
                {
                    filterContext.Result = new HttpStatusCodeResult(404, "Not found");
                    return;
                }
            }

            base.OnAuthorization(filterContext);
        }
    }
}