// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/resharper/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Collections.Viewable;
using JetBrains.DataFlow;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Tasks;
using JetBrains.ReSharper.Host.Features;
using JetBrains.ReSharper.Host.Features.Components;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;

namespace ReSharper.Nuke.Rider
{
    [SolutionComponent]
    internal class NukeModelUpdater
    {
        private readonly Lifetime _lifetime;
        private readonly IPsiServices _services;
        private readonly ChangedTypeElementsProvider _changedTypeElementsProvider;
        private readonly ISolution _solution;
        private readonly ShellRdDispatcher _dispatcher;

        public NukeModelUpdater(
            Lifetime lifetime,
            IPsiServices services,
            ChangedTypeElementsProvider changedTypeElementsProvider,
            ISolution solution,
            ShellRdDispatcher dispatcher,
            ISolutionLoadTasksScheduler scheduler)
        {
            _lifetime = lifetime;
            _services = services;
            _changedTypeElementsProvider = changedTypeElementsProvider;
            _solution = solution;
            _dispatcher = dispatcher;

            scheduler.EnqueueTask(new SolutionLoadTask("Scan Build Targets",
                SolutionLoadTaskKinds.AsLateAsPossible,
                () => services.CachesState.IsInitialUpdateFinished.WhenTrue(_lifetime, FirstTimeLoad)));
        }

        private void FirstTimeLoad(Lifetime lifetime)
        {
            using (ReadLockCookie.Create())
            {
                // TODO: will this also catch types that transitively inherit?
                // TODO: use Finder
                var projects = _solution.GetAllProjects().Where(x => x.IsNukeProject());
                var nukeBuildClasses = _services.Finder.FindNukeBuildClasses(projects.SelectMany(x => x.GetPsiModules()).ToArray());
                // JetBrains.Util.MessageBox.ShowInfo(nukeBuildClasses.Select(x => x.ShortName).JoinSpace());
                HandleClasses(nukeBuildClasses);
                // var classes = _services.Symbols.GetPossibleInheritors("NukeBuild").OfType<IClass>();
                // HandleClasses(classes);
                // foreach (var project in _solution.GetAllProjects())
                // {
                //     if (!project.IsNukeProject(_nuGetPackageReferenceTracker))
                //         continue;
                //
                //     foreach (var projectFile in project.GetAllProjectFiles())
                //     {
                //         projectFile.ToSourceFiles().ForEach(_services.Symbols,
                //             (c, x) =>
                //             {
                //                 var classes = c.GetTypesAndNamespacesInFile(x).OfType<IClass>();
                //                 HandleClasses(classes);
                //             });
                //     }
                // }
            }

            _changedTypeElementsProvider.TypeElementsChanged.Advise(_lifetime,
                changedTypeElements => HandleClasses(changedTypeElements.OfType<IClass>()));
        }

        private void HandleClasses(IEnumerable<IClass> classes)
        {
            using (CompilationContextCookie.GetExplicitUniversalContextIfNotSet())
            {
                var nukeBuildClasses = classes.Where(x => x.IsNukeClass());
                foreach (var nukeBuildClass in nukeBuildClasses)
                {
                    var members = nukeBuildClass.GetAllClassMembers().Select(x => x.Member).ToList();
                    var targets = members.Select(x => x.GetNukeTarget()).WhereNotNull().ToArray();
                    var parameters = members.Select(x => x.GetNukeParameter()).WhereNotNull().ToArray();

                    _dispatcher.InvokeOrQueue(_lifetime,
                        () =>
                        {
                            var nukeModel = _solution.GetProtocolSolution().GetNukeModel();
                            nukeModel.Targets.Value = targets;
                            nukeModel.Parameters.Value = parameters;
                        });
                }
            }
        }
    }
}