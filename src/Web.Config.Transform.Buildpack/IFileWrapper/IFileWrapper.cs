using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Web.Config.Transform.Buildpack
{
    public interface IFileWrapper
    {
        bool Exists(string file);

        void Move(string sourceFileName, string destFileName);

        void Copy(string sourceFileName, string destFileName);

        string[] GetFiles(string path, string searchPattern);
        
        string Combine(string path1, string path2);
    }
}
