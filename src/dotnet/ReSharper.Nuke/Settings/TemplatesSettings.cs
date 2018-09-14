// Copyright 2018 Maintainers and Contributors of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.Util;

namespace ReSharper.Nuke.Settings
{
    [ShellComponent]
    public class TemplatesSettings : IHaveDefaultSettingsStream
    {
        public Stream GetDefaultSettingsStream(Lifetime lifetime)
        {
            var manifestResourceStream = typeof(TemplatesSettings).Assembly
                .GetManifestResourceStream(typeof(TemplatesSettings).Namespace + ".Templates.DotSettings").NotNull();
            lifetime.AddDispose(manifestResourceStream);
            return manifestResourceStream;
        }

        public string Name => "NUKE Template Settings";
    }
}