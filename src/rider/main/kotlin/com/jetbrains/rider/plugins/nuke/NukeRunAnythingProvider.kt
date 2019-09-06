package com.jetbrains.rider.plugins.nuke

import com.intellij.execution.DefaultExecutionTarget
import com.intellij.execution.ExecutionManager
import com.intellij.execution.RunManager
import com.intellij.ide.actions.runAnything.RunAnythingAction.EXECUTOR_KEY
import com.intellij.ide.actions.runAnything.RunAnythingUtil
import com.intellij.ide.actions.runAnything.activity.RunAnythingProviderBase
import com.intellij.ide.actions.runAnything.items.RunAnythingItem
import com.intellij.openapi.actionSystem.DataContext
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.util.text.StringUtil
import com.intellij.openapi.util.text.StringUtil.*
import com.jetbrains.rider.model.runnableProjectsModel
import com.jetbrains.rider.projectView.path
import com.jetbrains.rider.projectView.solution
import com.jetbrains.rider.util.idea.Project
import java.io.File

class NukeRunAnythingProvider : RunAnythingProviderBase<String?>() {
    override fun getHelpCommandPlaceholder() = "nuke <target ...> <--parameter ...>"

    override fun getCompletionGroupTitle() = "NUKE"

    override fun getIcon(value: String) = NukeIcons.Icon

    override fun getHelpIcon() = NukeIcons.Icon

    override fun getHelpCommand() = "nuke"

    override fun getValues(dataContext: DataContext, pattern: String): MutableCollection<String?> {
        return if (!pattern.startsWith(helpCommand)) mutableListOf()
        else dataContext.Project!!.solution.nukeModel.complete.sync(pattern)!!.toMutableList()

    }

    override fun execute(dataContext: DataContext, command: String) {
        val commandToExecute = trimStart(command, helpCommand)
        val project = RunAnythingUtil.fetchProject(dataContext)
        val executionManager = ExecutionManager.getInstance(project)
        val executor = EXECUTOR_KEY.getData(dataContext) ?: return

        // HACK: Assume build project name
        val buildProjectPath = project.solution.runnableProjectsModel.projects.valueOrNull!!.single { it -> it.projectFilePath.endsWith("_build.csproj") }.projectFilePath
        val runManager = RunManager.getInstance(project)
        val runConfiguration = findOrCreateConfiguration(
            commandToExecute,
            runManager,
            project,
            buildProjectPath,
            commandToExecute,
            emptyMap()
        )

        executionManager.restartRunProfile(
            project,
            executor,
            DefaultExecutionTarget.INSTANCE,
            runConfiguration,
            null
        )
    }

    override fun findMatchingValue(dataContext: DataContext, pattern: String): String? {
        return if(pattern.startsWith(helpCommand)) getCommand(pattern) else null
    }

    override fun getMainListItem(dataContext: DataContext, value: String): RunAnythingItem {
        return RunAnythingNukeItem(value)
    }

    override fun getCommand(command: String): String {
        return command
    }
}
