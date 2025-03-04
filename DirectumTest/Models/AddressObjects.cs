
using System.Xml.Serialization;

namespace DirectumTest.Models
{
    

    [Serializable, XmlRoot("ADDRESSOBJECTS")]
    public class AddressObjects
    {
        [XmlElement("OBJECT")]
        public AObject[] aObjects { get; set; }

        [Serializable]

        public class AObject
        {
            [XmlAttribute("ID")]
            public string ID { get; set; }

            [XmlAttribute("OBJECTID")]
            public int ObjectID { get; set; }
            [XmlAttribute("OBJECTGUID")]
            public string ObjectGUID { get; set; }
            [XmlAttribute("CHANGEID")]
            public int CHANGE { get; set; }
            [XmlAttribute("NAME")]
            public string Name { get; set; }
            [XmlAttribute("TYPENAME")]
            public string TypeName { get; set; }
            [XmlAttribute("LEVEL")]
            public int Level { get; set; }
            [XmlAttribute("OPERTYPEID")]
            public int OperTypeID { get; set; }
            [XmlAttribute("PREVID")]
            public int PrevID { get; set; }
            [XmlAttribute("NEXTID")]
            public int NextID { get; set; }
            [XmlAttribute("UPDATEDATE")]
            public DateTime UpdateDate { get; set; }
            [XmlAttribute("STARTDATE")]
            public DateTime StartDate { get; set; }
            [XmlAttribute("ENDDATE")]
            public DateTime EndDate { get; set; }
            [XmlAttribute("ISACTUAL")]
            public bool IsActual { get; set; }
            [XmlAttribute("ISACTIVE")]
            public bool IsActive { get; set; }
        }
    }
}
