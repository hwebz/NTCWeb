using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Niteco.Common.Consts;
using Niteco.ContentTypes.Blocks;
using Niteco.ContentTypes.Pages;
using Niteco.Web.Controllers;
using EPiServer.Web;
using EPiServer.Web.Mvc;

namespace Niteco.Web.Business.Rendering
{
    [ServiceConfiguration(typeof(IViewTemplateModelRegistrator))]
    public class TemplateCoordinator : IViewTemplateModelRegistrator
    {
        public const string BlockFolder = "~/Views/Shared/Blocks/";
        public const string PagePartialsFolder = "~/Views/Shared/PagePartials/";

        public static void OnTemplateResolved(object sender, TemplateResolverEventArgs args)
        {
            //Disable DefaultPageController for page types that shouldn't have any renderer as pages
            if (args.ItemToRender is IContainerPage && args.SelectedTemplate != null && args.SelectedTemplate.TemplateType == typeof(DefaultPageController))
            {
                args.SelectedTemplate = null;
            }
        }

        /// <summary>
        /// Registers renderers/templates which are not automatically discovered, 
        /// i.e. partial views whose names does not match a content type's name.
        /// </summary>
        /// <remarks>
        /// Using only partial views instead of controllers for blocks and page partials
        /// has performance benefits as they will only require calls to RenderPartial instead of
        /// RenderAction for controllers.
        /// Registering partial views as templates this way also enables specifying tags and 
        /// that a template supports all types inheriting from the content type/model type.
        /// </remarks>
        public void Register(TemplateModelCollection viewTemplateModelRegistrator)
        {
            viewTemplateModelRegistrator.Add(typeof(SitePageData), new TemplateModel
            {
                Name = "SitePageOnLeft",
                Inherit = true,
                Tags = new[] { ContentAreaTags.SitePageOnLeft },
                AvailableWithoutTag = false,
                Path = PagePartialPath("SitePageOnLeft.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(SitePageData), new TemplateModel
            {
                Name = "SitePageOnRight",
                Inherit = true,
                Tags = new[] { ContentAreaTags.SitePageOnRight },
                AvailableWithoutTag = false,
                Path = PagePartialPath("SitePageOnRight.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(SitePageData), new TemplateModel
            {
                Name = "SitePageOnBottom",
                Inherit = true,
                Tags = new[] { ContentAreaTags.SitePageOnBottom },
                AvailableWithoutTag = false,
                Path = PagePartialPath("SitePageOnBottom.cshtml")
            });


            viewTemplateModelRegistrator.Add(typeof(CaseStudyPage), new TemplateModel
            {
                Name = "CaseStudyPagePartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = PagePartialPath("CaseStudyPagePartial.cshtml")
            });
           
            viewTemplateModelRegistrator.Add(typeof(StandardPage), new TemplateModel
            {
                Name = "StandardPage",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = PagePartialPath("StandardPagePartial.cshtml")
            });
            viewTemplateModelRegistrator.Add(typeof(ClientQuoteItemBlock), new TemplateModel
            {
                Name = "ClientQuoteItemBlockPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("ClientQuoteItemBlock.cshtml")
            });
            viewTemplateModelRegistrator.Add(typeof(HeadingBlock), new TemplateModel
            {
                Name = "HeadingBlockPartial",
                Inherit = true,
                AvailableWithoutTag = false,
                Path = BlockPath("HeadingBlock.cshtml")
            });
            viewTemplateModelRegistrator.Add(typeof(ServicePage), new TemplateModel
            {
                Name = "SmallServicePage",
                Inherit = true,
                Tags = new[] { ContentAreaTags.SmallServicePartial },
                AvailableWithoutTag = false,
                Path = PagePartialPath("SmallServicePagePartial.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(ServicePage), new TemplateModel
            {
                Name = "WideServicePage",
                Inherit = true,
                Tags = new[] { ContentAreaTags.WideServicePartial },
                AvailableWithoutTag = false,
                Path = PagePartialPath("WideServicePagePartial.cshtml")
            });
            viewTemplateModelRegistrator.Add(typeof(ServicePage), new TemplateModel
            {
                Name = "Technologies",
                Inherit = true,
                Tags = new[] { ContentAreaTags.TechnologiesPartial },
                AvailableWithoutTag = false,
                Path = PagePartialPath("TechnologiesPartial.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(ContactUsPage), new TemplateModel
            {
                Name = "ContactUsPagePartial",
                Inherit = true,
                AvailableWithoutTag = false,
                Path = PagePartialPath("ContactUsPage.cshtml")
            });
         
            viewTemplateModelRegistrator.Add(typeof(AboutUsPage), new TemplateModel
            {
                Name = "AboutUsPagePartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = PagePartialPath("AboutUsPagePartial.cshtml")
            });
          
            viewTemplateModelRegistrator.Add(typeof(OfficeBlock), new TemplateModel
            {
                Name = "OfficeBlockPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("OfficeBlock.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(ClientQuoteBlock), new TemplateModel
            {
                Name = "ClientQuoteBlockPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("ClientQuoteBlock.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(OurValuesBlock), new TemplateModel
            {
                Name = "OurValuesBlockPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("OurValuesBlock.cshtml")
            });
            viewTemplateModelRegistrator.Add(typeof(BlogQuoteBlock),new TemplateModel
           {
               Name = "InlineBlogQuoteBlock",
               AvailableWithoutTag = false,
               Tags = new[] { "InlineContent" },
                Path = BlockPath("BlogQuoteBlock.cshtml")
                //               TemplateTypeCategory = TemplateTypeCategories.MvcPartialController
            });
            viewTemplateModelRegistrator.Add(typeof(AboutUsInfoItemBlock), new TemplateModel
            {
                Name = "AboutUsInfoItemBlockPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("AboutUsInfoItemBlock.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(MissionItemBlock), new TemplateModel
            {
                Name = "MissionItemBlockPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("MissionItemBlock.cshtml")
            });
            
            viewTemplateModelRegistrator.Add(typeof(AboutUsOfficeBlock), new TemplateModel
            {
                Name = "AboutUsOfficeBlockPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("AboutUsOfficeBlock.cshtml")
            });
            viewTemplateModelRegistrator.Add(typeof(JobPage), new TemplateModel
            {
                Name = "JobPagePartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = PagePartialPath("JobPagePartial.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(CommunityPartnerItemBlock), new TemplateModel
            {
                Name = "CommunityPartnerItemBlockPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("CommunityPartnerItemBlock.cshtml")
            });
            viewTemplateModelRegistrator.Add(typeof(PeopleBlock), new TemplateModel
            {
                Name = "PeopleBlockPartial",
                Inherit = true,
                Tags = new[] { ContentAreaTags.PeopleBlockInfo },
                AvailableWithoutTag = false,
                Path = PagePartialPath("PeopleBlockPartial.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(CommunityPartnerBlock), new TemplateModel
            {
                Name = "CommunityPartnerBlockPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("CommunityPartnerBlock.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(OfficeStayConnectedBlock), new TemplateModel
            {
                Name = "OfficeStayConnectedBlockPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("OfficeStayConnectedBlock.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(CaseStudyDetailBlock), new TemplateModel
            {
                Name = "CaseStudyDetailBlockPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("CaseStudyDetailBlock.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(VideoDemoBlock), new TemplateModel
            {
                Name = "VideoDemoBlockPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("VideoDemoBlock.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(VideoDemoBlockItem), new TemplateModel
            {
                Name = "VideoDemoBlockItemPartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = BlockPath("VideoDemoBlockItem.cshtml")
            });
 
        }

        private static string BlockPath(string fileName)
        {
            return string.Format("{0}{1}", BlockFolder, fileName);
        }

        private static string PagePartialPath(string fileName)
        {
            return string.Format("{0}{1}", PagePartialsFolder, fileName);
        }
    }
}
