using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public interface IFileWrapper
    {
        bool Exists(string file);

        void Move(string sourceFileName, string destFileName);

        void Copy(string sourceFileName, string destFileName);
    }
}
