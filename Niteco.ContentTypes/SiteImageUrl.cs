using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes
{
    /// <summary>
    /// Attribute to set the default thumbnail for site page and block types
    /// </summary>
    public class SiteImageUrl : ImageUrlAttribute
    {
        /// <summary>
        /// The parameterless constructor will initialize a SiteImageUrl attribute with a default thumbnail
        /// </summary>
        public SiteImageUrl()
            : base("~/ClientResources/Images/icons/pagetypes/page-type-thumbnail.png")
        {

        }

        public SiteImageUrl(string path)
            : base(path)
        {

        }
    }
}
