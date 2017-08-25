using EPiServer.Editor;
using EPiServer.Shell;
using Niteco.ContentTypes.Pages;


namespace Niteco.Web.Business.UIDescriptors
{
    /// <summary>
    /// Describes how the UI should appear for <see cref="Niteco.ContentTypes.Pages.ContainerPage"/> content.
    /// </summary>
    [UIDescriptorRegistration]
    public class ContainerPageUIDescriptor : UIDescriptor<ContainerPage>
    {
        public ContainerPageUIDescriptor()
            : base(ContentTypeCssClassNames.Container)
        {
            DefaultView = CmsViewNames.AllPropertiesView;
        }
    }
}
