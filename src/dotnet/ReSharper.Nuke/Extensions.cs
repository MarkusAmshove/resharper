// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/resharper/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.BuildScripts.Tree;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util.Reflection;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
#if RIDER
using ReSharper.Nuke.Rider;
#endif

namespace ReSharper.Nuke
{
    public static class Extensions
    {
        private static readonly string s_commonAssemblyName = typeof(NukeBuild).Assembly.GetName().Name.NotNull();
        private static readonly string s_nukeBuildType = typeof(NukeBuild).FullName.NotNull();
        private static readonly string s_targetType = typeof(Target).FullName.NotNull();
        private static readonly string s_parameterAttributeType = typeof(ParameterAttribute).FullName.NotNull();

        public static IEnumerable<IClass> FindNukeBuildClasses(this IFinder finder, params IPsiModule[] modules)
        {
            var nukeBuildClasses = new List<IClass>();

            using (CompilationContextCookie.GetExplicitUniversalContextIfNotSet())
            {
                foreach (var module in modules)
                {
                    var nukeBuildDeclaredType = TypeFactory.CreateTypeByCLRName(s_nukeBuildType, module).GetTypeElement();
                    if (nukeBuildDeclaredType == null)
                        continue;

                    finder.FindInheritors(
                        nukeBuildDeclaredType,
                        new FindResultConsumer(x =>
                        {
                            var clazz = (x as FindResultDeclaredElement)?.DeclaredElement as IClass;
                            if (clazz == null)
                                return FindExecution.Continue;

                            if (clazz.GetDeclarations().FirstOrDefault()?.GetProject()?.Name.EndsWith("Tests") ?? true)
                                return FindExecution.Continue;

                            nukeBuildClasses.Add(clazz);

                            return FindExecution.Continue;
                        }),
                        NullProgressIndicator.Create());
                }
            }

            return nukeBuildClasses;
        }

        public static bool IsNukeProject(this IProject project)
        {
            return ReferencedAssembliesService.IsProjectReferencingAssemblyByName(
                project,
                project.GetCurrentTargetFrameworkId(),
                AssemblyNameInfoFactory.Create2(s_commonAssemblyName, version: null),
                out _);

            // var packages = nugetPackageReferenceTracker.GetInstalledPackages(project);
            // return packages.Any(x => x.PackageIdentity.Id == "Nuke.Common");
        }

        public static bool IsNukeClass(this ITypeElement clazz)
        {
            var nukeBuildDeclaredType = TypeFactory.CreateTypeByCLRName(s_nukeBuildType, clazz.Module).GetTypeElement();
            return clazz.IsDescendantOf(nukeBuildDeclaredType);
            // return clazz.GetAllSuperClasses().Any(x => x.GetClrName().FullName == "Nuke.Common.NukeBuild");
        }

