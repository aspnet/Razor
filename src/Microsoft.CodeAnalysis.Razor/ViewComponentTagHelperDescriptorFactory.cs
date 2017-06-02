﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;

namespace Microsoft.CodeAnalysis.Razor
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

        public ViewComponentTagHelperDescriptorFactory(Compilation compilation)
        {
            _viewComponentAttributeSymbol = compilation.GetTypeByMetadataName(ViewComponentTypes.ViewComponentAttribute);
            _genericTaskSymbol = compilation.GetTypeByMetadataName(ViewComponentTypes.GenericTask);
            _taskSymbol = compilation.GetTypeByMetadataName(ViewComponentTypes.Task);
            _iDictionarySymbol = compilation.GetTypeByMetadataName(TagHelperTypes.IDictionary);
        }

        public virtual TagHelperDescriptor CreateDescriptor(INamedTypeSymbol type)
        {
            var assemblyName = type.ContainingAssembly.Name;
            var shortName = GetShortName(type);
            var tagName = $"vc:{DefaultTagHelperDescriptorFactory.ToHtmlCase(shortName)}";
            var typeName = $"__Generated__{shortName}ViewComponentTagHelper";
            var displayName = shortName + "ViewComponentTagHelper";
            var methodParameters = GetInvokeMethodParameters(type);
            var descriptorBuilder = TagHelperDescriptorBuilder.Create(typeName, assemblyName)
                .DisplayName(displayName)
                .TagMatchingRule(ruleBuilder =>
            {
                ruleBuilder.RequireTagName(tagName);
                AddRequiredAttributes(methodParameters, ruleBuilder);
            });

            AddBoundAttributes(methodParameters, descriptorBuilder);

            descriptorBuilder.AddMetadata(ViewComponentTypes.ViewComponentNameKey, shortName);

            var descriptor = descriptorBuilder.Build();
            return descriptor;
        }

        private void AddRequiredAttributes(ImmutableArray<IParameterSymbol> methodParameters, TagMatchingRuleBuilder builder)
        {
            foreach (var parameter in methodParameters)
            {
                if (GetIndexerValueTypeName(parameter) == null)
                {
                    // Set required attributes only for non-indexer attributes. Indexer attributes can't be required attributes
                    // because there are two ways of setting values for the attribute.
                    builder.RequireAttribute(attributeBuilder =>
                    {
                        var lowerKebabName = DefaultTagHelperDescriptorFactory.ToHtmlCase(parameter.Name);
                        attributeBuilder.Name(lowerKebabName);
                    });
                }
            }
        }

        private void AddBoundAttributes(ImmutableArray<IParameterSymbol> methodParameters, TagHelperDescriptorBuilder builder)
        {
            foreach (var parameter in methodParameters)
            {
                var lowerKebabName = DefaultTagHelperDescriptorFactory.ToHtmlCase(parameter.Name);
                var typeName = parameter.Type.ToDisplayString(FullNameTypeDisplayFormat);
                builder.BindAttribute(attributeBuilder =>
                {
                    attributeBuilder
                        .Name(lowerKebabName)
                        .PropertyName(parameter.Name)
                        .TypeName(typeName);

                    if (parameter.Type.TypeKind == TypeKind.Enum)
                    {
                        attributeBuilder.AsEnum();
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

        private ImmutableArray<IParameterSymbol> GetInvokeMethodParameters(INamedTypeSymbol componentType)
        {
            // Note: we have some code here that formerly attempted to validate the signature of the
            // VC methods and would throw. We definitely don't want to throw since that will block
            // discovery of ALL tag helpers.
            //
            // Now what happens here is that we just return an empty parameter list.
            var methods = componentType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(method =>
                    method.DeclaredAccessibility == Accessibility.Public &&
                    (string.Equals(method.Name, ViewComponentTypes.AsyncMethodName, StringComparison.Ordinal) ||
                    string.Equals(method.Name, ViewComponentTypes.SyncMethodName, StringComparison.Ordinal)))
                .ToArray();

            if (methods.Length == 0)
            {
                // Invalid, no Invoke methods.
                return ImmutableArray<IParameterSymbol>.Empty;
            }
            else if (methods.Length > 1)
            {
                // Invalid, too manyInvoke methods.
                return ImmutableArray<IParameterSymbol>.Empty;
            }

            var selectedMethod = methods[0];
            var returnType = selectedMethod.ReturnType as INamedTypeSymbol;
            if (string.Equals(selectedMethod.Name, ViewComponentTypes.AsyncMethodName, StringComparison.Ordinal) && returnType != null)
            {
                if (returnType == _taskSymbol)
                {
                    // Task - This is fine.
                }
                else if (returnType.IsGenericType && returnType.ConstructedFrom == _genericTaskSymbol)
                {
                    // Task<T> - This is fine.
                }
                else
                {
                    // Invalid, needs to be Task or Task<T>
                    return ImmutableArray<IParameterSymbol>.Empty;
                }
            }
            else if (returnType != null)
            {
                // Will invoke synchronously. Method must not return void, Task or Task<T>.
                if (returnType.SpecialType == SpecialType.System_Void)
                {
                    // Invalid, cannot be void.
                    return ImmutableArray<IParameterSymbol>.Empty;
                }
                else if (returnType == _taskSymbol)
                {
                    // Invalid, cannot be Task.
                    return ImmutableArray<IParameterSymbol>.Empty;
                }
                else if (returnType.IsGenericType && returnType.ConstructedFrom == _genericTaskSymbol)
                {
                    // Invalid, cannot be Task<T>.
                    return ImmutableArray<IParameterSymbol>.Empty;
                }
            }

            var methodParameters = selectedMethod.Parameters;

            return methodParameters;
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
