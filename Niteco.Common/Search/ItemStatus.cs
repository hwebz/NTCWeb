using System;
namespace Niteco.Common.Search
{
	[System.Flags]
	public enum ItemStatus
	{
		Approved = 1,
		Pending = 2,
		Removed = 4
	}
}
