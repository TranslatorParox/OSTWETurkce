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
	public class Tile
	{
		[XmlArray(Form = XmlSchemaForm.Unqualified)]
		[XmlArrayItem("property", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
		public Property[] properties;

		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public Image image;

		[XmlElement("objectgroup", Form = XmlSchemaForm.Unqualified)]
		public ObjectGroup[] objectgroup;

		[XmlArray(Form = XmlSchemaForm.Unqualified)]
		[XmlArrayItem("frame", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
		public Frame[] animation;

		[XmlAttribute]
		public int id;

		[XmlAttribute]
		public string type;

		[XmlAttribute]
		public string terrain;

		[XmlAttribute]
		public double probability;

		[XmlIgnore]
		public bool idSpecified;

		[XmlIgnore]
		public bool probabilitySpecified;
	}
}
