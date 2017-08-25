using EPiServer;
using EPiServer.Core;

namespace Niteco.ContentTypes.Pages
{
    public interface ISiteSettings
    {
        Url LogoImage { get; set; }
        string Slogan { get; set; }
        ContentArea MenuItemsContent { get; set; }
        Url FacebookLink { get; set; }
        Url LinkedinLink { get; set; }
        Url TwitterLink { get; set; }
        string Email { get; set; }
        ContentReference ServiceListRoot { get; set; }
        ContentReference SearchPageLink { get; set; }
        ContentArea FooterItemsContent { get; set; }
        ContentArea PartnerImages { get; set; }
        string FooterCopyright { get; set; }
        string GlobalHeadScripts { get; set; }
        string GlobalBodyScripts { get; set; }
        string GlobalFooterScripts { get; set; }

        string EmailAdmin { get; set; }
        string AssetsVersion { get; set; }
        string ListCountry { get; set; }
        string SendgridApiKey { get; set; }
        string MailChimpApiKey { get; set; }
        string MailChimpListId { get; set; }
        string SubscriptionText { get; set; }
        string SubscriptionDescription { get; set; }
        string SubcriptionUrlPost { get; set; }

        string CommentConfig { get; set; }
        ContentReference AuthorContainer { get; }
    }
}
