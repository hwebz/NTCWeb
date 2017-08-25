using Microsoft.Owin;
using Owin;
using Niteco.Web.Business;
using WebMarkupMin.AspNet4.Common;

[assembly: OwinStartup(typeof(Niteco.Web.Startup))]

namespace Niteco.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            WebMarkupMinConfig.Configure(WebMarkupMinConfiguration.Instance);
        }
    }
}
