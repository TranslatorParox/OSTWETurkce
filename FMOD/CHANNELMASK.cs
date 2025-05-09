using System;

namespace FMOD
{
	[Flags]
	public enum CHANNELMASK : uint
	{
		FRONT_LEFT = 1u,
		FRONT_RIGHT = 2u,
		FRONT_CENTER = 4u,
		LOW_FREQUENCY = 8u,
		SURROUND_LEFT = 0x10u,
		SURROUND_RIGHT = 0x20u,
		BACK_LEFT = 0x40u,
		BACK_RIGHT = 0x80u,
		BACK_CENTER = 0x100u,
		MONO = 1u,
		STEREO = 3u,
		LRC = 7u,
		QUAD = 0x33u,
		SURROUND = 0x37u,
		_5POINT1 = 0x3Fu,
		_5POINT1_REARS = 0xCFu,
		_7POINT0 = 0xF7u,
		_7POINT1 = 0xFFu
	}
}
