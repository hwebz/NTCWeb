using EPiServer.Core;

namespace Niteco.ContentTypes.Statistics.Interfaces
{
    public interface IHaveAuthor : IContentData
    {
        ContentReference Author { get; }
    }
}
