namespace Niteco.Search.IndexingService
{
	internal struct STAT_CHUNK
	{
		internal int idChunk;
		[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
		internal CHUNK_BREAKTYPE breakType;
		[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
		internal CHUNKSTATE flags;
		internal int locale;
		[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Struct)]
		internal FULLPROPSPEC attribute;
		internal int idChunkSource;
		internal int cwcStartSource;
		internal int cwcLenSource;
	}
}
