using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Web.Config.Transform.Buildpack
{
    public class FileWrapper : IFileWrapper
    {
        public bool Exists(string file)
        {
            return File.Exists(file);
        }

        public void Move(string sourceFileName, string destFilename)
        {
            File.Move(sourceFileName, destFilename);
        }

        public void Copy(string sourceFileName, string destFilename)
        {
            File.Copy(sourceFileName, destFilename, true);
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            return string.IsNullOrEmpty(path) switch
            {
                false => Directory.GetFiles(path, searchPattern),
                _ => default
            };
        }

        public string Combine(string path1, string path2) => Path.Combine(path1, path2);
    }
}
