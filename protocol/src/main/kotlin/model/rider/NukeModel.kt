package model.rider

import com.jetbrains.rd.generator.nova.*
import com.jetbrains.rd.generator.nova.PredefinedType.*
import com.jetbrains.rd.generator.nova.csharp.CSharp50Generator
import com.jetbrains.rd.generator.nova.kotlin.Kotlin11Generator
import com.jetbrains.rider.model.nova.ide.SolutionModel.Solution

@Suppress("unused")
object NukeModel : Ext(Solution) {
    init {
        setting(CSharp50Generator.Namespace, "ReSharper.Nuke.Rider")
        setting(Kotlin11Generator.Namespace, "com.jetbrains.rider.plugins.nuke")

        signal("build", structdef("BuildInvocation") {
            field("projectFile", string)
            field("target", string)
            field("debugMode", bool)
            field("skipDependencies", bool)
        })

        property("targets", array(structdef("NukeTarget") {
            field("name", string)
            field("description", string.nullable)
            field("dependsOn", array(string))
            field("dependentFor", array(string))
            field("before", array(string))
            field("after", array(string))
            field("triggers", array(string))
            field("triggeredBy", array(string))
        }))

        property("parameters", array(structdef("NukeParameter") {
            field("name", string)
            field("description", string.nullable)
            field("defaultValue", string.nullable)
        }))
    }
}