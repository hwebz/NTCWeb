using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using Niteco.ContentTypes.Pages;

namespace Niteco.ContentTypes.Blocks
{
    [ContentType(DisplayName = "NisServicesBlock", GUID = "b0e20426-35dd-4909-8589-579d9e6ab307", Description = "")]
    public class NisServicesBlock : SiteBlockData
    {
        [Display(GroupName = SystemTabNames.Content, Order = 70)]
        [AllowedTypes(typeof(NisServiceItemBlock))]
        public virtual ContentArea ServiceItems{get;set;}
    }
}