using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Editor.TinyMCE;
using EPiServer.Web;
using Niteco.Common.Consts;
using Niteco.Common.Search.Custom;
using Niteco.ContentTypes.Blocks;
using Niteco.ContentTypes.CustomProperties;

namespace Niteco.ContentTypes.Pages
{
    /// <summary>
    /// Base class for all page types
    /// </summary>
    /// 
    [TinyMCEPluginNonVisual(AlwaysEnabled = true, PlugInName = "nonbreaking", EditorInitConfigurationOptions = SiteConst.TinyMCE_EditorInitConfigurationOptions)]
    public abstract class SitePageData : PageData, ISearchable
    {
        #region Search tab

        [Display(
            Name = "Include this page in search index",
            Description = "If unchecked, this page will not be added to the search index.",
            GroupName = GroupNames.SiteSettings)
          ]
        
        public virtual bool IsSearchable { get; set; }

        [Display(
            Name = "Allow index children",
            Description = "Include searchable child pages in search index.",
             GroupName = GroupNames.SiteSettings)]
        public virtual bool AllowIndexChildren { get; set; }

        #endregion

        #region Metadata
        [Display(
            GroupName = GroupNames.MetaData,
            Order = 100)]
        [CultureSpecific]
        public virtual string MetaTitle
        {
            get
            {
                var metaTitle = this.GetPropertyValue(p => p.MetaTitle);

                // Use explicitly set meta title, otherwise fall back to page name
                return !string.IsNullOrWhiteSpace(metaTitle)
                       ? metaTitle
                       : PageName;
            }
            set { this.SetPropertyValue(p => p.MetaTitle, value); }
        }

        [Display(
            GroupName = GroupNames.MetaData,
            Order = 200)]
        [CultureSpecific]
        [BackingType(typeof(PropertyStringList))]
        public virtual string[] MetaKeywords { get; set; }

        [Display(
            GroupName = GroupNames.MetaData,
            Order = 300)]
        [CultureSpecific]
        [UIHint(UIHint.LongString)]
        public virtual string MetaDescription { get; set; }

        [Display(
            GroupName = GroupNames.MetaData,
            Order = 400)]
        
        public virtual bool DisableIndexing { get; set; }

        [Display(
            GroupName = GroupNames.MetaData,
            Order = 500)]
        
        public virtual string HeadScripts { get; set; }

        [Display(
            GroupName = GroupNames.MetaData,
            Order = 600)]
        
        public virtual string BodyScripts { get; set; }

        [Display(
            GroupName = GroupNames.MetaData,
            Order = 700)]
        
        public virtual string FooterScripts { get; set; }
        #endregion

        #region content
        [Display(GroupName = SystemTabNames.Content, Order = 10)]
        [CultureSpecific]
        public virtual string Heading { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 30)]
        [CultureSpecific]
        public virtual XhtmlString ShortDescription { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 40)]
        [UIHint(UIHint.Image)]
        public virtual Url BackgroundImage { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 41)]
        [UIHint(UIHint.Image)]
        public virtual Url MediumBackgroundUrl { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 42)]
        [UIHint(UIHint.Image)]
        public virtual Url SmallBackgroundUrl { get; set; }


        [Display(GroupName = SystemTabNames.Content, Order = 50)]
        public virtual HtmlBlock MainContent { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 500)]
        [AllowedTypes(typeof(SitePageData))]
        public virtual ContentArea BottomContentArea { get; set; }

        [CultureSpecific]
        [ScaffoldColumn(false)]
        [Searchable]        
        public virtual string SearchText { get; set; }
        #endregion

        #region teaser
        [Display(GroupName = GroupNames.Teaser, Order = 10)]
        [CultureSpecific]
        public virtual string TeaserTitle { get; set; }

        [Display(GroupName = GroupNames.Teaser, Order = 20)]
        [CultureSpecific]
        public virtual string TeaserSubTitle { get; set; }

     
        [Display(GroupName = GroupNames.Teaser, Order = 30)]
        [UIHint(UIHint.Image)]
        public virtual Url TeaserBackgroundImage { get; set; }

        [Display(GroupName = GroupNames.Teaser, Order = 35)]
        [UIHint(UIHint.Image)]
        public virtual Url HoverImage { get; set; }
        #endregion
        [Ignore]
        public virtual int TeaserOrder { get; set; }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Heading))
                    return this.Heading;
                return this.PageName;
            }
        }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            this.IsSearchable = true;
            this.AllowIndexChildren = true;
        }

//        public override void AccessDenied()
//        {
//            // Important! Do not access CurrentPage directly with an anonymous user
//            PageData currentPage = DataFactory.Instance.GetPage(this.CurrentPageLink);
//
//            if (!this.IsEditOrPreviewMode && (!currentPage.CheckPublishedStatus(PagePublishedStatus.Published) || currentPage.IsDeleted))
//            {
//                Response.Status = "404 Not Found";
//                Response.StatusCode = 404;
//                Response.End();
//            }
//
//            base.AccessDenied();
//        }
    }
}
