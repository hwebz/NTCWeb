using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.DataAnnotations;

namespace Niteco.Common.Consts
{
    public class SiteConst
    {
        public static Dictionary<string, int> ContentAreaTagWidths = new Dictionary<string, int>
            {
                { ContentAreaTags.FullWidth, ContentAreaWidths.FullWidth },
                { ContentAreaTags.TwoThirdsWidth, ContentAreaWidths.TwoThirdsWidth },
                { ContentAreaTags.HalfWidth, ContentAreaWidths.HalfWidth },
                { ContentAreaTags.OneThirdWidth, ContentAreaWidths.OneThirdWidth }
            };

        /// <summary>
        /// Virtual path to folder with static graphics, such as "~/Static/gfx/"
        /// </summary>
        public const string StaticGraphicsFolderPath = "~/Static/gfx/";
        public const string SiteSettingCacheKey = "Niteco.SiteSetting";
        public const string TinyMCE_EditorInitConfigurationOptions = "{ forced_root_block : false}";
    }
    /// <summary>
    /// Group names for content types and properties
    /// </summary>
    [GroupDefinitions()]
    public static class GroupNames
    {
        [Display(Name = "Contact", Order = 10)]
        public const string Contact = "Contact";

        [Display(Name = "Default", Order = 20)]
        public const string Default = "Default";

        [Display(Name = "Metadata", Order = 30)]
        public const string MetaData = "Metadata";

        [Display(Name = "News", Order = 40)]
        public const string News = "News";

        [Display(Name = "Teaser", Order = 50)]
        public const string Teaser = "Teaser";

        [Display(Name = "Products", Order = 60)]
        public const string Products = "Products";

        [Display(Name = "SiteSettings", Order = 70)]
        public const string SiteSettings = "SiteSettings";

        [Display(Name = "Header", Order = 80)]
        public const string Header = "Header";

        [Display(Name = "Footer", Order = 90)]
        public const string Footer = "Footer";

        [Display(Name = "Specialized", Order = 100)]
        public const string Specialized = "Specialized";

        [Display(Name = "AboutUs", Order = 110)]
        public const string AboutUs = "AboutUs";

        [Display(Name = "Career", Order = 120)]
        public const string Career = "Career";

        [Display(Name = "HowWeWork", Order = 120)]
        public const string HowWeWork = "How We Work";

        [Display(Name = "Nis", Order = 120)]
        public const string Nis = "Nis";

        [Display(Name = "Thumbnail", Order = 130)]
        public const string Thumbnail = "Thumbnail";

        [Display(Name = "Subscription", Order = 140)]
        public const string Subscription = "Subscription";

        [Display(Name = "News Content", Order = 1)]
        public const string NewsContent = "News Content";

        [Display(Name = "Categories", Order = 1)]
        public const string Categories = "Categories";

        [Display(Name = "Social", Order = 70)]
        public const string Social = "Social";

        [Display(Name = "Right Side Bar", Order = 70)]
        public const string RightColumnnTitle = "Right Columnn Title";

        [Display(Name = "Comment config", Order = 140)]
        public const string CommentConfig = "Comment config";
    }

    /// <summary>
    /// Tags to use for the main widths used in the Bootstrap HTML framework
    /// </summary>
    public static class ContentAreaTags
    {
        public const string FullWidth = "span12";
        public const string TwoThirdsWidth = "span8";
        public const string HalfWidth = "span6";
        public const string OneThirdWidth = "span4";
        public const string NoRenderer = "norenderer";

        public const string WideServicePartial = "WideServicePartial";
        public const string SmallServicePartial = "SmallServicePartial";
        public const string TechnologiesPartial = "TechnologiesPartial";

        public const string NoWrappersContentArea = "NoWrappersContentArea";
        public const string SitePageOnLeft = "SitePageOnLeft";
        public const string SitePageOnRight = "SitePageOnRight";
        public const string SitePageOnBottom = "SitePageOnBottom";
        public const string PeopleBlockInfo = "PeopleBlockInfo";
    }

    /// <summary>
    /// Main widths used in the Bootstrap HTML framework
    /// </summary>
    public static class ContentAreaWidths
    {
        public const int FullWidth = 12;
        public const int TwoThirdsWidth = 8;
        public const int HalfWidth = 6;
        public const int OneThirdWidth = 4;
    }

    /// <summary>
    /// Names used for UIHint attributes to map specific rendering controls to page properties
    /// </summary>
    public static class SiteUIHints
    {
        public const string Contact = "contact";
        public const string Strings = "StringList";
        public const string ServiceList = "ServiceList";
        public const string TechnologiesList = "TechnologiesList";
    }


    public class CategoryConstants
    {
        public const string BlogCategories = "Blog Categories";
    }

}
