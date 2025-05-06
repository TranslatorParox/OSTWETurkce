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
	public class Group
	{
		[XmlArray(Form = XmlSchemaForm.Unqualified)]
		[XmlArrayItem("property", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
		public Property[] properties;

		[XmlElement("layer", Form = XmlSchemaForm.Unqualified)]
		public TileLayer[] layer;

		[XmlElement("objectgroup", Form = XmlSchemaForm.Unqualified)]
		public ObjectGroup[] objectgroup;

		[XmlElement("imagelayer", Form = XmlSchemaForm.Unqualified)]
		public ImageLayer[] imagelayer;

		[XmlElement("group", Form = XmlSchemaForm.Unqualified)]
		public Group[] group;

		[XmlAttribute]
		public string name;

		[XmlAttribute]
		public int offsetx;

		[XmlIgnore]
		public bool offsetxSpecified;

		[XmlAttribute]
		public int offsety;

		[XmlIgnore]
		public bool offsetySpecified;

		[XmlAttribute]
		public int x;

		[XmlIgnore]
		public bool xSpecified;

		[XmlAttribute]
		public int y;

		[XmlIgnore]
		public bool ySpecified;

		[XmlAttribute]
		public double opacity;

		[XmlIgnore]
		public bool opacitySpecified;

		[XmlAttribute]
		public bool visible;

		[XmlIgnore]
		public bool visibleSpecified;
	}
}
