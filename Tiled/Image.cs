using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Tiled
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3752.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class Image
	{
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public Data data;

		[XmlAttribute]
		public string format;

		[XmlAttribute]
		public int id;

		[XmlAttribute(DataType = "anyURI")]
		public string source;

		[XmlAttribute]
		public string trans;

		[XmlAttribute]
		public int width;

		[XmlAttribute]
		public int height;

		[XmlIgnore]
		public bool idSpecified;

		[XmlIgnore]
		public bool widthSpecified;

		[XmlIgnore]
		public bool heightSpecified;
	}
}
