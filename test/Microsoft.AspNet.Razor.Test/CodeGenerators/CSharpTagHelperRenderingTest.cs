// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
#if DNXCORE50
using System.Reflection;
#endif
using Microsoft.AspNet.Razor.CodeGenerators;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.AspNet.Razor.Compilation.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Generator
{
    public class CSharpTagHelperRenderingTest : TagHelperTestBase
    {
        private static IEnumerable<TagHelperDescriptor> DefaultPAndInputTagHelperDescriptors { get; }
            = BuildPAndInputTagHelperDescriptors(prefix: string.Empty);
        private static IEnumerable<TagHelperDescriptor> PrefixedPAndInputTagHelperDescriptors { get; }
            = BuildPAndInputTagHelperDescriptors(prefix: "THS");

        private static IEnumerable<TagHelperDescriptor> EnumTagHelperDescriptors
        {
            get
            {
                return new[]
                {
                    new TagHelperDescriptor
                    {
                        TagName = "*",
                        TypeName = "CatchAllTagHelper",
                        AssemblyName = "SomeAssembly",
                        Attributes = new[]
                        {
                            new TagHelperAttributeDescriptor
                            {
                                Name = "catch-all",
                                PropertyName = "CatchAll",
                                IsEnum = true,
                                TypeName = typeof(MyEnum).FullName
                            },
                        }
                    },
                    new TagHelperDescriptor
                    {
                        TagName = "input",
                        TypeName = "InputTagHelper",
                        AssemblyName = "SomeAssembly",
                        Attributes = new[]
                        {
                            new TagHelperAttributeDescriptor
                            {
                                Name = "value",
                                PropertyName = "Value",
                                IsEnum = true,
                                TypeName = typeof(MyEnum).FullName
                            },
                        }
                    },
                };
            }
        }

        private static IEnumerable<TagHelperDescriptor> SymbolBoundTagHelperDescriptors
        {
            get
            {
                return new[]
                {
                    new TagHelperDescriptor
                    {
                        TagName = "*",
                        TypeName = "CatchAllTagHelper",
                        AssemblyName = "SomeAssembly",
                        Attributes = new[]
                        {
                            new TagHelperAttributeDescriptor
                            {
                                Name = "[item]",
                                PropertyName = "ListItems",
                                TypeName = typeof(List<string>).FullName
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "[(item)]",
                                PropertyName = "ArrayItems",
                                TypeName = typeof(string[]).FullName
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "(click)",
                                PropertyName = "Event1",
                                TypeName = typeof(Action).FullName
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "(^click)",
                                PropertyName = "Event2",
                                TypeName = typeof(Action).FullName
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "*something",
                                PropertyName = "StringProperty1",
                                TypeName = typeof(string).FullName
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "#local",
                                PropertyName = "StringProperty2",
                                TypeName = typeof(string).FullName
                            },
                        },
                        RequiredAttributes = new[] { "bound" },
                    },
                };
            }
        }

        private static IEnumerable<TagHelperDescriptor> MinimizedTagHelpers_Descriptors
        {
            get
            {
                return new[]
                {
                    new TagHelperDescriptor
                    {
                        TagName = "*",
                        TypeName = "CatchAllTagHelper",
                        AssemblyName = "SomeAssembly",
                        Attributes = new[]
                        {
                            new TagHelperAttributeDescriptor
                            {
                                Name = "catchall-bound-string",
                                PropertyName = "BoundRequiredString",
                                TypeName = typeof(string).FullName,
                                IsStringProperty = true
                            }
                        },
                        RequiredAttributes = new[] { "catchall-unbound-required" },
                    },
                    new TagHelperDescriptor
                    {
                        TagName = "input",
                        TypeName = "InputTagHelper",
                        AssemblyName = "SomeAssembly",
                        Attributes = new[]
                        {
                            new TagHelperAttributeDescriptor
                            {
                                Name = "input-bound-required-string",
                                PropertyName = "BoundRequiredString",
                                TypeName = typeof(string).FullName,
                                IsStringProperty = true
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "input-bound-string",
                                PropertyName = "BoundString",
                                TypeName = typeof(string).FullName,
                                IsStringProperty = true
                            }
                        },
                        RequiredAttributes = new[] { "input-bound-required-string", "input-unbound-required" },
                    }
                };
            }
        }

        private static IEnumerable<TagHelperDescriptor> DynamicAttributeTagHelpers_Descriptors
        {
            get
            {
                return new[]
                {
                    new TagHelperDescriptor
                    {
                        TagName = "input",
                        TypeName = "InputTagHelper",
                        AssemblyName = "SomeAssembly",
                        Attributes = new[]
                        {
                            new TagHelperAttributeDescriptor
                            {
                                Name = "bound",
                                PropertyName = "Bound",
                                TypeName = typeof(string).FullName,
                                IsStringProperty = true
                            }
                        }
                    }
                };
            }
        }

        private static IEnumerable<TagHelperDescriptor> DuplicateTargetTagHelperDescriptors
        {
            get
            {
                var inputTypePropertyInfo = typeof(TestType).GetProperty("Type");
                var inputCheckedPropertyInfo = typeof(TestType).GetProperty("Checked");
                return new[]
                {
                    new TagHelperDescriptor
                    {
                        TagName = "*",
                        TypeName = "CatchAllTagHelper",
                        AssemblyName = "SomeAssembly",
                        Attributes = new TagHelperAttributeDescriptor[]
                        {
                            new TagHelperAttributeDescriptor("type", inputTypePropertyInfo),
                            new TagHelperAttributeDescriptor("checked", inputCheckedPropertyInfo)
                        },
                        RequiredAttributes = new[] { "type" },
                    },
                    new TagHelperDescriptor
                    {
                        TagName = "*",
                        TypeName = "CatchAllTagHelper",
                        AssemblyName = "SomeAssembly",
                        Attributes = new TagHelperAttributeDescriptor[]
                        {
                            new TagHelperAttributeDescriptor("type", inputTypePropertyInfo),
                            new TagHelperAttributeDescriptor("checked", inputCheckedPropertyInfo)
                        },
                        RequiredAttributes = new[] { "checked" },
                    },
                    new TagHelperDescriptor
                    {
                        TagName = "input",
                        TypeName = "InputTagHelper",
                        AssemblyName = "SomeAssembly",
                        Attributes = new TagHelperAttributeDescriptor[]
                        {
                            new TagHelperAttributeDescriptor("type", inputTypePropertyInfo),
                            new TagHelperAttributeDescriptor("checked", inputCheckedPropertyInfo)
                        },
                        RequiredAttributes = new[] { "type" },
                    },
                    new TagHelperDescriptor
                    {
                        TagName = "input",
                        TypeName = "InputTagHelper",
                        AssemblyName = "SomeAssembly",
                        Attributes = new TagHelperAttributeDescriptor[]
                        {
                            new TagHelperAttributeDescriptor("type", inputTypePropertyInfo),
                            new TagHelperAttributeDescriptor("checked", inputCheckedPropertyInfo)
                        },
                        RequiredAttributes = new[] { "checked" },
                    }
                };
            }
        }

        private static IEnumerable<TagHelperDescriptor> AttributeTargetingTagHelperDescriptors
        {
            get
            {
                var inputTypePropertyInfo = typeof(TestType).GetProperty("Type");
                var inputCheckedPropertyInfo = typeof(TestType).GetProperty("Checked");
                return new[]
                {
                    new TagHelperDescriptor
                    {
                        TagName = "p",
                        TypeName = "PTagHelper",
                        AssemblyName = "SomeAssembly",
                        RequiredAttributes = new[] { "class" },
                    },
                    new TagHelperDescriptor
                    {
                        TagName = "input",
                        TypeName = "InputTagHelper",
                        AssemblyName = "SomeAssembly",
                        Attributes = new TagHelperAttributeDescriptor[]
                        {
                            new TagHelperAttributeDescriptor("type", inputTypePropertyInfo)
                        },
                        RequiredAttributes = new[] { "type" },
                    },
                    new TagHelperDescriptor
                    {
                        TagName = "input",
                        TypeName = "InputTagHelper2",
                        AssemblyName = "SomeAssembly",
                        Attributes = new TagHelperAttributeDescriptor[]
                        {
                            new TagHelperAttributeDescriptor("type", inputTypePropertyInfo),
                            new TagHelperAttributeDescriptor("checked", inputCheckedPropertyInfo)
                        },
                        RequiredAttributes = new[] { "type", "checked" },
                    },
                    new TagHelperDescriptor
                    {
                        TagName = "*",
                        TypeName = "CatchAllTagHelper",
                        AssemblyName = "SomeAssembly",
                        RequiredAttributes = new[] { "catchAll" },
                    }
                };
            }
        }

        private static IEnumerable<TagHelperDescriptor> PrefixedAttributeTagHelperDescriptors
        {
            get
            {
                return new[]
                {
                    new TagHelperDescriptor
                    {
                        TagName = "input",
                        TypeName = "InputTagHelper1",
                        AssemblyName = "SomeAssembly",
                        Attributes = new[]
                        {
                            new TagHelperAttributeDescriptor
                            {
                                Name = "int-prefix-grabber",
                                PropertyName = "IntProperty",
                                TypeName = typeof(int).FullName
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "int-dictionary",
                                PropertyName = "IntDictionaryProperty",
                                TypeName = typeof(IDictionary<string, int>).FullName
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "string-dictionary",
                                PropertyName = "StringDictionaryProperty",
                                TypeName = "Namespace.DictionaryWithoutParameterlessConstructor<string, string>"
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "string-prefix-grabber",
                                PropertyName = "StringProperty",
                                TypeName = typeof(string).FullName,
                                IsStringProperty = true
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "int-prefix-",
                                PropertyName = "IntDictionaryProperty",
                                TypeName = typeof(int).FullName,
                                IsIndexer = true
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "string-prefix-",
                                PropertyName = "StringDictionaryProperty",
                                TypeName = typeof(string).FullName,
                                IsIndexer = true,
                                IsStringProperty = true
                            }
                        }
                    },
                    new TagHelperDescriptor
                    {
                        TagName = "input",
                        TypeName = "InputTagHelper2",
                        AssemblyName = "SomeAssembly",
                        Attributes = new[]
                        {
                            new TagHelperAttributeDescriptor
                            {
                                Name = "int-dictionary",
                                PropertyName = "IntDictionaryProperty",
                                TypeName = typeof(int).FullName
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "string-dictionary",
                                PropertyName = "StringDictionaryProperty",
                                TypeName = "Namespace.DictionaryWithoutParameterlessConstructor<string, string>"
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "int-prefix-",
                                PropertyName = "IntDictionaryProperty",
                                TypeName = typeof(int).FullName,
                                IsIndexer = true
                            },
                            new TagHelperAttributeDescriptor
                            {
                                Name = "string-prefix-",
                                PropertyName = "StringDictionaryProperty",
                                TypeName = typeof(string).FullName,
                                IsIndexer = true,
                                IsStringProperty = true
                            }
                        }
                    }
                };
            }
        }

        public static TheoryData TagHelperDescriptorFlowTestData
        {
            get
            {
                return new TheoryData<string, // Test name
                                      string, // Baseline name
                                      IEnumerable<TagHelperDescriptor>, // TagHelperDescriptors provided
                                      IEnumerable<TagHelperDescriptor>, // Expected TagHelperDescriptors
                                      bool> // Design time mode.
                {
                    {
                        "SingleTagHelper",
                        "SingleTagHelper",
                        DefaultPAndInputTagHelperDescriptors,
                        DefaultPAndInputTagHelperDescriptors,
                        false
                    },
                    {
                        "SingleTagHelper",
                        "SingleTagHelper.DesignTime",
                        DefaultPAndInputTagHelperDescriptors,
                        DefaultPAndInputTagHelperDescriptors,
                        true
                    },
                    {
                        "BasicTagHelpers",
                        "BasicTagHelpers",
                        DefaultPAndInputTagHelperDescriptors,
                        DefaultPAndInputTagHelperDescriptors,
                        false
                    },
                    {
                        "DuplicateTargetTagHelper",
                        "DuplicateTargetTagHelper",
                        DuplicateTargetTagHelperDescriptors,
                        DuplicateTargetTagHelperDescriptors,
                        false
                    },
                    {
                        "BasicTagHelpers",
                        "BasicTagHelpers.DesignTime",
                        DefaultPAndInputTagHelperDescriptors,
                        DefaultPAndInputTagHelperDescriptors,
                        true
                    },
                    {
                        "BasicTagHelpers.RemoveTagHelper",
                        "BasicTagHelpers.RemoveTagHelper",
                        DefaultPAndInputTagHelperDescriptors,
                        Enumerable.Empty<TagHelperDescriptor>(),
                        false
                    },
                    {
                        "BasicTagHelpers.Prefixed",
                        "BasicTagHelpers.Prefixed",
                        PrefixedPAndInputTagHelperDescriptors,
                        PrefixedPAndInputTagHelperDescriptors,
                        false
                    },
                    {
                        "BasicTagHelpers.Prefixed",
                        "BasicTagHelpers.Prefixed.DesignTime",
                        PrefixedPAndInputTagHelperDescriptors,
                        PrefixedPAndInputTagHelperDescriptors,
                        true
                    },
                    {
                        "ComplexTagHelpers",
                        "ComplexTagHelpers",
                        DefaultPAndInputTagHelperDescriptors,
                        DefaultPAndInputTagHelperDescriptors,
                        false
                    },
                    {
                        "ComplexTagHelpers",
                        "ComplexTagHelpers.DesignTime",
                        DefaultPAndInputTagHelperDescriptors,
                        DefaultPAndInputTagHelperDescriptors,
                        true
                    },
                    {
                        "AttributeTargetingTagHelpers",
                        "AttributeTargetingTagHelpers",
                        AttributeTargetingTagHelperDescriptors,
                        AttributeTargetingTagHelperDescriptors,
                        false
                    },
                    {
                        "AttributeTargetingTagHelpers",
                        "AttributeTargetingTagHelpers.DesignTime",
                        AttributeTargetingTagHelperDescriptors,
                        AttributeTargetingTagHelperDescriptors,
                        true
                    },
                    {
                        "MinimizedTagHelpers",
                        "MinimizedTagHelpers",
                        MinimizedTagHelpers_Descriptors,
                        MinimizedTagHelpers_Descriptors,
                        false
                    },
                    {
                        "MinimizedTagHelpers",
                        "MinimizedTagHelpers.DesignTime",
                        MinimizedTagHelpers_Descriptors,
                        MinimizedTagHelpers_Descriptors,
                        true
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(TagHelperDescriptorFlowTestData))]
        public void TagHelpers_RenderingOutputFlowsFoundTagHelperDescriptors(
            string testName,
            string baselineName,
            IEnumerable<TagHelperDescriptor> tagHelperDescriptors,
            IEnumerable<TagHelperDescriptor> expectedTagHelperDescriptors,
            bool designTimeMode)
        {
            RunTagHelperTest(
                testName,
                baseLineName: baselineName,
                tagHelperDescriptors: tagHelperDescriptors,
                onResults: (results) =>
                {
                    Assert.Equal(expectedTagHelperDescriptors,
                                 results.TagHelperDescriptors,
                                 TagHelperDescriptorComparer.Default);
                },
                designTimeMode: designTimeMode);
        }

        public static TheoryData DesignTimeTagHelperTestData
        {
            get
            {
                // Test resource name, baseline resource name, expected TagHelperDescriptors, expected LineMappings
                return new TheoryData<string, string, IEnumerable<TagHelperDescriptor>, IList<LineMapping>>
                {
                    {
                        "SingleTagHelper",
                        "SingleTagHelper.DesignTime",
                        DefaultPAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 421,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 17),
                            BuildLineMapping(
                                documentAbsoluteIndex: 63,
                                documentLineIndex: 2,
                                generatedAbsoluteIndex: 926,
                                generatedLineIndex: 33,
                                characterOffsetIndex: 28,
                                contentLength: 4),
                        }
                    },
                    {
                        "BasicTagHelpers",
                        "BasicTagHelpers.DesignTime",
                        DefaultPAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 421,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 17),
                            BuildLineMapping(
                                documentAbsoluteIndex: 202,
                                documentLineIndex: 5,
                                generatedAbsoluteIndex: 1220,
                                generatedLineIndex: 37,
                                characterOffsetIndex: 38,
                                contentLength: 23),
                            BuildLineMapping(
                                documentAbsoluteIndex: 285,
                                documentLineIndex: 6,
                                generatedAbsoluteIndex: 1719,
                                generatedLineIndex: 48,
                                characterOffsetIndex: 40,
                                contentLength: 4),
                        }
                    },
                    {
                        "BasicTagHelpers.Prefixed",
                        "BasicTagHelpers.Prefixed.DesignTime",
                        PrefixedPAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 17,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 442,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 17,
                                contentLength: 5),
                            BuildLineMapping(
                                documentAbsoluteIndex: 38,
                                documentLineIndex: 1,
                                generatedAbsoluteIndex: 601,
                                generatedLineIndex: 21,
                                characterOffsetIndex: 14,
                                contentLength: 17),
                            BuildLineMapping(
                                documentAbsoluteIndex: 226,
                                documentLineIndex: 7,
                                generatedAbsoluteIndex: 1466,
                                generatedLineIndex: 45,
                                characterOffsetIndex: 43,
                                contentLength: 4),
                        }
                    },
                    {
                        "ComplexTagHelpers",
                        "ComplexTagHelpers.DesignTime",
                        DefaultPAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 425,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 17),
                            BuildLineMapping(
                                documentAbsoluteIndex: 36,
                                documentLineIndex: 2,
                                documentCharacterOffsetIndex: 1,
                                generatedAbsoluteIndex: 971,
                                generatedLineIndex: 34,
                                generatedCharacterOffsetIndex: 0,
                                contentLength: 48),
                            BuildLineMapping(
                                documentAbsoluteIndex: 211,
                                documentLineIndex: 9,
                                generatedAbsoluteIndex: 1089,
                                generatedLineIndex: 43,
                                characterOffsetIndex: 0,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 224,
                                documentLineIndex: 9,
                                documentCharacterOffsetIndex: 13,
                                generatedAbsoluteIndex: 1185,
                                generatedLineIndex: 49,
                                generatedCharacterOffsetIndex: 12,
                                contentLength: 27),
                            BuildLineMapping(
                                documentAbsoluteIndex: 352,
                                documentLineIndex: 12,
                                generatedAbsoluteIndex: 1607,
                                generatedLineIndex: 61,
                                characterOffsetIndex: 0,
                                contentLength: 48),
                            BuildLineMapping(
                                documentAbsoluteIndex: 446,
                                documentLineIndex: 15,
                                generatedAbsoluteIndex: 1923,
                                generatedLineIndex: 71,
                                characterOffsetIndex: 46,
                                contentLength: 8),
                            BuildLineMapping(
                                documentAbsoluteIndex: 463,
                                documentLineIndex: 15,
                                generatedAbsoluteIndex: 2177,
                                generatedLineIndex: 78,
                                characterOffsetIndex: 63,
                                contentLength: 4),
                            BuildLineMapping(
                                documentAbsoluteIndex: 507,
                                documentLineIndex: 16,
                                generatedAbsoluteIndex: 2502,
                                generatedLineIndex: 86,
                                characterOffsetIndex: 31,
                                contentLength: 30),
                            BuildLineMapping(
                                documentAbsoluteIndex: 574,
                                documentLineIndex: 17,
                                documentCharacterOffsetIndex: 30,
                                generatedAbsoluteIndex: 2896,
                                generatedLineIndex: 95,
                                generatedCharacterOffsetIndex: 29,
                                contentLength: 10),
                            BuildLineMapping(
                                documentAbsoluteIndex: 606,
                                documentLineIndex: 17,
                                documentCharacterOffsetIndex: 62,
                                generatedAbsoluteIndex: 3039,
                                generatedLineIndex: 101,
                                generatedCharacterOffsetIndex: 61,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 607,
                                documentLineIndex: 17,
                                documentCharacterOffsetIndex: 63,
                                generatedAbsoluteIndex: 3174,
                                generatedLineIndex: 107,
                                generatedCharacterOffsetIndex: 62,
                                contentLength: 8),
                            BuildLineMapping(
                                documentAbsoluteIndex: 637,
                                documentLineIndex: 17,
                                documentCharacterOffsetIndex: 93,
                                generatedAbsoluteIndex: 3345,
                                generatedLineIndex: 113,
                                generatedCharacterOffsetIndex: 91,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 638,
                                documentLineIndex: 17,
                                documentCharacterOffsetIndex: 94,
                                generatedAbsoluteIndex: 3510,
                                generatedLineIndex: 119,
                                generatedCharacterOffsetIndex: 92,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 643,
                                documentLineIndex: 18,
                                generatedAbsoluteIndex: 3695,
                                generatedLineIndex: 127,
                                characterOffsetIndex: 0,
                                contentLength: 15),
                            BuildLineMapping(
                                documentAbsoluteIndex: 163,
                                documentLineIndex: 7,
                                generatedAbsoluteIndex: 3878,
                                generatedLineIndex: 134,
                                characterOffsetIndex: 32,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 769,
                                documentLineIndex: 21,
                                generatedAbsoluteIndex: 3961,
                                generatedLineIndex: 139,
                                characterOffsetIndex: 0,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 783,
                                documentLineIndex: 21,
                                generatedAbsoluteIndex: 4059,
                                generatedLineIndex: 145,
                                characterOffsetIndex: 14,
                                contentLength: 21),
                            BuildLineMapping(
                                documentAbsoluteIndex: 836,
                                documentLineIndex: 22,
                                documentCharacterOffsetIndex: 29,
                                generatedAbsoluteIndex: 4332,
                                generatedLineIndex: 153,
                                generatedCharacterOffsetIndex: 28,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 837,
                                documentLineIndex: 22,
                                documentCharacterOffsetIndex: 30,
                                generatedAbsoluteIndex: 4333,
                                generatedLineIndex: 153,
                                generatedCharacterOffsetIndex: 29,
                                contentLength: 7),
                            BuildLineMapping(
                                documentAbsoluteIndex: 844,
                                documentLineIndex: 22,
                                documentCharacterOffsetIndex: 37,
                                generatedAbsoluteIndex: 4340,
                                generatedLineIndex: 153,
                                generatedCharacterOffsetIndex: 36,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 711,
                                documentLineIndex: 20,
                                documentCharacterOffsetIndex: 39,
                                generatedAbsoluteIndex: 4517,
                                generatedLineIndex: 159,
                                generatedCharacterOffsetIndex: 38,
                                contentLength: 23),
                            BuildLineMapping(
                                documentAbsoluteIndex: 734,
                                documentLineIndex: 20,
                                documentCharacterOffsetIndex: 62,
                                generatedAbsoluteIndex: 4540,
                                generatedLineIndex: 159,
                                generatedCharacterOffsetIndex: 61,
                                contentLength: 7),
                            BuildLineMapping(
                                documentAbsoluteIndex: 976,
                                documentLineIndex: 25,
                                documentCharacterOffsetIndex: 61,
                                generatedAbsoluteIndex: 4830,
                                generatedLineIndex: 166,
                                generatedCharacterOffsetIndex: 60,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 977,
                                documentLineIndex: 25,
                                documentCharacterOffsetIndex: 62,
                                generatedAbsoluteIndex: 4831,
                                generatedLineIndex: 166,
                                generatedCharacterOffsetIndex: 61,
                                contentLength: 30),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1007,
                                documentLineIndex: 25,
                                documentCharacterOffsetIndex: 92,
                                generatedAbsoluteIndex: 4861,
                                generatedLineIndex: 166,
                                generatedCharacterOffsetIndex: 91,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 879,
                                documentLineIndex: 24,
                                documentCharacterOffsetIndex: 16,
                                generatedAbsoluteIndex: 5019,
                                generatedLineIndex: 172,
                                generatedCharacterOffsetIndex: 19,
                                contentLength: 8),
                            BuildLineMapping(
                                documentAbsoluteIndex: 887,
                                documentLineIndex: 24,
                                documentCharacterOffsetIndex: 24,
                                generatedAbsoluteIndex: 5027,
                                generatedLineIndex: 172,
                                generatedCharacterOffsetIndex: 27,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 888,
                                documentLineIndex: 24,
                                documentCharacterOffsetIndex: 25,
                                generatedAbsoluteIndex: 5028,
                                generatedLineIndex: 172,
                                generatedCharacterOffsetIndex: 28,
                                contentLength: 23),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1106,
                                documentLineIndex: 28,
                                generatedAbsoluteIndex: 5302,
                                generatedLineIndex: 179,
                                characterOffsetIndex: 28,
                                contentLength: 30),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1044,
                                documentLineIndex: 27,
                                documentCharacterOffsetIndex: 16,
                                generatedAbsoluteIndex: 5489,
                                generatedLineIndex: 185,
                                generatedCharacterOffsetIndex: 19,
                                contentLength: 30),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1234,
                                documentLineIndex: 31,
                                generatedAbsoluteIndex: 5770,
                                generatedLineIndex: 192,
                                characterOffsetIndex: 28,
                                contentLength: 3),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1237,
                                documentLineIndex: 31,
                                generatedAbsoluteIndex: 5773,
                                generatedLineIndex: 192,
                                characterOffsetIndex: 31,
                                contentLength: 2),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1239,
                                documentLineIndex: 31,
                                generatedAbsoluteIndex: 5775,
                                generatedLineIndex: 192,
                                characterOffsetIndex: 33,
                                contentLength: 27),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1266,
                                documentLineIndex: 31,
                                generatedAbsoluteIndex: 5802,
                                generatedLineIndex: 192,
                                characterOffsetIndex: 60,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1267,
                                documentLineIndex: 31,
                                generatedAbsoluteIndex: 5803,
                                generatedLineIndex: 192,
                                characterOffsetIndex: 61,
                                contentLength: 10),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1171,
                                documentLineIndex: 30,
                                documentCharacterOffsetIndex: 17,
                                generatedAbsoluteIndex: 5970,
                                generatedLineIndex: 198,
                                generatedCharacterOffsetIndex: 19,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1172,
                                documentLineIndex: 30,
                                documentCharacterOffsetIndex: 18,
                                generatedAbsoluteIndex: 5971,
                                generatedLineIndex: 198,
                                generatedCharacterOffsetIndex: 20,
                                contentLength: 29),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1201,
                                documentLineIndex: 30,
                                documentCharacterOffsetIndex: 47,
                                generatedAbsoluteIndex: 6000,
                                generatedLineIndex: 198,
                                generatedCharacterOffsetIndex: 49,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1306,
                                documentLineIndex: 33,
                                generatedAbsoluteIndex: 6081,
                                generatedLineIndex: 203,
                                characterOffsetIndex: 9,
                                contentLength: 11),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1361,
                                documentLineIndex: 33,
                                documentCharacterOffsetIndex: 64,
                                generatedAbsoluteIndex: 6386,
                                generatedLineIndex: 207,
                                generatedCharacterOffsetIndex: 63,
                                contentLength: 7),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1326,
                                documentLineIndex: 33,
                                generatedAbsoluteIndex: 6552,
                                generatedLineIndex: 213,
                                characterOffsetIndex: 29,
                                contentLength: 3),
                            BuildLineMapping(
                                documentAbsoluteIndex: 1390,
                                documentLineIndex: 35,
                                generatedAbsoluteIndex: 6667,
                                generatedLineIndex: 224,
                                characterOffsetIndex: 0,
                                contentLength: 1),
                        }
                    },
                    {
                        "EmptyAttributeTagHelpers",
                        "EmptyAttributeTagHelpers.DesignTime",
                        DefaultPAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 439,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 11),
                            BuildLineMapping(
                                documentAbsoluteIndex: 62,
                                documentLineIndex: 3,
                                documentCharacterOffsetIndex: 26,
                                generatedAbsoluteIndex: 1275,
                                generatedLineIndex: 38,
                                generatedCharacterOffsetIndex: 28,
                                contentLength: 0),
                            BuildLineMapping(
                                documentAbsoluteIndex: 122,
                                documentLineIndex: 5,
                                generatedAbsoluteIndex: 1636,
                                generatedLineIndex: 47,
                                characterOffsetIndex: 30,
                                contentLength: 0),
                            BuildLineMapping(
                                documentAbsoluteIndex: 88,
                                documentLineIndex: 4,
                                documentCharacterOffsetIndex: 12,
                                generatedAbsoluteIndex: 1799,
                                generatedLineIndex: 53,
                                generatedCharacterOffsetIndex: 19,
                                contentLength: 0),
                        }
                    },
                    {
                        "EscapedTagHelpers",
                        "EscapedTagHelpers.DesignTime",
                        DefaultPAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 425,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 11),
                            BuildLineMapping(
                                documentAbsoluteIndex: 102,
                                documentLineIndex: 3,
                                generatedAbsoluteIndex: 937,
                                generatedLineIndex: 33,
                                characterOffsetIndex: 29,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 200,
                                documentLineIndex: 5,
                                generatedAbsoluteIndex: 1222,
                                generatedLineIndex: 40,
                                characterOffsetIndex: 51,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 223,
                                documentLineIndex: 5,
                                generatedAbsoluteIndex: 1490,
                                generatedLineIndex: 47,
                                characterOffsetIndex: 74,
                                contentLength: 4),
                        }
                    },
                    {
                        "AttributeTargetingTagHelpers",
                        "AttributeTargetingTagHelpers.DesignTime",
                        AttributeTargetingTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 447,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 14),
                            BuildLineMapping(
                                documentAbsoluteIndex: 186,
                                documentLineIndex: 5,
                                generatedAbsoluteIndex: 1462,
                                generatedLineIndex: 40,
                                characterOffsetIndex: 36,
                                contentLength: 4),
                            BuildLineMapping(
                                documentAbsoluteIndex: 232,
                                documentLineIndex: 6,
                                generatedAbsoluteIndex: 1926,
                                generatedLineIndex: 50,
                                characterOffsetIndex: 36,
                                contentLength: 4),
                        }
                    },
                    {
                        "PrefixedAttributeTagHelpers",
                        "PrefixedAttributeTagHelpers.DesignTime",
                        PrefixedAttributeTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 445,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 17),
                            BuildLineMapping(
                                documentAbsoluteIndex: 37,
                                documentLineIndex: 2,
                                generatedAbsoluteIndex: 958,
                                generatedLineIndex: 33,
                                characterOffsetIndex: 2,
                                contentLength: 242),
                            BuildLineMapping(
                                documentAbsoluteIndex: 370,
                                documentLineIndex: 15,
                                generatedAbsoluteIndex: 1477,
                                generatedLineIndex: 50,
                                characterOffsetIndex: 43,
                                contentLength: 13),
                            BuildLineMapping(
                                documentAbsoluteIndex: 404,
                                documentLineIndex: 15,
                                generatedAbsoluteIndex: 1744,
                                generatedLineIndex: 56,
                                characterOffsetIndex: 77,
                                contentLength: 16),
                            BuildLineMapping(
                                documentAbsoluteIndex: 468,
                                documentLineIndex: 16,
                                generatedAbsoluteIndex: 2140,
                                generatedLineIndex: 64,
                                characterOffsetIndex: 43,
                                contentLength: 13),
                            BuildLineMapping(
                                documentAbsoluteIndex: 502,
                                documentLineIndex: 16,
                                generatedAbsoluteIndex: 2407,
                                generatedLineIndex: 70,
                                characterOffsetIndex: 77,
                                contentLength: 2),
                            BuildLineMapping(
                                documentAbsoluteIndex: 526,
                                documentLineIndex: 16,
                                generatedAbsoluteIndex: 2707,
                                generatedLineIndex: 76,
                                characterOffsetIndex: 101,
                                contentLength: 2),
                            BuildLineMapping(
                                documentAbsoluteIndex: 590,
                                documentLineIndex: 18,
                                documentCharacterOffsetIndex: 31,
                                generatedAbsoluteIndex: 3073,
                                generatedLineIndex: 84,
                                generatedCharacterOffsetIndex: 32,
                                contentLength: 2),
                            BuildLineMapping(
                                documentAbsoluteIndex: 611,
                                documentLineIndex: 18,
                                generatedAbsoluteIndex: 3305,
                                generatedLineIndex: 90,
                                characterOffsetIndex: 52,
                                contentLength: 2),
                            BuildLineMapping(
                                documentAbsoluteIndex: 634,
                                documentLineIndex: 18,
                                generatedAbsoluteIndex: 3575,
                                generatedLineIndex: 96,
                                characterOffsetIndex: 75,
                                contentLength: 2),
                            BuildLineMapping(
                                documentAbsoluteIndex: 783,
                                documentLineIndex: 20,
                                generatedAbsoluteIndex: 4188,
                                generatedLineIndex: 106,
                                characterOffsetIndex: 42,
                                contentLength: 8),
                            BuildLineMapping(
                                documentAbsoluteIndex: 826,
                                documentLineIndex: 21,
                                documentCharacterOffsetIndex: 29,
                                generatedAbsoluteIndex: 4683,
                                generatedLineIndex: 115,
                                generatedCharacterOffsetIndex: 51,
                                contentLength: 2),
                        }
                    },
                    {
                        "DuplicateAttributeTagHelpers",
                        "DuplicateAttributeTagHelpers.DesignTime",
                        DefaultPAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 447,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 17),
                            BuildLineMapping(
                                documentAbsoluteIndex: 146,
                                documentLineIndex: 4,
                                generatedAbsoluteIndex: 1569,
                                generatedLineIndex: 42,
                                characterOffsetIndex: 34,
                                contentLength: 4),
                            BuildLineMapping(
                                documentAbsoluteIndex: 43,
                                documentLineIndex: 2,
                                documentCharacterOffsetIndex: 8,
                                generatedAbsoluteIndex: 1740,
                                generatedLineIndex: 48,
                                generatedCharacterOffsetIndex: 19,
                                contentLength: 1),
                        }
                    },
                    {
                        "DynamicAttributeTagHelpers",
                        "DynamicAttributeTagHelpers.DesignTime",
                        DynamicAttributeTagHelpers_Descriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 443,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 17),
                            BuildLineMapping(
                                documentAbsoluteIndex: 59,
                                documentLineIndex: 2,
                                generatedAbsoluteIndex: 982,
                                generatedLineIndex: 33,
                                characterOffsetIndex: 24,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 96,
                                documentLineIndex: 4,
                                documentCharacterOffsetIndex: 17,
                                generatedAbsoluteIndex: 1164,
                                generatedLineIndex: 39,
                                generatedCharacterOffsetIndex: 16,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 109,
                                documentLineIndex: 4,
                                generatedAbsoluteIndex: 1286,
                                generatedLineIndex: 45,
                                characterOffsetIndex: 30,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 121,
                                documentLineIndex: 4,
                                generatedAbsoluteIndex: 1419,
                                generatedLineIndex: 50,
                                characterOffsetIndex: 42,
                                contentLength: 10),
                            BuildLineMapping(
                                documentAbsoluteIndex: 132,
                                documentLineIndex: 4,
                                generatedAbsoluteIndex: 1562,
                                generatedLineIndex: 56,
                                characterOffsetIndex: 53,
                                contentLength: 5),
                            BuildLineMapping(
                                documentAbsoluteIndex: 137,
                                documentLineIndex: 4,
                                generatedAbsoluteIndex: 1704,
                                generatedLineIndex: 61,
                                characterOffsetIndex: 58,
                                contentLength: 2),
                            BuildLineMapping(
                                documentAbsoluteIndex: 176,
                                documentLineIndex: 6,
                                generatedAbsoluteIndex: 1883,
                                generatedLineIndex: 68,
                                characterOffsetIndex: 22,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 214,
                                documentLineIndex: 6,
                                generatedAbsoluteIndex: 2086,
                                generatedLineIndex: 74,
                                characterOffsetIndex: 60,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 256,
                                documentLineIndex: 8,
                                generatedAbsoluteIndex: 2267,
                                generatedLineIndex: 80,
                                characterOffsetIndex: 15,
                                contentLength: 13),
                            BuildLineMapping(
                                documentAbsoluteIndex: 271,
                                documentLineIndex: 8,
                                documentCharacterOffsetIndex: 30,
                                generatedAbsoluteIndex: 2388,
                                generatedLineIndex: 85,
                                generatedCharacterOffsetIndex: 29,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 284,
                                documentLineIndex: 8,
                                generatedAbsoluteIndex: 2523,
                                generatedLineIndex: 91,
                                characterOffsetIndex: 43,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 296,
                                documentLineIndex: 8,
                                generatedAbsoluteIndex: 2669,
                                generatedLineIndex: 96,
                                characterOffsetIndex: 55,
                                contentLength: 10),
                            BuildLineMapping(
                                documentAbsoluteIndex: 307,
                                documentLineIndex: 8,
                                generatedAbsoluteIndex: 2825,
                                generatedLineIndex: 102,
                                characterOffsetIndex: 66,
                                contentLength: 5),
                            BuildLineMapping(
                                documentAbsoluteIndex: 312,
                                documentLineIndex: 8,
                                generatedAbsoluteIndex: 2980,
                                generatedLineIndex: 107,
                                characterOffsetIndex: 71,
                                contentLength: 2),
                            BuildLineMapping(
                                documentAbsoluteIndex: 316,
                                documentLineIndex: 8,
                                generatedAbsoluteIndex: 3137,
                                generatedLineIndex: 113,
                                characterOffsetIndex: 75,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 348,
                                documentLineIndex: 9,
                                generatedAbsoluteIndex: 3298,
                                generatedLineIndex: 119,
                                characterOffsetIndex: 17,
                                contentLength: 13),
                            BuildLineMapping(
                                documentAbsoluteIndex: 363,
                                documentLineIndex: 9,
                                documentCharacterOffsetIndex: 32,
                                generatedAbsoluteIndex: 3422,
                                generatedLineIndex: 124,
                                generatedCharacterOffsetIndex: 31,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 376,
                                documentLineIndex: 9,
                                generatedAbsoluteIndex: 3560,
                                generatedLineIndex: 130,
                                characterOffsetIndex: 45,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 388,
                                documentLineIndex: 9,
                                generatedAbsoluteIndex: 3709,
                                generatedLineIndex: 135,
                                characterOffsetIndex: 57,
                                contentLength: 10),
                            BuildLineMapping(
                                documentAbsoluteIndex: 399,
                                documentLineIndex: 9,
                                generatedAbsoluteIndex: 3868,
                                generatedLineIndex: 141,
                                characterOffsetIndex: 68,
                                contentLength: 5),
                            BuildLineMapping(
                                documentAbsoluteIndex: 404,
                                documentLineIndex: 9,
                                generatedAbsoluteIndex: 4026,
                                generatedLineIndex: 146,
                                characterOffsetIndex: 73,
                                contentLength: 2),
                            BuildLineMapping(
                                documentAbsoluteIndex: 408,
                                documentLineIndex: 9,
                                generatedAbsoluteIndex: 4186,
                                generatedLineIndex: 152,
                                characterOffsetIndex: 77,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 445,
                                documentLineIndex: 11,
                                generatedAbsoluteIndex: 4370,
                                generatedLineIndex: 158,
                                characterOffsetIndex: 17,
                                contentLength: 13),
                            BuildLineMapping(
                                documentAbsoluteIndex: 460,
                                documentLineIndex: 11,
                                generatedAbsoluteIndex: 4495,
                                generatedLineIndex: 163,
                                characterOffsetIndex: 32,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 492,
                                documentLineIndex: 11,
                                generatedAbsoluteIndex: 4651,
                                generatedLineIndex: 168,
                                characterOffsetIndex: 64,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 529,
                                documentLineIndex: 13,
                                documentCharacterOffsetIndex: 17,
                                generatedAbsoluteIndex: 4834,
                                generatedLineIndex: 174,
                                generatedCharacterOffsetIndex: 16,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 542,
                                documentLineIndex: 13,
                                generatedAbsoluteIndex: 4957,
                                generatedLineIndex: 180,
                                characterOffsetIndex: 30,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 554,
                                documentLineIndex: 13,
                                generatedAbsoluteIndex: 5091,
                                generatedLineIndex: 185,
                                characterOffsetIndex: 42,
                                contentLength: 10),
                            BuildLineMapping(
                                documentAbsoluteIndex: 565,
                                documentLineIndex: 13,
                                generatedAbsoluteIndex: 5235,
                                generatedLineIndex: 191,
                                characterOffsetIndex: 53,
                                contentLength: 5),
                            BuildLineMapping(
                                documentAbsoluteIndex: 570,
                                documentLineIndex: 13,
                                generatedAbsoluteIndex: 5378,
                                generatedLineIndex: 196,
                                characterOffsetIndex: 58,
                                contentLength: 2),
                        }
                    },
                    {
                        "TransitionsInTagHelperAttributes",
                        "TransitionsInTagHelperAttributes.DesignTime",
                        DefaultPAndInputTagHelperDescriptors,
                        new[]
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 455,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 17),
                            BuildLineMapping(
                                documentAbsoluteIndex: 35,
                                documentLineIndex: 1,
                                generatedAbsoluteIndex: 901,
                                generatedLineIndex: 32,
                                characterOffsetIndex: 2,
                                contentLength: 59),
                            BuildLineMapping(
                                documentAbsoluteIndex: 122,
                                documentLineIndex: 6,
                                generatedAbsoluteIndex: 1134,
                                generatedLineIndex: 41,
                                characterOffsetIndex: 23,
                                contentLength: 4),
                            BuildLineMapping(
                                documentAbsoluteIndex: 157,
                                documentLineIndex: 7,
                                generatedAbsoluteIndex: 1302,
                                generatedLineIndex: 47,
                                characterOffsetIndex: 12,
                                contentLength: 6),
                            BuildLineMapping(
                                documentAbsoluteIndex: 171,
                                documentLineIndex: 7,
                                generatedAbsoluteIndex: 1419,
                                generatedLineIndex: 52,
                                characterOffsetIndex: 26,
                                contentLength: 2),
                            BuildLineMapping(
                                documentAbsoluteIndex: 202,
                                documentLineIndex: 8,
                                generatedAbsoluteIndex: 1594,
                                generatedLineIndex: 58,
                                characterOffsetIndex: 21,
                                contentLength: 5),
                            BuildLineMapping(
                                documentAbsoluteIndex: 207,
                                documentLineIndex: 8,
                                generatedAbsoluteIndex: 1599,
                                generatedLineIndex: 58,
                                characterOffsetIndex: 26,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 208,
                                documentLineIndex: 8,
                                generatedAbsoluteIndex: 1600,
                                generatedLineIndex: 58,
                                characterOffsetIndex: 27,
                                contentLength: 3),
                            BuildLineMapping(
                                documentAbsoluteIndex: 241,
                                documentLineIndex: 9,
                                documentCharacterOffsetIndex: 22,
                                generatedAbsoluteIndex: 1777,
                                generatedLineIndex: 64,
                                generatedCharacterOffsetIndex: 21,
                                contentLength: 3),
                            BuildLineMapping(
                                documentAbsoluteIndex: 274,
                                documentLineIndex: 10,
                                documentCharacterOffsetIndex: 22,
                                generatedAbsoluteIndex: 1954,
                                generatedLineIndex: 70,
                                generatedCharacterOffsetIndex: 21,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 275,
                                documentLineIndex: 10,
                                documentCharacterOffsetIndex: 23,
                                generatedAbsoluteIndex: 1955,
                                generatedLineIndex: 70,
                                generatedCharacterOffsetIndex: 22,
                                contentLength: 4),
                            BuildLineMapping(
                                documentAbsoluteIndex: 279,
                                documentLineIndex: 10,
                                documentCharacterOffsetIndex: 27,
                                generatedAbsoluteIndex: 1959,
                                generatedLineIndex: 70,
                                generatedCharacterOffsetIndex: 26,
                                contentLength: 1),
                            BuildLineMapping(
                                documentAbsoluteIndex: 307,
                                documentLineIndex: 11,
                                generatedAbsoluteIndex: 2132,
                                generatedLineIndex: 76,
                                characterOffsetIndex: 19,
                                contentLength: 6),
                            BuildLineMapping(
                                documentAbsoluteIndex: 321,
                                documentLineIndex: 11,
                                generatedAbsoluteIndex: 2257,
                                generatedLineIndex: 81,
                                characterOffsetIndex: 33,
                                contentLength: 4),
                            BuildLineMapping(
                                documentAbsoluteIndex: 325,
                                documentLineIndex: 11,
                                generatedAbsoluteIndex: 2261,
                                generatedLineIndex: 81,
                                characterOffsetIndex: 37,
                                contentLength: 2),
                            BuildLineMapping(
                                documentAbsoluteIndex: 327,
                                documentLineIndex: 11,
                                generatedAbsoluteIndex: 2263,
                                generatedLineIndex: 81,
                                characterOffsetIndex: 39,
                                contentLength: 8),
                            BuildLineMapping(
                                documentAbsoluteIndex: 335,
                                documentLineIndex: 11,
                                generatedAbsoluteIndex: 2271,
                                generatedLineIndex: 81,
                                characterOffsetIndex: 47,
                                contentLength: 1),
                        }
                    },
                    {
                        "NestedScriptTagTagHelpers",
                        "NestedScriptTagTagHelpers.DesignTime",
                        DefaultPAndInputTagHelperDescriptors,
                        new[]
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 441,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 17),
                            BuildLineMapping(
                                documentAbsoluteIndex: 182,
                                documentLineIndex: 5,
                                generatedAbsoluteIndex: 1003,
                                generatedLineIndex: 34,
                                characterOffsetIndex: 0,
                                contentLength: 12),
                            BuildLineMapping(
                                documentAbsoluteIndex: 195,
                                documentLineIndex: 5,
                                documentCharacterOffsetIndex: 13,
                                generatedAbsoluteIndex: 1106,
                                generatedLineIndex: 40,
                                generatedCharacterOffsetIndex: 12,
                                contentLength: 30),
                            BuildLineMapping(
                                documentAbsoluteIndex: 339,
                                documentLineIndex: 7,
                                generatedAbsoluteIndex: 1415,
                                generatedLineIndex: 48,
                                characterOffsetIndex: 50,
                                contentLength: 23),
                            BuildLineMapping(
                                documentAbsoluteIndex: 389,
                                documentLineIndex: 7,
                                generatedAbsoluteIndex: 1722,
                                generatedLineIndex: 55,
                                characterOffsetIndex: 100,
                                contentLength: 4),
                            BuildLineMapping(
                                documentAbsoluteIndex: 424,
                                documentLineIndex: 9,
                                generatedAbsoluteIndex: 1805,
                                generatedLineIndex: 60,
                                characterOffsetIndex: 0,
                                contentLength: 15),
                        }
                    },
                    {
                        "SymbolBoundAttributes",
                        "SymbolBoundAttributes.DesignTime",
                        SymbolBoundTagHelperDescriptors,
                        new[]
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 433,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 9),
                            BuildLineMapping(
                                documentAbsoluteIndex: 296,
                                documentLineIndex: 11,
                                documentCharacterOffsetIndex: 18,
                                generatedAbsoluteIndex: 975,
                                generatedLineIndex: 33,
                                generatedCharacterOffsetIndex: 32,
                                contentLength: 5),
                            BuildLineMapping(
                                documentAbsoluteIndex: 345,
                                documentLineIndex: 12,
                                documentCharacterOffsetIndex: 20,
                                generatedAbsoluteIndex: 1169,
                                generatedLineIndex: 39,
                                generatedCharacterOffsetIndex: 33,
                                contentLength: 5),
                            BuildLineMapping(
                                documentAbsoluteIndex: 399,
                                documentLineIndex: 13,
                                documentCharacterOffsetIndex: 23,
                                generatedAbsoluteIndex: 1359,
                                generatedLineIndex: 45,
                                generatedCharacterOffsetIndex: 29,
                                contentLength: 13),
                            BuildLineMapping(
                                documentAbsoluteIndex: 481,
                                documentLineIndex: 14,
                                documentCharacterOffsetIndex: 24,
                                generatedAbsoluteIndex: 1557,
                                generatedLineIndex: 51,
                                generatedCharacterOffsetIndex: 29,
                                contentLength: 13),
                        }
                    },
                    {
                        "EnumTagHelpers",
                        "EnumTagHelpers.DesignTime",
                        EnumTagHelperDescriptors,
                        new[]
                        {
                            BuildLineMapping(
                                documentAbsoluteIndex: 14,
                                documentLineIndex: 0,
                                generatedAbsoluteIndex: 419,
                                generatedLineIndex: 14,
                                characterOffsetIndex: 14,
                                contentLength: 17),
                            BuildLineMapping(
                                documentAbsoluteIndex: 37,
                                documentLineIndex: 2,
                                generatedAbsoluteIndex: 908,
                                generatedLineIndex: 33,
                                characterOffsetIndex: 2,
                                contentLength: 39),
                            BuildLineMapping(
                                documentAbsoluteIndex: 96,
                                documentLineIndex: 6,
                                documentCharacterOffsetIndex: 15,
                                generatedAbsoluteIndex: 1194,
                                generatedLineIndex: 42,
                                generatedCharacterOffsetIndex: 25,
                                contentLength: 14),
                            BuildLineMapping(
                                documentAbsoluteIndex: 131,
                                documentLineIndex: 7,
                                generatedAbsoluteIndex: 1446,
                                generatedLineIndex: 49,
                                characterOffsetIndex: 15,
                                contentLength: 20),
                            BuildLineMapping(
                                documentAbsoluteIndex: 171,
                                documentLineIndex: 8,
                                documentCharacterOffsetIndex: 14,
                                generatedAbsoluteIndex: 1759,
                                generatedLineIndex: 56,
                                generatedCharacterOffsetIndex: 70,
                                contentLength: 7),
                            BuildLineMapping(
                                documentAbsoluteIndex: 198,
                                documentLineIndex: 9,
                                documentCharacterOffsetIndex: 14,
                                generatedAbsoluteIndex: 2060,
                                generatedLineIndex: 63,
                                generatedCharacterOffsetIndex: 70,
                                contentLength: 13),
                            BuildLineMapping(
                                documentAbsoluteIndex: 224,
                                documentLineIndex: 9,
                                documentCharacterOffsetIndex: 40,
                                generatedAbsoluteIndex: 2226,
                                generatedLineIndex: 68,
                                generatedCharacterOffsetIndex: 85,
                                contentLength: 7),
                            BuildLineMapping(
                                documentAbsoluteIndex: 251,
                                documentLineIndex: 10,
                                documentCharacterOffsetIndex: 15,
                                generatedAbsoluteIndex: 2482,
                                generatedLineIndex: 75,
                                generatedCharacterOffsetIndex: 25,
                                contentLength: 9),
                            BuildLineMapping(
                                documentAbsoluteIndex: 274,
                                documentLineIndex: 10,
                                documentCharacterOffsetIndex: 38,
                                generatedAbsoluteIndex: 2596,
                                generatedLineIndex: 80,
                                generatedCharacterOffsetIndex: 37,
                                contentLength: 9),
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(DesignTimeTagHelperTestData))]
        public void TagHelpers_GenerateExpectedDesignTimeOutput(
            string testName,
            string baseLineName,
            IEnumerable<TagHelperDescriptor> tagHelperDescriptors,
            IList<LineMapping> expectedDesignTimePragmas)
        {
            // Act & Assert
            RunTagHelperTest(testName,
                             baseLineName,
                             designTimeMode: true,
                             tagHelperDescriptors: tagHelperDescriptors,
                             expectedDesignTimePragmas: expectedDesignTimePragmas);
        }

        public static TheoryData RuntimeTimeTagHelperTestData
        {
            get
            {
                // Test resource name, expected TagHelperDescriptors
                // Note: The baseline resource name is equivalent to the test resource name.
                return new TheoryData<string, string, IEnumerable<TagHelperDescriptor>>
                {
                    { "IncompleteTagHelper", null, DefaultPAndInputTagHelperDescriptors },
                    { "SingleTagHelper", null, DefaultPAndInputTagHelperDescriptors },
                    { "SingleTagHelperWithNewlineBeforeAttributes", null, DefaultPAndInputTagHelperDescriptors },
                    { "TagHelpersWithWeirdlySpacedAttributes", null, DefaultPAndInputTagHelperDescriptors },
                    { "BasicTagHelpers", null, DefaultPAndInputTagHelperDescriptors },
                    { "BasicTagHelpers.RemoveTagHelper", null, DefaultPAndInputTagHelperDescriptors },
                    { "BasicTagHelpers.Prefixed", null, PrefixedPAndInputTagHelperDescriptors },
                    { "ComplexTagHelpers", null, DefaultPAndInputTagHelperDescriptors },
                    { "DuplicateTargetTagHelper", null, DuplicateTargetTagHelperDescriptors },
                    { "EmptyAttributeTagHelpers", null, DefaultPAndInputTagHelperDescriptors },
                    { "EscapedTagHelpers", null, DefaultPAndInputTagHelperDescriptors },
                    { "AttributeTargetingTagHelpers", null, AttributeTargetingTagHelperDescriptors },
                    { "PrefixedAttributeTagHelpers", null, PrefixedAttributeTagHelperDescriptors },
                    {
                        "PrefixedAttributeTagHelpers",
                        "PrefixedAttributeTagHelpers.Reversed",
                        PrefixedAttributeTagHelperDescriptors.Reverse()
                    },
                    { "DuplicateAttributeTagHelpers", null, DefaultPAndInputTagHelperDescriptors },
                    { "DynamicAttributeTagHelpers", null, DynamicAttributeTagHelpers_Descriptors },
                    { "TransitionsInTagHelperAttributes", null, DefaultPAndInputTagHelperDescriptors },
                    { "NestedScriptTagTagHelpers", null, DefaultPAndInputTagHelperDescriptors },
                    { "SymbolBoundAttributes", null, SymbolBoundTagHelperDescriptors },
                    { "EnumTagHelpers", null, EnumTagHelperDescriptors },
                };
            }
        }

        [Theory]
        [MemberData(nameof(RuntimeTimeTagHelperTestData))]
        public void TagHelpers_GenerateExpectedRuntimeOutput(
            string testName,
            string baseLineName,
            IEnumerable<TagHelperDescriptor> tagHelperDescriptors)
        {
            // Arrange & Act & Assert
            RunTagHelperTest(testName, baseLineName, tagHelperDescriptors: tagHelperDescriptors);
        }

        [Fact]
        public void CSharpChunkGenerator_CorrectlyGeneratesMappings_ForRemoveTagHelperDirective()
        {
            // Act & Assert
            RunTagHelperTest("RemoveTagHelperDirective",
                             designTimeMode: true,
                             expectedDesignTimePragmas: new List<LineMapping>()
                             {
                                    BuildLineMapping(documentAbsoluteIndex: 17,
                                                     documentLineIndex: 0,
                                                     generatedAbsoluteIndex: 442,
                                                     generatedLineIndex: 14,
                                                     characterOffsetIndex: 17,
                                                     contentLength: 17)
                             });
        }

        [Fact]
        public void CSharpChunkGenerator_CorrectlyGeneratesMappings_ForAddTagHelperDirective()
        {
            // Act & Assert
            RunTagHelperTest("AddTagHelperDirective",
                             designTimeMode: true,
                             expectedDesignTimePragmas: new List<LineMapping>()
                             {
                                    BuildLineMapping(documentAbsoluteIndex: 14,
                                                     documentLineIndex: 0,
                                                     generatedAbsoluteIndex: 433,
                                                     generatedLineIndex: 14,
                                                     characterOffsetIndex: 14,
                                                     contentLength: 17)
                             });
        }

        [Fact]
        public void TagHelpers_Directive_GenerateDesignTimeMappings()
        {
            // Act & Assert
            RunTagHelperTest("AddTagHelperDirective",
                             designTimeMode: true,
                             tagHelperDescriptors: new[]
                             {
                                 new TagHelperDescriptor
                                 {
                                     TagName = "p",
                                     TypeName = "pTagHelper",
                                     AssemblyName = "SomeAssembly"
                                 }
                             });
        }

        [Fact]
        public void TagHelpers_WithinHelpersAndSections_GeneratesExpectedOutput()
        {
            // Arrange
            var propertyInfo = typeof(TestType).GetProperty("BoundProperty");
            var tagHelperDescriptors = new TagHelperDescriptor[]
            {
                new TagHelperDescriptor
                {
                    TagName = "MyTagHelper",
                    TypeName = "MyTagHelper",
                    AssemblyName = "SomeAssembly",
                    Attributes = new []
                    {
                        new TagHelperAttributeDescriptor("BoundProperty", propertyInfo)
                    }
                },
                new TagHelperDescriptor
                {
                    TagName = "NestedTagHelper",
                    TypeName = "NestedTagHelper",
                    AssemblyName = "SomeAssembly"
                }
            };

            // Act & Assert
            RunTagHelperTest("TagHelpersInSection", tagHelperDescriptors: tagHelperDescriptors);
        }

        private static IEnumerable<TagHelperDescriptor> BuildPAndInputTagHelperDescriptors(string prefix)
        {
            var pAgePropertyInfo = typeof(TestType).GetProperty("Age");
            var inputTypePropertyInfo = typeof(TestType).GetProperty("Type");
            var checkedPropertyInfo = typeof(TestType).GetProperty("Checked");

            return new[]
            {
                new TagHelperDescriptor
                {
                    Prefix = prefix,
                    TagName = "p",
                    TypeName = "PTagHelper",
                    AssemblyName = "SomeAssembly",
                    Attributes = new TagHelperAttributeDescriptor[]
                    {
                        new TagHelperAttributeDescriptor("age", pAgePropertyInfo)
                    },
                    TagStructure = TagStructure.NormalOrSelfClosing
                },
                new TagHelperDescriptor
                {
                    Prefix = prefix,
                    TagName = "input",
                    TypeName = "InputTagHelper",
                    AssemblyName = "SomeAssembly",
                    Attributes = new TagHelperAttributeDescriptor[]
                    {
                        new TagHelperAttributeDescriptor("type", inputTypePropertyInfo)
                    },
                    TagStructure = TagStructure.WithoutEndTag
                },
                new TagHelperDescriptor
                {
                    Prefix = prefix,
                    TagName = "input",
                    TypeName = "InputTagHelper2",
                    AssemblyName = "SomeAssembly",
                    Attributes = new TagHelperAttributeDescriptor[]
                    {
                        new TagHelperAttributeDescriptor("type", inputTypePropertyInfo),
                        new TagHelperAttributeDescriptor("checked", checkedPropertyInfo)
                    },
                }
            };
        }

        private class TestType
        {
            public int Age { get; set; }

            public string Type { get; set; }

            public bool Checked { get; set; }

            public string BoundProperty { get; set; }
        }
    }

    public enum MyEnum
    {
        MyValue,
        MySecondValue
    }
}
