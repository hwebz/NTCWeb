using System.Runtime.InteropServices;

namespace Niteco.Search.IndexingService
{
	[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Size = 8)]
	internal struct PropSpec
	{
		[System.Runtime.InteropServices.FieldOffset(0)]
		internal int ulKind;
		[System.Runtime.InteropServices.FieldOffset(4)]
		internal System.IntPtr Name_Or_ID;
	}
}
