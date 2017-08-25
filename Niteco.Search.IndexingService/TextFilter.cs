using System.Runtime.InteropServices;

namespace Niteco.Search.IndexingService
{
	internal class TextFilter
	{
		[System.Runtime.InteropServices.DllImport("query.dll", CharSet = CharSet.Unicode)]
		internal static extern int LoadIFilter(string pwcsPath, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)] object pUnkOuter, ref IFilter ppIUnk);
		[System.Runtime.InteropServices.DllImport("iprop.dll", CharSet = CharSet.Unicode)]
		internal static extern int PropVariantClear(System.IntPtr pvar);
	}
}
