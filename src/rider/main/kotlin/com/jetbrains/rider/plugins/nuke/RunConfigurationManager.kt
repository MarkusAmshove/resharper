package com.jetbrains.rider.plugins.nuke

import com.intellij.execution.configurations.runConfigurationType
import com.intellij.execution.DefaultExecutionTarget
import com.intellij.execution.ExecutionManager
import com.intellij.execution.RunManager
import com.intellij.execution.RunnerAndConfigurationSettings
import com.intellij.execution.executors.DefaultDebugExecutor
import com.intellij.execution.executors.DefaultRunExecutor
import com.intellij.openapi.project.Project
import com.intellij.openapi.util.io.FileUtil
import com.jetbrains.rdclient.util.idea.LifetimedProjectComponent
import com.jetbrains.rider.model.nukeModel
import com.jetbrains.rider.model.runnableProjectsModel
import com.jetbrains.rider.projectView.solution
import com.jetbrains.rider.run.configurations.project.DotNetProjectConfiguration

class RunConfigurationManager(project: Project, private val runManager: RunManager) : LifetimedProjectComponent(project) {
    init {
        project.solution.nukeModel.build.advise(componentLifetime) { buildInvocation ->
            var configuration = runManager.findConfigurationByName(buildInvocation.target)
                ?: createAndAddConfiguration(buildInvocation.target, buildInvocation.projectFile, buildInvocation.target)

            if (buildInvocation.skipDependencies) {
                val dotnetConfiguration = configuration.configuration as DotNetProjectConfiguration
                configuration = createAndAddConfiguration(
                        dotnetConfiguration.name + " (Temp)",
                        dotnetConfiguration.parameters.projectFilePath,
                        dotnetConfiguration.parameters.programParameters + " --skip")
                runManager.addConfiguration(configuration)
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

    private fun createAndAddConfiguration(name: String, projectFile: String, arguments: String): RunnerAndConfigurationSettings {

        val configurationType = runConfigurationType<NukeBuildTargetConfigurationType>()
        val configurationFactory = configurationType.configurationFactories.first()
        val configuration = runManager.createConfiguration(name, configurationFactory)
        configuration.isTemporary = true

        val buildProjectFile = FileUtil.toSystemIndependentName(projectFile)
        var dotnetProject = project.solution.runnableProjectsModel.projects.valueOrNull!!
                .single { it -> it.projectFilePath == buildProjectFile }

        val dotnetConfiguration = configuration.configuration as DotNetProjectConfiguration
        dotnetConfiguration.parameters.projectFilePath = dotnetProject.projectFilePath
        dotnetConfiguration.parameters.projectKind = dotnetProject.kind
        dotnetConfiguration.parameters.programParameters = arguments
        //  dotnetConfiguration.parameters.envs

        configuration!!.checkSettings()
        runManager.addConfiguration(configuration)

        return configuration
    }
}