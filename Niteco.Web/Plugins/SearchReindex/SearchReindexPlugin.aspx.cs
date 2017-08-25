using System;
using System.Linq;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;
using Niteco.Common.Search;
using Niteco.Search;

namespace Niteco.Web.App.Plugins.SearchReindex
{
    [GuiPlugIn(
     DisplayName = "Search Reindex",
     Description = "This is used for re-indexing page and document for search",
     Area = PlugInArea.AdminMenu,
     Url = "~/Plugins/SearchReindex/SearchReindexPlugin.aspx")]
    public partial class SearchReindexPlugin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //this.TotalItems.Text = string.Format("Total item(s): {0}", SearchSettings.GetDynamicDataStore().Items().Count());   
        }

        protected void ReIndex_Click(object sender, EventArgs e)
        {
            var contentSearchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();
            contentSearchHandler.ReIndex();
            this.ReIndex.Enabled = false;
            var totalItems = SearchSettings.GetDynamicDataStore().Items().Count();
            this.TotalItems.Text = string.Format("Starting...<br/>Total item(s): {0}", totalItems);
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "runindex", string.Format("initialIndexRequestCount = {0}; GetIndexingStatus();", totalItems), true);
        }
        
        protected void ClearIndexDataStoreClick(object sender, EventArgs e)
        {
            SearchSettings.GetDynamicDataStore().DeleteAll();
            this.ReIndex.Enabled = true;
        }
    }
}