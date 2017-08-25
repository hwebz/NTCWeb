using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Shell;
using Niteco.ContentTypes.Blocks;
using Niteco.ContentTypes.Pages;

namespace Niteco.Web.Business.UIDescriptorRegistration
{
    [UIDescriptorRegistration]
    public class CodeWrapBlockDescriptor : UIDescriptor<CodeWrapBlock>, IEditorDropBehavior
    {
        public EditorDropBehavior EditorDropBehaviour { get; set; }
        public CodeWrapBlockDescriptor()
        {
            EditorDropBehaviour = EditorDropBehavior.CreateContentBlock;
        }
    }
}