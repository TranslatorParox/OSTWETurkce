using System;

namespace FMOD
{
	public delegate RESULT SYSTEM_CALLBACK(IntPtr system, SYSTEM_CALLBACK_TYPE type, IntPtr commanddata1, IntPtr commanddata2, IntPtr userdata);
}
