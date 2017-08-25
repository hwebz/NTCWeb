using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Shell.Services.Rest;

namespace Niteco.ContentTypes.RestServices
{
    public interface ITechnologiesListStore
    {
        RestResult Get(string id);
        RestResult GetTags(string pageId);
    }
}
