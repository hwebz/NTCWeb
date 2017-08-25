using System.Linq;
using System.ServiceModel.Syndication;

namespace Niteco.Search.IndexingService.FieldSerializers
{
	internal class AuthorsFieldStoreSerializer : PipeSeparatedFieldStoreSerializer
	{
		internal AuthorsFieldStoreSerializer(SyndicationItem syndicationItem) : base(syndicationItem)
		{
		}
		internal AuthorsFieldStoreSerializer(string indexFieldStoreValue) : base(indexFieldStoreValue)
		{
		}
		internal override string ToFieldStoreValue()
		{
			if (base.SyndicationItem != null)
			{
				System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
				foreach (SyndicationPerson current in 
					from a in base.SyndicationItem.Authors
					where a != null && !string.IsNullOrEmpty(a.Name)
					select a)
				{
					stringBuilder.Append(current.Name.Trim());
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
		internal override void AddFieldStoreValueToSyndicationItem(SyndicationItem syndicationItem)
		{
			string[] array = base.SplitFieldStoreValue();
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text = array2[i];
				if (!string.IsNullOrEmpty(text))
				{
					syndicationItem.Authors.Add(new SyndicationPerson(string.Empty, text, string.Empty));
				}
			}
		}
	}
}
