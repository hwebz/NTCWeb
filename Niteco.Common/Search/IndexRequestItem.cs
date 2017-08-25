using System.Globalization;
using System.Xml;
namespace Niteco.Common.Search
{
	public class IndexRequestItem : IndexItemBase
	{
	    public IndexAction IndexAction { get; set; }

	    public bool? AutoUpdateVirtualPath { get; set; }

	    public IndexRequestItem(string id, IndexAction indexAction) : base(id)
		{
			this.IndexAction = indexAction;
		}

		protected internal override string ToSyndicationItemXml()
		{
			var key = new XmlQualifiedName(SearchSettings.Config.SyndicationItemAttributeNameIndexAction, SearchSettings.Config.XmlQualifiedNamespace);
			base.SyndicationItem.AttributeExtensions[key] = SearchSettings.GetIndexActionName(this.IndexAction);
			key = new XmlQualifiedName(SearchSettings.Config.SyndicationItemAttributeNameAutoUpdateVirtualPath, SearchSettings.Config.XmlQualifiedNamespace);
			if (this.AutoUpdateVirtualPath.HasValue)
			{
				base.SyndicationItem.AttributeExtensions[key] = this.AutoUpdateVirtualPath.Value.ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				if (base.SyndicationItem.AttributeExtensions.ContainsKey(key))
				{
					base.SyndicationItem.AttributeExtensions.Remove(key);
				}
			}

			return base.ToSyndicationItemXml();
		}
	}
}
