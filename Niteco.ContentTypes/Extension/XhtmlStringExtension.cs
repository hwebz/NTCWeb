using System.Linq;
using System.Text;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Niteco.ContentTypes.Blocks;
using Niteco.ContentTypes.Pages;

namespace Niteco.ContentTypes.Extension
{
    public static class XhtmlStringExtension
    {
        public static string ToBlocksIncludedString(this XhtmlString xhtmlString)
        {
            var sb = new StringBuilder();
            if (xhtmlString != null && xhtmlString.Fragments.Any())
            {
                var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
                foreach (IStringFragment fragment in xhtmlString.Fragments.GetFilteredFragments(PrincipalInfo.AnonymousPrincipal))
                {
                    if (fragment is ContentFragment)
                    {
                        var contentFragment = fragment as ContentFragment;
                        if (contentFragment.ContentLink != null &&
                            contentFragment.ContentLink != ContentReference.EmptyReference)
                        {
                            var referencedContent = contentLoader.Get<IContent>(contentFragment.ContentLink);
                            //                            sb.Append(referencedContent.SearchText + " ");
                            if (referencedContent is CodeWrapBlock)
                            {
                                var searchText = (referencedContent as CodeWrapBlock).Content;
                                if (!string.IsNullOrWhiteSpace(searchText))
                                {
                                    sb.Append(searchText + " ");
                                }
                            }
                            else if (referencedContent is BlogQuoteBlock)
                            {
                                var searchText = (referencedContent as BlogQuoteBlock).Quote;
                                if (!string.IsNullOrWhiteSpace(searchText))
                                {
                                    sb.Append(searchText + " ");
                                }
                            }
                        }
                    }
                    else if (fragment is StaticFragment)
                    {
                        var staticFragment = fragment as StaticFragment;
                        sb.Append(staticFragment.InternalFormat + " ");
                    }
                }
            }
            return sb.ToString();
        }
    }
}
