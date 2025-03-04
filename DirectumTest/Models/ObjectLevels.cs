
using System.Xml.Serialization;

namespace DirectumTest.Models
{
    [Serializable, XmlRoot("OBJECTLEVELS")]
    public class ObjectLevels
    {
        [XmlElement("OBJECTLEVEL")]
        public ObjectLevel[] objectLevels { get; set; }

        [Serializable]

        public class ObjectLevel
        {
            [XmlAttribute("LEVEL")]
            public int Level { get; set; }

            [XmlAttribute("NAME")]
            public string Name { get; set; }

            [XmlAttribute("STARTDATE")]
            public DateTime StartDate { get; set; }

            [XmlAttribute("ENDDATE")]
            public DateTime EndDate { get; set; }

            [XmlAttribute("UPDATEDATE")]
            public DateTime UpdateDate { get; set; }

            [XmlAttribute("ISACTIVE")]
            public bool IsActive { get; set; }


        }
    }
}
