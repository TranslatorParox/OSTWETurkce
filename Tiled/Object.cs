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
	public class Object
	{
		[XmlArray(Form = XmlSchemaForm.Unqualified)]
		[XmlArrayItem("property", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
		public Property[] properties;

		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public Ellipse ellipse;

		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public Polygon polygon;

		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public Polyline polyline;

		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public Text text;

		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public Image image;

		[XmlAttribute]
		public int id;

		[XmlIgnore]
		public bool idSpecified;

		[XmlAttribute]
		public string name;

		[XmlAttribute]
		public string type;

		[XmlAttribute]
		public double x;

		[XmlAttribute]
		public double y;

		[XmlAttribute]
		public double width;

		[XmlIgnore]
		public bool widthSpecified;

		[XmlAttribute]
		public double height;

		[XmlIgnore]
		public bool heightSpecified;

		[XmlAttribute]
		public double rotation;

		[XmlAttribute]
		public int gid;

		[XmlIgnore]
		public bool gidSpecified;

		[XmlAttribute]
		public bool visible;

		[XmlIgnore]
		public bool visibleSpecified;
	}
}
