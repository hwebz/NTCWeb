using System;
using System.Globalization;
namespace Niteco.Common.Search.Queries.Lucene
{
	public class ItemStatusQuery : GroupQuery
	{
		public ItemStatusQuery(ItemStatus status) : base(LuceneOperator.OR)
		{
			if ((status & ItemStatus.Approved) == ItemStatus.Approved)
			{
				base.QueryExpressions.Add(new FieldQuery(1.ToString(System.Globalization.CultureInfo.InvariantCulture), Field.ItemStatus));
			}
			if ((status & ItemStatus.Pending) == ItemStatus.Pending)
			{
				base.QueryExpressions.Add(new FieldQuery(2.ToString(System.Globalization.CultureInfo.InvariantCulture), Field.ItemStatus));
			}
			if ((status & ItemStatus.Removed) == ItemStatus.Removed)
			{
				base.QueryExpressions.Add(new FieldQuery(4.ToString(System.Globalization.CultureInfo.InvariantCulture), Field.ItemStatus));
			}
		}
	}
}
