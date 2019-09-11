// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/resharper/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.Application.UI.Controls.BulbMenu.Items;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.Nuke.Actions;
using ReSharper.Nuke.GutterMarks;
using ReSharper.Nuke.Resources;

[assembly: RegisterConfigurableHighlightingsGroup(NukeTargetMarkOnGutter.Nuke, "Nuke")]

namespace ReSharper.Nuke.GutterMarks
{
    [StaticSeverityHighlighting(Severity.INFO,
        HighlightingGroupIds.GutterMarksGroup,
        OverlapResolve = OverlapResolveKind.NONE,
        AttributeId = NukeGutterIconAttribute)]
    public class NukeTargetMarkOnGutter : IHighlighting
    {
        public const string NukeGutterIconAttribute = "Nuke Gutter Icon";
        public const string Nuke = "Nuke";

        public static BulbMenuItem[] CreateRunTargetMenu(
            IProject project,
            string targetName,
            IAnchor gutterMarkAnchor,
            ISolution solution,
            ITextControl textControl)
        {
            var mainAnchor = new SubmenuAnchor(gutterMarkAnchor,
                new SubmenuBehavior(text: null, icon: null, executable: true, removeFirst: true));
            var subAnchor2 = new InvisibleAnchor(mainAnchor);
            var subAnchor3 = subAnchor2.CreateNext(separate: true);

            BulbMenuItem CreateItem(bool debug, bool skipDependencies, IAnchor anchor)
            {
                var action = new NukeTargetExecutionAction(project.ProjectFileLocation.FullPath, targetName, debug, skipDependencies);
                return new BulbMenuItem(
                    new IntentionAction.MyExecutableProxi(
                        action,
                        solution,
                        textControl),
                    action.Text,
                    LogoThemedIcons.NukeLogo.Id,
                    anchor);
            }

            return new[]
                   {
                       CreateItem(debug: false, skipDependencies: false, subAnchor3),
                       CreateItem(debug: true, skipDependencies: false, subAnchor3),
                       CreateItem(debug: false, skipDependencies: true, mainAnchor),
                       CreateItem(debug: true, skipDependencies: true, mainAnchor)
                   };
        }

        [CanBeNull] private readonly ITreeNode _myElement;
        private readonly DocumentRange _myRange;

        public NukeTargetMarkOnGutter([CanBeNull] ITreeNode element, DocumentRange range, string tooltip)
        {
            _myElement = element;
            _myRange = range;
            ToolTip = tooltip;
        }

        public IEnumerable<BulbMenuItem> GetBulbMenuItems(ISolution solution, ITextControl textControl, IAnchor gutterMarkAnchor)
        {
            var project = textControl.Document.GetPsiSourceFile(solution)?.GetProject();
            var propertyDeclaration = _myElement as IPropertyDeclaration;
            var property = propertyDeclaration?.DeclaredElement;
            if (property == null)
                return EmptyList<BulbMenuItem>.Enumerable;

            var propertyName = propertyDeclaration.DeclaredName;
            return property.GetNukeTarget() != null
                ? CreateRunTargetMenu(project, propertyName, gutterMarkAnchor, solution, textControl)
                : EmptyList<BulbMenuItem>.Enumerable;
        }

        #region IHighlighting

        public string ToolTip { get; }
        public string ErrorStripeToolTip => ToolTip;

        public bool IsValid()
        {
            return _myElement == null || _myElement.IsValid();
        }

        public DocumentRange CalculateRange()
        {
            return _myRange;
        }

        #endregion
    }
}