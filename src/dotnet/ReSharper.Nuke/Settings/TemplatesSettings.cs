// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/resharper/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;

namespace ReSharper.Nuke.Settings
{
    [ShellComponent]
    public class TemplatesSettings : IHaveDefaultSettingsStream
    {
        #region IHaveDefaultSettingsStream

        public string Name => "NUKE Template Settings";

        public Stream GetDefaultSettingsStream(Lifetime lifetime)
        {
            var manifestResourceStream = typeof(TemplatesSettings).Assembly
                .GetManifestResourceStream(typeof(TemplatesSettings).Namespace + ".Templates.DotSettings").NotNull();
            lifetime.OnTermination(manifestResourceStream);
            return manifestResourceStream;
        }

        #endregion
    }
}