using EPiServer.Core;

namespace Niteco.ContentTypes.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}
