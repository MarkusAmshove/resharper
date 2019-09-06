using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

using JetBrains.Core;
using JetBrains.Diagnostics;
using JetBrains.Collections;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using JetBrains.Serialization;
using JetBrains.Rd;
using JetBrains.Rd.Base;
using JetBrains.Rd.Impl;
using JetBrains.Rd.Tasks;
using JetBrains.Rd.Util;
using JetBrains.Rd.Text;


// ReSharper disable RedundantEmptyObjectCreationArgumentList
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantOverflowCheckingContext


namespace ReSharper.Nuke.Rider
{
  
  
  public class NukeModel : RdExtBase
  {
    //fields
    //public fields
    [NotNull] public ISignal<BuildInvocation> Build => _Build;
    [NotNull] public IViewableProperty<NukeTarget[]> Targets => _Targets;
    [NotNull] public IViewableProperty<NukeParameter[]> Parameters => _Parameters;
    [NotNull] public RdEndpoint<string, string[]> Complete => _Complete;
    
    //private fields
    [NotNull] private readonly RdSignal<BuildInvocation> _Build;
    [NotNull] private readonly RdProperty<NukeTarget[]> _Targets;
    [NotNull] private readonly RdProperty<NukeParameter[]> _Parameters;
    [NotNull] private readonly RdEndpoint<string, string[]> _Complete;
    
    //primary constructor
    private NukeModel(
      [NotNull] RdSignal<BuildInvocation> build,
      [NotNull] RdProperty<NukeTarget[]> targets,
      [NotNull] RdProperty<NukeParameter[]> parameters,
      [NotNull] RdEndpoint<string, string[]> complete
    )
    {
      if (build == null) throw new ArgumentNullException("build");
      if (targets == null) throw new ArgumentNullException("targets");
      if (parameters == null) throw new ArgumentNullException("parameters");
      if (complete == null) throw new ArgumentNullException("complete");
      
      _Build = build;
      _Targets = targets;
      _Parameters = parameters;
      _Complete = complete;
      _Targets.OptimizeNested = true;
      _Parameters.OptimizeNested = true;
      _Complete.Async = true;
      BindableChildren.Add(new KeyValuePair<string, object>("build", _Build));
      BindableChildren.Add(new KeyValuePair<string, object>("targets", _Targets));
      BindableChildren.Add(new KeyValuePair<string, object>("parameters", _Parameters));
      BindableChildren.Add(new KeyValuePair<string, object>("complete", _Complete));
    }
    //secondary constructor
    internal NukeModel (
    ) : this (
      new RdSignal<BuildInvocation>(BuildInvocation.Read, BuildInvocation.Write),
      new RdProperty<NukeTarget[]>(ReadNukeTargetArray, WriteNukeTargetArray),
      new RdProperty<NukeParameter[]>(ReadNukeParameterArray, WriteNukeParameterArray),
      new RdEndpoint<string, string[]>(JetBrains.Rd.Impl.Serializers.ReadString, JetBrains.Rd.Impl.Serializers.WriteString, ReadStringArray, WriteStringArray)
    ) {}
    //statics
    
    public static CtxReadDelegate<NukeTarget[]> ReadNukeTargetArray = NukeTarget.Read.Array();
    public static CtxReadDelegate<NukeParameter[]> ReadNukeParameterArray = NukeParameter.Read.Array();
    public static CtxReadDelegate<string[]> ReadStringArray = JetBrains.Rd.Impl.Serializers.ReadString.Array();
    
    public static CtxWriteDelegate<NukeTarget[]> WriteNukeTargetArray = NukeTarget.Write.Array();
    public static CtxWriteDelegate<NukeParameter[]> WriteNukeParameterArray = NukeParameter.Write.Array();
    public static CtxWriteDelegate<string[]> WriteStringArray = JetBrains.Rd.Impl.Serializers.WriteString.Array();
    
    protected override long SerializationHash => -6243300511775699317L;
    
