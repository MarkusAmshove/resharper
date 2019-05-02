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
    private val _build: RdSignal<BuildInvocation>
) : RdExtBase() {
    //companion
    
    companion object : ISerializersOwner {
        
        override fun registerSerializersCore(serializers: ISerializers) {
            serializers.register(BuildInvocation)
        }
        
        
        
        
        const val serializationHash = -3461171631543031882L
    }
    override val serializersOwner: ISerializersOwner get() = NukeModel
    override val serializationHash: Long get() = NukeModel.serializationHash
    
    //fields
    val build: ISignal<BuildInvocation> get() = _build
    //initializer
    init {
        bindableChildren.add("build" to _build)
    }
    
    //secondary constructor
    internal constructor(
    ) : this(
        RdSignal<BuildInvocation>(BuildInvocation)
    )
    
    //equals trait
    //hash code trait
    //pretty print
    override fun print(printer: PrettyPrinter) {
        printer.println("NukeModel (")
        printer.indent {
            print("build = "); _build.print(printer); println()
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
