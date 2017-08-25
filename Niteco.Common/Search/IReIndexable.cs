using System;
namespace Niteco.Common.Search
{
	public interface IReIndexable
	{
		string NamedIndex
		{
			get;
		}
		string NamedIndexingService
		{
			get;
		}
		void ReIndex();
	}
}
