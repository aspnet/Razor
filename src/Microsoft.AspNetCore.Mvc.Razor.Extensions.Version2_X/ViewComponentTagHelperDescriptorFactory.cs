﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions.Version2_X
{
    internal class ViewComponentTagHelperDescriptorFactory
    {
        private readonly INamedTypeSymbol _viewComponentAttributeSymbol;
        private readonly INamedTypeSymbol _genericTaskSymbol;
        private readonly INamedTypeSymbol _taskSymbol;
        private readonly INamedTypeSymbol _iDictionarySymbol;

        private static readonly SymbolDisplayFormat FullNameTypeDisplayFormat =
            SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
                .WithMiscellaneousOptions(SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions & (~SymbolDisplayMiscellaneousOptions.UseSpecialTypes));

        private static readonly IReadOnlyDictionary<string, string> PrimitiveDisplayTypeNameLookups = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [typeof(byte).FullName] = "byte",
            [typeof(sbyte).FullName] = "sbyte",
            [typeof(int).FullName] = "int",
            [typeof(uint).FullName] = "uint",
            [typeof(short).FullName] = "short",
            [typeof(ushort).FullName] = "ushort",
            [typeof(long).FullName] = "long",
            [typeof(ulong).FullName] = "ulong",
            [typeof(float).FullName] = "float",
            [typeof(double).FullName] = "double",
            [typeof(char).FullName] = "char",
            [typeof(bool).FullName] = "bool",
            [typeof(object).FullName] = "object",
            [typeof(string).FullName] = "string",
            [typeof(decimal).FullName] = "decimal",
        };

        public ViewComponentTagHelperDescriptorFactory(Compilation compilation)
        {
            _viewComponentAttributeSymbol = compilation.GetTypeByMetadataName(ViewComponentTypes.ViewComponentAttribute);
            _genericTaskSymbol = compilation.GetTypeByMetadataName(ViewComponentTypes.GenericTask);
            _taskSymbol = compilation.GetTypeByMetadataName(ViewComponentTypes.Task);
            _iDictionarySymbol = compilation.GetTypeByMetadataName(ViewComponentTypes.IDictionary);
        }

        public virtual TagHelperDescriptor CreateDescriptor(INamedTypeSymbol type)
        {
            var assemblyName = type.ContainingAssembly.Name;
            var shortName = GetShortName(type);
            var tagName = $"vc:{HtmlConventions.ToHtmlCase(shortName)}";
            var typeName = $"__Generated__{shortName}ViewComponentTagHelper";
            var displayName = shortName + "ViewComponentTagHelper";
            var descriptorBuilder = TagHelperDescriptorBuilder.Create(ViewComponentTagHelperConventions.Kind, typeName, assemblyName);
            descriptorBuilder.SetTypeName(typeName);
            descriptorBuilder.DisplayName = displayName;

            if (TryFindInvokeMethod(type, out var method, out var diagnostic))
            {
                var methodParameters = method.Parameters;
                descriptorBuilder.TagMatchingRule(ruleBuilder =>
                {
                    ruleBuilder.TagName = tagName;
                    AddRequiredAttributes(methodParameters, ruleBuilder);
                });

                AddBoundAttributes(methodParameters, displayName, descriptorBuilder);
            }
            else
            {
                descriptorBuilder.Diagnostics.Add(diagnostic);
            }

            descriptorBuilder.Metadata[ViewComponentTagHelperMetadata.Name] = shortName;

            var descriptor = descriptorBuilder.Build();
            return descriptor;
        }

        private bool TryFindInvokeMethod(INamedTypeSymbol type, out IMethodSymbol method, out RazorDiagnostic diagnostic)
        {
            var methods = GetInvokeMethods(type);

            if (methods.Count == 0)
            {
                diagnostic = RazorExtensionsDiagnosticFactory.CreateViewComponent_CannotFindMethod(type.ToDisplayString(FullNameTypeDisplayFormat));
                method = null;
                return false;
            }
            else if (methods.Count > 1)
            {
                diagnostic = RazorExtensionsDiagnosticFactory.CreateViewComponent_AmbiguousMethods(type.ToDisplayString(FullNameTypeDisplayFormat));
                method = null;
                return false;
            }

            var selectedMethod = methods[0];
            var returnType = selectedMethod.ReturnType as INamedTypeSymbol;
            if (string.Equals(selectedMethod.Name, ViewComponentTypes.AsyncMethodName, StringComparison.Ordinal))
            {
                // Will invoke asynchronously. Method must not return Task or Task<T>.
                if (returnType == _taskSymbol)
                {
                    // This is ok.
                }
                else if (returnType.IsGenericType && returnType.ConstructedFrom == _genericTaskSymbol)
                {
                    // This is ok.
                }
                else
                {
                    diagnostic = RazorExtensionsDiagnosticFactory.CreateViewComponent_AsyncMethod_ShouldReturnTask(type.ToDisplayString(FullNameTypeDisplayFormat));
                    method = null;
                    return false;
                }
            }
            else
            {
                // Will invoke synchronously. Method must not return void, Task or Task<T>.
                if (returnType.SpecialType == SpecialType.System_Void)
                {
                    diagnostic = RazorExtensionsDiagnosticFactory.CreateViewComponent_SyncMethod_ShouldReturnValue(type.ToDisplayString(FullNameTypeDisplayFormat));
                    method = null;
                    return false;
                }
                else if (returnType == _taskSymbol)
                {
                    diagnostic = RazorExtensionsDiagnosticFactory.CreateViewComponent_SyncMethod_CannotReturnTask(type.ToDisplayString(FullNameTypeDisplayFormat));
                    method = null;
                    return false;
                }
                else if (returnType.IsGenericType && returnType.ConstructedFrom == _genericTaskSymbol)
                {
                    diagnostic = RazorExtensionsDiagnosticFactory.CreateViewComponent_SyncMethod_CannotReturnTask(type.ToDisplayString(FullNameTypeDisplayFormat));
                    method = null;
                    return false;
                }
            }

            method = selectedMethod;
            diagnostic = null;
            return true;
        }

        private static IReadOnlyList<IMethodSymbol> GetInvokeMethods(INamedTypeSymbol type)
        {
            var methods = new List<IMethodSymbol>();
            while (type != null)
            {
                var currentTypeMethods = type.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(m =>
                        m.DeclaredAccessibility == Accessibility.Public &&
                        !m.IsStatic &&
                        (string.Equals(m.Name, ViewComponentTypes.AsyncMethodName, StringComparison.Ordinal) ||
                        string.Equals(m.Name, ViewComponentTypes.SyncMethodName, StringComparison.Ordinal)));

                methods.AddRange(currentTypeMethods);

                type = type.BaseType;
            }

            return methods;
        }

        private void AddRequiredAttributes(ImmutableArray<IParameterSymbol> methodParameters, TagMatchingRuleDescriptorBuilder builder)
        {
            foreach (var parameter in methodParameters)
            {
                if (GetIndexerValueTypeName(parameter) == null)
                {
                    // Set required attributes only for non-indexer attributes. Indexer attributes can't be required attributes
                    // because there are two ways of setting values for the attribute.
                    builder.Attribute(attributeBuilder =>
                    {
                        var lowerKebabName = HtmlConventions.ToHtmlCase(parameter.Name);
                        attributeBuilder.Name = lowerKebabName;
                    });
                }
            }
        }

        private void AddBoundAttributes(ImmutableArray<IParameterSymbol> methodParameters, string containingDisplayName, TagHelperDescriptorBuilder builder)
        {
            foreach (var parameter in methodParameters)
            {
                var lowerKebabName = HtmlConventions.ToHtmlCase(parameter.Name);
                var typeName = parameter.Type.ToDisplayString(FullNameTypeDisplayFormat);

                if (!PrimitiveDisplayTypeNameLookups.TryGetValue(typeName, out var simpleName))
                {
                    simpleName = typeName;
                }

                builder.BindAttribute(attributeBuilder =>
                {
                    attributeBuilder.Name = lowerKebabName;
                    attributeBuilder.TypeName = typeName;
                    attributeBuilder.DisplayName = $"{simpleName} {containingDisplayName}.{parameter.Name}";
                    attributeBuilder.SetPropertyName(parameter.Name);

                    if (parameter.Type.TypeKind == TypeKind.Enum)
                    {
                        attributeBuilder.IsEnum = true;
                    }
                    else
                    {
                        var dictionaryValueType = GetIndexerValueTypeName(parameter);
                        if (dictionaryValueType != null)
                        {
                            attributeBuilder.AsDictionary(lowerKebabName + "-", dictionaryValueType);
                        }
                    }
                });
            }
        }

        private string GetIndexerValueTypeName(IParameterSymbol parameter)
        {
            INamedTypeSymbol dictionaryType;
            if ((parameter.Type as INamedTypeSymbol)?.ConstructedFrom == _iDictionarySymbol)
            {
                dictionaryType = (INamedTypeSymbol)parameter.Type;
            }
            else if (parameter.Type.AllInterfaces.Any(s => s.ConstructedFrom == _iDictionarySymbol))
            {
                dictionaryType = parameter.Type.AllInterfaces.First(s => s.ConstructedFrom == _iDictionarySymbol);
            }
            else
            {
                dictionaryType = null;
            }

            if (dictionaryType == null || dictionaryType.TypeArguments[0].SpecialType != SpecialType.System_String)
            {
                return null;
            }

            var type = dictionaryType.TypeArguments[1];
            var typeName = type.ToDisplayString(FullNameTypeDisplayFormat);

            return typeName;
        }

        private string GetShortName(INamedTypeSymbol componentType)
        {
            var viewComponentAttribute = componentType.GetAttributes().Where(a => a.AttributeClass == _viewComponentAttributeSymbol).FirstOrDefault();
            var name = viewComponentAttribute
                ?.NamedArguments
                .Where(namedArgument => string.Equals(namedArgument.Key, ViewComponentTypes.ViewComponent.Name, StringComparison.Ordinal))
                .FirstOrDefault()
                .Value
                .Value as string;

            if (!string.IsNullOrEmpty(name))
            {
                var separatorIndex = name.LastIndexOf('.');
                if (separatorIndex >= 0)
                {
                    return name.Substring(separatorIndex + 1);
                }
                else
                {
                    return name;
                }
            }

            // Get name by convention
            if (componentType.Name.EndsWith(ViewComponentTypes.ViewComponentSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return componentType.Name.Substring(0, componentType.Name.Length - ViewComponentTypes.ViewComponentSuffix.Length);
            }
            else
            {
                return componentType.Name;
            }
        }
    }
}
