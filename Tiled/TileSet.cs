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
	[XmlRoot("tileset", IsNullable = false)]
	public class TileSet
	{
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public TileOffset tileoffset;

		[XmlArray(Form = XmlSchemaForm.Unqualified)]
		[XmlArrayItem("property", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
		public Property[] properties;

		[XmlArray(Form = XmlSchemaForm.Unqualified)]
		[XmlArrayItem("wangset", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
		public Wangset[] wangsets;

		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public Image image;

		[XmlArray(Form = XmlSchemaForm.Unqualified)]
		[XmlArrayItem("terrain", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
		public Terrain[] terraintypes;

		[XmlElement("tile", Form = XmlSchemaForm.Unqualified)]
		public Tile[] tile;

		[XmlAttribute]
		public int firstgid;

		[XmlIgnore]
		public bool firstgidSpecified;

		[XmlAttribute]
		public string name;

		[XmlAttribute(DataType = "anyURI")]
		public string source;

		[XmlAttribute]
		public int tilewidth;

		[XmlAttribute]
		public int tileheight;

		[XmlAttribute]
		public int spacing;

		[XmlIgnore]
		public bool spacingSpecified;

		[XmlAttribute]
		public int margin;

		[XmlIgnore]
		public bool marginSpecified;

		[XmlAttribute]
		public int tilecount;

		[XmlIgnore]
		public bool tilecountSpecified;

		[XmlAttribute]
		public int columns;
	}
}
