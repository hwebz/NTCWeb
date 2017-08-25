using System;
using System.Globalization;
using System.Text.RegularExpressions;
namespace Niteco.Common.Search.Queries.Lucene
{
	public class ModifiedDateRangeQuery : RangeQuery
	{
		public ModifiedDateRangeQuery(System.DateTime start, System.DateTime end, bool inclusive) : base(Regex.Replace(start.ToString("u", System.Globalization.CultureInfo.InvariantCulture), "\\D", ""), Regex.Replace(end.ToString("u", System.Globalization.CultureInfo.InvariantCulture), "\\D", ""), Field.Modified, inclusive)
		{
		}
	}
}
