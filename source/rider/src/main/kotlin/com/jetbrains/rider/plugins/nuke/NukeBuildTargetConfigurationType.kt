package com.jetbrains.rider.plugins.nuke

import com.intellij.execution.RunManager
import com.intellij.execution.configurations.ConfigurationTypeBase
import com.jetbrains.rider.model.RunnableProject
import com.jetbrains.rider.model.RunnableProjectKind
import com.jetbrains.rider.run.configurations.IRunnableProjectConfigurationType
import com.jetbrains.rider.run.configurations.IRunConfigurationWithDefault
import com.jetbrains.rider.run.configurations.project.DotNetProjectConfigurationFactory

class NukeBuildTargetConfigurationType
    : ConfigurationTypeBase(
        "NukeBuildTarget",
        "NUKE",
        "NUKE target execution configuration",
        NukeIcons.Icon),
        IRunnableProjectConfigurationType,
        IRunConfigurationWithDefault {
    private val factoryConfiguration: DotNetProjectConfigurationFactory = DotNetProjectConfigurationFactory(this)

    init {
        addFactory(factoryConfiguration)
    }

    override val priority: IRunConfigurationWithDefault.Priority
        get() = IRunConfigurationWithDefault.Priority.Top

    override fun isApplicable(kind: RunnableProjectKind) =
            kind == RunnableProjectKind.Console || kind == RunnableProjectKind.DotNetCore

    override fun tryCreateDefault(projects: List<RunnableProject>, runManager: RunManager)  = null
}