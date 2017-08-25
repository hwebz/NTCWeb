namespace Niteco.Common.Search.Queries.Lucene
{
	public class VirtualPathQuery : IQueryExpression
	{
		private System.Collections.ObjectModel.Collection<string> _virtualPathNodes = new System.Collections.ObjectModel.Collection<string>();
		public System.Collections.ObjectModel.Collection<string> VirtualPathNodes
		{
			get
			{
				return this._virtualPathNodes;
			}
		}
		public virtual string GetQueryExpression()
		{
			var stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append(SearchSettings.Config.IndexingServiceFieldNameVirtualPath + ":(");
			foreach (string current in this.VirtualPathNodes)
			{
				stringBuilder.Append(LuceneHelpers.Escape(current.Replace(" ", "")));
				stringBuilder.Append("|");
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			stringBuilder.Append("*)");
			return stringBuilder.ToString();
		}
	}
}
