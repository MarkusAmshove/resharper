package com.jetbrains.rider.plugins.nuke

import com.intellij.execution.DefaultExecutionTarget
import com.intellij.execution.ExecutionManager
import com.intellij.execution.RunManager
import com.intellij.execution.executors.DefaultDebugExecutor
import com.intellij.execution.executors.DefaultRunExecutor
import com.intellij.openapi.project.Project
import com.jetbrains.rdclient.util.idea.LifetimedProjectComponent
import com.jetbrains.rider.projectView.solution
import com.jetbrains.rider.run.configurations.project.DotNetProjectConfiguration

class RunConfigurationManager(project: Project, private val runManager: RunManager) : LifetimedProjectComponent(project) {
    init {
        project.solution.nukeModel.build.advise(componentLifetime) { buildInvocation ->
            var configuration = runManager.findConfigurationByName(buildInvocation.target)
                ?: findOrCreateConfiguration(
                    buildInvocation.target,
                    runManager,
                    project,
                    buildInvocation.target,
                    buildInvocation.projectFile,
                    emptyMap()
                )

            if (buildInvocation.skipDependencies) {
                val dotnetConfiguration = configuration.configuration as DotNetProjectConfiguration
                configuration = findOrCreateConfiguration(
                    dotnetConfiguration.name + " (Temp)",
                    runManager,
                    project,
                    dotnetConfiguration.parameters.projectFilePath,
                    dotnetConfiguration.parameters.programParameters + " --skip",
                    dotnetConfiguration.parameters.envs)

            }

            runManager.selectedConfiguration = configuration
            val executionManager = ExecutionManager.getInstance(project)
            val executor = if (buildInvocation.debugMode) DefaultDebugExecutor.getDebugExecutorInstance()
                            else DefaultRunExecutor.getRunExecutorInstance()
            executionManager.restartRunProfile(
                    project,
                    executor,
                    DefaultExecutionTarget.INSTANCE,
                    configuration,
                    null)

            if (buildInvocation.skipDependencies) {
                runManager.removeConfiguration(configuration)
                runManager.selectedConfiguration = runManager.findConfigurationByName(buildInvocation.target)
            }
        }
    }
}