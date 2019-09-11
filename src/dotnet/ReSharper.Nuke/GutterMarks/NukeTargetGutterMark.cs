﻿// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/resharper/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.Application.UI.Controls.BulbMenu.Items;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.Util;
using ReSharper.Nuke.GutterMarks;
using ReSharper.Nuke.Resources;

[assembly:
    RegisterHighlighter(NukeTargetMarkOnGutter.NukeGutterIconAttribute,
        EffectType = EffectType.GUTTER_MARK,
        GutterMarkType = typeof(NukeTargetGutterMark),
        Layer = HighlighterLayer.SYNTAX + 1)]

namespace ReSharper.Nuke.GutterMarks
{
    public class NukeTargetGutterMark : IconGutterMark
    {
        public NukeTargetGutterMark()
            : base(LogoThemedIcons.NukeLogo.Id)
        {
        }

        [CanBeNull] public override IAnchor Anchor => BulbMenuAnchors.FirstClassContextItems;

        public override IEnumerable<BulbMenuItem> GetBulbMenuItems(IHighlighter highlighter)
        {
            var solution = Shell.Instance.GetComponent<SolutionsManager>().Solution;
            if (solution == null)
                return EmptyList<BulbMenuItem>.InstanceList;

            var textControlManager = solution.GetComponent<ITextControlManager>();
            var textControl = textControlManager.FocusedTextControl.Value;

            var daemon = solution.GetComponent<IDaemon>();
            if (daemon.GetHighlighting(highlighter) is NukeTargetMarkOnGutter highlighting)
            {
                using (CompilationContextCookie.GetExplicitUniversalContextIfNotSet())
                    return highlighting.GetBulbMenuItems(solution, textControl, (InvisibleAnchor) Anchor.NotNull());
            }

            return EmptyList<BulbMenuItem>.InstanceList;
        }
    }
}