using System;
namespace Niteco.Common.Search.Queries.Lucene
{
	public class AccessControlListQuery : CollectionQueryBase
	{
		public AccessControlListQuery() : this(LuceneOperator.OR)
		{
		}
		public AccessControlListQuery(LuceneOperator innerOperator) : base(SearchSettings.Config.IndexingServiceFieldNameAcl, innerOperator)
		{
		}
		public void AddRole(string roleName)
		{
			base.Items.Add("G:" + roleName);
		}
		public void AddUser(string userName)
		{
			base.Items.Add("U:" + userName);
		}
	}
}
