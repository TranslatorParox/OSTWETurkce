using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Tiled
{
	[Serializable]
	[XmlInclude(typeof(TileLayer))]
	[XmlInclude(typeof(ObjectGroup))]
	[XmlInclude(typeof(ImageLayer))]
	[GeneratedCode("xsd", "4.8.3752.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public abstract class Layer
	{
		[XmlArray(Form = XmlSchemaForm.Unqualified)]
		[XmlArrayItem("property", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
		public Property[] properties;

		[XmlAttribute]
		public string name;

		[XmlAttribute]
		public int x;

		[XmlAttribute]
		public int y;

		[XmlAttribute]
		public int width;

		[XmlAttribute]
		public int height;

		[XmlAttribute]
		public float opacity;

		[XmlAttribute]
		public bool visible;

		[XmlAttribute]
		public int offsetx;

		[XmlAttribute]
		public int offsety;

		[XmlIgnore]
		public bool xSpecified;

		[XmlIgnore]
		public bool ySpecified;

		[XmlIgnore]
		public bool opacitySpecified;

		[XmlIgnore]
		public bool visibleSpecified;

		[XmlIgnore]
		public bool offsetxSpecified;

		[XmlIgnore]
		public bool offsetySpecified;
	}
}
