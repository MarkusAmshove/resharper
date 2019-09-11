// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/resharper/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.Application.UI.ActionSystem.ActionsRevised.Menu;
using JetBrains.Application.UI.Controls.GotoByName;
using JetBrains.Application.UI.DataContext;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl.DataContext;
using ReSharper.Nuke.GutterMarks;

namespace ReSharper.Nuke.Actions
{
    [Action("GlobalNukeTargetExecution", "Execute NUKE Target", Id = 3453)]
    public class GlobalNukeTargetExecutionAction : IActionWithExecuteRequirement, IExecutableAction
    {
        #region IActionWithExecuteRequirement

        public IActionRequirement GetRequirement(IDataContext dataContext)
        {
            return CommitAllDocumentsRequirement.TryGetInstance(dataContext);
        }

        #endregion

        #region IExecutableAction

        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            return true;
        }

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            var solution = context.GetData(ProjectModelDataConstants.SOLUTION).NotNull();
            var popupWindowContextSource = context.GetData(UIDataConstants.PopupWindowContextSource);
            var bulbMenuComponent = solution.GetComponent<BulbMenuComponent>();

            var services = solution.GetPsiServices();
            var symbolScope = services.Symbols.GetSymbolScope(LibrarySymbolScope.FULL, caseSensitive: true);
            var typeElements = symbolScope.GetTypeElementsByCLRName("Nuke.Common.NukeBuild");
            var textControl = context.GetData(TextControlDataConstants.TEXT_CONTROL).NotNull();

            var buildClasses = new List<IClass>();
            foreach (var x in typeElements)
                services.Finder.FindInheritors(x,
                    new FindResultConsumer(y =>
                    {
                        var clazz = (y as FindResultDeclaredElement)?.DeclaredElement as IClass;
                        if (clazz == null)
                            return FindExecution.Continue;

                        if (clazz.GetDeclarations().FirstOrDefault()?.GetProject()?.Name.EndsWith("Tests") ?? true)
                            return FindExecution.Continue;

                        buildClasses.Add(clazz);

                        return FindExecution.Continue;
                    }),
                    NullProgressIndicator.Create());

            var items = buildClasses.SelectMany(x => x.GetMembers())
                .OfType<IProperty>()
                .Where(x => x.GetNukeTarget() != null)
                .SelectMany(x => NukeTargetMarkOnGutter.CreateRunTargetMenu(
                    x.GetDeclarations().First().GetProject(),
                    x.ShortName,
                    gutterMarkAnchor: null,
                    solution,
                    textControl)).ToList();

            bulbMenuComponent.ShowBulbMenu(items, popupWindowContextSource);
        }

        #endregion
    }
}