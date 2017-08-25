namespace Niteco.Search.IndexingService
{
	internal enum STGC
	{
		DEFAULT,
		OVERWRITE,
		ONLYIFCURRENT,
		DANGEROUSLYCOMMITMERELYTODISKCACHE = 4,
		CONSOLIDATE = 8
	}
}
