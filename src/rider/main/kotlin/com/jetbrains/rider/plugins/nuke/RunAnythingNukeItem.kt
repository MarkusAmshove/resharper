package com.jetbrains.rider.plugins.nuke

import com.intellij.ide.actions.runAnything.items.RunAnythingItemBase
import com.intellij.openapi.util.text.StringUtil
import com.intellij.ui.SimpleColoredComponent
import com.intellij.ui.SimpleTextAttributes
import java.awt.Component

class RunAnythingNukeItem(command: String) : RunAnythingItemBase(command, NukeIcons.Icon) {

    override fun createComponent(isSelected: Boolean): Component {
        val component = SimpleColoredComponent()
        component.append(command)
        component.appendTextPadding(20)
        setupIcon(component, myIcon)

        description?.let {
            component.append(
                " ${StringUtil.shortenTextWithEllipsis(it, 200, 0)}",
                SimpleTextAttributes.GRAYED_ITALIC_ATTRIBUTES)
        }

        return component
    }

    override fun getDescription() : String? {
        return null
    }

}
