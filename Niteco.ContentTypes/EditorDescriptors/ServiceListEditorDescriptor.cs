using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using Niteco.Common.Consts;
using Niteco.ContentTypes.Pages;

namespace Niteco.ContentTypes.EditorDescriptors
{
    [EditorDescriptorRegistration(TargetType = typeof(string), UIHint = SiteUIHints.ServiceList)]
    public class ServiceListEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            SelectionFactoryType = typeof(ServiceListSelectionFactory);
            ClientEditingClass = "epi-cms/contentediting/editors/CheckBoxListEditor";
            base.ModifyMetadata(metadata, attributes);
        }
    }

    public class ServiceListSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            var settingPage = SiteSettingsHandler.Instance.SiteSettings;
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            if (settingPage != null && settingPage.ServiceListRoot!=null)
            {
                var services = contentRepository.GetChildren<ServicePage>(settingPage.ServiceListRoot);
                return services.Select(x => new SelectItem() {Value = x.ContentLink.ID.ToString(), Text = x.DisplayName});
            }
           
            return new List<ISelectItem>();
        }
    }
}
