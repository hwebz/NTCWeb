using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Web.Hosting;
using Niteco.Common.Search.Queries.Lucene;

namespace Niteco.Search.Queries.Lucene
{
    public static class VirtualPathQueryExtensions
    {
        public static void AddContentNodes(this VirtualPathQuery query, ContentReference contentLink,
            IContentLoader contentLoader)
        {
            if (ContentReference.IsNullOrEmpty(contentLink))
            {
                return;
            }
            Validator.ThrowIfNull("contentLoader", contentLoader);
            foreach (string current in LuceneContentSearchHandler.GetVirtualPathNodes(contentLink, contentLoader))
            {
                query.VirtualPathNodes.Add(current);
            }
        }

        //public static void AddDirectoryNodes(this VirtualPathQuery query, VersioningDirectory directory)
        //{
        //    if (directory == null)
        //    {
        //        return;
        //    }
        //    foreach (string current in VersioningFileSystemSearchHandler.GetVirtualPathNodes(directory))
        //    {
        //        query.VirtualPathNodes.Add(current);
        //    }
        //}
    }
}
