using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace Tiled
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3752.0")]
	public enum RenderOrder
	{
		[XmlEnum("right-down")]
		rightdown,
		[XmlEnum("right-up")]
		rightup,
		[XmlEnum("left-down")]
		leftdown,
		[XmlEnum("left-up")]
		leftup
	}
}
