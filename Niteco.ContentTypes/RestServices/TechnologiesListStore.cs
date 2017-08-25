using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using Niteco.ContentTypes.Pages;

namespace Niteco.ContentTypes.RestServices
{
    [ServiceConfiguration(ServiceType = typeof(ITechnologiesListStore), Lifecycle = ServiceInstanceScope.PerRequest)]
    [RestStore("technologiesstore")]
    public class TechnologiesListStore: RestControllerBase,ITechnologiesListStore
    {
        public RestResult Get(string id)
        {
            return Rest(null);
        }

        public RestResult GetTags(string pageId)
        {
            var repository = ServiceLocator.Current.GetInstance<IContentRepository>();
            var splits = pageId.Split('_');
            if (splits.Length > 0)
                pageId = splits[0];

            var page = repository.Get<ServicePage>(new ContentReference(pageId));
            var returnaerr = !string.IsNullOrEmpty(page.Technologies) ? page.Technologies.Split(',') : new[] { string.Empty };
            
            return Rest(returnaerr);
        }
    }
}
