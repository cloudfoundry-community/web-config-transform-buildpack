using System.IO;
using System.Xml;

namespace Web.Config.Transform.Buildpack
{
    public class XmlDocumentWrapper : IXmlDocumentWrapper
    {
        public XmlDocument CreateXmlDocFromFile(string filename)
        {
            var doc = new XmlDocument();
            doc.Load(filename);
            return doc;
        }

        public void SaveXmlDocAsFile(XmlDocument doc, string filename)
        {
            doc.Save(filename);
        }

        public string ConvertXmlDocToString(XmlDocument doc)
        {
            using (var stringWriter = new StringWriter())
            { 
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    return stringWriter.GetStringBuilder().ToString();
                }
            }
        }

        public XmlDocument CreateXmlDocFromString(string xmlData)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlData);
            return doc;
        }
    }
}
