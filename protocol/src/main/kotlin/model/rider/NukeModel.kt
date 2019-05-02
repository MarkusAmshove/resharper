package model.rider

import com.jetbrains.rd.generator.nova.Ext
import com.jetbrains.rd.generator.nova.PredefinedType.*
import com.jetbrains.rd.generator.nova.csharp.CSharp50Generator
import com.jetbrains.rd.generator.nova.field
import com.jetbrains.rd.generator.nova.kotlin.Kotlin11Generator
import com.jetbrains.rd.generator.nova.setting
import com.jetbrains.rd.generator.nova.signal
import com.jetbrains.rider.model.nova.ide.SolutionModel.Solution

@Suppress("unused")
object NukeModel : Ext(Solution) {

    val BuildInvocation = structdef {
        field("projectFile", string)
        field("target", string)
        field("debugMode", bool)
        field("skipDependencies", bool)
    }

    init {
        setting(CSharp50Generator.Namespace, "ReSharper.Nuke.Rider")
        setting(Kotlin11Generator.Namespace, "com.jetbrains.rider.plugins.nuke")

        signal("build", BuildInvocation)
    }

}