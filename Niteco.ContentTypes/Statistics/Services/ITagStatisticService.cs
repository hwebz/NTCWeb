using System.Collections.Generic;
using EPiServer.Core;
using Niteco.ContentTypes.Statistics.Models;

namespace Niteco.ContentTypes.Statistics.Services
{
    public interface ITagStatisticService
    {
        IEnumerable<IStatisticItem> GetStatisticItems(ContentReference rootContentReference);
    }
}
