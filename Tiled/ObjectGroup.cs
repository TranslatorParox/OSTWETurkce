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
	public class ObjectGroup : Layer
	{
		[XmlElement("object", Form = XmlSchemaForm.Unqualified)]
		public Object[] @object;

		[XmlAttribute]
		public string color;

		[XmlAttribute]
		public string draworder;
	}
}
