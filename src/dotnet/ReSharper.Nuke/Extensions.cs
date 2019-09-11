// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/resharper/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Utils;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util.Dotnet.TargetFrameworkIds;
using JetBrains.Util.Reflection;
using Nuke.Common;

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
        
        public static bool IsNukeTargetProperty(this ITypeMember member)
        {
            var targetDeclaredType = TypeFactory.CreateTypeByCLRName(s_targetType, member.Module);
            return member is IProperty property && property.Type.Equals(targetDeclaredType);
        }

        public static bool IsNukeParameterMember(this ITypeMember member)
        {
            var parameterAttributeDeclaredType = TypeFactory.CreateTypeByCLRName(s_parameterAttributeType, member.Module);
            return member.HasAttributeInstance(parameterAttributeDeclaredType.GetClrName(), inherit: true);
        }
        
        [CanBeNull]
        public static string GetNukeParameterData(this ITypeMember member)
        {
            var parameterAttributeDeclaredType = TypeFactory.CreateTypeByCLRName(s_parameterAttributeType, member.Module);
            var attribute = member.GetAttributeInstances(parameterAttributeDeclaredType.GetClrName(), inherit: true).SingleOrDefault();
            if (attribute == null)
                return null;
            
            var (_, attributeNameValue) = attribute.NamedParameters().SingleOrDefault(x => x.First == nameof(ParameterAttribute.Name));
            var parameterName = attributeNameValue != null && attributeNameValue.IsConstant
                ? attributeNameValue.ConstantValue.Value?.ToString() ?? member.ShortName
                : member.ShortName;
            
            return parameterName;
        }
    }
}