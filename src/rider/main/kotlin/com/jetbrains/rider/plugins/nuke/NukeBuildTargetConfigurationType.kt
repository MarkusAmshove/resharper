package com.jetbrains.rider.plugins.nuke

import com.intellij.execution.configurations.ConfigurationTypeBase
import com.jetbrains.rider.model.RunnableProjectKind
import com.jetbrains.rider.run.configurations.IRunnableProjectConfigurationType
import com.jetbrains.rider.run.configurations.project.DotNetProjectConfigurationFactory

class NukeBuildTargetConfigurationType
    : ConfigurationTypeBase(
        "NukeBuildTarget",
        "NUKE",
        "NUKE",
    NukeIcons.Icon),
        IRunnableProjectConfigurationType {

    private val factoryConfiguration: DotNetProjectConfigurationFactory = DotNetProjectConfigurationFactory(this)

    init {
        addFactory(factoryConfiguration)
    }

    override fun isApplicable(kind: RunnableProjectKind) =
            kind == RunnableProjectKind.Console
}