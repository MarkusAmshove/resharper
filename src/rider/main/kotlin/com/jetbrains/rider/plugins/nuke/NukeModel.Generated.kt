@file:Suppress("PackageDirectoryMismatch", "UnusedImport", "unused", "LocalVariableName")
package com.jetbrains.rider.plugins.nuke

import com.jetbrains.rd.framework.*
import com.jetbrains.rd.framework.base.*
import com.jetbrains.rd.framework.impl.*

import com.jetbrains.rd.util.lifetime.*
import com.jetbrains.rd.util.reactive.*
import com.jetbrains.rd.util.string.*
import com.jetbrains.rd.util.*
import kotlin.reflect.KClass



class NukeModel private constructor(
    private val _build: RdSignal<BuildInvocation>,
    private val _targets: RdOptionalProperty<Array<NukeTarget>>,
    private val _parameters: RdOptionalProperty<Array<NukeParameter>>
) : RdExtBase() {
    //companion
    
    companion object : ISerializersOwner {
        
        override fun registerSerializersCore(serializers: ISerializers) {
            serializers.register(BuildInvocation)
            serializers.register(NukeTarget)
            serializers.register(NukeParameter)
        }
        
        
        
        private val __NukeTargetArraySerializer = NukeTarget.array()
        private val __NukeParameterArraySerializer = NukeParameter.array()
        
        const val serializationHash = 4328651711297058255L
    }
    override val serializersOwner: ISerializersOwner get() = NukeModel
    override val serializationHash: Long get() = NukeModel.serializationHash
    
    //fields
    val build: ISignal<BuildInvocation> get() = _build
    val targets: IOptProperty<Array<NukeTarget>> get() = _targets
    val parameters: IOptProperty<Array<NukeParameter>> get() = _parameters
    //initializer
    init {
        _targets.optimizeNested = true
        _parameters.optimizeNested = true
    }
    
    init {
        bindableChildren.add("build" to _build)
        bindableChildren.add("targets" to _targets)
        bindableChildren.add("parameters" to _parameters)
    }
    
    //secondary constructor
    internal constructor(
    ) : this(
        RdSignal<BuildInvocation>(BuildInvocation),
        RdOptionalProperty<Array<NukeTarget>>(__NukeTargetArraySerializer),
        RdOptionalProperty<Array<NukeParameter>>(__NukeParameterArraySerializer)
    )
    
    //equals trait
    //hash code trait
    //pretty print
    override fun print(printer: PrettyPrinter) {
        printer.println("NukeModel (")
        printer.indent {
            print("build = "); _build.print(printer); println()
            print("targets = "); _targets.print(printer); println()
            print("parameters = "); _parameters.print(printer); println()
        }
        printer.print(")")
    }
}
val com.jetbrains.rider.model.Solution.nukeModel get() = getOrCreateExtension("nukeModel", ::NukeModel)



data class BuildInvocation (
    val projectFile: String,
    val target: String,
    val debugMode: Boolean,
    val skipDependencies: Boolean
) : IPrintable {
    //companion
    
    companion object : IMarshaller<BuildInvocation> {
        override val _type: KClass<BuildInvocation> = BuildInvocation::class
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): BuildInvocation {
            val projectFile = buffer.readString()
            val target = buffer.readString()
            val debugMode = buffer.readBool()
            val skipDependencies = buffer.readBool()
            return BuildInvocation(projectFile, target, debugMode, skipDependencies)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: BuildInvocation) {
            buffer.writeString(value.projectFile)
            buffer.writeString(value.target)
            buffer.writeBool(value.debugMode)
            buffer.writeBool(value.skipDependencies)
        }
        
    }
    //fields
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other == null || other::class != this::class) return false
        
        other as BuildInvocation
        
        if (projectFile != other.projectFile) return false
        if (target != other.target) return false
        if (debugMode != other.debugMode) return false
        if (skipDependencies != other.skipDependencies) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int {
        var __r = 0
        __r = __r*31 + projectFile.hashCode()
        __r = __r*31 + target.hashCode()
        __r = __r*31 + debugMode.hashCode()
        __r = __r*31 + skipDependencies.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter) {
        printer.println("BuildInvocation (")
        printer.indent {
            print("projectFile = "); projectFile.print(printer); println()
            print("target = "); target.print(printer); println()
            print("debugMode = "); debugMode.print(printer); println()
            print("skipDependencies = "); skipDependencies.print(printer); println()
        }
        printer.print(")")
    }
}


