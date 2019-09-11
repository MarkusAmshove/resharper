// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/resharper/blob/master/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.TextControl;
using JetBrains.Util;
#if RESHARPER
using JetBrains.ProjectModel.Features.RunConfig;
using JetBrains.VsIntegration.IDE.RunConfig;
using JetBrains.VsIntegration.Interop;
#elif RIDER
using JetBrains.ReSharper.Host.Features;
using ReSharper.Nuke.Rider;

#endif

namespace ReSharper.Nuke.Actions
{
    public class NukeTargetExecutionAction : BulbActionBase
    {
        private readonly string _projectPath;
        private readonly string _targetName;
        private readonly bool _debug;
        private readonly bool _skipDependencies;

        public NukeTargetExecutionAction(string projectPath, string targetName, bool debug, bool skipDependencies)
        {
            _projectPath = projectPath;
            _targetName = targetName;
            _debug = debug;
            _skipDependencies = skipDependencies;
        }

        [CanBeNull]
        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            return textControl =>
            {
                var project = solution.FindProjectByProjectFilePath(FileSystemPath.Parse(_projectPath));
                if (project == null)
                    return;

#if RESHARPER
                var runConfig = CreateRunConfig(solution, project);
                var runConfigBuilder = solution.GetComponent<IRunConfigBuilder>();
                var dte = solution.GetComponent<VsDtePropertiesFacade>().DTE;

                var nukeArgumentsFile = NukeApi.GetNukeArgumentsFilePath(project);

                NukeApi.CreateNukeArgumentsFile(nukeArgumentsFile.FullPath, _skipDependencies, _targetName);
                var tempFileLifetime = NukeApi.StartNukeTempFileWatcher(nukeArgumentsFile, textControl.Lifetime);

                NukeApi.RestoreStartupProjectAfterExecution(tempFileLifetime, dte);
                NukeApi.TemporaryEnableNukeProjectBuild(tempFileLifetime, project, solution, dte);

                var executionProvider = solution.GetComponent<ExecutionProviders>().GetByIdOrDebug(runConfig.DefaultAction);
                var context = new RunConfigContext
                              {
                                  Solution = solution,
                                  ExecutionProvider = executionProvider
                              };

                runConfigBuilder.BuildRunConfigAndExecute(solution, context, runConfig);
#else
                var nukeModel = solution.GetProtocolSolution().GetNukeModel();
                nukeModel.Build.Fire(new BuildInvocation(_projectPath, _targetName, _debug, _skipDependencies));
#endif
            };
        }

        /// <summary>
        /// The text of the bulb action.
        /// </summary>
        public override string Text =>
            !_debug && !_skipDependencies
                ? _targetName
                : (_debug ? "Debug" : "Run") +
                  (_skipDependencies ? " without dependencies" : string.Empty);

#if RESHARPER
        private IRunConfig CreateRunConfig(ISolution solution, IProject buildProject)
        {
            var runConfig = solution.GetComponent<RunConfigProjectProvider>().CreateNew();
            runConfig.ProjectGuid = buildProject.Guid;
            runConfig.DefaultAction = _debug ? "debug" : "run";
            return runConfig;
        }
#endif
    }
}