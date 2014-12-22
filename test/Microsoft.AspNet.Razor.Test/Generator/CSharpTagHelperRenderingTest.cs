// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
#if ASPNETCORE50
using System.Reflection;
#endif
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Generator
{
    public class CSharpTagHelperRenderingTest : TagHelperTestBase
    {
        private static IEnumerable<TagHelperDescriptor> PAndInputTagHelperDescriptors
        {
            get
            {
                var pAgePropertyInfo = typeof(TestType).GetProperty("Age");
                var inputTypePropertyInfo = typeof(TestType).GetProperty("Type");
                var checkedPropertyInfo = typeof(TestType).GetProperty("Checked");
                return new[]
                {
                    new TagHelperDescriptor("p",
                                            "PTagHelper",
                                            "SomeAssembly",
                                            new [] {
                                                new TagHelperAttributeDescriptor("age", pAgePropertyInfo)
                                            }),
                    new TagHelperDescriptor("input",
                                            "InputTagHelper",
                                            "SomeAssembly",
                                            new TagHelperAttributeDescriptor[] {
                                                new TagHelperAttributeDescriptor("type", inputTypePropertyInfo)
                                            }),
                    new TagHelperDescriptor("input",
                                            "InputTagHelper2",
                                            "SomeAssembly",
                                            new TagHelperAttributeDescriptor[] {
                                                new TagHelperAttributeDescriptor("type", inputTypePropertyInfo),
                                                new TagHelperAttributeDescriptor("checked", checkedPropertyInfo)
                                            })
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
                        PAndInputTagHelperDescriptors,
                        PAndInputTagHelperDescriptors,
                        false
                    },
                    {
                        "SingleTagHelper",
                        "SingleTagHelper.DesignTime",
                        PAndInputTagHelperDescriptors,
                        PAndInputTagHelperDescriptors,
                        true
                    },
                    {
                        "BasicTagHelpers",
                        "BasicTagHelpers",
                        PAndInputTagHelperDescriptors,
                        PAndInputTagHelperDescriptors,
                        false
                    },
                    {
                        "BasicTagHelpers",
                        "BasicTagHelpers.DesignTime",
                        PAndInputTagHelperDescriptors,
                        PAndInputTagHelperDescriptors,
                        true
                    },
                    {
                        "BasicTagHelpers.RemoveTagHelper",
                        "BasicTagHelpers.RemoveTagHelper",
                        PAndInputTagHelperDescriptors,
                        Enumerable.Empty<TagHelperDescriptor>(),
                        false
                    },
                    {
                        "ComplexTagHelpers",
                        "ComplexTagHelpers",
                        PAndInputTagHelperDescriptors,
                        PAndInputTagHelperDescriptors,
                        false
                    },
                    {
                        "ComplexTagHelpers",
                        "ComplexTagHelpers.DesignTime",
                        PAndInputTagHelperDescriptors,
                        PAndInputTagHelperDescriptors,
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
                return new TheoryData<string, string, IEnumerable<TagHelperDescriptor>, List<LineMapping>>
                {
                    {
                        "SingleTagHelper",
                        "SingleTagHelper.DesignTime",
                        PAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(documentAbsoluteIndex: 14,
                                             documentLineIndex: 0,
                                             generatedAbsoluteIndex: 475,
                                             generatedLineIndex: 15,
                                             characterOffsetIndex: 14,
                                             contentLength: 11),
                            BuildLineMapping(documentAbsoluteIndex: 57,
                                             documentLineIndex: 2,
                                             documentCharacterOffsetIndex: 28,
                                             generatedAbsoluteIndex: 927,
                                             generatedLineIndex: 33,
                                             generatedCharacterOffsetIndex: 31,
                                             contentLength: 4)
                        }
                    },
                    {
                        "BasicTagHelpers",
                        "BasicTagHelpers.DesignTime",
                        PAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(documentAbsoluteIndex: 14,
                                             documentLineIndex: 0,
                                             generatedAbsoluteIndex: 475,
                                             generatedLineIndex: 15,
                                             characterOffsetIndex: 14,
                                             contentLength: 11),
                            BuildLineMapping(documentAbsoluteIndex: 189,
                                             documentLineIndex: 6,
                                             documentCharacterOffsetIndex: 40,
                                             generatedAbsoluteIndex: 1540,
                                             generatedLineIndex: 43,
                                             generatedCharacterOffsetIndex: 40,
                                             contentLength: 4)
                        }
                    },
                    {
                        "ComplexTagHelpers",
                        "ComplexTagHelpers.DesignTime",
                        PAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(14, 0, 479, 15, 14, 11),
                            BuildLineMapping(30, 2, 1, 995, 35, 0, 48),
                            BuildLineMapping(205, 9, 1113, 44, 0, 12),
                            BuildLineMapping(218, 9, 13, 1209, 50, 12, 27),
                            BuildLineMapping(346, 12, 1607, 62, 0, 48),
                            BuildLineMapping(440, 15, 46, 1798, 71, 6, 8),
                            BuildLineMapping(457, 15, 63, 2061, 78, 40, 4),
                            BuildLineMapping(501, 16, 31, 2237, 82, 6, 30),
                            BuildLineMapping(568, 17, 30, 2586, 91, 0, 10),
                            BuildLineMapping(601, 17, 63, 2668, 97, 0, 8),
                            BuildLineMapping(632, 17, 94, 2748, 103, 0, 1),
                            BuildLineMapping(639, 18, 3002, 112, 0, 15),
                            BuildLineMapping(157, 7, 32, 3151, 119, 6, 12),
                            BuildLineMapping(680, 21, 3234, 124, 0, 1)
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(DesignTimeTagHelperTestData))]
        public void TagHelpers_GenerateExpectedDesignTimeOutput(string testName,
                                                                string baseLineName,
                                                                IEnumerable<TagHelperDescriptor> tagHelperDescriptors,
                                                                List<LineMapping> expectedDesignTimePragmas)
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
                return new TheoryData<string, IEnumerable<TagHelperDescriptor>>
                {
                    { "SingleTagHelper", PAndInputTagHelperDescriptors },
                    { "BasicTagHelpers", PAndInputTagHelperDescriptors },
                    { "BasicTagHelpers.RemoveTagHelper", PAndInputTagHelperDescriptors },
                    { "ComplexTagHelpers", PAndInputTagHelperDescriptors },
                };
            }
        }

        [Theory]
        [MemberData(nameof(RuntimeTimeTagHelperTestData))]
        public void TagHelpers_GenerateExpectedRuntimeOutput(string testName,
                                                             IEnumerable<TagHelperDescriptor> tagHelperDescriptors)
        {
            // Act & Assert
            RunTagHelperTest(testName, tagHelperDescriptors: tagHelperDescriptors);
        }

        [Fact]
        public void CSharpCodeGenerator_CorrectlyGeneratesMappings_ForRemoveTagHelperDirective()
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
                                                     contentLength: 11)
                             });
        }

        [Fact]
        public void CSharpCodeGenerator_CorrectlyGeneratesMappings_ForAddTagHelperDirective()
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
                                                     contentLength: 11)
                             });
        }

        [Fact]
        public void TagHelpers_Directive_GenerateDesignTimeMappings()
        {
            // Act & Assert
            RunTagHelperTest("AddTagHelperDirective",
                             designTimeMode: true,
                             tagHelperDescriptors: new[] {
                                new TagHelperDescriptor("p", "pTagHelper", "SomeAssembly")
                             });
        }

        [Theory]
        [InlineData("TagHelpersInSection")]
        [InlineData("TagHelpersInHelper")]
        public void TagHelpers_WithinHelpersAndSections_GeneratesExpectedOutput(string testType)
        {
            // Arrange
            var propertyInfo = typeof(TestType).GetProperty("BoundProperty");
            var tagHelperDescriptors = new TagHelperDescriptor[]
            {
                new TagHelperDescriptor("MyTagHelper",
                                        "MyTagHelper",
                                        "SomeAssembly",
                                        new [] {
                                            new TagHelperAttributeDescriptor("BoundProperty",
                                                                             propertyInfo)
                                        }),
                new TagHelperDescriptor("NestedTagHelper", "NestedTagHelper", "SomeAssembly")
            };

            // Act & Assert
            RunTagHelperTest(testType, tagHelperDescriptors: tagHelperDescriptors);
        }

        private class TestType
        {
            public int Age { get; set; }

            public string Type { get; set; }

            public bool Checked { get; set; }

            public string BoundProperty { get; set; }
        }
    }
}