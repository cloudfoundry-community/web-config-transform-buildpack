using System.Collections.Generic;

namespace Web.Config.Transform.Buildpack
{
    public interface IConfigReader
    {
        string GetAppSetting(string key);

        List<KeyValuePair<string, string>> GetAppSettings();

        string GetConnectionString(string name);

        List<KeyValuePair<string, string>> GetConnectionStrings();

        bool ValueExistsInXmlDoc(string value);
    }
}
