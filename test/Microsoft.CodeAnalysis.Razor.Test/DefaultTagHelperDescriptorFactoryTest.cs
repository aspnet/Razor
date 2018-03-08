// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor.Workspaces.Test;
using Xunit;

namespace Microsoft.CodeAnalysis.Razor.Workspaces
{
    public class DefaultTagHelperDescriptorFactoryTest
    {
        private static readonly Assembly _assembly = typeof(DefaultTagHelperDescriptorFactoryTest).GetTypeInfo().Assembly;

        protected static readonly AssemblyName TagHelperDescriptorFactoryTestAssembly = _assembly.GetName();

        protected static readonly string AssemblyName = TagHelperDescriptorFactoryTestAssembly.Name;

        private static Compilation Compilation { get; } = TestCompilation.Create(_assembly);

        public static TheoryData RequiredAttributeParserErrorData
        {
            get
            {
                return new TheoryData<string, Action<RequiredAttributeDescriptorBuilder>[]>
                {
                    {
                        "name,",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("name")
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_CouldNotFindMatchingEndBrace("name,")),
                        }
                    },
                    {
                        " ",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name(string.Empty)
                                .AddDiagnostic(AspNetCore.Razor.Language.RazorDiagnosticFactory.CreateTagHelper_InvalidTargetedAttributeNameNullOrWhitespace()),
                        }
                    },
                    {
                        "n@me",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("n@me")
                                .AddDiagnostic(AspNetCore.Razor.Language.RazorDiagnosticFactory.CreateTagHelper_InvalidTargetedAttributeName("n@me", '@')),
                        }
                    },
                    {
                        "name extra",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("name")
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_InvalidRequiredAttributeCharacter('e', "name extra")),
                        }
                    },
                    {
                        "[[ ",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("[")
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_CouldNotFindMatchingEndBrace("[[ ")),
                        }
                    },
                    {
                        "[ ",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("")
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_CouldNotFindMatchingEndBrace("[ ")),
                        }
                    },
                    {
                        "[name='unended]",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("name")
                                .ValueComparisonMode(RequiredAttributeDescriptor.ValueComparisonMode.FullMatch)
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_InvalidRequiredAttributeMismatchedQuotes('\'', "[name='unended]")),
                        }
                    },
                    {
                        "[name='unended",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("name")
                                .ValueComparisonMode(RequiredAttributeDescriptor.ValueComparisonMode.FullMatch)
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_InvalidRequiredAttributeMismatchedQuotes('\'', "[name='unended")),
                        }
                    },
                    {
                        "[name",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("name")
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_CouldNotFindMatchingEndBrace("[name")),
                        }
                    },
                    {
                        "[ ]",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name(string.Empty)
                                .AddDiagnostic(AspNetCore.Razor.Language.RazorDiagnosticFactory.CreateTagHelper_InvalidTargetedAttributeNameNullOrWhitespace()),
                        }
                    },
                    {
                        "[n@me]",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("n@me")
                                .AddDiagnostic(AspNetCore.Razor.Language.RazorDiagnosticFactory.CreateTagHelper_InvalidTargetedAttributeName("n@me", '@')),
                        }
                    },
                    {
                        "[name@]",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("name@")
                                .AddDiagnostic(AspNetCore.Razor.Language.RazorDiagnosticFactory.CreateTagHelper_InvalidTargetedAttributeName("name@", '@')),
                        }
                    },
                    {
                        "[name^]",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("name")
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_PartialRequiredAttributeOperator('^', "[name^]")),
                        }
                    },
                    {
                        "[name='value'",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("name")
                                .Value("value")
                                .ValueComparisonMode(RequiredAttributeDescriptor.ValueComparisonMode.FullMatch)
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_CouldNotFindMatchingEndBrace("[name='value'")),
                        }
                    },
                    {
                        "[name ",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("name")
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_CouldNotFindMatchingEndBrace("[name ")),
                        }
                    },
                    {
                        "[name extra]",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("name")
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_InvalidRequiredAttributeOperator('e', "[name extra]")),
                        }
                    },
                    {
                        "[name=value ",
                        new Action<RequiredAttributeDescriptorBuilder>[]
                        {
                            builder => builder
                                .Name("name")
                                .Value("value")
                                .ValueComparisonMode(RequiredAttributeDescriptor.ValueComparisonMode.FullMatch)
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_CouldNotFindMatchingEndBrace("[name=value ")),
                        }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(RequiredAttributeParserErrorData))]
        public void RequiredAttributeParser_ParsesRequiredAttributesAndLogsDiagnosticsCorrectly(
            string requiredAttributes,
            IEnumerable<Action<RequiredAttributeDescriptorBuilder>> configureBuilders)
        {
            // Arrange
            var ruleBuilder = new DefaultTagMatchingRuleDescriptorBuilder();

            var expectedRules = new List<RequiredAttributeDescriptor>();
            foreach (var configureBuilder in configureBuilders)
            {
                var builder = new DefaultRequiredAttributeDescriptorBuilder();
                configureBuilder(builder);

                expectedRules.Add(builder.Build());
            }

            // Act
            RequiredAttributeParser.AddRequiredAttributes(requiredAttributes, ruleBuilder);

            // Assert
            var descriptors = ruleBuilder.Build().Attributes;
            Assert.Equal(expectedRules, descriptors, RequiredAttributeDescriptorComparer.CaseSensitive);
        }

        public static TheoryData RequiredAttributeParserData
        {
            get
            {
                Func<string, RequiredAttributeDescriptor.NameComparisonMode, Action<RequiredAttributeDescriptorBuilder>> plain =
                    (name, nameComparison) => (builder) => builder
                        .Name(name)
                        .NameComparisonMode(nameComparison);

                Func<string, string, RequiredAttributeDescriptor.ValueComparisonMode, Action<RequiredAttributeDescriptorBuilder>> css =
                    (name, value, valueComparison) => (builder) => builder
                        .Name(name)
                        .Value(value)
                        .ValueComparisonMode(valueComparison);

                return new TheoryData<string, IEnumerable<Action<RequiredAttributeDescriptorBuilder>>>
                {
                    { null, Enumerable.Empty<Action<RequiredAttributeDescriptorBuilder>>() },
                    { string.Empty, Enumerable.Empty<Action<RequiredAttributeDescriptorBuilder>>() },
                    { "name", new[] { plain("name", RequiredAttributeDescriptor.NameComparisonMode.FullMatch) } },
                    { "name-*", new[] { plain("name-", RequiredAttributeDescriptor.NameComparisonMode.PrefixMatch) } },
                    { "  name-*   ", new[] { plain("name-", RequiredAttributeDescriptor.NameComparisonMode.PrefixMatch) } },
                    {
                        "asp-route-*,valid  ,  name-*   ,extra",
                        new[]
                        {
                            plain("asp-route-", RequiredAttributeDescriptor.NameComparisonMode.PrefixMatch),
                            plain("valid", RequiredAttributeDescriptor.NameComparisonMode.FullMatch),
                            plain("name-", RequiredAttributeDescriptor.NameComparisonMode.PrefixMatch),
                            plain("extra", RequiredAttributeDescriptor.NameComparisonMode.FullMatch),
                        }
                    },
                    { "[name]", new[] { css("name", null, RequiredAttributeDescriptor.ValueComparisonMode.None) } },
                    { "[ name ]", new[] { css("name", null, RequiredAttributeDescriptor.ValueComparisonMode.None) } },
                    { " [ name ] ", new[] { css("name", null, RequiredAttributeDescriptor.ValueComparisonMode.None) } },
                    { "[name=]", new[] { css("name", "", RequiredAttributeDescriptor.ValueComparisonMode.FullMatch) } },
                    { "[name='']", new[] { css("name", "", RequiredAttributeDescriptor.ValueComparisonMode.FullMatch) } },
                    { "[name ^=]", new[] { css("name", "", RequiredAttributeDescriptor.ValueComparisonMode.PrefixMatch) } },
                    { "[name=hello]", new[] { css("name", "hello", RequiredAttributeDescriptor.ValueComparisonMode.FullMatch) } },
                    { "[name= hello]", new[] { css("name", "hello", RequiredAttributeDescriptor.ValueComparisonMode.FullMatch) } },
                    { "[name='hello']", new[] { css("name", "hello", RequiredAttributeDescriptor.ValueComparisonMode.FullMatch) } },
                    { "[name=\"hello\"]", new[] { css("name", "hello", RequiredAttributeDescriptor.ValueComparisonMode.FullMatch) } },
                    { " [ name  $= \" hello\" ]  ", new[] { css("name", " hello", RequiredAttributeDescriptor.ValueComparisonMode.SuffixMatch) } },
                    {
                        "[name=\"hello\"],[other^=something ], [val = 'cool']",
                        new[]
                        {
                            css("name", "hello", RequiredAttributeDescriptor.ValueComparisonMode.FullMatch),
                            css("other", "something", RequiredAttributeDescriptor.ValueComparisonMode.PrefixMatch),
                            css("val", "cool", RequiredAttributeDescriptor.ValueComparisonMode.FullMatch) }
                    },
                    {
                        "asp-route-*,[name=\"hello\"],valid  ,[other^=something ],   name-*   ,[val = 'cool'],extra",
                        new[]
                        {
                            plain("asp-route-", RequiredAttributeDescriptor.NameComparisonMode.PrefixMatch),
                            css("name", "hello", RequiredAttributeDescriptor.ValueComparisonMode.FullMatch),
                            plain("valid", RequiredAttributeDescriptor.NameComparisonMode.FullMatch),
                            css("other", "something", RequiredAttributeDescriptor.ValueComparisonMode.PrefixMatch),
                            plain("name-", RequiredAttributeDescriptor.NameComparisonMode.PrefixMatch),
                            css("val", "cool", RequiredAttributeDescriptor.ValueComparisonMode.FullMatch),
                            plain("extra", RequiredAttributeDescriptor.NameComparisonMode.FullMatch),
                        }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(RequiredAttributeParserData))]
        public void RequiredAttributeParser_ParsesRequiredAttributesCorrectly(
            string requiredAttributes,
            IEnumerable<Action<RequiredAttributeDescriptorBuilder>> configureBuilders)
        {
            // Arrange
            var ruleBuilder = new DefaultTagMatchingRuleDescriptorBuilder();

            var expectedRules = new List<RequiredAttributeDescriptor>();
            foreach (var configureBuilder in configureBuilders)
            {
                var builder = new DefaultRequiredAttributeDescriptorBuilder();
                configureBuilder(builder);

                expectedRules.Add(builder.Build());
            }

            // Act
            RequiredAttributeParser.AddRequiredAttributes(requiredAttributes, ruleBuilder);

            // Assert
            var descriptors = ruleBuilder.Build().Attributes;
            Assert.Equal(expectedRules, descriptors, RequiredAttributeDescriptorComparer.CaseSensitive);
        }

        public static TheoryData IsEnumData
        {
            get
            {
                // tagHelperType, expectedDescriptor
                return new TheoryData<Type, TagHelperDescriptor>
                {
                    {
                        typeof(EnumTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(EnumTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(EnumTagHelper).FullName)
                            .TagMatchingRuleDescriptor(ruleBuilder => ruleBuilder.RequireTagName("enum"))
                            .BoundAttributeDescriptor(builder =>
                                builder
                                    .Name("non-enum-property")
                                    .PropertyName(nameof(EnumTagHelper.NonEnumProperty))
                                    .TypeName(typeof(int).FullName))
                            .BoundAttributeDescriptor(builder =>
                                builder
                                    .Name("enum-property")
                                    .PropertyName(nameof(EnumTagHelper.EnumProperty))
                                    .TypeName(typeof(CustomEnum).FullName)
                                    .AsEnum())
                            .Build()
                    },
                    {
                        typeof(MultiEnumTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(MultiEnumTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(MultiEnumTagHelper).FullName)
                            .TagMatchingRuleDescriptor(ruleBuilder => ruleBuilder.RequireTagName("p"))
                            .TagMatchingRuleDescriptor(ruleBuilder => ruleBuilder.RequireTagName("input"))
                            .BoundAttributeDescriptor(builder =>
                                builder
                                    .Name("non-enum-property")
                                    .PropertyName(nameof(MultiEnumTagHelper.NonEnumProperty))
                                    .TypeName(typeof(int).FullName))
                            .BoundAttributeDescriptor(builder =>
                                builder
                                    .Name("enum-property")
                                    .PropertyName(nameof(MultiEnumTagHelper.EnumProperty))
                                    .TypeName(typeof(CustomEnum).FullName)
                                    .AsEnum())
                            .Build()
                    },
                    {
                        typeof(NestedEnumTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(NestedEnumTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(NestedEnumTagHelper).FullName)
                            .TagMatchingRuleDescriptor(ruleBuilder => ruleBuilder.RequireTagName("nested-enum"))
                            .BoundAttributeDescriptor(builder =>
                                builder
                                    .Name("non-enum-property")
                                    .PropertyName(nameof(NestedEnumTagHelper.NonEnumProperty))
                                    .TypeName(typeof(int).FullName))
                            .BoundAttributeDescriptor(builder =>
                                builder
                                    .Name("enum-property")
                                    .PropertyName(nameof(NestedEnumTagHelper.EnumProperty))
                                    .TypeName(typeof(CustomEnum).FullName)
                                    .AsEnum())
                            .BoundAttributeDescriptor(builder =>
                                builder
                                    .Name("nested-enum-property")
                                    .PropertyName(nameof(NestedEnumTagHelper.NestedEnumProperty))
                                    .TypeName($"{typeof(NestedEnumTagHelper).FullName}.{nameof(NestedEnumTagHelper.NestedEnum)}")
                                    .AsEnum())
                            .Build()
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsEnumData))]
        public void CreateDescriptor_IsEnumIsSetCorrectly(
            Type tagHelperType,
            TagHelperDescriptor expectedDescriptor)
        {
            // Arrange
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(tagHelperType.FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        public static TheoryData RequiredParentData
        {
            get
            {
                // tagHelperType, expectedDescriptor
                return new TheoryData<Type, TagHelperDescriptor>
                {
                    {
                        typeof(RequiredParentTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(RequiredParentTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(RequiredParentTagHelper).FullName)
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("input").RequireParentTag("div"))
                            .Build()
                    },
                    {
                        typeof(MultiSpecifiedRequiredParentTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(MultiSpecifiedRequiredParentTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(MultiSpecifiedRequiredParentTagHelper).FullName)
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("p").RequireParentTag("div"))
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("input").RequireParentTag("section"))
                            .Build()
                    },
                    {
                        typeof(MultiWithUnspecifiedRequiredParentTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(MultiWithUnspecifiedRequiredParentTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(MultiWithUnspecifiedRequiredParentTagHelper).FullName)
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("p"))
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("input").RequireParentTag("div"))
                            .Build()
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(RequiredParentData))]
        public void CreateDescriptor_CreatesDesignTimeDescriptorsWithRequiredParent(
            Type tagHelperType,
            TagHelperDescriptor expectedDescriptor)
        {
            // Arrange
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(tagHelperType.FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        public static TheoryData RestrictChildrenData
        {
            get
            {
                // tagHelperType, expectedDescriptor
                return new TheoryData<Type, TagHelperDescriptor>
                {
                    {
                        typeof(RestrictChildrenTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(RestrictChildrenTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(RestrictChildrenTagHelper).FullName)
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("restrict-children"))
                            .AllowChildTag("p")
                            .Build()
                    },
                    {
                        typeof(DoubleRestrictChildrenTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(DoubleRestrictChildrenTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(DoubleRestrictChildrenTagHelper).FullName)
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("double-restrict-children"))
                            .AllowChildTag("p")
                            .AllowChildTag("strong")
                            .Build()
                    },
                    {
                        typeof(MultiTargetRestrictChildrenTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(MultiTargetRestrictChildrenTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(MultiTargetRestrictChildrenTagHelper).FullName)
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("p"))
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("div"))
                            .AllowChildTag("p")
                            .AllowChildTag("strong")
                            .Build()
                    },
                };
            }
        }


        [Theory]
        [MemberData(nameof(RestrictChildrenData))]
        public void CreateDescriptor_CreatesDescriptorsWithAllowedChildren(
            Type tagHelperType,
            TagHelperDescriptor expectedDescriptor)
        {
            // Arrange
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(tagHelperType.FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        public static TheoryData TagStructureData
        {
            get
            {
                // tagHelperType, expectedDescriptor
                return new TheoryData<Type, TagHelperDescriptor>
                {
                    {
                        typeof(TagStructureTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(TagStructureTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(TagStructureTagHelper).FullName)
                            .TagMatchingRuleDescriptor(builder => builder
                                .RequireTagName("input")
                                .RequireTagStructure(TagStructure.WithoutEndTag))
                            .Build()
                    },
                    {
                        typeof(MultiSpecifiedTagStructureTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(MultiSpecifiedTagStructureTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(MultiSpecifiedTagStructureTagHelper).FullName)
                            .TagMatchingRuleDescriptor(builder => builder
                                .RequireTagName("p")
                                .RequireTagStructure(TagStructure.NormalOrSelfClosing))
                            .TagMatchingRuleDescriptor(builder => builder
                                .RequireTagName("input")
                                .RequireTagStructure(TagStructure.WithoutEndTag))
                            .Build()
                    },
                    {
                        typeof(MultiWithUnspecifiedTagStructureTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(MultiWithUnspecifiedTagStructureTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(MultiWithUnspecifiedTagStructureTagHelper).FullName)
                            .TagMatchingRuleDescriptor(builder => builder
                                .RequireTagName("p"))
                            .TagMatchingRuleDescriptor(builder => builder
                                .RequireTagName("input")
                                .RequireTagStructure(TagStructure.WithoutEndTag))
                            .Build()
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TagStructureData))]
        public void CreateDescriptor_CreatesDesignTimeDescriptorsWithTagStructure(
            Type tagHelperType,
            TagHelperDescriptor expectedDescriptor)
        {
            // Arrange
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(tagHelperType.FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        public static TheoryData EditorBrowsableData
        {
            get
            {
                // tagHelperType, designTime, expectedDescriptor
                return new TheoryData<Type, bool, TagHelperDescriptor>
                {
                    {
                        typeof(InheritedEditorBrowsableTagHelper),
                        true,
                        CreateTagHelperDescriptor(
                            tagName: "inherited-editor-browsable",
                            typeName: typeof(InheritedEditorBrowsableTagHelper).FullName,
                            assemblyName: AssemblyName,
                            attributes: new Action<BoundAttributeDescriptorBuilder>[]
                            {
                                builder => builder
                                    .Name("property")
                                    .PropertyName(nameof(InheritedEditorBrowsableTagHelper.Property))
                                    .TypeName(typeof(int).FullName),
                            })
                    },
                    { typeof(EditorBrowsableTagHelper), true, null },
                    {
                        typeof(EditorBrowsableTagHelper),
                        false,
                        CreateTagHelperDescriptor(
                            tagName: "editor-browsable",
                            typeName: typeof(EditorBrowsableTagHelper).FullName,
                            assemblyName: AssemblyName,
                            attributes: new Action<BoundAttributeDescriptorBuilder>[]
                            {
                                builder => builder
                                    .Name("property")
                                    .PropertyName(nameof(EditorBrowsableTagHelper.Property))
                                    .TypeName(typeof(int).FullName),
                            })
                    },
                    {
                        typeof(HiddenPropertyEditorBrowsableTagHelper),
                        true,
                        CreateTagHelperDescriptor(
                            tagName: "hidden-property-editor-browsable",
                            typeName: typeof(HiddenPropertyEditorBrowsableTagHelper).FullName,
                            assemblyName: AssemblyName)
                    },
                    {
                        typeof(HiddenPropertyEditorBrowsableTagHelper),
                        false,
                        CreateTagHelperDescriptor(
                            tagName: "hidden-property-editor-browsable",
                            typeName: typeof(HiddenPropertyEditorBrowsableTagHelper).FullName,
                            assemblyName: AssemblyName,
                            attributes: new Action<BoundAttributeDescriptorBuilder>[]
                            {
                                builder => builder
                                    .Name("property")
                                    .PropertyName(nameof(HiddenPropertyEditorBrowsableTagHelper.Property))
                                    .TypeName(typeof(int).FullName),
                            })
                    },
                    {
                        typeof(OverriddenEditorBrowsableTagHelper),
                        true,
                        CreateTagHelperDescriptor(
                            tagName: "overridden-editor-browsable",
                            typeName: typeof(OverriddenEditorBrowsableTagHelper).FullName,
                            assemblyName: AssemblyName,
                            attributes: new Action<BoundAttributeDescriptorBuilder>[]
                            {
                                builder => builder
                                    .Name("property")
                                    .PropertyName(nameof(OverriddenEditorBrowsableTagHelper.Property))
                                    .TypeName(typeof(int).FullName),
                            })
                    },
                    {
                        typeof(MultiPropertyEditorBrowsableTagHelper),
                        true,
                        CreateTagHelperDescriptor(
                            tagName: "multi-property-editor-browsable",
                            typeName: typeof(MultiPropertyEditorBrowsableTagHelper).FullName,
                            assemblyName: AssemblyName,
                            attributes: new Action<BoundAttributeDescriptorBuilder>[]
                            {
                                builder => builder
                                    .Name("property2")
                                    .PropertyName(nameof(MultiPropertyEditorBrowsableTagHelper.Property2))
                                    .TypeName(typeof(int).FullName),
                            })
                    },
                    {
                        typeof(MultiPropertyEditorBrowsableTagHelper),
                        false,
                        CreateTagHelperDescriptor(
                            tagName: "multi-property-editor-browsable",
                            typeName: typeof(MultiPropertyEditorBrowsableTagHelper).FullName,
                            assemblyName: AssemblyName,
                            attributes: new Action<BoundAttributeDescriptorBuilder>[]
                            {
                                builder => builder
                                    .Name("property")
                                    .PropertyName(nameof(MultiPropertyEditorBrowsableTagHelper.Property))
                                    .TypeName(typeof(int).FullName),
                                builder => builder
                                    .Name("property2")
                                    .PropertyName(nameof(MultiPropertyEditorBrowsableTagHelper.Property2))
                                    .TypeName(typeof(int).FullName),
                            })
                    },
                    {
                        typeof(OverriddenPropertyEditorBrowsableTagHelper),
                        true,
                        CreateTagHelperDescriptor(
                            tagName: "overridden-property-editor-browsable",
                            typeName: typeof(OverriddenPropertyEditorBrowsableTagHelper).FullName,
                            assemblyName: AssemblyName)
                    },
                    {
                        typeof(OverriddenPropertyEditorBrowsableTagHelper),
                        false,
                        CreateTagHelperDescriptor(
                            tagName: "overridden-property-editor-browsable",
                            typeName: typeof(OverriddenPropertyEditorBrowsableTagHelper).FullName,
                            assemblyName: AssemblyName,
                            attributes: new Action<BoundAttributeDescriptorBuilder>[]
                            {
                                builder => builder
                                    .Name("property2")
                                    .PropertyName(nameof(OverriddenPropertyEditorBrowsableTagHelper.Property2))
                                    .TypeName(typeof(int).FullName),
                                builder => builder
                                    .Name("property")
                                    .PropertyName(nameof(OverriddenPropertyEditorBrowsableTagHelper.Property))
                                    .TypeName(typeof(int).FullName),
                            })
                    },
                    {
                        typeof(DefaultEditorBrowsableTagHelper),
                        true,
                        CreateTagHelperDescriptor(
                            tagName: "default-editor-browsable",
                            typeName: typeof(DefaultEditorBrowsableTagHelper).FullName,
                            assemblyName: AssemblyName,
                            attributes: new Action<BoundAttributeDescriptorBuilder>[]
                            {
                                builder => builder
                                    .Name("property")
                                    .PropertyName(nameof(DefaultEditorBrowsableTagHelper.Property))
                                    .TypeName(typeof(int).FullName),
                            })
                    },
                    { typeof(MultiEditorBrowsableTagHelper), true, null }
                };
            }
        }

        [Theory]
        [MemberData(nameof(EditorBrowsableData))]
        public void CreateDescriptor_UnderstandsEditorBrowsableAttribute(
            Type tagHelperType,
            bool designTime,
            TagHelperDescriptor expectedDescriptor)
        {
            // Arrange
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, designTime, designTime);
            var typeSymbol = Compilation.GetTypeByMetadataName(tagHelperType.FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        public static TheoryData AttributeTargetData
        {
            get
            {
                var attributes = Enumerable.Empty<BoundAttributeDescriptor>();

                // tagHelperType, expectedDescriptor
                return new TheoryData<Type, TagHelperDescriptor>
                {
                    {
                        typeof(AttributeTargetingTagHelper),
                        CreateTagHelperDescriptor(
                            TagHelperMatchingConventions.ElementCatchAllName,
                            typeof(AttributeTargetingTagHelper).FullName,
                            AssemblyName,
                            ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                            {
                                builder => builder.RequireAttributeDescriptor(attribute => attribute.Name("class")),
                            })
                    },
                    {
                        typeof(MultiAttributeTargetingTagHelper),
                        CreateTagHelperDescriptor(
                            TagHelperMatchingConventions.ElementCatchAllName,
                            typeof(MultiAttributeTargetingTagHelper).FullName,
                            AssemblyName,
                            ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                            {
                                builder =>
                                {
                                    builder
                                        .RequireAttributeDescriptor(attribute => attribute.Name("class"))
                                        .RequireAttributeDescriptor(attribute => attribute.Name("style"));
                                },
                            })
                    },
                    {
                        typeof(MultiAttributeAttributeTargetingTagHelper),
                        CreateTagHelperDescriptor(
                            TagHelperMatchingConventions.ElementCatchAllName,
                            typeof(MultiAttributeAttributeTargetingTagHelper).FullName,
                            AssemblyName,
                            ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                            {
                                builder => builder.RequireAttributeDescriptor(attribute => attribute.Name("custom")),
                                builder =>
                                {
                                    builder
                                        .RequireAttributeDescriptor(attribute => attribute.Name("class"))
                                        .RequireAttributeDescriptor(attribute => attribute.Name("style"));
                                },
                            })
                    },
                    {
                        typeof(InheritedAttributeTargetingTagHelper),
                        CreateTagHelperDescriptor(
                            TagHelperMatchingConventions.ElementCatchAllName,
                            typeof(InheritedAttributeTargetingTagHelper).FullName,
                            AssemblyName,
                            ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                            {
                                builder => builder.RequireAttributeDescriptor(attribute => attribute.Name("style")),
                            })
                    },
                    {
                        typeof(RequiredAttributeTagHelper),
                        CreateTagHelperDescriptor(
                            "input",
                            typeof(RequiredAttributeTagHelper).FullName,
                            AssemblyName,
                            ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                            {
                                builder => builder.RequireAttributeDescriptor(attribute => attribute.Name("class")),
                            })
                    },
                    {
                        typeof(InheritedRequiredAttributeTagHelper),
                        CreateTagHelperDescriptor(
                            "div",
                            typeof(InheritedRequiredAttributeTagHelper).FullName,
                            AssemblyName,
                            ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                            {
                                builder => builder.RequireAttributeDescriptor(attribute => attribute.Name("class")),
                            })
                    },
                    {
                        typeof(MultiAttributeRequiredAttributeTagHelper),
                        CreateTagHelperDescriptor(
                            "div",
                            typeof(MultiAttributeRequiredAttributeTagHelper).FullName,
                            AssemblyName,
                            ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                            {
                                builder => builder
                                    .RequireTagName("div")
                                    .RequireAttributeDescriptor(attribute => attribute.Name("class")),
                                builder => builder
                                    .RequireTagName("input")
                                    .RequireAttributeDescriptor(attribute => attribute.Name("class")),
                            })
                    },
                    {
                        typeof(MultiAttributeSameTagRequiredAttributeTagHelper),
                        CreateTagHelperDescriptor(
                            "input",
                            typeof(MultiAttributeSameTagRequiredAttributeTagHelper).FullName,
                            AssemblyName,
                            ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                            {
                                builder => builder.RequireAttributeDescriptor(attribute => attribute.Name("style")),
                                builder => builder.RequireAttributeDescriptor(attribute => attribute.Name("class")),
                            })
                    },
                    {
                        typeof(MultiRequiredAttributeTagHelper),
                        CreateTagHelperDescriptor(
                            "input",
                            typeof(MultiRequiredAttributeTagHelper).FullName,
                            AssemblyName,
                            ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                            {
                                builder => builder
                                    .RequireAttributeDescriptor(attribute => attribute.Name("class"))
                                    .RequireAttributeDescriptor(attribute => attribute.Name("style")),
                            })
                    },
                    {
                        typeof(MultiTagMultiRequiredAttributeTagHelper),
                        CreateTagHelperDescriptor(
                            "div",
                            typeof(MultiTagMultiRequiredAttributeTagHelper).FullName,
                            AssemblyName,
                            ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                            {
                                builder => builder
                                    .RequireTagName("div")
                                    .RequireAttributeDescriptor(attribute => attribute.Name("class"))
                                    .RequireAttributeDescriptor(attribute => attribute.Name("style")),
                                builder => builder
                                    .RequireTagName("input")
                                    .RequireAttributeDescriptor(attribute => attribute.Name("class"))
                                    .RequireAttributeDescriptor(attribute => attribute.Name("style")),
                            })
                    },
                    {
                        typeof(AttributeWildcardTargetingTagHelper),
                        CreateTagHelperDescriptor(
                            TagHelperMatchingConventions.ElementCatchAllName,
                            typeof(AttributeWildcardTargetingTagHelper).FullName,
                            AssemblyName,
                            ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                            {
                                builder => builder
                                    .RequireAttributeDescriptor(attribute => attribute
                                        .Name("class")
                                        .NameComparisonMode(RequiredAttributeDescriptor.NameComparisonMode.PrefixMatch)),
                            })
                    },
                    {
                        typeof(MultiAttributeWildcardTargetingTagHelper),
                        CreateTagHelperDescriptor(
                            TagHelperMatchingConventions.ElementCatchAllName,
                            typeof(MultiAttributeWildcardTargetingTagHelper).FullName,
                            AssemblyName,
                            ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                            {
                                builder => builder
                                    .RequireAttributeDescriptor(attribute => attribute
                                        .Name("class")
                                        .NameComparisonMode(RequiredAttributeDescriptor.NameComparisonMode.PrefixMatch))
                                    .RequireAttributeDescriptor(attribute => attribute
                                        .Name("style")
                                        .NameComparisonMode(RequiredAttributeDescriptor.NameComparisonMode.PrefixMatch)),
                            })
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AttributeTargetData))]
        public void CreateDescriptor_ReturnsExpectedDescriptors(
            Type tagHelperType,
            TagHelperDescriptor expectedDescriptor)
        {
            // Arrange
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(tagHelperType.FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        public static TheoryData HtmlCaseData
        {
            get
            {
                // tagHelperType, expectedTagName, expectedAttributeName
                return new TheoryData<Type, string, string>
                {
                    { typeof(SingleAttributeTagHelper), "single-attribute", "int-attribute" },
                    { typeof(ALLCAPSTAGHELPER), "allcaps", "allcapsattribute" },
                    { typeof(CAPSOnOUTSIDETagHelper), "caps-on-outside", "caps-on-outsideattribute" },
                    { typeof(capsONInsideTagHelper), "caps-on-inside", "caps-on-insideattribute" },
                    { typeof(One1Two2Three3TagHelper), "one1-two2-three3", "one1-two2-three3-attribute" },
                    { typeof(ONE1TWO2THREE3TagHelper), "one1two2three3", "one1two2three3-attribute" },
                    { typeof(First_Second_ThirdHiTagHelper), "first_second_third-hi", "first_second_third-attribute" },
                    { typeof(UNSuffixedCLASS), "un-suffixed-class", "un-suffixed-attribute" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(HtmlCaseData))]
        public void CreateDescriptor_HtmlCasesTagNameAndAttributeName(
            Type tagHelperType,
            string expectedTagName,
            string expectedAttributeName)
        {
            // Arrange
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(tagHelperType.FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            var rule = Assert.Single(descriptor.TagMatchingRules);
            Assert.Equal(expectedTagName, rule.TagName, StringComparer.Ordinal);
            var attributeDescriptor = Assert.Single(descriptor.BoundAttributes);
            Assert.Equal(expectedAttributeName, attributeDescriptor.Name);
        }

        [Fact]
        public void CreateDescriptor_OverridesAttributeNameFromAttribute()
        {
            // Arrange
            var validProperty1 = typeof(OverriddenAttributeTagHelper).GetProperty(
                nameof(OverriddenAttributeTagHelper.ValidAttribute1));
            var validProperty2 = typeof(OverriddenAttributeTagHelper).GetProperty(
                nameof(OverriddenAttributeTagHelper.ValidAttribute2));
            var expectedDescriptor =
                CreateTagHelperDescriptor(
                    "overridden-attribute",
                    typeof(OverriddenAttributeTagHelper).FullName,
                    AssemblyName,
                    new Action<BoundAttributeDescriptorBuilder>[]
                    {
                        builder => builder
                            .Name("SomethingElse")
                            .PropertyName(validProperty1.Name)
                            .TypeName(validProperty1.PropertyType.FullName),
                        builder => builder
                            .Name("Something-Else")
                            .PropertyName(validProperty2.Name)
                            .TypeName(validProperty2.PropertyType.FullName),
                    });
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(OverriddenAttributeTagHelper).FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        [Fact]
        public void CreateDescriptor_DoesNotInheritOverridenAttributeName()
        {
            // Arrange
            var validProperty1 = typeof(InheritedOverriddenAttributeTagHelper).GetProperty(
                nameof(InheritedOverriddenAttributeTagHelper.ValidAttribute1));
            var validProperty2 = typeof(InheritedOverriddenAttributeTagHelper).GetProperty(
                nameof(InheritedOverriddenAttributeTagHelper.ValidAttribute2));
            var expectedDescriptor =
                CreateTagHelperDescriptor(
                    "inherited-overridden-attribute",
                    typeof(InheritedOverriddenAttributeTagHelper).FullName,
                    AssemblyName,
                    new Action<BoundAttributeDescriptorBuilder>[]
                    {
                        builder => builder
                            .Name("valid-attribute1")
                            .PropertyName(validProperty1.Name)
                            .TypeName(validProperty1.PropertyType.FullName),
                        builder => builder
                            .Name("Something-Else")
                            .PropertyName(validProperty2.Name)
                            .TypeName(validProperty2.PropertyType.FullName),
                    });
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(InheritedOverriddenAttributeTagHelper).FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        [Fact]
        public void CreateDescriptor_AllowsOverriddenAttributeNameOnUnimplementedVirtual()
        {
            // Arrange
            var validProperty1 = typeof(InheritedNotOverriddenAttributeTagHelper).GetProperty(
                nameof(InheritedNotOverriddenAttributeTagHelper.ValidAttribute1));
            var validProperty2 = typeof(InheritedNotOverriddenAttributeTagHelper).GetProperty(
                nameof(InheritedNotOverriddenAttributeTagHelper.ValidAttribute2));

            var expectedDescriptor =  CreateTagHelperDescriptor(
                    "inherited-not-overridden-attribute",
                    typeof(InheritedNotOverriddenAttributeTagHelper).FullName,
                    AssemblyName,
                    new Action<BoundAttributeDescriptorBuilder>[]
                    {
                        builder => builder
                            .Name("SomethingElse")
                            .PropertyName(validProperty1.Name)
                            .TypeName(validProperty1.PropertyType.FullName),
                        builder => builder
                            .Name("Something-Else")
                            .PropertyName(validProperty2.Name)
                            .TypeName(validProperty2.PropertyType.FullName),
                    });
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(InheritedNotOverriddenAttributeTagHelper).FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        [Fact]
        public void CreateDescriptor_BuildsDescriptorsWithInheritedProperties()
        {
            // Arrange
            var expectedDescriptor = CreateTagHelperDescriptor(
                "inherited-single-attribute",
                typeof(InheritedSingleAttributeTagHelper).FullName,
                AssemblyName,
                new Action<BoundAttributeDescriptorBuilder>[]
                {
                    builder => builder
                        .Name("int-attribute")
                        .PropertyName(nameof(InheritedSingleAttributeTagHelper.IntAttribute))
                        .TypeName(typeof(int).FullName)
                });
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(InheritedSingleAttributeTagHelper).FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        [Fact]
        public void CreateDescriptor_BuildsDescriptorsWithConventionNames()
        {
            // Arrange
            var intProperty = typeof(SingleAttributeTagHelper).GetProperty(nameof(SingleAttributeTagHelper.IntAttribute));
            var expectedDescriptor = CreateTagHelperDescriptor(
                "single-attribute",
                typeof(SingleAttributeTagHelper).FullName,
                AssemblyName,
                new Action<BoundAttributeDescriptorBuilder>[]
                {
                    builder => builder
                        .Name("int-attribute")
                        .PropertyName(intProperty.Name)
                        .TypeName(intProperty.PropertyType.FullName)
                });
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(SingleAttributeTagHelper).FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        [Fact]
        public void CreateDescriptor_OnlyAcceptsPropertiesWithGetAndSet()
        {
            // Arrange
            var validProperty = typeof(MissingAccessorTagHelper).GetProperty(
                nameof(MissingAccessorTagHelper.ValidAttribute));
            var expectedDescriptor = CreateTagHelperDescriptor(
                "missing-accessor",
                typeof(MissingAccessorTagHelper).FullName,
                AssemblyName,
                new Action<BoundAttributeDescriptorBuilder>[]
                {
                    builder => builder
                        .Name("valid-attribute")
                        .PropertyName(validProperty.Name)
                        .TypeName(validProperty.PropertyType.FullName)
                });
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(MissingAccessorTagHelper).FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        [Fact]
        public void CreateDescriptor_OnlyAcceptsPropertiesWithPublicGetAndSet()
        {
            // Arrange
            var validProperty = typeof(NonPublicAccessorTagHelper).GetProperty(
                nameof(NonPublicAccessorTagHelper.ValidAttribute));
            var expectedDescriptor = CreateTagHelperDescriptor(
                "non-public-accessor",
                typeof(NonPublicAccessorTagHelper).FullName,
                AssemblyName,
                new Action<BoundAttributeDescriptorBuilder>[]
                {
                    builder => builder
                        .Name("valid-attribute")
                        .PropertyName(validProperty.Name)
                        .TypeName(validProperty.PropertyType.FullName)
                });
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(NonPublicAccessorTagHelper).FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        [Fact]
        public void CreateDescriptor_DoesNotIncludePropertiesWithNotBound()
        {
            // Arrange
            var expectedDescriptor = CreateTagHelperDescriptor(
                "not-bound-attribute",
                typeof(NotBoundAttributeTagHelper).FullName,
                AssemblyName,
                new Action<BoundAttributeDescriptorBuilder>[]
                {
                    builder => builder
                        .Name("bound-property")
                        .PropertyName(nameof(NotBoundAttributeTagHelper.BoundProperty))
                        .TypeName(typeof(object).FullName)
                });
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(NotBoundAttributeTagHelper).FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        [Fact]
        public void CreateDescriptor_ResolvesMultipleTagHelperDescriptorsFromSingleType()
        {
            // Arrange
            var expectedDescriptor =
                CreateTagHelperDescriptor(
                    string.Empty,
                    typeof(MultiTagTagHelper).FullName,
                    AssemblyName,
                    new Action<BoundAttributeDescriptorBuilder>[]
                    {
                        builder => builder
                            .Name("valid-attribute")
                            .PropertyName(nameof(MultiTagTagHelper.ValidAttribute))
                            .TypeName(typeof(string).FullName),
                    },
                    new Action<TagMatchingRuleDescriptorBuilder>[]
                    {
                        builder => builder.RequireTagName("p"),
                        builder => builder.RequireTagName("div"),
                    });
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(MultiTagTagHelper).FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        [Fact]
        public void CreateDescriptor_DoesNotResolveInheritedTagNames()
        {
            // Arrange
            var validProp = typeof(InheritedMultiTagTagHelper).GetProperty(nameof(InheritedMultiTagTagHelper.ValidAttribute));
            var expectedDescriptor = CreateTagHelperDescriptor(
                    "inherited-multi-tag",
                    typeof(InheritedMultiTagTagHelper).FullName,
                    AssemblyName,
                    new Action<BoundAttributeDescriptorBuilder>[]
                    {
                        builder => builder
                            .Name("valid-attribute")
                            .PropertyName(validProp.Name)
                            .TypeName(validProp.PropertyType.FullName),
                    });
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(InheritedMultiTagTagHelper).FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        [Fact]
        public void CreateDescriptor_IgnoresDuplicateTagNamesFromAttribute()
        {
            // Arrange
            var expectedDescriptor = CreateTagHelperDescriptor(
                string.Empty,
                typeof(DuplicateTagNameTagHelper).FullName,
                AssemblyName,
                ruleBuilders: new Action<TagMatchingRuleDescriptorBuilder>[]
                {
                    builder => builder.RequireTagName("p"),
                    builder => builder.RequireTagName("div"),
                });

            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(DuplicateTagNameTagHelper).FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        [Fact]
        public void CreateDescriptor_OverridesTagNameFromAttribute()
        {
            // Arrange
            var expectedDescriptor =
                CreateTagHelperDescriptor(
                    "data-condition",
                    typeof(OverrideNameTagHelper).FullName,
                    AssemblyName);
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(OverrideNameTagHelper).FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        // name, expectedErrorMessages
        public static TheoryData<string, string[]> InvalidNameData
        {
            get
            {
                Func<string, string, string> onNameError =
                    (invalidText, invalidCharacter) => $"Tag helpers cannot target tag name '{invalidText}' because it contains a '{invalidCharacter}' character.";
                var whitespaceErrorString = "Targeted tag name cannot be null or whitespace.";

                var data = GetInvalidNameOrPrefixData(onNameError, whitespaceErrorString, onDataError: null);
                data.Add(string.Empty, new[] { whitespaceErrorString });

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(InvalidNameData))]
        public void CreateDescriptor_CreatesErrorOnInvalidNames(
            string name, string[] expectedErrorMessages)
        {
            // Arrange
            name = name.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\"", "\\\"");
            var text = $@"
        [{typeof(AspNetCore.Razor.TagHelpers.HtmlTargetElementAttribute).FullName}(""{name}"")]
        public class DynamicTestTagHelper : {typeof(AspNetCore.Razor.TagHelpers.TagHelper).FullName}
        {{
        }}";
            var syntaxTree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestCompilation.Create(_assembly, syntaxTree);
            var tagHelperType = compilation.GetTypeByMetadataName("DynamicTestTagHelper");
            var attribute = tagHelperType.GetAttributes().Single();
            var factory = new DefaultTagHelperDescriptorFactory(compilation, includeDocumentation: false, excludeHidden: false);

            // Act
            var descriptor = factory.CreateDescriptor(tagHelperType);

            // Assert
            var rule = Assert.Single(descriptor.TagMatchingRules);
            var errorMessages = rule.GetAllDiagnostics().Select(diagnostic => diagnostic.GetMessage()).ToArray();
            Assert.Equal(expectedErrorMessages.Length, errorMessages.Length);
            for (var i = 0; i < expectedErrorMessages.Length; i++)
            {
                Assert.Equal(expectedErrorMessages[i], errorMessages[i], StringComparer.Ordinal);
            }
        }

        public static TheoryData ValidNameData
        {
            get
            {
                // name, expectedNames
                return new TheoryData<string, IEnumerable<string>>
                {
                    { "p", new[] { "p" } },
                    { " p", new[] { "p" } },
                    { "p ", new[] { "p" } },
                    { " p ", new[] { "p" } },
                    { "p,div", new[] { "p", "div" } },
                    { " p,div", new[] { "p", "div" } },
                    { "p ,div", new[] { "p", "div" } },
                    { " p ,div", new[] { "p", "div" } },
                    { "p, div", new[] { "p", "div" } },
                    { "p,div ", new[] { "p", "div" } },
                    { "p, div ", new[] { "p", "div" } },
                    { " p, div ", new[] { "p", "div" } },
                    { " p , div ", new[] { "p", "div" } },
                };
            }
        }

        public static TheoryData InvalidTagHelperAttributeDescriptorData
        {
            get
            {
                var invalidBoundAttributeBuilder = new DefaultTagHelperDescriptorBuilder(TagHelperConventions.DefaultKind, nameof(InvalidBoundAttribute), "Test");
                invalidBoundAttributeBuilder.TypeName(typeof(InvalidBoundAttribute).FullName);

                // type, expectedAttributeDescriptors
                return new TheoryData<Type, IEnumerable<BoundAttributeDescriptor>>
                {
                    {
                        typeof(InvalidBoundAttribute),
                        new[]
                        {
                            CreateAttributeFor(typeof(InvalidBoundAttribute), attribute =>
                            {
                                attribute
                                .Name("data-something")
                                .PropertyName(nameof(InvalidBoundAttribute.DataSomething))
                                .TypeName(typeof(string).FullName);
                            }),
                        }
                    },
                    {
                        typeof(InvalidBoundAttributeWithValid),
                        new[]
                        {
                            CreateAttributeFor(typeof(InvalidBoundAttributeWithValid), attribute =>
                            {
                                attribute
                                .Name("data-something")
                                .PropertyName(nameof(InvalidBoundAttributeWithValid.DataSomething))
                                .TypeName(typeof(string).FullName); ;
                            }),
                            CreateAttributeFor(typeof(InvalidBoundAttributeWithValid), attribute =>
                            {
                                attribute
                                .Name("int-attribute")
                                .PropertyName(nameof(InvalidBoundAttributeWithValid.IntAttribute))
                                .TypeName(typeof(int).FullName);
                            }),
                        }
                    },
                    {
                        typeof(OverriddenInvalidBoundAttributeWithValid),
                        new[]
                        {
                            CreateAttributeFor(typeof(OverriddenInvalidBoundAttributeWithValid), attribute =>
                            {
                                attribute
                                .Name("valid-something")
                                .PropertyName(nameof(OverriddenInvalidBoundAttributeWithValid.DataSomething))
                                .TypeName(typeof(string).FullName);
                            }),
                        }
                    },
                    {
                        typeof(OverriddenValidBoundAttributeWithInvalid),
                        new[]
                        {
                            CreateAttributeFor(typeof(OverriddenValidBoundAttributeWithInvalid), attribute =>
                            {
                                attribute
                                .Name("data-something")
                                .PropertyName(nameof(OverriddenValidBoundAttributeWithInvalid.ValidSomething))
                                .TypeName(typeof(string).FullName);
                            }),
                        }
                    },
                    {
                        typeof(OverriddenValidBoundAttributeWithInvalidUpperCase),
                        new[]
                        {
                            CreateAttributeFor(typeof(OverriddenValidBoundAttributeWithInvalidUpperCase), attribute =>
                            {
                                attribute
                                .Name("DATA-SOMETHING")
                                .PropertyName(nameof(OverriddenValidBoundAttributeWithInvalidUpperCase.ValidSomething))
                                .TypeName(typeof(string).FullName);
                            }),
                        }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidTagHelperAttributeDescriptorData))]
        public void CreateDescriptor_DoesNotAllowDataDashAttributes(
            Type type,
            IEnumerable<BoundAttributeDescriptor> expectedAttributeDescriptors)
        {
            // Arrange
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(type.FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(
                expectedAttributeDescriptors,
                descriptor.BoundAttributes,
                BoundAttributeDescriptorComparer.Default);

            var id = AspNetCore.Razor.Language.RazorDiagnosticFactory.TagHelper_InvalidBoundAttributeNameStartsWith.Id;
            foreach (var attribute in descriptor.BoundAttributes.Where(a => a.Name.StartsWith("data-", StringComparison.OrdinalIgnoreCase)))
            {
                var diagnostic = Assert.Single(attribute.Diagnostics);
                Assert.Equal(id, diagnostic.Id);
            }
        }

        public static TheoryData<string> ValidAttributeNameData
        {
            get
            {
                return new TheoryData<string>
                {
                    "data",
                    "dataa-",
                    "ValidName",
                    "valid-name",
                    "--valid--name--",
                    ",,--__..oddly.valid::;;",
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidAttributeNameData))]
        public void CreateDescriptor_WithValidAttributeName_HasNoErrors(string name)
        {
            // Arrange
            var text = $@"
            public class DynamicTestTagHelper : {typeof(AspNetCore.Razor.TagHelpers.TagHelper).FullName}
            {{
                [{typeof(AspNetCore.Razor.TagHelpers.HtmlAttributeNameAttribute).FullName}(""{name}"")]
                public string SomeAttribute {{ get; set; }}
            }}";
            var syntaxTree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestCompilation.Create(_assembly, syntaxTree);
            var tagHelperType = compilation.GetTypeByMetadataName("DynamicTestTagHelper");
            var factory = new DefaultTagHelperDescriptorFactory(compilation, includeDocumentation: false, excludeHidden: false);

            // Act
            var descriptor = factory.CreateDescriptor(tagHelperType);

            // Assert
            Assert.False(descriptor.HasErrors);
        }

        public static TheoryData<string> ValidAttributePrefixData
        {
            get
            {
                return new TheoryData<string>
                {
                    string.Empty,
                    "data",
                    "dataa-",
                    "ValidName",
                    "valid-name",
                    "--valid--name--",
                    ",,--__..oddly.valid::;;",
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidAttributePrefixData))]
        public void CreateDescriptor_WithValidAttributePrefix_HasNoErrors(string prefix)
        {
            // Arrange
            var text = $@"
            public class DynamicTestTagHelper : {typeof(AspNetCore.Razor.TagHelpers.TagHelper).FullName}
            {{
                [{typeof(AspNetCore.Razor.TagHelpers.HtmlAttributeNameAttribute).FullName}({nameof(AspNetCore.Razor.TagHelpers.HtmlAttributeNameAttribute.DictionaryAttributePrefix)} = ""{prefix}"")]
                public System.Collections.Generic.IDictionary<string, int> SomeAttribute {{ get; set; }}
            }}";
            var syntaxTree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestCompilation.Create(_assembly, syntaxTree);
            var tagHelperType = compilation.GetTypeByMetadataName("DynamicTestTagHelper");
            var factory = new DefaultTagHelperDescriptorFactory(compilation, includeDocumentation: false, excludeHidden: false);

            // Act
            var descriptor = factory.CreateDescriptor(tagHelperType);

            // Assert
            Assert.False(descriptor.HasErrors);
        }

        // name, expectedErrorMessages
        public static TheoryData<string, string[]> InvalidAttributeNameData
        {
            get
            {
                Func<string, string, string> onNameError = (invalidText, invalidCharacter) =>
                    "Invalid tag helper bound property 'string DynamicTestTagHelper.InvalidProperty' on tag helper 'DynamicTestTagHelper'. Tag helpers " +
                    $"cannot bind to HTML attributes with name '{invalidText}' because the name contains a '{invalidCharacter}' character.";
                var whitespaceErrorString =
                    "Invalid tag helper bound property 'string DynamicTestTagHelper.InvalidProperty' on tag helper 'DynamicTestTagHelper'. Tag helpers cannot " +
                    "bind to HTML attributes with a null or empty name.";
                Func<string, string> onDataError = invalidText =>
                "Invalid tag helper bound property 'string DynamicTestTagHelper.InvalidProperty' on tag helper 'DynamicTestTagHelper'. Tag helpers cannot bind " +
                $"to HTML attributes with name '{invalidText}' because the name starts with 'data-'.";

                return GetInvalidNameOrPrefixData(onNameError, whitespaceErrorString, onDataError);
            }
        }

        [Theory]
        [MemberData(nameof(InvalidAttributeNameData))]
        public void CreateDescriptor_WithInvalidAttributeName_HasErrors(string name, string[] expectedErrorMessages)
        {
            // Arrange
            name = name.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\"", "\\\"");
            var text = $@"
            public class DynamicTestTagHelper : {typeof(AspNetCore.Razor.TagHelpers.TagHelper).FullName}
            {{
                [{typeof(AspNetCore.Razor.TagHelpers.HtmlAttributeNameAttribute).FullName}(""{name}"")]
                public string InvalidProperty {{ get; set; }}
            }}";
            var syntaxTree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestCompilation.Create(_assembly, syntaxTree);
            var tagHelperType = compilation.GetTypeByMetadataName("DynamicTestTagHelper");
            var factory = new DefaultTagHelperDescriptorFactory(compilation, includeDocumentation: false, excludeHidden: false);

            // Act
            var descriptor = factory.CreateDescriptor(tagHelperType);

            // Assert
            var errorMessages = descriptor.GetAllDiagnostics().Select(diagnostic => diagnostic.GetMessage());
            Assert.Equal(expectedErrorMessages, errorMessages);
        }

        // prefix, expectedErrorMessages
        public static TheoryData<string, string[]> InvalidAttributePrefixData
        {
            get
            {
                Func<string, string, string> onPrefixError = (invalidText, invalidCharacter) =>
                    "Invalid tag helper bound property 'System.Collections.Generic.IDictionary<System.String, System.Int32> DynamicTestTagHelper.InvalidProperty' " +
                    "on tag helper 'DynamicTestTagHelper'. Tag helpers " +
                    $"cannot bind to HTML attributes with prefix '{invalidText}' because the prefix contains a '{invalidCharacter}' character.";
                var whitespaceErrorString =
                    "Invalid tag helper bound property 'System.Collections.Generic.IDictionary<System.String, System.Int32> DynamicTestTagHelper.InvalidProperty' " +
                    "on tag helper 'DynamicTestTagHelper'. Tag helpers cannot bind to HTML attributes with a null or empty name.";
                Func<string, string> onDataError = invalidText =>
                    "Invalid tag helper bound property 'System.Collections.Generic.IDictionary<System.String, System.Int32> DynamicTestTagHelper.InvalidProperty' " +
                    "on tag helper 'DynamicTestTagHelper'. Tag helpers cannot bind to HTML attributes " +
                    $"with prefix '{invalidText}' because the prefix starts with 'data-'.";

                return GetInvalidNameOrPrefixData(onPrefixError, whitespaceErrorString, onDataError);
            }
        }

        [Theory]
        [MemberData(nameof(InvalidAttributePrefixData))]
        public void CreateDescriptor_WithInvalidAttributePrefix_HasErrors(string prefix, string[] expectedErrorMessages)
        {
            // Arrange
            prefix = prefix.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\"", "\\\"");
            var text = $@"
            public class DynamicTestTagHelper : {typeof(AspNetCore.Razor.TagHelpers.TagHelper).FullName}
            {{
                [{typeof(AspNetCore.Razor.TagHelpers.HtmlAttributeNameAttribute).FullName}({nameof(AspNetCore.Razor.TagHelpers.HtmlAttributeNameAttribute.DictionaryAttributePrefix)} = ""{prefix}"")]
                public System.Collections.Generic.IDictionary<System.String, System.Int32> InvalidProperty {{ get; set; }}
            }}";
            var syntaxTree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestCompilation.Create(_assembly, syntaxTree);
            var tagHelperType = compilation.GetTypeByMetadataName("DynamicTestTagHelper");
            var factory = new DefaultTagHelperDescriptorFactory(compilation, includeDocumentation: false, excludeHidden: false);

            // Act
            var descriptor = factory.CreateDescriptor(tagHelperType);

            // Assert
            var errorMessages = descriptor.GetAllDiagnostics().Select(diagnostic => diagnostic.GetMessage());
            Assert.Equal(expectedErrorMessages, errorMessages);
        }

        public static TheoryData<string, string[]> InvalidRestrictChildrenNameData
        {
            get
            {
                var nullOrWhiteSpaceError =
                    AspNetCore.Razor.Language.Resources.FormatTagHelper_InvalidRestrictedChildNullOrWhitespace("DynamicTestTagHelper");

                return GetInvalidNameOrPrefixData(
                    onNameError: (invalidInput, invalidCharacter) =>
                        AspNetCore.Razor.Language.Resources.FormatTagHelper_InvalidRestrictedChild(
                            "DynamicTestTagHelper",
                            invalidInput,
                            invalidCharacter),
                    whitespaceErrorString: nullOrWhiteSpaceError,
                    onDataError: null);
            }
        }

        [Theory]
        [MemberData(nameof(InvalidRestrictChildrenNameData))]
        public void CreateDescriptor_WithInvalidAllowedChildren_HasErrors(string name, string[] expectedErrorMessages)
        {
            // Arrange
            name = name.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\"", "\\\"");
            var text = $@"
            [{typeof(AspNetCore.Razor.TagHelpers.RestrictChildrenAttribute).FullName}(""{name}"")]
            public class DynamicTestTagHelper : {typeof(AspNetCore.Razor.TagHelpers.TagHelper).FullName}
            {{
            }}";
            var syntaxTree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestCompilation.Create(_assembly, syntaxTree);
            var tagHelperType = compilation.GetTypeByMetadataName("DynamicTestTagHelper");
            var factory = new DefaultTagHelperDescriptorFactory(compilation, includeDocumentation: false, excludeHidden: false);

            // Act
            var descriptor = factory.CreateDescriptor(tagHelperType);

            // Assert
            var errorMessages = descriptor.GetAllDiagnostics().Select(diagnostic => diagnostic.GetMessage());
            Assert.Equal(expectedErrorMessages, errorMessages);
        }

        public static TheoryData<string, string[]> InvalidParentTagData
        {
            get
            {
                var nullOrWhiteSpaceError =
                    AspNetCore.Razor.Language.Resources.TagHelper_InvalidTargetedParentTagNameNullOrWhitespace;

                return GetInvalidNameOrPrefixData(
                    onNameError: (invalidInput, invalidCharacter) =>
                        AspNetCore.Razor.Language.Resources.FormatTagHelper_InvalidTargetedParentTagName(
                            invalidInput,
                            invalidCharacter),
                    whitespaceErrorString: nullOrWhiteSpaceError,
                    onDataError: null);
            }
        }

        [Theory]
        [MemberData(nameof(InvalidParentTagData))]
        public void CreateDescriptor_WithInvalidParentTag_HasErrors(string name, string[] expectedErrorMessages)
        {
            // Arrange
            name = name.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\"", "\\\"");
            var text = $@"
            [{typeof(AspNetCore.Razor.TagHelpers.HtmlTargetElementAttribute).FullName}({nameof(AspNetCore.Razor.TagHelpers.HtmlTargetElementAttribute.ParentTag)} = ""{name}"")]
            public class DynamicTestTagHelper : {typeof(AspNetCore.Razor.TagHelpers.TagHelper).FullName}
            {{
            }}";
            var syntaxTree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestCompilation.Create(_assembly, syntaxTree);
            var tagHelperType = compilation.GetTypeByMetadataName("DynamicTestTagHelper");
            var factory = new DefaultTagHelperDescriptorFactory(compilation, includeDocumentation: false, excludeHidden: false);

            // Act
            var descriptor = factory.CreateDescriptor(tagHelperType);

            // Assert
            var errorMessages = descriptor.GetAllDiagnostics().Select(diagnostic => diagnostic.GetMessage());
            Assert.Equal(expectedErrorMessages, errorMessages);
        }

        [Fact]
        public void CreateDescriptor_BuildsDescriptorsFromSimpleTypes()
        {
            // Arrange
            var objectAssemblyName = typeof(Enumerable).GetTypeInfo().Assembly.GetName().Name;
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(typeof(Enumerable).FullName);
            var expectedDescriptor =
                CreateTagHelperDescriptor("enumerable", "System.Linq.Enumerable", typeSymbol.ContainingAssembly.Identity.Name);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        public static TheoryData TagHelperWithPrefixData
        {
            get
            {
                var dictionaryNamespace = typeof(IDictionary<,>).FullName;
                dictionaryNamespace = dictionaryNamespace.Substring(0, dictionaryNamespace.IndexOf('`'));

                // tagHelperType, expectedAttributeDescriptors, expectedDiagnostics
                return new TheoryData<Type, IEnumerable<BoundAttributeDescriptor>, IEnumerable<RazorDiagnostic>>
                {
                    {
                        typeof(DefaultValidHtmlAttributePrefix),
                        new[]
                        {
                            CreateAttributeFor(typeof(DefaultValidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("dictionary-property")
                                .PropertyName(nameof(DefaultValidHtmlAttributePrefix.DictionaryProperty))
                                .TypeName($"{dictionaryNamespace}<System.String, System.String>")
                                .AsDictionaryAttribute("dictionary-property-", typeof(string).FullName);
                            }),
                        },
                        Enumerable.Empty<RazorDiagnostic>()
                    },
                    {
                        typeof(SingleValidHtmlAttributePrefix),
                        new[]
                        {
                            CreateAttributeFor(typeof(SingleValidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name")
                                .PropertyName(nameof(SingleValidHtmlAttributePrefix.DictionaryProperty))
                                .TypeName($"{dictionaryNamespace}<System.String, System.String>")
                                .AsDictionaryAttribute("valid-name-", typeof(string).FullName);
                            }),
                        },
                        Enumerable.Empty<RazorDiagnostic>()
                    },
                    {
                        typeof(MultipleValidHtmlAttributePrefix),
                        new[]
                        {
                            CreateAttributeFor(typeof(MultipleValidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name1")
                                .PropertyName(nameof(MultipleValidHtmlAttributePrefix.DictionaryProperty))
                                .TypeName($"{typeof(Dictionary<,>).Namespace}.Dictionary<System.String, System.Object>")
                                .AsDictionaryAttribute("valid-prefix1-", typeof(object).FullName);
                            }),
                            CreateAttributeFor(typeof(MultipleValidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name2")
                                .PropertyName(nameof(MultipleValidHtmlAttributePrefix.DictionarySubclassProperty))
                                .TypeName(typeof(DictionarySubclass).FullName)
                                .AsDictionaryAttribute("valid-prefix2-", typeof(string).FullName);
                            }),
                            CreateAttributeFor(typeof(MultipleValidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name3")
                                .PropertyName(nameof(MultipleValidHtmlAttributePrefix.DictionaryWithoutParameterlessConstructorProperty))
                                .TypeName(typeof(DictionaryWithoutParameterlessConstructor).FullName)
                                .AsDictionaryAttribute("valid-prefix3-", typeof(string).FullName);
                            }),
                            CreateAttributeFor(typeof(MultipleValidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name4")
                                .PropertyName(nameof(MultipleValidHtmlAttributePrefix.GenericDictionarySubclassProperty))
                                .TypeName(typeof(GenericDictionarySubclass<object>).Namespace + ".GenericDictionarySubclass<System.Object>")
                                .AsDictionaryAttribute("valid-prefix4-", typeof(object).FullName);
                            }),
                            CreateAttributeFor(typeof(MultipleValidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name5")
                                .PropertyName(nameof(MultipleValidHtmlAttributePrefix.SortedDictionaryProperty))
                                .TypeName(typeof(SortedDictionary<string, int>).Namespace + ".SortedDictionary<System.String, System.Int32>")
                                .AsDictionaryAttribute("valid-prefix5-", typeof(int).FullName);
                            }),
                            CreateAttributeFor(typeof(MultipleValidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name6")
                                .PropertyName(nameof(MultipleValidHtmlAttributePrefix.StringProperty))
                                .TypeName(typeof(string).FullName);
                            }),
                            CreateAttributeFor(typeof(MultipleValidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .PropertyName(nameof(MultipleValidHtmlAttributePrefix.GetOnlyDictionaryProperty))
                                .TypeName($"{dictionaryNamespace}<System.String, System.Int32>")
                                .AsDictionaryAttribute("get-only-dictionary-property-", typeof(int).FullName);
                            }),
                            CreateAttributeFor(typeof(MultipleValidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .PropertyName(nameof(MultipleValidHtmlAttributePrefix.GetOnlyDictionaryPropertyWithAttributePrefix))
                                .TypeName($"{dictionaryNamespace}<System.String, System.String>")
                                .AsDictionaryAttribute("valid-prefix6", typeof(string).FullName);
                            }),
                        },
                        Enumerable.Empty<RazorDiagnostic>()
                    },
                    {
                        typeof(SingleInvalidHtmlAttributePrefix),
                        new[]
                        {
                            CreateAttributeFor(typeof(SingleInvalidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name")
                                .PropertyName(nameof(SingleInvalidHtmlAttributePrefix.StringProperty))
                                .TypeName(typeof(string).FullName)
                                .AddDiagnostic(RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNotNull(
                                    typeof(SingleInvalidHtmlAttributePrefix).FullName,
                                    nameof(SingleInvalidHtmlAttributePrefix.StringProperty)));
                            }),
                        },
                        new[]
                        {
                            RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNotNull(
                                typeof(SingleInvalidHtmlAttributePrefix).FullName,
                                nameof(SingleInvalidHtmlAttributePrefix.StringProperty))
                        }
                    },
                    {
                        typeof(MultipleInvalidHtmlAttributePrefix),
                        new[]
                        {
                            CreateAttributeFor(typeof(MultipleInvalidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name1")
                                .PropertyName(nameof(MultipleInvalidHtmlAttributePrefix.LongProperty))
                                .TypeName(typeof(long).FullName);
                            }),
                            CreateAttributeFor(typeof(MultipleInvalidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name2")
                                .PropertyName(nameof(MultipleInvalidHtmlAttributePrefix.DictionaryOfIntProperty))
                                .TypeName($"{typeof(Dictionary<,>).Namespace}.Dictionary<System.Int32, System.String>")
                                .AsDictionaryAttribute("valid-prefix2-", typeof(string).FullName)
                                .AddDiagnostic(
                                    RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNotNull(
                                        typeof(MultipleInvalidHtmlAttributePrefix).FullName,
                                        nameof(MultipleInvalidHtmlAttributePrefix.DictionaryOfIntProperty)));
                            }),
                            CreateAttributeFor(typeof(MultipleInvalidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name3")
                                .PropertyName(nameof(MultipleInvalidHtmlAttributePrefix.ReadOnlyDictionaryProperty))
                                .TypeName($"{typeof(IReadOnlyDictionary<,>).Namespace}.IReadOnlyDictionary<System.String, System.Object>")
                                .AddDiagnostic(
                                    RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNotNull(
                                        typeof(MultipleInvalidHtmlAttributePrefix).FullName,
                                        nameof(MultipleInvalidHtmlAttributePrefix.ReadOnlyDictionaryProperty)));
                            }),
                            CreateAttributeFor(typeof(MultipleInvalidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name4")
                                .PropertyName(nameof(MultipleInvalidHtmlAttributePrefix.IntProperty))
                                .TypeName(typeof(int).FullName)
                                .AddDiagnostic(
                                    RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNotNull(
                                        typeof(MultipleInvalidHtmlAttributePrefix).FullName,
                                        nameof(MultipleInvalidHtmlAttributePrefix.IntProperty)));
                            }),
                            CreateAttributeFor(typeof(MultipleInvalidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .Name("valid-name5")
                                .PropertyName(nameof(MultipleInvalidHtmlAttributePrefix.DictionaryOfIntSubclassProperty))
                                .TypeName(typeof(DictionaryOfIntSubclass).FullName)
                                .AsDictionaryAttribute("valid-prefix5-", typeof(string).FullName)
                                .AddDiagnostic(
                                    RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNotNull(
                                        typeof(MultipleInvalidHtmlAttributePrefix).FullName,
                                        nameof(MultipleInvalidHtmlAttributePrefix.DictionaryOfIntSubclassProperty)));
                            }),
                            CreateAttributeFor(typeof(MultipleInvalidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .PropertyName(nameof(MultipleInvalidHtmlAttributePrefix.GetOnlyDictionaryAttributePrefix))
                                .TypeName($"{dictionaryNamespace}<System.Int32, System.String>")
                                .AsDictionaryAttribute("valid-prefix6", typeof(string).FullName)
                                .AddDiagnostic(
                                    RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNotNull(
                                        typeof(MultipleInvalidHtmlAttributePrefix).FullName,
                                        nameof(MultipleInvalidHtmlAttributePrefix.GetOnlyDictionaryAttributePrefix)));
                            }),
                            CreateAttributeFor(typeof(MultipleInvalidHtmlAttributePrefix), attribute =>
                            {
                                attribute
                                .PropertyName(nameof(MultipleInvalidHtmlAttributePrefix.GetOnlyDictionaryPropertyWithAttributeName))
                                .TypeName($"{dictionaryNamespace}<System.String, System.Object>")
                                .AsDictionaryAttribute("invalid-name7-", typeof(object).FullName)
                                .AddDiagnostic(
                                    RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNull(
                                        typeof(MultipleInvalidHtmlAttributePrefix).FullName,
                                        nameof(MultipleInvalidHtmlAttributePrefix.GetOnlyDictionaryPropertyWithAttributeName)));
                            }),
                        },
                        new[]
                        {
                            RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNotNull(
                                typeof(MultipleInvalidHtmlAttributePrefix).FullName,
                                nameof(MultipleInvalidHtmlAttributePrefix.DictionaryOfIntProperty)),
                            RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNotNull(
                                typeof(MultipleInvalidHtmlAttributePrefix).FullName,
                                nameof(MultipleInvalidHtmlAttributePrefix.ReadOnlyDictionaryProperty)),
                            RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNotNull(
                                typeof(MultipleInvalidHtmlAttributePrefix).FullName,
                                nameof(MultipleInvalidHtmlAttributePrefix.IntProperty)),
                            RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNotNull(
                                typeof(MultipleInvalidHtmlAttributePrefix).FullName,
                                nameof(MultipleInvalidHtmlAttributePrefix.DictionaryOfIntSubclassProperty)),
                            RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNotNull(
                                typeof(MultipleInvalidHtmlAttributePrefix).FullName,
                                nameof(MultipleInvalidHtmlAttributePrefix.GetOnlyDictionaryAttributePrefix)),
                            RazorDiagnosticFactory.CreateTagHelper_InvalidAttributePrefixNull(
                                typeof(MultipleInvalidHtmlAttributePrefix).FullName,
                                nameof(MultipleInvalidHtmlAttributePrefix.GetOnlyDictionaryPropertyWithAttributeName)),
                        }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TagHelperWithPrefixData))]
        public void CreateDescriptor_WithPrefixes_ReturnsExpectedAttributeDescriptors(
            Type tagHelperType,
            IEnumerable<BoundAttributeDescriptor> expectedAttributeDescriptors,
            IEnumerable<RazorDiagnostic> expectedDiagnostics)
        {
            // Arrange
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(tagHelperType.FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(
                expectedAttributeDescriptors,
                descriptor.BoundAttributes,
                BoundAttributeDescriptorComparer.CaseSensitive);
            Assert.Equal(expectedDiagnostics, descriptor.GetAllDiagnostics());
        }

        public static TheoryData TagOutputHintData
        {
            get
            {
                // tagHelperType, expectedDescriptor
                return new TheoryData<Type, TagHelperDescriptor>
                {
                    {
                        typeof(MultipleDescriptorTagHelperWithOutputElementHint),
                        TagHelperDescriptorBuilder.Create(typeof(MultipleDescriptorTagHelperWithOutputElementHint).FullName, AssemblyName)
                            .TypeName(typeof(MultipleDescriptorTagHelperWithOutputElementHint).FullName)
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("a"))
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("p"))
                            .TagOutputHint("div")
                            .Build()
                    },
                    {
                        typeof(InheritedOutputElementHintTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(InheritedOutputElementHintTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(InheritedOutputElementHintTagHelper).FullName)
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("inherited-output-element-hint"))
                            .Build()
                    },
                    {
                        typeof(OutputElementHintTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(OutputElementHintTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(OutputElementHintTagHelper).FullName)
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("output-element-hint"))
                            .TagOutputHint("hinted-value")
                            .Build()
                    },
                    {
                        typeof(OverriddenOutputElementHintTagHelper),
                        TagHelperDescriptorBuilder.Create(typeof(OverriddenOutputElementHintTagHelper).FullName, AssemblyName)
                            .TypeName(typeof(OverriddenOutputElementHintTagHelper).FullName)
                            .TagMatchingRuleDescriptor(builder => builder.RequireTagName("overridden-output-element-hint"))
                            .TagOutputHint("overridden")
                            .Build()
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TagOutputHintData))]
        public void CreateDescriptor_CreatesDescriptorsWithOutputElementHint(
            Type tagHelperType,
            TagHelperDescriptor expectedDescriptor)
        {
            // Arrange
            var factory = new DefaultTagHelperDescriptorFactory(Compilation, includeDocumentation: false, excludeHidden: false);
            var typeSymbol = Compilation.GetTypeByMetadataName(tagHelperType.FullName);

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDescriptor, descriptor, TagHelperDescriptorComparer.CaseSensitive);
        }

        [Fact]
        public void CreateDescriptor_CapturesDocumentationOnTagHelperClass()
        {
            // Arrange
            var errorSink = new ErrorSink();
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
        using Microsoft.AspNetCore.Razor.TagHelpers;

        /// <summary>
        /// The summary for <see cref=""DocumentedTagHelper""/>.
        /// </summary>
        /// <remarks>
        /// Inherits from <see cref=""TagHelper""/>.
        /// </remarks>
        public class DocumentedTagHelper : " + typeof(AspNetCore.Razor.TagHelpers.TagHelper).Name + @"
        {
        }");
            var compilation = TestCompilation.Create(_assembly, syntaxTree);
            var factory = new DefaultTagHelperDescriptorFactory(compilation, includeDocumentation: true, excludeHidden: false);
            var typeSymbol = compilation.GetTypeByMetadataName("DocumentedTagHelper");
            var expectedDocumentation =
@"<member name=""T:DocumentedTagHelper"">
    <summary>
    The summary for <see cref=""T:DocumentedTagHelper""/>.
    </summary>
    <remarks>
    Inherits from <see cref=""T:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper""/>.
    </remarks>
</member>
";

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            Assert.Equal(expectedDocumentation, descriptor.Documentation);
        }

        [Fact]
        public void CreateDescriptor_CapturesDocumentationOnTagHelperProperties()
        {
            // Arrange
            var errorSink = new ErrorSink();
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
        using System.Collections.Generic;

        public class DocumentedTagHelper : " + typeof(AspNetCore.Razor.TagHelpers.TagHelper).FullName + @"
        {
            /// <summary>
            /// This <see cref=""SummaryProperty""/> is of type <see cref=""string""/>.
            /// </summary>
            public string SummaryProperty { get; set; }

            /// <remarks>
            /// The <see cref=""SummaryProperty""/> may be <c>null</c>.
            /// </remarks>
            public int RemarksProperty { get; set; }

            /// <summary>
            /// This is a complex <see cref=""List{bool}""/>.
            /// </summary>
            /// <remarks>
            /// <see cref=""SummaryProperty""/><see cref=""RemarksProperty""/>
            /// </remarks>
            public List<bool> RemarksAndSummaryProperty { get; set; }
        }");
            var compilation = TestCompilation.Create(_assembly, syntaxTree);
            var factory = new DefaultTagHelperDescriptorFactory(compilation, includeDocumentation: true, excludeHidden: false);
            var typeSymbol = compilation.GetTypeByMetadataName("DocumentedTagHelper");
            var expectedDocumentations = new[]
            {

@"<member name=""P:DocumentedTagHelper.SummaryProperty"">
    <summary>
    This <see cref=""P:DocumentedTagHelper.SummaryProperty""/> is of type <see cref=""T:System.String""/>.
    </summary>
</member>
",
@"<member name=""P:DocumentedTagHelper.RemarksProperty"">
    <remarks>
    The <see cref=""P:DocumentedTagHelper.SummaryProperty""/> may be <c>null</c>.
    </remarks>
</member>
",
@"<member name=""P:DocumentedTagHelper.RemarksAndSummaryProperty"">
    <summary>
    This is a complex <see cref=""T:System.Collections.Generic.List`1""/>.
    </summary>
    <remarks>
    <see cref=""P:DocumentedTagHelper.SummaryProperty""/><see cref=""P:DocumentedTagHelper.RemarksProperty""/>
    </remarks>
</member>
",
                    };

            // Act
            var descriptor = factory.CreateDescriptor(typeSymbol);

            // Assert
            var documentations = descriptor.BoundAttributes.Select(boundAttribute => boundAttribute.Documentation);
            Assert.Equal(expectedDocumentations, documentations);
        }

        private static TheoryData<string, string[]> GetInvalidNameOrPrefixData(
            Func<string, string, string> onNameError,
            string whitespaceErrorString,
            Func<string, string> onDataError)
        {
            // name, expectedErrorMessages
            var data = new TheoryData<string, string[]>
            {
                { "!", new[] {  onNameError("!", "!") } },
                { "hello!", new[] { onNameError("hello!", "!") } },
                { "!hello", new[] { onNameError("!hello", "!") } },
                { "he!lo", new[] { onNameError("he!lo", "!") } },
                { "!he!lo!", new[] { onNameError("!he!lo!", "!") } },
                { "@", new[] { onNameError("@", "@") } },
                { "hello@", new[] { onNameError("hello@", "@") } },
                { "@hello", new[] { onNameError("@hello", "@") } },
                { "he@lo", new[] { onNameError("he@lo", "@") } },
                { "@he@lo@", new[] { onNameError("@he@lo@", "@") } },
                { "/", new[] { onNameError("/", "/") } },
                { "hello/", new[] { onNameError("hello/", "/") } },
                { "/hello", new[] { onNameError("/hello", "/") } },
                { "he/lo", new[] { onNameError("he/lo", "/") } },
                { "/he/lo/", new[] { onNameError("/he/lo/", "/") } },
                { "<", new[] { onNameError("<", "<") } },
                { "hello<", new[] { onNameError("hello<", "<") } },
                { "<hello", new[] { onNameError("<hello", "<") } },
                { "he<lo", new[] { onNameError("he<lo", "<") } },
                { "<he<lo<", new[] { onNameError("<he<lo<", "<") } },
                { "?", new[] { onNameError("?", "?") } },
                { "hello?", new[] { onNameError("hello?", "?") } },
                { "?hello", new[] { onNameError("?hello", "?") } },
                { "he?lo", new[] { onNameError("he?lo", "?") } },
                { "?he?lo?", new[] { onNameError("?he?lo?", "?") } },
                { "[", new[] { onNameError("[", "[") } },
                { "hello[", new[] { onNameError("hello[", "[") } },
                { "[hello", new[] { onNameError("[hello", "[") } },
                { "he[lo", new[] { onNameError("he[lo", "[") } },
                { "[he[lo[", new[] { onNameError("[he[lo[", "[") } },
                { ">", new[] { onNameError(">", ">") } },
                { "hello>", new[] { onNameError("hello>", ">") } },
                { ">hello", new[] { onNameError(">hello", ">") } },
                { "he>lo", new[] { onNameError("he>lo", ">") } },
                { ">he>lo>", new[] { onNameError(">he>lo>", ">") } },
                { "]", new[] { onNameError("]", "]") } },
                { "hello]", new[] { onNameError("hello]", "]") } },
                { "]hello", new[] { onNameError("]hello", "]") } },
                { "he]lo", new[] { onNameError("he]lo", "]") } },
                { "]he]lo]", new[] { onNameError("]he]lo]", "]") } },
                { "=", new[] { onNameError("=", "=") } },
                { "hello=", new[] { onNameError("hello=", "=") } },
                { "=hello", new[] { onNameError("=hello", "=") } },
                { "he=lo", new[] { onNameError("he=lo", "=") } },
                { "=he=lo=", new[] { onNameError("=he=lo=", "=") } },
                { "\"", new[] { onNameError("\"", "\"") } },
                { "hello\"", new[] { onNameError("hello\"", "\"") } },
                { "\"hello", new[] { onNameError("\"hello", "\"") } },
                { "he\"lo", new[] { onNameError("he\"lo", "\"") } },
                { "\"he\"lo\"", new[] { onNameError("\"he\"lo\"", "\"") } },
                { "'", new[] { onNameError("'", "'") } },
                { "hello'", new[] { onNameError("hello'", "'") } },
                { "'hello", new[] { onNameError("'hello", "'") } },
                { "he'lo", new[] { onNameError("he'lo", "'") } },
                { "'he'lo'", new[] { onNameError("'he'lo'", "'") } },
                { "hello*", new[] { onNameError("hello*", "*") } },
                { "*hello", new[] { onNameError("*hello", "*") } },
                { "he*lo", new[] { onNameError("he*lo", "*") } },
                { "*he*lo*", new[] { onNameError("*he*lo*", "*") } },
                { Environment.NewLine, new[] { whitespaceErrorString } },
                { "\t", new[] { whitespaceErrorString } },
                { " \t ", new[] { whitespaceErrorString } },
                { " ", new[] { whitespaceErrorString } },
                { Environment.NewLine + " ", new[] { whitespaceErrorString } },
                {
                    "! \t\r\n@/<>?[]=\"'*",
                    new[]
                    {
                        onNameError("! \t\r\n@/<>?[]=\"'*", "!"),
                        onNameError("! \t\r\n@/<>?[]=\"'*", " "),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "\t"),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "\r"),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "\n"),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "@"),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "/"),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "<"),
                        onNameError("! \t\r\n@/<>?[]=\"'*", ">"),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "?"),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "["),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "]"),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "="),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "\""),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "'"),
                        onNameError("! \t\r\n@/<>?[]=\"'*", "*"),
                    }
                },
                {
                    "! \tv\ra\nl@i/d<>?[]=\"'*",
                    new[]
                    {
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "!"),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", " "),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "\t"),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "\r"),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "\n"),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "@"),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "/"),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "<"),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", ">"),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "?"),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "["),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "]"),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "="),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "\""),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "'"),
                        onNameError("! \tv\ra\nl@i/d<>?[]=\"'*", "*"),
                    }
                },
            };

            if (onDataError != null)
            {
                data.Add("data-", new[] { onDataError("data-") });
                data.Add("data-something", new[] { onDataError("data-something") });
                data.Add("Data-Something", new[] { onDataError("Data-Something") });
                data.Add("DATA-SOMETHING", new[] { onDataError("DATA-SOMETHING") });
            }

            return data;
        }

        protected static TagHelperDescriptor CreateTagHelperDescriptor(
            string tagName,
            string typeName,
            string assemblyName,
            IEnumerable<Action<BoundAttributeDescriptorBuilder>> attributes = null,
            IEnumerable<Action<TagMatchingRuleDescriptorBuilder>> ruleBuilders = null)
        {
            var builder = TagHelperDescriptorBuilder.Create(typeName, assemblyName);
            builder.TypeName(typeName);

            if (attributes != null)
            {
                foreach (var attributeBuilder in attributes)
                {
                    builder.BoundAttributeDescriptor(attributeBuilder);
                }
            }

            if (ruleBuilders != null)
            {
                foreach (var ruleBuilder in ruleBuilders)
                {
                    builder.TagMatchingRuleDescriptor(innerRuleBuilder =>
                    {
                        innerRuleBuilder.RequireTagName(tagName);
                        ruleBuilder(innerRuleBuilder);
                    });
                }
            }
            else
            {
                builder.TagMatchingRuleDescriptor(ruleBuilder => ruleBuilder.RequireTagName(tagName));
            }

            var descriptor = builder.Build();

            return descriptor;
        }

        private static BoundAttributeDescriptor CreateAttributeFor(Type tagHelperType, Action<BoundAttributeDescriptorBuilder> configure)
        {
            var tagHelperBuilder = new DefaultTagHelperDescriptorBuilder(TagHelperConventions.DefaultKind, tagHelperType.Name, "Test");
            tagHelperBuilder.TypeName(tagHelperType.FullName);

            var attributeBuilder = new DefaultBoundAttributeDescriptorBuilder(tagHelperBuilder, TagHelperConventions.DefaultKind);
            configure(attributeBuilder);
            return attributeBuilder.Build();
        }
    }

    [AspNetCore.Razor.TagHelpers.OutputElementHint("hinted-value")]
    public class OutputElementHintTagHelper : AspNetCore.Razor.TagHelpers.TagHelper
    {
    }

    public class InheritedOutputElementHintTagHelper : OutputElementHintTagHelper
    {
    }

    [AspNetCore.Razor.TagHelpers.OutputElementHint("overridden")]
    public class OverriddenOutputElementHintTagHelper : OutputElementHintTagHelper
    {
    }
}