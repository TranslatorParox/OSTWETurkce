using System.Runtime.InteropServices;

namespace FMOD
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Debug
	{
		public static RESULT Initialize(DEBUG_FLAGS flags, DEBUG_MODE mode = DEBUG_MODE.TTY, DEBUG_CALLBACK callback = null, string filename = null)
		{
			using (StringHelper.ThreadSafeEncoding threadSafeEncoding = StringHelper.GetFreeHelper())
			{
				return FMOD5_Debug_Initialize(flags, mode, callback, threadSafeEncoding.byteFromStringUTF8(filename));
			}
		}

		[DllImport("x64/fmod")]
		private static extern RESULT FMOD5_Debug_Initialize(DEBUG_FLAGS flags, DEBUG_MODE mode, DEBUG_CALLBACK callback, byte[] filename);
	}
}
