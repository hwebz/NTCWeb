using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Niteco.Search.IndexingService.Custom;

namespace Niteco.Search.IndexingService.FieldSerializers
{
	internal class TaggedFieldStoreSerializer : IndexFieldStoreSerializerBase
	{
		internal TaggedFieldStoreSerializer(SyndicationItem syndicationItem) : base(syndicationItem)
		{
		}
		internal TaggedFieldStoreSerializer(string fieldStoreValue) : base(fieldStoreValue)
		{
		}
		internal virtual string ToFieldStoreString(string syndicationItemElementExtensionName)
		{
			string result = base.ToFieldStoreValue();
			if (base.SyndicationItem != null)
			{
				System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
				System.Collections.ObjectModel.Collection<XElement> collection = base.SyndicationItem.ElementExtensions.ReadElementExtensions<XElement>(syndicationItemElementExtensionName, IndexingServiceConstants.Namespace);
				if (collection.Count > 0)
				{
					XElement xElement = collection.ElementAt(0);
					foreach (XElement current in xElement.Elements())
					{
						stringBuilder.Append("[[");
						stringBuilder.Append(current.Value.Trim());
						stringBuilder.Append("]]");
						stringBuilder.Append(" ");
					}
					result = stringBuilder.ToString().Trim();
				}
			}
			return result;
		}
		internal void AddFieldStoreValueToSyndicationItem(SyndicationItem syndicationItem, string syndicationItemElementExtensionName)
		{
			if (!string.IsNullOrEmpty(base.FieldStoreValue))
			{
                XNamespace ns = IndexingServiceConstants.Namespace;
				MatchCollection matchCollection = this.SplitFieldStoreValue();
				XElement xElement = new XElement(ns + syndicationItemElementExtensionName);
				foreach (Match match in matchCollection)
				{
					if (match.Value != null)
					{
						string originalValue = this.GetOriginalValue(match.Value);
						xElement.Add(new XElement(ns + "Item", originalValue));
					}
				}
				syndicationItem.ElementExtensions.Add(xElement.CreateReader());
				return;
			}
			base.AddFieldStoreValueToSyndicationItem(syndicationItem);
		}
		protected MatchCollection SplitFieldStoreValue()
		{
			return Regex.Matches(base.FieldStoreValue, "\\[\\[.*?\\]\\]");
		}
		protected string GetOriginalValue(string storedValue)
		{
			return storedValue.Replace("[[", "").Replace("]]", "");
		}
	}
}
