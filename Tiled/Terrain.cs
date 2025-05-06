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
	public class Terrain
	{
		[XmlArray(Form = XmlSchemaForm.Unqualified)]
		[XmlArrayItem("property", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
		public Property[] properties;

		[XmlAttribute]
		public string name;

		[XmlAttribute]
		public int tile;

		[XmlIgnore]
		public bool tileSpecified;
	}
}
