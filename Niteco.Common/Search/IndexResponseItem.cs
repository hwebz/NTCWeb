using System;
namespace Niteco.Common.Search
{
	public class IndexResponseItem : IndexItemBase
	{
		public float Score
		{
			get;
			set;
		}
		public IndexResponseItem(string id) : base(id)
		{
		}
	}
}
