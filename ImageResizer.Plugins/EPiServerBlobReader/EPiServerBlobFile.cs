using System;
using System.Collections.Specialized;
using System.IO;
using EPiServer.Core;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace ImageResizer.Plugins.EPiServerBlobReader
{
    public class EPiServerBlobFile : IVirtualFileWithModifiedDate
    {
        private Blob blob;
        private IContent content;
        protected readonly ContentRouteHelper ContentRouteHelper;

        public EPiServerBlobFile(string virtualPath, NameValueCollection queryString)
            : this(virtualPath, queryString, ServiceLocator.Current.GetInstance<ContentRouteHelper>())
        {
        }

        public EPiServerBlobFile(string virtualPath, NameValueCollection queryString, ContentRouteHelper contentRouteHelper)
        {
            ContentRouteHelper = contentRouteHelper;
            VirtualPath = virtualPath;
            QueryString = queryString;
        }

        public NameValueCollection QueryString { get; private set; }

        public Blob Blob
        {
            get { return blob ?? (blob = GetBlob()); }
        }

        public IContent Content
        {
            get
            {
                if (content != null)
                {
                    return content;
                }
                content = ContentRouteHelper.Content;
                if (!content.QueryDistinctAccess(AccessLevel.Read))
                {
                    content = null;
                }
                return content;
            }
        }

        public bool BlobExists
        {
            get { return Content != null; }
        }

        public Stream Open()
        {
            return Blob != null ? Blob.OpenRead() : null;
        }

        public string VirtualPath { get; private set; }

        public DateTime ModifiedDateUTC
        {
            get
            {
                if (Content != null)
                {
                    var trackable = Content as IChangeTrackable;
                    if (trackable != null)
                    {
                        return trackable.Changed.ToUniversalTime();
                    }
                }
                return DateTime.MinValue.ToUniversalTime();
            }
        }

        protected Blob GetBlob()
        {
            if (Content == null)
            {
                return null;
            }
            var binaryStorable = Content as IBinaryStorable;
            if (binaryStorable == null || binaryStorable.BinaryData == null)
            {
                return null;
            }
            return binaryStorable.BinaryData;
        }
    }
}

