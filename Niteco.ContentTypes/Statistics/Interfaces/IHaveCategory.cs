using EPiServer.Core;

namespace Niteco.ContentTypes.Statistics.Interfaces
{
    public interface IHaveCategory : IContentData
    {
        string BlogCategory { get; }
    }
}
