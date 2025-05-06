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
	public class Property
	{
		[XmlAttribute]
		public string name;

		[XmlAttribute]
		public PropertyType type;

		[XmlIgnore]
		public bool typeSpecified;

		[XmlAttribute]
		public string value;

		[XmlText]
		public string text;
	}
}
