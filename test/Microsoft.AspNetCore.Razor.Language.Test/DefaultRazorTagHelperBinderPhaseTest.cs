﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language
{
    public class DefaultRazorTagHelperBinderPhaseTest
    {
        [Fact]
        public void Execute_CanHandleSingleLengthAddTagHelperDirective()
        {
            // Arrange
            var engine = RazorEngine.Create(builder =>
            {
                builder.AddTagHelpers(new TagHelperDescriptor[0]);
            });

            var phase = new DefaultRazorTagHelperBinderPhase()
            {
                Engine = engine,
            };
            var expectedDiagnostics = new[]
            {
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.ParseError_Unterminated_String_Literal,
                        new SourceLocation(14 + Environment.NewLine.Length, 1, 14),
                        length: 1)),
                RazorDiagnostic.Create(
                    new RazorError(
                        Resources.FormatInvalidTagHelperLookupText("\""),
                        new SourceLocation(14 + Environment.NewLine.Length, 1, 14),
                        length: 1))
            };


            var content =
            @"
@addTagHelper """;
            var sourceDocument = TestRazorSourceDocument.Create(content, fileName: null);
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var originalTree = RazorSyntaxTree.Parse(sourceDocument);
            codeDocument.SetSyntaxTree(originalTree);

            // Act
            phase.Execute(codeDocument);

            // Assert
            var rewrittenTree = codeDocument.GetSyntaxTree();
            var directiveValue = rewrittenTree.Root.Children.OfType<Block>().First().Children.Last() as Span;
            var chunkGenerator = Assert.IsType<AddTagHelperChunkGenerator>(directiveValue.ChunkGenerator);
            Assert.Equal(expectedDiagnostics, chunkGenerator.Diagnostics);
        }

        [Fact]
        public void Execute_CanHandleSingleLengthRemoveTagHelperDirective()
        {
            // Arrange
            var engine = RazorEngine.Create(builder =>
            {
                builder.AddTagHelpers(new TagHelperDescriptor[0]);
            });

            var phase = new DefaultRazorTagHelperBinderPhase()
            {
                Engine = engine,
            };
            var expectedDiagnostics = new[]
            {
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.ParseError_Unterminated_String_Literal,
                        new SourceLocation(17 + Environment.NewLine.Length, 1, 17),
                        length: 1)),
                RazorDiagnostic.Create(
                    new RazorError(
                        Resources.FormatInvalidTagHelperLookupText("\""),
                        new SourceLocation(17 + Environment.NewLine.Length, 1, 17),
                        length: 1))
            };


            var content =
            @"
@removeTagHelper """;
            var sourceDocument = TestRazorSourceDocument.Create(content, fileName: null);
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var originalTree = RazorSyntaxTree.Parse(sourceDocument);
            codeDocument.SetSyntaxTree(originalTree);

            // Act
            phase.Execute(codeDocument);

            // Assert
            var rewrittenTree = codeDocument.GetSyntaxTree();
            var directiveValue = rewrittenTree.Root.Children.OfType<Block>().First().Children.Last() as Span;
            var chunkGenerator = Assert.IsType<RemoveTagHelperChunkGenerator>(directiveValue.ChunkGenerator);
            Assert.Equal(expectedDiagnostics, chunkGenerator.Diagnostics);
        }

        [Fact]
        public void Execute_CanHandleSingleLengthTagHelperPrefix()
        {
            // Arrange
            var engine = RazorEngine.Create(builder =>
            {
                builder.AddTagHelpers(new TagHelperDescriptor[0]);
            });

            var phase = new DefaultRazorTagHelperBinderPhase()
            {
                Engine = engine,
            };
            var expectedDiagnostics = new[]
            {
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.ParseError_Unterminated_String_Literal,
                        new SourceLocation(17 + Environment.NewLine.Length, 1, 17),
                        length: 1)),
                RazorDiagnostic.Create(
                    new RazorError(
                        Resources.FormatInvalidTagHelperPrefixValue("tagHelperPrefix", "\"", "\""),
                        new SourceLocation(17 + Environment.NewLine.Length, 1, 17),
                        length: 1))
            };


            var content =
            @"
