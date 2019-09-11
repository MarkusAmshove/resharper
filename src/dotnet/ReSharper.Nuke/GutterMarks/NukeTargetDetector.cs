// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/resharper/blob/master/LICENSE

using System;
using System.Linq;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.Nuke.GutterMarks
{
    [ElementProblemAnalyzer(typeof(IPropertyDeclaration), HighlightingTypes = new[] { typeof(NukeTargetMarkOnGutter) })]
    public class NukeTargetDetector : ElementProblemAnalyzer<IPropertyDeclaration>
    {
        protected override void Run(IPropertyDeclaration element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            if (data.GetDaemonProcessKind() != DaemonProcessKind.VISIBLE_DOCUMENT)
                return;

            if (!element.DeclaredElement.IsNukeTargetProperty())
                return;

            var documentRange = element.GetDocumentRange();
            var tooltip = "Nuke Target";
            var highlighting = new NukeTargetMarkOnGutter(element, documentRange, tooltip);
            consumer.AddHighlighting(highlighting, documentRange);
        }
    }
}