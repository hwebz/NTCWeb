
using EPiServer.Core;

namespace Niteco.ContentTypes.Statistics.Interfaces
{
    public interface IHaveTag : IContentData
    {
        string Tags { get; }
    }
}
