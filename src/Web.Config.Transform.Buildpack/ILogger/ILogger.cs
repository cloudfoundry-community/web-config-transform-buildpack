using System;

namespace Web.Config.Transform.Buildpack
{
    public interface ILogger
    {
        void WriteLog(string message);
        void WriteError(string message, Exception exception = null);
    }
}
