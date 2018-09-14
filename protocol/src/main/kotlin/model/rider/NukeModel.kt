package model.rider

import com.jetbrains.rider.generator.nova.*
import com.jetbrains.rider.generator.nova.PredefinedType.bool
import com.jetbrains.rider.generator.nova.PredefinedType.string
import com.jetbrains.rider.model.nova.ide.SolutionModel

@Suppress("unused")
object NukeModel : Ext(SolutionModel.Solution) {

    val BuildInvocation = structdef {
        field("projectFile", string)
        field("target", string)
        field("debugMode", bool)
        field("skipDependencies", bool)
    }

    init {
        //setting(CSharp50Generator.Namespace, "ReSharper.Nuke.Rider")
        //setting(Kotlin11Generator.Namespace, "com.jetbrains.rider.nuke")

        signal("build", BuildInvocation)
    }

}