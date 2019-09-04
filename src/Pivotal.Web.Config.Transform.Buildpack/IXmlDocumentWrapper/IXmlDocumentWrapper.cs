using System.Xml;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public interface IXmlDocumentWrapper

    {
        XmlDocument CreateXmlDocFromFile(string filename);

        void SaveXmlDocAsFile(XmlDocument doc, string filename);

        string ConvertXmlDocToString(XmlDocument doc);

        XmlDocument CreateXmlDocFromString(string xmlData);
    }
}
