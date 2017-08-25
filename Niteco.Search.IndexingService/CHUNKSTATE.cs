namespace Niteco.Search.IndexingService
{
	internal enum CHUNKSTATE : uint
	{
		CHUNK_TEXT = 1u,
		CHUNK_VALUE,
		CHUNK_FILTER_OWNED_VALUE = 4u
	}
}