@tagHelperPrefix """;
            var sourceDocument = TestRazorSourceDocument.Create(content, fileName: null);
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var originalTree = RazorSyntaxTree.Parse(sourceDocument);
            codeDocument.SetSyntaxTree(originalTree);

            // Act
            phase.Execute(codeDocument);

            // Assert
            var rewrittenTree = codeDocument.GetSyntaxTree();
            var directiveValue = rewrittenTree.Root.Children.OfType<Block>().First().Children.Last() as Span;
            var chunkGenerator = Assert.IsType<TagHelperPrefixDirectiveChunkGenerator>(directiveValue.ChunkGenerator);
            Assert.Equal(expectedDiagnostics, chunkGenerator.Diagnostics);
        }

        [Fact]
        public void Execute_RewritesTagHelpers()
        {
            // Arrange
            var engine = RazorEngine.Create(builder =>
            {
                builder.AddTagHelpers(new[]
                {
                    CreateTagHelperDescriptor(
                        tagName: "form",
                        typeName: null,
                        assemblyName: "TestAssembly"),
                    CreateTagHelperDescriptor(
                        tagName: "input",
                        typeName: null,
                        assemblyName: "TestAssembly"),
                });
            });

            var phase = new DefaultRazorTagHelperBinderPhase()
            {
                Engine = engine,
            };

            var sourceDocument = CreateTestSourceDocument();
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var originalTree = RazorSyntaxTree.Parse(sourceDocument);
            codeDocument.SetSyntaxTree(originalTree);

            // Act
            phase.Execute(codeDocument);

            // Assert
            var rewrittenTree = codeDocument.GetSyntaxTree();
            Assert.Empty(rewrittenTree.Diagnostics);
            Assert.Equal(3, rewrittenTree.Root.Children.Count);
            var formTagHelper = Assert.IsType<TagHelperBlock>(rewrittenTree.Root.Children[2]);
            Assert.Equal("form", formTagHelper.TagName);
            Assert.Equal(3, formTagHelper.Children.Count);
            var inputTagHelper = Assert.IsType<TagHelperBlock>(formTagHelper.Children[1]);
            Assert.Equal("input", inputTagHelper.TagName);
        }

        [Fact]
        public void Execute_DirectiveWithoutQuotes_RewritesTagHelpers_TagHelperMatchesElementTwice()
        {
            // Arrange
            var descriptor = CreateTagHelperDescriptor(
                tagName: "form",
                typeName: "TestFormTagHelper",
                assemblyName: "TestAssembly",
                ruleBuilders: new Action<TagMatchingRuleBuilder>[]
                {
                    ruleBuilder => ruleBuilder
                        .RequireAttribute(attribute => attribute
                            .Name("a")
                            .NameComparisonMode(RequiredAttributeDescriptor.NameComparisonMode.FullMatch)),
                    ruleBuilder => ruleBuilder
                        .RequireAttribute(attribute => attribute
                            .Name("b")
                            .NameComparisonMode(RequiredAttributeDescriptor.NameComparisonMode.FullMatch)),
                });

            var engine = RazorEngine.Create(builder =>
            {
                builder.AddTagHelpers(new[] { descriptor });
            });

            var phase = new DefaultRazorTagHelperBinderPhase()
            {
                Engine = engine,
            };

            var content = @"
@addTagHelper *, TestAssembly
<form a=""hi"" b=""there"">
</form>";

            var sourceDocument = TestRazorSourceDocument.Create(content);
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var originalTree = RazorSyntaxTree.Parse(sourceDocument);
            codeDocument.SetSyntaxTree(originalTree);

            // Act
            phase.Execute(codeDocument);

            // Assert
            var rewrittenTree = codeDocument.GetSyntaxTree();
            Assert.Empty(rewrittenTree.Diagnostics);
            Assert.Equal(3, rewrittenTree.Root.Children.Count);

            var formTagHelper = Assert.IsType<TagHelperBlock>(rewrittenTree.Root.Children[2]);
            Assert.Equal("form", formTagHelper.TagName);
            Assert.Equal(2, formTagHelper.Binding.GetBoundRules(descriptor).Count());
        }

        [Fact]
        public void Execute_DirectiveWithQuotes_RewritesTagHelpers_TagHelperMatchesElementTwice()
        {
            // Arrange
            var descriptor = CreateTagHelperDescriptor(
                tagName: "form",
                typeName: "TestFormTagHelper",
                assemblyName: "TestAssembly",
                ruleBuilders: new Action<TagMatchingRuleBuilder>[]
                {
                    ruleBuilder => ruleBuilder
                        .RequireAttribute(attribute => attribute
                            .Name("a")
                            .NameComparisonMode(RequiredAttributeDescriptor.NameComparisonMode.FullMatch)),
                    ruleBuilder => ruleBuilder
                        .RequireAttribute(attribute => attribute
                            .Name("b")
                            .NameComparisonMode(RequiredAttributeDescriptor.NameComparisonMode.FullMatch)),
                });

            var engine = RazorEngine.Create(builder =>
            {
                builder.AddTagHelpers(new[] { descriptor });
            });

            var phase = new DefaultRazorTagHelperBinderPhase()
            {
                Engine = engine,
            };

            var content = @"
@addTagHelper ""*, TestAssembly""
<form a=""hi"" b=""there"">
</form>";

            var sourceDocument = TestRazorSourceDocument.Create(content);
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var originalTree = RazorSyntaxTree.Parse(sourceDocument);
            codeDocument.SetSyntaxTree(originalTree);

            // Act
            phase.Execute(codeDocument);

            // Assert
            var rewrittenTree = codeDocument.GetSyntaxTree();
            Assert.Empty(rewrittenTree.Diagnostics);
            Assert.Equal(3, rewrittenTree.Root.Children.Count);

            var formTagHelper = Assert.IsType<TagHelperBlock>(rewrittenTree.Root.Children[2]);
            Assert.Equal("form", formTagHelper.TagName);
            Assert.Equal(2, formTagHelper.Binding.GetBoundRules(descriptor).Count());
        }

        [Fact]
        public void Execute_NoopsWhenNoTagHelperFeature()
        {
            // Arrange
            var engine = RazorEngine.Create();
            var phase = new DefaultRazorTagHelperBinderPhase()
            {
                Engine = engine,
            };
            var sourceDocument = CreateTestSourceDocument();
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var originalTree = RazorSyntaxTree.Parse(sourceDocument);
            codeDocument.SetSyntaxTree(originalTree);

            // Act
            phase.Execute(codeDocument);

            // Assert
            var outputTree = codeDocument.GetSyntaxTree();
            Assert.Empty(outputTree.Diagnostics);
            Assert.Same(originalTree, outputTree);
        }

        [Fact]
        public void Execute_NoopsWhenNoFeature()
        {
            // Arrange
            var engine = RazorEngine.Create(builder =>
            {
            });
            var phase = new DefaultRazorTagHelperBinderPhase()
            {
                Engine = engine,
            };
            var sourceDocument = CreateTestSourceDocument();
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var originalTree = RazorSyntaxTree.Parse(sourceDocument);
            codeDocument.SetSyntaxTree(originalTree);

            // Act
            phase.Execute(codeDocument);

            // Assert
            var outputTree = codeDocument.GetSyntaxTree();
            Assert.Empty(outputTree.Diagnostics);
            Assert.Same(originalTree, outputTree);
        }

        [Fact]
        public void Execute_NoopsWhenNoTagHelperDescriptorsAreResolved()
        {
            // Arrange
            var engine = RazorEngine.Create(builder =>
            {
                builder.Features.Add(new TestTagHelperFeature());
            });

            var phase = new DefaultRazorTagHelperBinderPhase()
            {
                Engine = engine,
            };

            // No taghelper directives here so nothing is resolved.
            var sourceDocument = TestRazorSourceDocument.Create("Hello, world");
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var originalTree = RazorSyntaxTree.Parse(sourceDocument);
            codeDocument.SetSyntaxTree(originalTree);

            // Act
            phase.Execute(codeDocument);

            // Assert
            var outputTree = codeDocument.GetSyntaxTree();
            Assert.Empty(outputTree.Diagnostics);
            Assert.Same(originalTree, outputTree);
        }

        [Fact]
        public void Execute_SetsTagHelperDocumentContext()
        {
            // Arrange
            var engine = RazorEngine.Create(builder =>
            {
                builder.Features.Add(new TestTagHelperFeature());
            });

            var phase = new DefaultRazorTagHelperBinderPhase()
            {
                Engine = engine,
            };

            // No taghelper directives here so nothing is resolved.
            var sourceDocument = TestRazorSourceDocument.Create("Hello, world");
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var originalTree = RazorSyntaxTree.Parse(sourceDocument);
            codeDocument.SetSyntaxTree(originalTree);

            // Act
            phase.Execute(codeDocument);

            // Assert
            var context = codeDocument.GetTagHelperContext();
            Assert.Null(context.Prefix);
            Assert.Empty(context.TagHelpers);
        }

        [Fact]
        public void Execute_CombinesErrorsOnRewritingErrors()
        {
            // Arrange
            var engine = RazorEngine.Create(builder =>
            {
                builder.AddTagHelpers(new[]
                {
                    CreateTagHelperDescriptor(
                        tagName: "form",
                        typeName: null,
                        assemblyName: "TestAssembly"),
                    CreateTagHelperDescriptor(
                        tagName: "input",
                        typeName: null,
                        assemblyName: "TestAssembly"),
                });
            });

            var phase = new DefaultRazorTagHelperBinderPhase()
            {
                Engine = engine,
            };

            var content =
            @"
@addTagHelper *, TestAssembly
<form>
    <input value='Hello' type='text' />";
            var sourceDocument = TestRazorSourceDocument.Create(content, fileName: null);
            var codeDocument = RazorCodeDocument.Create(sourceDocument);

            var originalTree = RazorSyntaxTree.Parse(sourceDocument);

            var initialError = RazorDiagnostic.Create(new RazorError("Initial test error", SourceLocation.Zero, length: 1));
            var expectedRewritingError = RazorDiagnostic.Create(
                new RazorError(
                    LegacyResources.FormatTagHelpersParseTreeRewriter_FoundMalformedTagHelper("form"),
                    new SourceLocation(Environment.NewLine.Length * 2 + 30, 2, 1),
                    length: 4));

            var erroredOriginalTree = RazorSyntaxTree.Create(originalTree.Root, originalTree.Source, new[] { initialError }, originalTree.Options);
            codeDocument.SetSyntaxTree(erroredOriginalTree);

            // Act
            phase.Execute(codeDocument);

            // Assert
            var outputTree = codeDocument.GetSyntaxTree();
            Assert.Empty(originalTree.Diagnostics);
            Assert.NotSame(erroredOriginalTree, outputTree);
            Assert.Equal(new[] { initialError, expectedRewritingError }, outputTree.Diagnostics);
        }

        [Theory]
        [InlineData("foo,assemblyName", 4)]
        [InlineData("foo, assemblyName", 5)]
        [InlineData("   foo, assemblyName", 8)]
        [InlineData("   foo   , assemblyName", 11)]
        [InlineData("foo,    assemblyName", 8)]
        [InlineData("   foo   ,    assemblyName   ", 14)]
        public void ParseAddOrRemoveDirective_CalculatesAssemblyLocationInLookupText(string text, int assemblyLocation)
        {
            // Arrange
            var phase = new DefaultRazorTagHelperBinderPhase();

            var directive = new TagHelperDirectiveDescriptor()
            {
                DirectiveText = text,
                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                Location = SourceLocation.Zero,
                Diagnostics = new List<RazorDiagnostic>(),
            };

            var expected = new SourceLocation(assemblyLocation, 0, assemblyLocation);

            // Act
            var result = phase.ParseAddOrRemoveDirective(directive);

            // Assert
            Assert.Empty(directive.Diagnostics);
            Assert.Equal("foo", result.TypePattern);
            Assert.Equal("assemblyName", result.AssemblyName);
            Assert.Equal(expected, result.AssemblyNameLocation);
        }

        public static TheoryData InvalidTagHelperPrefixData
        {
            get
            {
                var directiveLocation1 = new SourceLocation(1, 2, 3);
                var directiveLocation2 = new SourceLocation(4, 5, 6);

                var invalidTagHelperPrefixValueError =
                    "Invalid tag helper directive '{0}' value. '{1}' is not allowed in prefix '{2}'.";

                return new TheoryData<IEnumerable<TagHelperDirectiveDescriptor>, IEnumerable<RazorError>>
                {
                    {
                        new[]
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveText = "th ",
                                Location = directiveLocation1,
                                DirectiveType = TagHelperDirectiveType.TagHelperPrefix,
                                Diagnostics = new List<RazorDiagnostic>(),
                            },
                        },
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    ' ',
                                    "th "),
                                directiveLocation1,
                                length: 3)
                        }
                    },
                    {
                        new[]
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveText = "th\t",
                                Location = directiveLocation1,
                                DirectiveType = TagHelperDirectiveType.TagHelperPrefix,
                                Diagnostics = new List<RazorDiagnostic>(),
                            }
                        },
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    '\t',
                                    "th\t"),
                                directiveLocation1,
                                length: 3)
                        }
                    },
                    {
                        new[]
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveText = "th" + Environment.NewLine,
                                Location = directiveLocation1,
                                DirectiveType = TagHelperDirectiveType.TagHelperPrefix,
                                Diagnostics = new List<RazorDiagnostic>(),
                            }
                        },
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    Environment.NewLine[0],
                                    "th" + Environment.NewLine),
                                directiveLocation1,
                                length: 2 + Environment.NewLine.Length)
                        }
                    },
                    {
                        new[]
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveText = " th ",
                                Location = directiveLocation1,
                                DirectiveType = TagHelperDirectiveType.TagHelperPrefix,
                                Diagnostics = new List<RazorDiagnostic>(),
                            }
                        },
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    ' ',
                                    " th "),
                                directiveLocation1,
                                length: 4)
                        }
                    },
                    {
                        new[]
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveText = "@",
                                Location = directiveLocation1,
                                DirectiveType = TagHelperDirectiveType.TagHelperPrefix,
                                Diagnostics = new List<RazorDiagnostic>(),
                            }
                        },
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    '@',
                                    "@"),
                                directiveLocation1,
                                length: 1)
                        }
                    },
                    {
                        new[]
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveText = "t@h",
                                Location = directiveLocation1,
                                DirectiveType = TagHelperDirectiveType.TagHelperPrefix,
                                Diagnostics = new List<RazorDiagnostic>(),
                            }
                        },
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    '@',
                                    "t@h"),
                                directiveLocation1,
                                length: 3)
                        }
                    },
                    {
                        new[]
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveText = "!",
                                Location = directiveLocation1,
                                DirectiveType = TagHelperDirectiveType.TagHelperPrefix,
                                Diagnostics = new List<RazorDiagnostic>(),
                            }
                        },
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    '!',
                                    "!"),
                                directiveLocation1,
                                length: 1)
                        }
                    },
                    {
                        new[]
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveText = "!th",
                                Location = directiveLocation1,
                                DirectiveType = TagHelperDirectiveType.TagHelperPrefix,
                                Diagnostics = new List<RazorDiagnostic>(),
                            }
                        },
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    '!',
                                    "!th"),
                                directiveLocation1,
                                length: 3)
                        }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidTagHelperPrefixData))]
        public void IsValidTagHelperPrefix_ValidatesPrefix(
            object directives,
            object expectedErrors)
        {
            // Arrange
            var expectedDiagnostics = ((IEnumerable<RazorError>)expectedErrors).Select(RazorDiagnostic.Create);
            var tagHelperDirectives = (IEnumerable<TagHelperDirectiveDescriptor>)directives;
            var phase = new DefaultRazorTagHelperBinderPhase();

            // Act
            foreach (var directive in tagHelperDirectives)
            {
                Assert.False(phase.IsValidTagHelperPrefix(directive.DirectiveText, directive.Location, directive.Diagnostics));
            }

            // Assert
            Assert.Equal(expectedDiagnostics, tagHelperDirectives.SelectMany(directive => directive.Diagnostics));
        }

        private static string AssemblyA => "TestAssembly";

        private static string AssemblyB => "AnotherAssembly";

        private static TagHelperDescriptor Valid_PlainTagHelperDescriptor
        {
            get
            {
                return CreateTagHelperDescriptor(
                    tagName: "valid_plain",
                    typeName: "Microsoft.AspNetCore.Razor.TagHelpers.ValidPlainTagHelper",
                    assemblyName: AssemblyA);
            }
        }

        private static TagHelperDescriptor Valid_InheritedTagHelperDescriptor
        {
            get
            {
                return CreateTagHelperDescriptor(
                    tagName: "valid_inherited",
                    typeName: "Microsoft.AspNetCore.Razor.TagHelpers.ValidInheritedTagHelper",
                    assemblyName: AssemblyA);
            }
        }

        private static TagHelperDescriptor String_TagHelperDescriptor
        {
            get
            {
                // We're treating 'string' as a TagHelper so we can test TagHelpers in multiple assemblies without
                // building a separate assembly with a single TagHelper.
                return CreateTagHelperDescriptor(
                    tagName: "string",
                    typeName: "System.String",
                    assemblyName: AssemblyB);
            }
        }

        public static TheoryData ProcessTagHelperPrefixData
        {
            get
            {
                // directiveDescriptors, expected prefix
                return new TheoryData<IEnumerable<TagHelperDirectiveDescriptor>, string>
                {
                    {
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("", TagHelperDirectiveType.TagHelperPrefix),
                            CreateTagHelperDirectiveDescriptor(
                                "Microsoft.AspNetCore.Razor.TagHelpers.ValidPlain*, " + AssemblyA,
                                TagHelperDirectiveType.AddTagHelper),
                        },
                        null
                    },
                    {
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("th:", TagHelperDirectiveType.TagHelperPrefix),
                            CreateTagHelperDirectiveDescriptor(
                                "Microsoft.AspNetCore.Razor.TagHelpers.ValidPlain*, " + AssemblyA,
                                TagHelperDirectiveType.AddTagHelper),
                        },
                        "th:"
                    },
                    {
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("th:", TagHelperDirectiveType.TagHelperPrefix)
                        },
                        "th:"
                    },
                    {
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("th-", TagHelperDirectiveType.TagHelperPrefix),
                            CreateTagHelperDirectiveDescriptor(
                                "Microsoft.AspNetCore.Razor.TagHelpers.ValidPlain*, " + AssemblyA,
                                TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(
                                "Microsoft.AspNetCore.Razor.TagHelpers.ValidInherited*, " + AssemblyA,
                                TagHelperDirectiveType.AddTagHelper)
                        },
                        "th-"
                    },
                    {
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("", TagHelperDirectiveType.TagHelperPrefix),
                            CreateTagHelperDirectiveDescriptor(
                                "Microsoft.AspNetCore.Razor.TagHelpers.ValidPlain*, " + AssemblyA,
                                TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(
                                "Microsoft.AspNetCore.Razor.TagHelpers.ValidInherited*, " + AssemblyA,
                                TagHelperDirectiveType.AddTagHelper)
                        },
                        null
                    },
                    {
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("th", TagHelperDirectiveType.TagHelperPrefix),
                            CreateTagHelperDirectiveDescriptor(
                                "*, " + AssemblyA,
                                TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(
                                "*, " + AssemblyB,
                                TagHelperDirectiveType.AddTagHelper),
                        },
                        "th"
                    },
                    {
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor(
                                "*, " + AssemblyA,
                                TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("th:-", TagHelperDirectiveType.TagHelperPrefix),
                            CreateTagHelperDirectiveDescriptor(
                                "*, " + AssemblyB,
                                TagHelperDirectiveType.AddTagHelper),
                        },
                        "th:-"
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(ProcessTagHelperPrefixData))]
        public void ProcessTagHelperPrefix_ParsesPrefixFromDirectives(
            object directiveDescriptors,
            string expectedPrefix)
        {
            // Arrange
            var phase = new DefaultRazorTagHelperBinderPhase();
            var document = RazorCodeDocument.Create(new StringSourceDocument("Test content", encoding: Encoding.UTF8, filePath: "TestFile"));
            var tagHelperDirectives = (IEnumerable<TagHelperDirectiveDescriptor>)directiveDescriptors;

            // Act
            var prefix = phase.ProcessTagHelperPrefix(((IEnumerable<TagHelperDirectiveDescriptor>)directiveDescriptors).ToList(), document);

            // Assert
            Assert.Empty(tagHelperDirectives.SelectMany(directive => directive.Diagnostics));
            Assert.Equal(expectedPrefix, prefix);
        }

        public static TheoryData ProcessDirectivesData
        {
            get
            {
                return new TheoryData<IEnumerable<TagHelperDescriptor>, // tagHelpers
                                      IEnumerable<TagHelperDirectiveDescriptor>, // directiveDescriptors
                                      IEnumerable<TagHelperDescriptor>> // expectedDescriptors
                {
                    {
                        new [] { Valid_PlainTagHelperDescriptor, },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper)
                        },
                        new [] { Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, String_TagHelperDescriptor },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyB, TagHelperDirectiveType.AddTagHelper)
                        },
                        new [] { Valid_PlainTagHelperDescriptor, String_TagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, String_TagHelperDescriptor },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyB, TagHelperDirectiveType.RemoveTagHelper)
                        },
                        new [] { Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor, String_TagHelperDescriptor },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyB, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper)
                        },
                        new [] { String_TagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor, },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor(
                                Valid_PlainTagHelperDescriptor.Name + ", " + AssemblyA,
                                TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper)
                        },
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor, },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(
                                Valid_PlainTagHelperDescriptor.Name + ", " + AssemblyA,
                                TagHelperDirectiveType.RemoveTagHelper)
                        },
                        new [] { Valid_InheritedTagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor, },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(
                                Valid_PlainTagHelperDescriptor.Name + ", " + AssemblyA,
                                TagHelperDirectiveType.RemoveTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper)
                        },
                        new [] { Valid_InheritedTagHelperDescriptor, Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor, },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                        },
                        new [] { Valid_InheritedTagHelperDescriptor, Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor, },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor(
                                "Microsoft.AspNetCore.Razor.TagHelpers.ValidPlain*, " + AssemblyA,
                                TagHelperDirectiveType.AddTagHelper),
                        },
                        new [] { Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor, },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor(
                                "Microsoft.AspNetCore.Razor.TagHelpers.*, " + AssemblyA,
                                TagHelperDirectiveType.AddTagHelper),
                        },
                        new [] { Valid_PlainTagHelperDescriptor, Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor, },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(
                                "Microsoft.AspNetCore.Razor.TagHelpers.ValidP*, " + AssemblyA,
                                TagHelperDirectiveType.RemoveTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper)
                        },
                        new [] { Valid_InheritedTagHelperDescriptor, Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, String_TagHelperDescriptor, },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("Str*, " + AssemblyB, TagHelperDirectiveType.RemoveTagHelper)
                        },
                        new [] { Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, String_TagHelperDescriptor, },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyB, TagHelperDirectiveType.RemoveTagHelper)
                        },
                        new [] { Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, String_TagHelperDescriptor, },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("System." + String_TagHelperDescriptor.Name + ", " + AssemblyB, TagHelperDirectiveType.AddTagHelper)
                        },
                        new [] { Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor, String_TagHelperDescriptor },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyB, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("Microsoft.*, " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper)
                        },
                        new [] { String_TagHelperDescriptor }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor, String_TagHelperDescriptor },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor(
                                "*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(
                                "*, " + AssemblyB, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(
                                "?Microsoft*, " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                            CreateTagHelperDirectiveDescriptor(
                                "System." + String_TagHelperDescriptor.Name + ", " + AssemblyB, TagHelperDirectiveType.RemoveTagHelper)
                        },
                        new []
                        {
                            Valid_InheritedTagHelperDescriptor,
                            Valid_PlainTagHelperDescriptor,
                            String_TagHelperDescriptor
                        }
                    },
                    {
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor, String_TagHelperDescriptor },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor(
                                "*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(
                                "*, " + AssemblyB, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(
                                "TagHelper*, " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                            CreateTagHelperDirectiveDescriptor(
                                "System." + String_TagHelperDescriptor.Name + ", " + AssemblyB, TagHelperDirectiveType.RemoveTagHelper)
                        },
                        new []
                        {
                            Valid_InheritedTagHelperDescriptor,
                            Valid_PlainTagHelperDescriptor,
                            String_TagHelperDescriptor
                        }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(ProcessDirectivesData))]
        public void ProcessDirectives_FiltersTagHelpersByDirectives(
            object tagHelpers,
            object directiveDescriptors,
            object expectedDescriptors)
        {
            // Arrange
            var phase = new DefaultRazorTagHelperBinderPhase();
            var tagHelperDirectives = (IEnumerable<TagHelperDirectiveDescriptor>)directiveDescriptors;
            var expected = (IEnumerable<TagHelperDescriptor>)expectedDescriptors;

            // Act
            var results = phase.ProcessDirectives(
                new List<TagHelperDirectiveDescriptor>(tagHelperDirectives),
                new List<TagHelperDescriptor>((IEnumerable<TagHelperDescriptor>)tagHelpers));

            // Assert
            Assert.Empty(tagHelperDirectives.SelectMany(directive => directive.Diagnostics));
            Assert.Equal(expected.Count(), results.Count());

            foreach (var expectedDescriptor in expected)
            {
                Assert.Contains(expectedDescriptor, results, TagHelperDescriptorComparer.Default);
            }
        }

        public static TheoryData ProcessDirectives_EmptyResultData
        {
            get
            {
                return new TheoryData<IEnumerable<TagHelperDescriptor>, IEnumerable<TagHelperDirectiveDescriptor>>
                {
                    {
                        new TagHelperDescriptor[]
                        {
                            Valid_PlainTagHelperDescriptor,
                        },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                        }
                    },
                    {
                        new TagHelperDescriptor[]
                        {
                            Valid_PlainTagHelperDescriptor,
                            Valid_InheritedTagHelperDescriptor,
                        },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(Valid_PlainTagHelperDescriptor.Name + ", " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                            CreateTagHelperDirectiveDescriptor(Valid_InheritedTagHelperDescriptor.Name + ", " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                        }
                    },
                    {
                        new TagHelperDescriptor[]
                        {
                            Valid_PlainTagHelperDescriptor,
                            Valid_InheritedTagHelperDescriptor,
                            String_TagHelperDescriptor,
                        },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyB, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyB, TagHelperDirectiveType.RemoveTagHelper)
                        }
                    },
                    {
                        new TagHelperDescriptor[]
                        {
                            Valid_PlainTagHelperDescriptor,
                            Valid_InheritedTagHelperDescriptor,
                            String_TagHelperDescriptor,
                        },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyB, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(Valid_PlainTagHelperDescriptor.Name + ", " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                            CreateTagHelperDirectiveDescriptor(Valid_InheritedTagHelperDescriptor.Name + ", " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                            CreateTagHelperDirectiveDescriptor(String_TagHelperDescriptor.Name + ", " + AssemblyB, TagHelperDirectiveType.RemoveTagHelper)
                        }
                    },
                    {
                        new TagHelperDescriptor[0],
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                            CreateTagHelperDirectiveDescriptor(Valid_PlainTagHelperDescriptor.Name + ", " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                        }
                    },
                    {
                        new TagHelperDescriptor[]
                        {
                            Valid_PlainTagHelperDescriptor,
                        },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("Mic*, " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                        }
                    },
                    {
                        new TagHelperDescriptor[]
                        {
                            Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor
                        },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("Mic*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor(Valid_PlainTagHelperDescriptor.Name + ", " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                            CreateTagHelperDirectiveDescriptor(Valid_InheritedTagHelperDescriptor.Name + ", " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper)
                        }
                    },
                    {
                        new TagHelperDescriptor[]
                        {
                            Valid_PlainTagHelperDescriptor,
                            Valid_InheritedTagHelperDescriptor,
                            String_TagHelperDescriptor,
                        },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("Microsoft.*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("System.*, " + AssemblyB, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("Microsoft.AspNetCore.Razor.TagHelpers*, " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                            CreateTagHelperDirectiveDescriptor("System.*, " + AssemblyB, TagHelperDirectiveType.RemoveTagHelper)
                        }
                    },
                    {
                        new TagHelperDescriptor[]
                        {
                            Valid_PlainTagHelperDescriptor,
                            Valid_InheritedTagHelperDescriptor,
                            String_TagHelperDescriptor,
                        },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("?icrosoft.*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("?ystem.*, " + AssemblyB, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("*?????r, " + AssemblyA, TagHelperDirectiveType.RemoveTagHelper),
                            CreateTagHelperDirectiveDescriptor("Sy??em.*, " + AssemblyB, TagHelperDirectiveType.RemoveTagHelper)
                        }
                    },
                    {
                        new TagHelperDescriptor[]
                        {
                            Valid_PlainTagHelperDescriptor,
                            Valid_InheritedTagHelperDescriptor,
                            String_TagHelperDescriptor,
                        },
                        new []
                        {
                            CreateTagHelperDirectiveDescriptor("?i?crosoft.*, " + AssemblyA, TagHelperDirectiveType.AddTagHelper),
                            CreateTagHelperDirectiveDescriptor("??ystem.*, " + AssemblyB, TagHelperDirectiveType.AddTagHelper),
                        }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(ProcessDirectives_EmptyResultData))]
        public void ProcessDirectives_CanReturnEmptyDescriptorsBasedOnDirectiveDescriptors(
            object tagHelpers,
            object directiveDescriptors)
        {
            // Arrange
            var tagHelperDirectives = (IEnumerable<TagHelperDirectiveDescriptor>)directiveDescriptors;
            var phase = new DefaultRazorTagHelperBinderPhase();

            // Act
            var results = phase.ProcessDirectives(
                new List<TagHelperDirectiveDescriptor>(tagHelperDirectives),
                new List<TagHelperDescriptor>((IEnumerable<TagHelperDescriptor>)tagHelpers));

            // Assert
            Assert.Empty(results);
        }

        public static TheoryData<string> ProcessDirectives_IgnoresSpacesData
        {
            get
            {
                var assemblyName = Valid_PlainTagHelperDescriptor.AssemblyName;
                var typeName = Valid_PlainTagHelperDescriptor.Name;
                return new TheoryData<string>
                {
                    $"{typeName},{assemblyName}",
                    $"    {typeName},{assemblyName}",
                    $"{typeName}    ,{assemblyName}",
                    $"    {typeName}    ,{assemblyName}",
                    $"{typeName},    {assemblyName}",
                    $"{typeName},{assemblyName}    ",
                    $"{typeName},    {assemblyName}    ",
                    $"    {typeName},    {assemblyName}    ",
                    $"    {typeName}    ,    {assemblyName}    "
                };
            }
        }

        [Theory]
        [MemberData(nameof(ProcessDirectives_IgnoresSpacesData))]
        public void ProcessDirectives_IgnoresSpaces(string directiveText)
        {
            // Arrange
            var phase = new DefaultRazorTagHelperBinderPhase();
            var directives = new[]
            {
                new TagHelperDirectiveDescriptor()
                {
                    DirectiveText = directiveText,
                    DirectiveType = TagHelperDirectiveType.AddTagHelper,
                    Diagnostics = new List<RazorDiagnostic>(),
                }
            };

            // Act
            var results = phase.ProcessDirectives(
                directives,
                new[] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor });

            // Assert
            Assert.Empty(directives[0].Diagnostics);

            var single = Assert.Single(results);
            Assert.Equal(Valid_PlainTagHelperDescriptor, single, TagHelperDescriptorComparer.Default);
        }

        [Theory]
        [InlineData("", 1)]
        [InlineData("*,", 2)]
        [InlineData("?,", 2)]
        [InlineData(",", 1)]
        [InlineData(",,,", 3)]
        [InlineData("First, ", 7)]
        [InlineData("First , ", 8)]
        [InlineData(" ,Second", 8)]
        [InlineData(" , Second", 9)]
        [InlineData("SomeType,", 9)]
        [InlineData("SomeAssembly", 12)]
        [InlineData("First,Second,Third", 18)]
        public void DescriptorResolver_CreatesErrorIfInvalidLookupText_DoesNotThrow(string directiveText, int errorLength)
        {
            // Arrange
            var phase = new DefaultRazorTagHelperBinderPhase();

            var directive = new TagHelperDirectiveDescriptor()
            {
                DirectiveText = directiveText,
                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                Location = new SourceLocation(1, 2, 3),
                Diagnostics = new List<RazorDiagnostic>(),
            };

            var expectedErrorMessage = string.Format(
                "Invalid tag helper directive look up text '{0}'. The correct look up text " +
                "format is: \"typeName, assemblyName\".",
                directiveText);

            var expectedError = RazorDiagnostic.Create(
                new RazorError(
                    expectedErrorMessage,
                    new SourceLocation(1, 2, 3),
                    errorLength));


            // Act
            var result = phase.ParseAddOrRemoveDirective(directive);

            // Assert
            Assert.Null(result);

            var error = Assert.Single(directive.Diagnostics);
            Assert.Equal(expectedError, error);
        }

        private static TagHelperDescriptor CreatePrefixedValidPlainDescriptor(string prefix)
        {
            return Valid_PlainTagHelperDescriptor;
            //return new TagHelperDescriptor(Valid_PlainTagHelperDescriptor)
            //{
            //    Prefix = prefix,
            //};
        }

        private static TagHelperDescriptor CreatePrefixedValidInheritedDescriptor(string prefix)
        {
            return Valid_InheritedTagHelperDescriptor;
            //return new TagHelperDescriptor(Valid_InheritedTagHelperDescriptor)
            //{
            //    Prefix = prefix,
            //};
        }

        private static TagHelperDescriptor CreatePrefixedStringDescriptor(string prefix)
        {
            return String_TagHelperDescriptor;
            //return new TagHelperDescriptor(String_TagHelperDescriptor)
            //{
            //    Prefix = prefix,
            //};
        }

        private static TagHelperDirectiveDescriptor CreateTagHelperDirectiveDescriptor(
            string directiveText,
            TagHelperDirectiveType directiveType)
        {
            return new TagHelperDirectiveDescriptor
            {
                DirectiveText = directiveText,
                Location = SourceLocation.Zero,
                DirectiveType = directiveType,
                Diagnostics = new List<RazorDiagnostic>(),
            };
        }

        private static RazorSourceDocument CreateTestSourceDocument()
        {
            var content =
            @"
@addTagHelper *, TestAssembly
<form>
    <input value='Hello' type='text' />
</form>";
            var sourceDocument = TestRazorSourceDocument.Create(content, fileName: null);
            return sourceDocument;
        }

        private static TagHelperDescriptor CreateTagHelperDescriptor(
            string tagName,
            string typeName,
            string assemblyName,
            IEnumerable<Action<BoundAttributeDescriptorBuilder>> attributes = null,
            IEnumerable<Action<TagMatchingRuleBuilder>> ruleBuilders = null)
        {
            var builder = TagHelperDescriptorBuilder.Create(typeName, assemblyName);

            if (attributes != null)
            {
                foreach (var attributeBuilder in attributes)
                {
                    builder.BindAttribute(attributeBuilder);
                }
            }

            if (ruleBuilders != null)
            {
                foreach (var ruleBuilder in ruleBuilders)
                {
                    builder.TagMatchingRule(innerRuleBuilder =>
                    {
                        innerRuleBuilder.RequireTagName(tagName);
                        ruleBuilder(innerRuleBuilder);
                    });
                }
            }
            else
            {
                builder.TagMatchingRule(ruleBuilder => ruleBuilder.RequireTagName(tagName));
            }

            var descriptor = builder.Build();

            return descriptor;
        }
    }
}
