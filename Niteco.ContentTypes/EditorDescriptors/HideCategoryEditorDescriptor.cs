using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using Niteco.ContentTypes.Blocks;
using Niteco.ContentTypes.Pages;

namespace Niteco.ContentTypes.EditorDescriptors
{
    [EditorDescriptorRegistration(TargetType = typeof(CategoryList))]
    public class HideCategoryEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            base.ModifyMetadata(metadata, attributes);
            dynamic mayQuack = metadata;
            var ownerContent = mayQuack.OwnerContent;
            if ((ownerContent is BlogQuoteBlock || ownerContent is CodeWrapBlock ||  ownerContent is AuthorPage || ownerContent is NewsDetailPage || ownerContent is NewsListingPage || ownerContent is NewsQuoteBlock || ownerContent is BlogListingPage || ownerContent is BlogDetailPage) && metadata.PropertyName == "icategorizable_category")
            {
                metadata.ShowForEdit = false;
            }
        }
    }
}