        public static NukeTarget GetNukeTarget(this ITypeMember member)
        {
            var targetDeclaredType = TypeFactory.CreateTypeByCLRName(s_targetType, member.Module);
            var targetProperty = member as JetBrains.ReSharper.Psi.IProperty;
            if (!targetProperty?.Type.Equals(targetDeclaredType) ?? true)
                return null;

            var declaration = targetProperty.GetDeclarations().FirstOrDefault() as IPropertyDeclaration;
            var lambdaExpression = declaration?.ArrowClause.Expression as ILambdaExpression;
            var invocationExpression = lambdaExpression?.BodyExpression as IInvocationExpression;

            var lookupTable = new LookupTable<string, string>();
            var description = default(string);
            while (invocationExpression != null)
            {
                var invokedExpression = invocationExpression.InvokedExpression as IReferenceExpression;
                var invokedMethodName = invokedExpression?.NameIdentifier.Name;

                if (new[]
                    {
                        nameof(ITargetDefinition.DependsOn),
                        nameof(ITargetDefinition.DependentFor),
                        nameof(ITargetDefinition.Before),
                        nameof(ITargetDefinition.After),
                        nameof(ITargetDefinition.Triggers),
                        nameof(ITargetDefinition.TriggeredBy)
                    }.Contains(invokedMethodName))
                {
                    var referencedSymbols = invocationExpression.Arguments
                        .Select(x => x.Expression)
                        .OfType<IReferenceExpression>()
                        .Where(x => x.Reference.Resolve().ResolveErrorType == ResolveErrorType.OK)
                        .Select(x => x.NameIdentifier.Name).ToList();
                    lookupTable.AddRange(invokedMethodName, referencedSymbols);
                }
                else if (invokedMethodName == nameof(ITargetDefinition.Description))
                {
                    var descriptionArgument = invocationExpression.Arguments
                        .Select(x => x.Expression)
                        .OfType<ILiteralExpression>()
                        .SingleOrDefault();

                    if (descriptionArgument != null && descriptionArgument.IsConstantValue())
                        description = descriptionArgument.ConstantValue.Value?.ToString();
                }

                invocationExpression = invokedExpression?.QualifierExpression as IInvocationExpression;
            }

            return new NukeTarget(
                member.ShortName,
                description,
                lookupTable[nameof(ITargetDefinition.DependsOn)].ToArray(),
                lookupTable[nameof(ITargetDefinition.DependentFor)].ToArray(),
                lookupTable[nameof(ITargetDefinition.Before)].ToArray(),
                lookupTable[nameof(ITargetDefinition.After)].ToArray(),
                lookupTable[nameof(ITargetDefinition.Triggers)].ToArray(),
                lookupTable[nameof(ITargetDefinition.TriggeredBy)].ToArray());
        }

        [CanBeNull]
        public static NukeParameter GetNukeParameter(this ITypeMember member)
        {
            var parameterAttributeDeclaredType = TypeFactory.CreateTypeByCLRName(s_parameterAttributeType, member.Module);
            // TODO: check derived types from ParameterAttribute
            var attribute = member.GetAttributeInstances(parameterAttributeDeclaredType.GetClrName(), inherit: true).SingleOrDefault();
            if (attribute == null)
                return null;

            var (_, attributeNameValue) = attribute.NamedParameters().SingleOrDefault(x => x.First == nameof(ParameterAttribute.Name));
            var name = attributeNameValue != null && attributeNameValue.IsConstant
                ? attributeNameValue.ConstantValue.Value?.ToString() ?? member.ShortName
                : member.ShortName;

            var descriptionValue = attribute.PositionParameters().FirstOrDefault();
            var description = descriptionValue != null && descriptionValue.IsConstant
                ? descriptionValue.ConstantValue.Value?.ToString()
                : null;

            var declaration = member.GetDeclarations().FirstOrDefault() as IInitializerOwnerDeclaration;
            var defaultValue = declaration?.Initializer?.GetText().TrimMatchingDoubleQuotes();

            return new NukeParameter(name, description, defaultValue);
        }
    }

    #if RESHARPER
    public class NukeParameter
    {
        public NukeParameter(string name, string description, string defaultValue)
        {
            Name = name;
            Description = description;
            DefaultValue = defaultValue;
        }

        public string Name { get; }
        public string Description { get; }
        public string DefaultValue { get; }
    }

    public class NukeTarget
    {
        public NukeTarget(
            string name,
            string description,
            string[] dependsOn,
            string[] dependentFor,
            string[] before,
            string[] after,
            string[] triggers,
            string[] triggeredBy)
        {
            Name = name;
            Description = description;
            DependsOn = dependsOn;
            DependentFor = dependentFor;
            Before = before;
            After = after;
            Triggers = triggers;
            TriggeredBy = triggeredBy;
        }

        public string Name { get; }
        public string Description { get; }
        public string[] DependsOn { get; }
        public string[] DependentFor { get; }
        public string[] Before { get; }
        public string[] After { get; }
        public string[] Triggers { get; }
        public string[] TriggeredBy { get; }
    }
    #endif
}