    protected override Action<ISerializers> Register => RegisterDeclaredTypesSerializers;
    public static void RegisterDeclaredTypesSerializers(ISerializers serializers)
    {
      
      serializers.RegisterToplevelOnce(typeof(JetBrains.Rider.Model.IdeRoot), JetBrains.Rider.Model.IdeRoot.RegisterDeclaredTypesSerializers);
    }
    
    //custom body
    //equals trait
    //hash code trait
    //pretty print
    public override void Print(PrettyPrinter printer)
    {
      printer.Println("NukeModel (");
      using (printer.IndentCookie()) {
        printer.Print("build = "); _Build.PrintEx(printer); printer.Println();
        printer.Print("targets = "); _Targets.PrintEx(printer); printer.Println();
        printer.Print("parameters = "); _Parameters.PrintEx(printer); printer.Println();
        printer.Print("complete = "); _Complete.PrintEx(printer); printer.Println();
      }
      printer.Print(")");
    }
    //toString
    public override string ToString()
    {
      var printer = new SingleLinePrettyPrinter();
      Print(printer);
      return printer.ToString();
    }
  }
  public static class SolutionNukeModelEx
   {
    public static NukeModel GetNukeModel(this JetBrains.Rider.Model.Solution solution)
    {
      return solution.GetOrCreateExtension("nukeModel", () => new NukeModel());
    }
  }
  
  
  public class BuildInvocation : IPrintable, IEquatable<BuildInvocation>
  {
    //fields
    //public fields
    [NotNull] public string ProjectFile {get; private set;}
    [NotNull] public string Target {get; private set;}
    public bool DebugMode {get; private set;}
    public bool SkipDependencies {get; private set;}
    
    //private fields
    //primary constructor
    public BuildInvocation(
      [NotNull] string projectFile,
      [NotNull] string target,
      bool debugMode,
      bool skipDependencies
    )
    {
      if (projectFile == null) throw new ArgumentNullException("projectFile");
      if (target == null) throw new ArgumentNullException("target");
      
      ProjectFile = projectFile;
      Target = target;
      DebugMode = debugMode;
      SkipDependencies = skipDependencies;
    }
    //secondary constructor
    //statics
    
    public static CtxReadDelegate<BuildInvocation> Read = (ctx, reader) => 
    {
      var projectFile = reader.ReadString();
      var target = reader.ReadString();
      var debugMode = reader.ReadBool();
      var skipDependencies = reader.ReadBool();
      var _result = new BuildInvocation(projectFile, target, debugMode, skipDependencies);
      return _result;
    };
    
