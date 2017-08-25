using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;

namespace Niteco.Search.IndexingService.FieldSerializers
{
	internal class CategoriesFieldStoreSerializer : TaggedFieldStoreSerializer
	{
		internal CategoriesFieldStoreSerializer(SyndicationItem syndicationItem) : base(syndicationItem)
		{
		}
		internal CategoriesFieldStoreSerializer(string indexFieldStoreValue) : base(indexFieldStoreValue)
		{
		}
		internal override string ToFieldStoreValue()
		{
			if (base.SyndicationItem != null)
			{
				System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
				foreach (SyndicationCategory current in base.SyndicationItem.Categories)
				{
					stringBuilder.Append("[[");
					stringBuilder.Append(current.Name.Trim());
					stringBuilder.Append("]]");
					stringBuilder.Append(" ");
				}
				return stringBuilder.ToString().Trim();
			}
			return base.ToFieldStoreValue();
		}
		internal override void AddFieldStoreValueToSyndicationItem(SyndicationItem syndicationItem)
		{
			if (!string.IsNullOrEmpty(base.FieldStoreValue))
			{
				MatchCollection matchCollection = base.SplitFieldStoreValue();
				System.Collections.IEnumerator enumerator = matchCollection.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Match match = (Match)enumerator.Current;
						syndicationItem.Categories.Add(new SyndicationCategory(base.GetOriginalValue(match.Value)));
					}
					return;
				}
				finally
				{
					System.IDisposable disposable = enumerator as System.IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
			base.AddFieldStoreValueToSyndicationItem(syndicationItem);
		}
	}
}
