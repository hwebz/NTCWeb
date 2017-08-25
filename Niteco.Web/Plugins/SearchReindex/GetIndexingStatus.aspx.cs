using System;
using System.Linq;
using EPiServer.Data.Dynamic;
using Niteco.Common.Search;

namespace Niteco.Web.App.Plugins.SearchReindex
{
    public partial class GetIndexingStatus : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var count = GetStatus();
            if (count != 0)
            {
                Response.Write(count);
            }
            else
            {
                Response.Write("Done");
            }
        }
        protected int GetStatus()
        {
            int count = 0;

            var store = DynamicDataStoreFactory.Instance.GetStore(SearchSettings.Config.DynamicDataStoreName);

            if (store != null)
            {
                count = store.Items().Count();
            }
            return count;
        }
    }
}