using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace Niteco.Search.IndexingService
{
	internal class PerFieldQueryParserWrapper : QueryParser
	{
		private System.Collections.Generic.IList<string> _lowercaseFields;
		public PerFieldQueryParserWrapper(Lucene.Net.Util.Version matchVersion, string f, Analyzer a, System.Collections.Generic.IList<string> lowercaseFields) : base(matchVersion, f, a)
		{
			this._lowercaseFields = lowercaseFields;
		}
		protected override Query GetWildcardQuery(string field, string termStr)
		{
			Query wildcardQuery;
			try
			{
				if (!this._lowercaseFields.Contains(field))
				{
					this.LowercaseExpandedTerms = false;
				}
				wildcardQuery = base.GetWildcardQuery(field, termStr);
			}
			finally
			{
				if (!this._lowercaseFields.Contains(field))
				{
					this.LowercaseExpandedTerms = true;
				}
			}
			return wildcardQuery;
		}
		protected override Query GetPrefixQuery(string field, string termStr)
		{
			Query prefixQuery;
			try
			{
				if (!this._lowercaseFields.Contains(field))
				{
					this.LowercaseExpandedTerms = false;
				}
				prefixQuery = base.GetPrefixQuery(field, termStr);
			}
			finally
			{
				if (!this._lowercaseFields.Contains(field))
				{
					this.LowercaseExpandedTerms = true;
				}
			}
			return prefixQuery;
		}
		protected override Query GetFuzzyQuery(string field, string termStr, float minSimilarity)
		{
			Query fuzzyQuery;
			try
			{
				if (!this._lowercaseFields.Contains(field))
				{
					this.LowercaseExpandedTerms = false;
				}
				fuzzyQuery = base.GetFuzzyQuery(field, termStr, minSimilarity);
			}
			finally
			{
				if (!this._lowercaseFields.Contains(field))
				{
					this.LowercaseExpandedTerms = true;
				}
			}
			return fuzzyQuery;
		}
		protected override Query GetRangeQuery(string field, string part1, string part2, bool inclusive)
		{
			Query rangeQuery;
			try
			{
				if (!this._lowercaseFields.Contains(field))
				{
					this.LowercaseExpandedTerms = false;
				}
				rangeQuery = base.GetRangeQuery(field, part1, part2, inclusive);
			}
			finally
			{
				if (!this._lowercaseFields.Contains(field))
				{
					this.LowercaseExpandedTerms = true;
				}
			}
			return rangeQuery;
		}
	}
}