    public static CtxWriteDelegate<BuildInvocation> Write = (ctx, writer, value) => 
    {
      writer.Write(value.ProjectFile);
      writer.Write(value.Target);
      writer.Write(value.DebugMode);
      writer.Write(value.SkipDependencies);
    };
    //custom body
    //equals trait
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((BuildInvocation) obj);
    }
    public bool Equals(BuildInvocation other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return ProjectFile == other.ProjectFile && Target == other.Target && DebugMode == other.DebugMode && SkipDependencies == other.SkipDependencies;
    }
    //hash code trait
    public override int GetHashCode()
    {
      unchecked {
        var hash = 0;
        hash = hash * 31 + ProjectFile.GetHashCode();
        hash = hash * 31 + Target.GetHashCode();
        hash = hash * 31 + DebugMode.GetHashCode();
        hash = hash * 31 + SkipDependencies.GetHashCode();
        return hash;
      }
    }
    //pretty print
    public void Print(PrettyPrinter printer)
    {
      printer.Println("BuildInvocation (");
      using (printer.IndentCookie()) {
        printer.Print("projectFile = "); ProjectFile.PrintEx(printer); printer.Println();
        printer.Print("target = "); Target.PrintEx(printer); printer.Println();
        printer.Print("debugMode = "); DebugMode.PrintEx(printer); printer.Println();
        printer.Print("skipDependencies = "); SkipDependencies.PrintEx(printer); printer.Println();
      }
      printer.Print(")");
    }
    //toString
    public override string ToString()
    {
      var printer = new SingleLinePrettyPrinter();
      Print(printer);
      return printer.ToString();
    }
  }
  
  
  public class NukeParameter : IPrintable, IEquatable<NukeParameter>
  {
    //fields
    //public fields
    [NotNull] public string Name {get; private set;}
    [CanBeNull] public string Description {get; private set;}
    [CanBeNull] public string DefaultValue {get; private set;}
    
    //private fields
    //primary constructor
    public NukeParameter(
      [NotNull] string name,
      [CanBeNull] string description,
      [CanBeNull] string defaultValue
    )
    {
      if (name == null) throw new ArgumentNullException("name");
      
      Name = name;
      Description = description;
      DefaultValue = defaultValue;
    }
    //secondary constructor
    //statics
    
    public static CtxReadDelegate<NukeParameter> Read = (ctx, reader) => 
    {
      var name = reader.ReadString();
      var description = ReadStringNullable(ctx, reader);
      var defaultValue = ReadStringNullable(ctx, reader);
      var _result = new NukeParameter(name, description, defaultValue);
      return _result;
    };
    public static CtxReadDelegate<string> ReadStringNullable = JetBrains.Rd.Impl.Serializers.ReadString.NullableClass();
    
    public static CtxWriteDelegate<NukeParameter> Write = (ctx, writer, value) => 
    {
      writer.Write(value.Name);
      WriteStringNullable(ctx, writer, value.Description);
      WriteStringNullable(ctx, writer, value.DefaultValue);
    };
    public static CtxWriteDelegate<string> WriteStringNullable = JetBrains.Rd.Impl.Serializers.WriteString.NullableClass();
    //custom body
    //equals trait
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((NukeParameter) obj);
    }
    public bool Equals(NukeParameter other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Name == other.Name && Equals(Description, other.Description) && Equals(DefaultValue, other.DefaultValue);
    }
    //hash code trait
    public override int GetHashCode()
    {
      unchecked {
        var hash = 0;
        hash = hash * 31 + Name.GetHashCode();
        hash = hash * 31 + (Description != null ? Description.GetHashCode() : 0);
        hash = hash * 31 + (DefaultValue != null ? DefaultValue.GetHashCode() : 0);
        return hash;
      }
    }
    //pretty print
    public void Print(PrettyPrinter printer)
    {
      printer.Println("NukeParameter (");
      using (printer.IndentCookie()) {
        printer.Print("name = "); Name.PrintEx(printer); printer.Println();
        printer.Print("description = "); Description.PrintEx(printer); printer.Println();
        printer.Print("defaultValue = "); DefaultValue.PrintEx(printer); printer.Println();
      }
      printer.Print(")");
    }
    //toString
    public override string ToString()
    {
      var printer = new SingleLinePrettyPrinter();
      Print(printer);
      return printer.ToString();
    }
  }
  
  
  public class NukeTarget : IPrintable, IEquatable<NukeTarget>
  {
    //fields
    //public fields
    [NotNull] public string Name {get; private set;}
    [CanBeNull] public string Description {get; private set;}
    [NotNull] public string[] DependsOn {get; private set;}
    [NotNull] public string[] DependentFor {get; private set;}
    [NotNull] public string[] Before {get; private set;}
    [NotNull] public string[] After {get; private set;}
    [NotNull] public string[] Triggers {get; private set;}
    [NotNull] public string[] TriggeredBy {get; private set;}
    
    //private fields
    //primary constructor
    public NukeTarget(
      [NotNull] string name,
      [CanBeNull] string description,
      [NotNull] string[] dependsOn,
      [NotNull] string[] dependentFor,
      [NotNull] string[] before,
      [NotNull] string[] after,
      [NotNull] string[] triggers,
      [NotNull] string[] triggeredBy
    )
    {
      if (name == null) throw new ArgumentNullException("name");
      if (dependsOn == null) throw new ArgumentNullException("dependsOn");
      if (dependentFor == null) throw new ArgumentNullException("dependentFor");
      if (before == null) throw new ArgumentNullException("before");
      if (after == null) throw new ArgumentNullException("after");
      if (triggers == null) throw new ArgumentNullException("triggers");
      if (triggeredBy == null) throw new ArgumentNullException("triggeredBy");
      
      Name = name;
      Description = description;
      DependsOn = dependsOn;
      DependentFor = dependentFor;
      Before = before;
      After = after;
      Triggers = triggers;
      TriggeredBy = triggeredBy;
    }
    //secondary constructor
    //statics
    
    public static CtxReadDelegate<NukeTarget> Read = (ctx, reader) => 
    {
      var name = reader.ReadString();
      var description = ReadStringNullable(ctx, reader);
      var dependsOn = ReadStringArray(ctx, reader);
      var dependentFor = ReadStringArray(ctx, reader);
      var before = ReadStringArray(ctx, reader);
      var after = ReadStringArray(ctx, reader);
      var triggers = ReadStringArray(ctx, reader);
      var triggeredBy = ReadStringArray(ctx, reader);
      var _result = new NukeTarget(name, description, dependsOn, dependentFor, before, after, triggers, triggeredBy);
      return _result;
    };
    public static CtxReadDelegate<string> ReadStringNullable = JetBrains.Rd.Impl.Serializers.ReadString.NullableClass();
    public static CtxReadDelegate<string[]> ReadStringArray = JetBrains.Rd.Impl.Serializers.ReadString.Array();
    
    public static CtxWriteDelegate<NukeTarget> Write = (ctx, writer, value) => 
    {
      writer.Write(value.Name);
      WriteStringNullable(ctx, writer, value.Description);
      WriteStringArray(ctx, writer, value.DependsOn);
      WriteStringArray(ctx, writer, value.DependentFor);
      WriteStringArray(ctx, writer, value.Before);
      WriteStringArray(ctx, writer, value.After);
      WriteStringArray(ctx, writer, value.Triggers);
      WriteStringArray(ctx, writer, value.TriggeredBy);
    };
    public static CtxWriteDelegate<string> WriteStringNullable = JetBrains.Rd.Impl.Serializers.WriteString.NullableClass();
    public static CtxWriteDelegate<string[]> WriteStringArray = JetBrains.Rd.Impl.Serializers.WriteString.Array();
    //custom body
    //equals trait
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((NukeTarget) obj);
    }
    public bool Equals(NukeTarget other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Name == other.Name && Equals(Description, other.Description) && DependsOn.SequenceEqual(other.DependsOn) && DependentFor.SequenceEqual(other.DependentFor) && Before.SequenceEqual(other.Before) && After.SequenceEqual(other.After) && Triggers.SequenceEqual(other.Triggers) && TriggeredBy.SequenceEqual(other.TriggeredBy);
    }
    //hash code trait
    public override int GetHashCode()
    {
      unchecked {
        var hash = 0;
        hash = hash * 31 + Name.GetHashCode();
        hash = hash * 31 + (Description != null ? Description.GetHashCode() : 0);
        hash = hash * 31 + DependsOn.ContentHashCode();
        hash = hash * 31 + DependentFor.ContentHashCode();
        hash = hash * 31 + Before.ContentHashCode();
        hash = hash * 31 + After.ContentHashCode();
        hash = hash * 31 + Triggers.ContentHashCode();
        hash = hash * 31 + TriggeredBy.ContentHashCode();
        return hash;
      }
    }
    //pretty print
    public void Print(PrettyPrinter printer)
    {
      printer.Println("NukeTarget (");
      using (printer.IndentCookie()) {
        printer.Print("name = "); Name.PrintEx(printer); printer.Println();
        printer.Print("description = "); Description.PrintEx(printer); printer.Println();
        printer.Print("dependsOn = "); DependsOn.PrintEx(printer); printer.Println();
        printer.Print("dependentFor = "); DependentFor.PrintEx(printer); printer.Println();
        printer.Print("before = "); Before.PrintEx(printer); printer.Println();
        printer.Print("after = "); After.PrintEx(printer); printer.Println();
        printer.Print("triggers = "); Triggers.PrintEx(printer); printer.Println();
        printer.Print("triggeredBy = "); TriggeredBy.PrintEx(printer); printer.Println();
      }
      printer.Print(")");
    }
    //toString
    public override string ToString()
    {
      var printer = new SingleLinePrettyPrinter();
      Print(printer);
      return printer.ToString();
    }
  }
}
