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
	public class Data
	{
		[XmlAttribute]
		public Encoding encoding;

		[XmlIgnore]
		public bool encodingSpecified;

		[XmlAttribute]
		public Compression compression;

		[XmlIgnore]
		public bool compressionSpecified;

		[XmlText(DataType = "token")]
		public string Value;
	}
}
