using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Web.Config.Transform.Buildpack
{
    public interface IConfigurationFactory

    {
        IConfigurationRoot GetConfiguration(string environment);

    }
}
