using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml.Linq;
using Niteco.Search.IndexingService.Custom;

namespace Niteco.Search.IndexingService.FieldSerializers
{
	internal class PipeSeparatedFieldStoreSerializer : IndexFieldStoreSerializerBase
	{
		public PipeSeparatedFieldStoreSerializer(SyndicationItem syndicationItem) : base(syndicationItem)
		{
		}
		internal PipeSeparatedFieldStoreSerializer(string fieldStoreValue) : base(fieldStoreValue)
		{
		}
		internal string ToFieldStoreValue(string syndicationItemElementExtensionName)
		{
			if (base.SyndicationItem != null)
			{
				System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
				XElement xElement = base.SyndicationItem.ElementExtensions.ReadElementExtensions<XElement>(syndicationItemElementExtensionName, IndexingServiceConstants.Namespace).ElementAt(0);
				foreach (XElement current in xElement.Elements())
				{
					stringBuilder.Append(current.Value);
					stringBuilder.Append("|");
				}
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Remove(stringBuilder.Length - 1, 1);
				}
				return stringBuilder.ToString().Trim();
			}
			return base.ToFieldStoreValue();
		}
		internal void AddFieldStoreValueToSyndicationItem(SyndicationItem syndicationItem, string syndicationItemElementExtensionName)
		{
			if (!string.IsNullOrEmpty(base.FieldStoreValue))
			{
			    XNamespace ns = IndexingServiceConstants.Namespace;
				string[] array = this.SplitFieldStoreValue();
				XElement xElement = new XElement(ns + syndicationItemElementExtensionName);
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					string content = array2[i];
					xElement.Add(new XElement(ns + "Item", content));
				}
				syndicationItem.ElementExtensions.Add(xElement.CreateReader());
				return;
			}
			base.AddFieldStoreValueToSyndicationItem(syndicationItem);
		}
		protected string[] SplitFieldStoreValue()
		{
			char[] separator = new char[]
			{
				'|'
			};
			return base.FieldStoreValue.Split(separator);
		}
	}
}
