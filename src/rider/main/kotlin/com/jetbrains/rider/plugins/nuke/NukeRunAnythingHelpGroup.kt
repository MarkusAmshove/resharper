package com.jetbrains.rider.plugins.nuke

import com.intellij.ide.actions.runAnything.activity.RunAnythingProvider
import com.intellij.ide.actions.runAnything.groups.RunAnythingHelpGroup
import com.intellij.util.containers.ContainerUtil

class NukeRunAnythingHelpGroup : RunAnythingHelpGroup<NukeRunAnythingProvider>() {
    override fun getProviders() =
        ContainerUtil.immutableSingletonList(RunAnythingProvider.EP_NAME.findExtension(NukeRunAnythingProvider::class.java))

    override fun getTitle() = ".NET"
}
