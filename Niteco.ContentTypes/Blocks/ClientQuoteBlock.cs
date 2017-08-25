using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using Niteco.Common.Consts;
using Niteco.Common.Enums;
using Niteco.ContentTypes.EditorDescriptors;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.Default,
      GUID = "ABC1FF36-F142-4D7D-9263-67689601F17D")]
    public class ClientQuoteBlock : SiteBlockData
    {
        [AllowedTypes(typeof(ClientQuoteItemBlock))]
        [Display(GroupName = SystemTabNames.Content, Order = 10)]
        public virtual ContentArea ClientQuoteItems
        {
            get;
            set;
        }
    }
}
