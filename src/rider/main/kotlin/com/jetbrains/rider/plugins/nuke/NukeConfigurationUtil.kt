package com.jetbrains.rider.plugins.nuke

import com.intellij.execution.RunManager
import com.intellij.execution.RunnerAndConfigurationSettings
import com.intellij.execution.configurations.runConfigurationType
import com.intellij.openapi.project.Project
import com.intellij.openapi.util.io.FileUtil
import com.jetbrains.rider.model.runnableProjectsModel
import com.jetbrains.rider.projectView.solution
import com.jetbrains.rider.run.configurations.project.DotNetProjectConfiguration

fun findOrCreateConfiguration(
    target: String,
    runManager: RunManager,
    project: Project,
    projectFile: String,
    arguments: String,
    envs: Map<String, String>)
    : RunnerAndConfigurationSettings =
    runManager.findConfigurationByName(target) ?:
        runManager.createAndAddConfiguration(
            target,
            project,
            projectFile,
            arguments,
            envs
        )

private fun RunManager.createAndAddConfiguration(
    name: String,
    project: Project,
    projectFile: String,
    arguments: String,
    envs: Map<String, String>): RunnerAndConfigurationSettings {

    val configurationType = runConfigurationType<NukeBuildTargetConfigurationType>()
    val configurationFactory = configurationType.configurationFactories.first()
    val configuration = this.createConfiguration(name, configurationFactory)
    configuration.isTemporary = true

    val buildProjectFile = FileUtil.toSystemIndependentName(projectFile)
    val dotnetProject = project.solution.runnableProjectsModel.projects.valueOrNull!!
        .single { it -> it.projectFilePath == buildProjectFile }

    val dotnetConfiguration = configuration.configuration as DotNetProjectConfiguration
    dotnetConfiguration.parameters.projectFilePath = dotnetProject.projectFilePath
    dotnetConfiguration.parameters.projectKind = dotnetProject.kind
    dotnetConfiguration.parameters.programParameters = arguments
    dotnetConfiguration.parameters.envs = envs

    configuration.checkSettings()
    this.addConfiguration(configuration)

    return configuration
}
