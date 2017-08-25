using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace Niteco.ContentTypes.EditorDescriptors
{
    [EditorDescriptorRegistration(TargetType = typeof(string), UIHint = "blog-category")]
    public class BlogCategoryEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            base.ModifyMetadata(metadata, attributes);
            ClientEditingClass = "epi-cms/contentediting/editors/SelectionEditor";
            SelectionFactoryType = typeof(BlogCategorySelectionFactory);
        }
    }

    public class BlogCategorySelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            var list = new List<SelectItem>();
            var root = ServiceLocator.Current.GetInstance<CategoryRepository>().Get("Blog Category");
            if (root == null)
            {
                return list;
            }
            foreach (var category in root.Categories)
            {
                list.Add(new SelectItem
                {
                    Text = category.LocalizedDescription,
                    Value = category.Name
                });
                list.AddRange(category.Categories.Select(x => new SelectItem
                {
                    Text = x.LocalizedDescription,
                    Value = x.Name
                }));
            }
            return list;
        }
    }
}
