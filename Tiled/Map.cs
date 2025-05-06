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
	[XmlRoot("map", IsNullable = false)]
	public class Map
	{
		[XmlArray(Form = XmlSchemaForm.Unqualified)]
		[XmlArrayItem("property", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
		public Property[] properties;

		[XmlElement("tileset", Form = XmlSchemaForm.Unqualified)]
		public TileSet[] tileset;

		[XmlElement("imagelayer", typeof(ImageLayer), Form = XmlSchemaForm.Unqualified)]
		[XmlElement("layer", typeof(TileLayer), Form = XmlSchemaForm.Unqualified)]
		[XmlElement("objectgroup", typeof(ObjectGroup), Form = XmlSchemaForm.Unqualified)]
		public Layer[] Items;

		[XmlElement("group", Form = XmlSchemaForm.Unqualified)]
		public Group[] group;

		[XmlAttribute]
		public string version;

		[XmlAttribute]
		public string tiledversion;

		[XmlAttribute]
		public Orientation orientation;

		[XmlAttribute]
		[DefaultValue(RenderOrder.rightdown)]
		public RenderOrder renderorder;

		[XmlAttribute]
		public int width;

		[XmlAttribute]
		public int height;

		[XmlAttribute]
		public int tilewidth;

		[XmlAttribute]
		public int tileheight;

		[XmlAttribute]
		public int hexsidelength;

		[XmlIgnore]
		public bool hexsidelengthSpecified;

		[XmlAttribute]
		public StaggerAxis staggeraxis;

		[XmlIgnore]
		public bool staggeraxisSpecified;

		[XmlAttribute]
		public StaggerIndex staggerindex;

		[XmlIgnore]
		public bool staggerindexSpecified;

		[XmlAttribute]
		public string backgroundcolor;

		[XmlAttribute]
		public int nextobjectid;

		[XmlIgnore]
		public bool nextobjectidSpecified;

		public Map()
		{
			version = "1.0";
			renderorder = RenderOrder.rightdown;
		}
	}
}
