using System;

namespace FMOD
{
	public struct ASYNCREADINFO
	{
		public IntPtr handle;

		public uint offset;

		public uint sizebytes;

		public int priority;

		public IntPtr userdata;

		public IntPtr buffer;

		public uint bytesread;

		public FILE_ASYNCDONE_FUNC done;
	}
}