data class NukeParameter (
    val name: String,
    val description: String?,
    val defaultValue: String?
) : IPrintable {
    //companion
    
    companion object : IMarshaller<NukeParameter> {
        override val _type: KClass<NukeParameter> = NukeParameter::class
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): NukeParameter {
            val name = buffer.readString()
            val description = buffer.readNullable { buffer.readString() }
            val defaultValue = buffer.readNullable { buffer.readString() }
            return NukeParameter(name, description, defaultValue)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: NukeParameter) {
            buffer.writeString(value.name)
            buffer.writeNullable(value.description) { buffer.writeString(it) }
            buffer.writeNullable(value.defaultValue) { buffer.writeString(it) }
        }
        
    }
    //fields
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other == null || other::class != this::class) return false
        
        other as NukeParameter
        
        if (name != other.name) return false
        if (description != other.description) return false
        if (defaultValue != other.defaultValue) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int {
        var __r = 0
        __r = __r*31 + name.hashCode()
        __r = __r*31 + if (description != null) description.hashCode() else 0
        __r = __r*31 + if (defaultValue != null) defaultValue.hashCode() else 0
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter) {
        printer.println("NukeParameter (")
        printer.indent {
            print("name = "); name.print(printer); println()
            print("description = "); description.print(printer); println()
            print("defaultValue = "); defaultValue.print(printer); println()
        }
        printer.print(")")
    }
}


data class NukeTarget (
    val name: String,
    val description: String?,
    val dependsOn: Array<String>,
    val dependentFor: Array<String>,
    val before: Array<String>,
    val after: Array<String>,
    val triggers: Array<String>,
    val triggeredBy: Array<String>
) : IPrintable {
    //companion
    
    companion object : IMarshaller<NukeTarget> {
        override val _type: KClass<NukeTarget> = NukeTarget::class
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): NukeTarget {
            val name = buffer.readString()
            val description = buffer.readNullable { buffer.readString() }
            val dependsOn = buffer.readArray {buffer.readString()}
            val dependentFor = buffer.readArray {buffer.readString()}
            val before = buffer.readArray {buffer.readString()}
            val after = buffer.readArray {buffer.readString()}
            val triggers = buffer.readArray {buffer.readString()}
            val triggeredBy = buffer.readArray {buffer.readString()}
            return NukeTarget(name, description, dependsOn, dependentFor, before, after, triggers, triggeredBy)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: NukeTarget) {
            buffer.writeString(value.name)
            buffer.writeNullable(value.description) { buffer.writeString(it) }
            buffer.writeArray(value.dependsOn) { buffer.writeString(it) }
            buffer.writeArray(value.dependentFor) { buffer.writeString(it) }
            buffer.writeArray(value.before) { buffer.writeString(it) }
            buffer.writeArray(value.after) { buffer.writeString(it) }
            buffer.writeArray(value.triggers) { buffer.writeString(it) }
            buffer.writeArray(value.triggeredBy) { buffer.writeString(it) }
        }
        
    }
    //fields
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other == null || other::class != this::class) return false
        
        other as NukeTarget
        
        if (name != other.name) return false
        if (description != other.description) return false
        if (!(dependsOn contentDeepEquals other.dependsOn)) return false
        if (!(dependentFor contentDeepEquals other.dependentFor)) return false
        if (!(before contentDeepEquals other.before)) return false
        if (!(after contentDeepEquals other.after)) return false
        if (!(triggers contentDeepEquals other.triggers)) return false
        if (!(triggeredBy contentDeepEquals other.triggeredBy)) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int {
        var __r = 0
        __r = __r*31 + name.hashCode()
        __r = __r*31 + if (description != null) description.hashCode() else 0
        __r = __r*31 + dependsOn.contentDeepHashCode()
        __r = __r*31 + dependentFor.contentDeepHashCode()
        __r = __r*31 + before.contentDeepHashCode()
        __r = __r*31 + after.contentDeepHashCode()
        __r = __r*31 + triggers.contentDeepHashCode()
        __r = __r*31 + triggeredBy.contentDeepHashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter) {
        printer.println("NukeTarget (")
        printer.indent {
            print("name = "); name.print(printer); println()
            print("description = "); description.print(printer); println()
            print("dependsOn = "); dependsOn.print(printer); println()
            print("dependentFor = "); dependentFor.print(printer); println()
            print("before = "); before.print(printer); println()
            print("after = "); after.print(printer); println()
            print("triggers = "); triggers.print(printer); println()
            print("triggeredBy = "); triggeredBy.print(printer); println()
        }
        printer.print(")")
    }
}
