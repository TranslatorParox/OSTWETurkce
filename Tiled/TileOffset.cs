using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Tiled
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3752.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class TileOffset
	{
		[XmlAttribute]
		public int x;

		[XmlIgnore]
		public bool xSpecified;

		[XmlAttribute]
		public int y;

		[XmlIgnore]
		public bool ySpecified;
	}
}
