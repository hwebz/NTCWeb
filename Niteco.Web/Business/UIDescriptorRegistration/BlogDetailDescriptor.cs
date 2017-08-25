using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Shell;
using Niteco.ContentTypes.Pages;

namespace Niteco.Web.Business.UIDescriptorRegistration
{
    [UIDescriptorRegistration]
    public class BlogDetailDescriptor : UIDescriptor<BlogDetailPage>, IEditorDropBehavior
    {
        public EditorDropBehavior EditorDropBehaviour { get; set; }
        public BlogDetailDescriptor()
        {
            EditorDropBehaviour = EditorDropBehavior.CreateContentBlock;
        }
    }
}