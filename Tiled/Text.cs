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
	public class Text
	{
		[XmlAttribute]
		public string fontfamily;

		[XmlAttribute]
		public int pixelsize;

		[XmlIgnore]
		public bool pixelsizeSpecified;

		[XmlAttribute]
		public bool wrap;

		[XmlIgnore]
		public bool wrapSpecified;

		[XmlAttribute]
		public string color;

		[XmlAttribute]
		public bool bold;

		[XmlIgnore]
		public bool boldSpecified;

		[XmlAttribute]
		public bool italic;

		[XmlIgnore]
		public bool italicSpecified;

		[XmlAttribute]
		public bool underline;

		[XmlIgnore]
		public bool underlineSpecified;

		[XmlAttribute]
		public bool strikeout;

		[XmlIgnore]
		public bool strikeoutSpecified;

		[XmlAttribute]
		public bool kerning;

		[XmlIgnore]
		public bool kerningSpecified;

		[XmlAttribute]
		public HorizontalAlignment halign;

		[XmlIgnore]
		public bool halignSpecified;

		[XmlAttribute]
		public VerticalAlignment valign;

		[XmlIgnore]
		public bool valignSpecified;

		[XmlText]
		public string Value;
	}
}
