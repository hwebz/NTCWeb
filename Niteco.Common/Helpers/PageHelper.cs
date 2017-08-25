using System;
using System.Collections.ObjectModel;
using System.Text;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web;

namespace Niteco.Common.Helpers
{
    public  class PageHelper
    {
        public static ContentReference GetPageFromPageGuid(Guid pageGuid)
        {
            var map = PermanentLinkMapStore.Find(pageGuid) as PermanentContentLinkMap;
            return (map != null) ? map.ContentReference : PageReference.EmptyReference;
        }

        public static string BuildPageVirtualPath(PageData page)
        {
            var nodes = new Collection<string>();

            PageData parent = DataFactory.Instance.GetPage(page.ParentLink);

            // Add all nodes between root node and the page itself
            while (parent.PageLink.ID != ContentReference.RootPage.ID)
            {
                nodes.Insert(0, parent.PageGuid.ToString());
                parent = DataFactory.Instance.GetPage(parent.ParentLink);
            }

            // Add the root node as first node
            nodes.Insert(0, DataFactory.Instance.GetPage(ContentReference.RootPage).PageGuid.ToString());

            // Add the page itself as the last node
            nodes.Add(page.PageGuid.ToString());
            return BuildPageVirtualFromPageNodes(nodes);
        }

        public static string BuildPageVirtualFromPageNodes(Collection<string> nodes)
        {
            var sb = new StringBuilder();
            foreach (var n in nodes)
            {
                sb.Append(string.Format("{0}|", n));
            }
            return sb.ToString().Remove(sb.ToString().Length - 1);
        }

    }
}
