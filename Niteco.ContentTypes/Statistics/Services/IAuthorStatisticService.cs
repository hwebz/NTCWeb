
using System.Collections.Generic;
using EPiServer.Core;
using Niteco.ContentTypes.Statistics.Models;

namespace Niteco.ContentTypes.Statistics.Services
{
    public interface IAuthorStatisticService
    {
        IEnumerable<IStatisticItem> GetStatisticItems(ContentReference rootContentReference);
    }
}
