﻿// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/resharper/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using EnvDTE;
using JetBrains.Annotations;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Util;

namespace ReSharper.Nuke
{
    public static class NukeApi
    {
        public static void CreateNukeArgumentsFile(string argumentsFilePath, bool skipDependencies, [CanBeNull] string targetName)
        {
            File.WriteAllText(argumentsFilePath, $"{targetName}{(skipDependencies ? " --skip" : string.Empty)}");
        }

        public static FileSystemPath GetNukeArgumentsFilePath(IProject buildProject)
        {
            return buildProject.GetOutputDirectory(buildProject.GetCurrentTargetFrameworkId()) / "nuke.tmp";
        }

        public static Lifetime StartNukeTempFileWatcher(FileSystemPath tempFilePath, Lifetime parentLifetime = default)
        {
            var lifetimeDefinition = parentLifetime == null
                ? Lifetime.Define(nameof(StartNukeTempFileWatcher))
                : Lifetime.Define(parentLifetime, nameof(StartNukeTempFileWatcher));
            var watcher = new FileSystemWatcher(tempFilePath.Directory.FullPath, tempFilePath.Name) { EnableRaisingEvents = true };
            watcher.Deleted += DeletedEventHandler;

            lifetimeDefinition.Lifetime.OnTermination(() =>
            {
                watcher.Deleted -= DeletedEventHandler;
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                watcher = null;
            });
            return lifetimeDefinition.Lifetime;

            void DeletedEventHandler(object sender, FileSystemEventArgs args)
            {
                lifetimeDefinition.Terminate();
            }
        }

        public static void RestoreStartupProjectAfterExecution(Lifetime lifetime, _DTE dte)
        {
            var startupObjects = dte.Solution.SolutionBuild.StartupProjects;
            lifetime.OnTermination(() =>
            {
                if (startupObjects is object[] startupObjectsArray && startupObjectsArray.Length == 1)
                {
                    startupObjects = startupObjectsArray[0];
                }

                dte.Solution.SolutionBuild.StartupProjects = startupObjects;
            });
        }

        public static void TemporaryEnableNukeProjectBuild(Lifetime lifetime, IProject buildProject, ISolution solution, _DTE dte)
        {
            lifetime.Bracket(() => SetEnableBuild(shouldBuild: true, project: buildProject, solution: solution, dte: dte),
                () =>
                {
                    SetEnableBuild(shouldBuild: false, project: buildProject, solution: solution, dte: dte);
                    dte.Solution.SaveAs(solution.SolutionFilePath.FullPath);
                });
        }

        private static void SetEnableBuild(bool shouldBuild, IProject project, ISolution solution, _DTE dte)
        {
            var name = project.ProjectFileLocation.FullPath.Substring(solution.SolutionFilePath.Directory.FullPath.Length + 1);

            var activeConfiguration = dte.Solution.SolutionBuild.ActiveConfiguration;

            foreach (SolutionContext solutionContext in activeConfiguration.SolutionContexts)
            {
                if (solutionContext.ProjectName != name) continue;
                solutionContext.ShouldBuild = shouldBuild;
                break;
            }
        }
    }
}