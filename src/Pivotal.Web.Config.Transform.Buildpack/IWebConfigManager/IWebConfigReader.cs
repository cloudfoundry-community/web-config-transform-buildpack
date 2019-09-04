using System.Collections.Generic;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public interface IWebConfigReader
    {
        string GetAppSetting(string key);

        List<KeyValuePair<string, string>> GetAppSettings();

        string GetConnectionString(string name);

        List<KeyValuePair<string, string>> GetConnectionStrings();

        bool ValueExistsInXmlDoc(string value);
    }
}
