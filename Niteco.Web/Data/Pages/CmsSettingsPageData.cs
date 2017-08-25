using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using Niteco.Common.Data;

namespace Niteco.Web.Data.Pages
{
    [ContentType   (DisplayName = "Cms Settings Page",
        GUID = "bb8d7d4e-d83c-4102-85ef-2435584f1117",  
        Description = "General settings for the site",
        GroupName = PageGroups.Setting.Name,
        Order = PageGroups.Setting.Order
    )]
    public class CmsSettingsPageData : PageData
    {
        /*
                [CultureSpecific]
                [Display(
                    Name = "Main body",
                    Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
                    GroupName = SystemTabNames.Content,
                    Order = 1)]
                public virtual XhtmlString MainBody { get; set; }
         */
    }
}