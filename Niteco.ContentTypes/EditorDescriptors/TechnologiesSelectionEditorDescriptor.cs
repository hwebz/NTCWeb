using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using Niteco.Common.Consts;

namespace Niteco.ContentTypes.EditorDescriptors
{
    [EditorDescriptorRegistration(TargetType = typeof(string), UIHint = SiteUIHints.TechnologiesList)]
    public class TechnologiesSelectionEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            //SelectionFactoryType = typeof(LayoutListSelectionFactory);
            ClientEditingClass = "niteco/editors/TechnologiesSelection";
            base.ModifyMetadata(metadata, attributes);
        }
    }
}
