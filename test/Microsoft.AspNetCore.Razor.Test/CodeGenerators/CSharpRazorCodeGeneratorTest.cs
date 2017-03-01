// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Chunks.Generators;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;
using Microsoft.AspNetCore.Razor.Test.CodeGenerators;
using Xunit;

namespace Microsoft.AspNetCore.Razor.CodeGenerators
{
    public class CSharpRazorCodeGeneratorTest : RazorCodeGeneratorTest<CSharpRazorCodeLanguage>
    {
        protected override string FileExtension
        {
            get { return "cshtml"; }
        }

        protected override string LanguageName
        {
            get { return "CS"; }
        }

        protected override string BaselineExtension
        {
            get { return "cs"; }
        }

        private const string TestPhysicalPath = @"C:\Bar.cshtml";

        [Fact]
        public void ConstructorRequiresNonNullClassName()
        {
            Assert.Throws<ArgumentException>(
                "className",
                () => new RazorChunkGenerator(null, TestRootNamespaceName, TestPhysicalPath, CreateHost()));
        }

        [Fact]
        public void ConstructorRequiresNonEmptyClassName()
        {
            Assert.Throws<ArgumentException>(
                "className",
                () => new RazorChunkGenerator(string.Empty, TestRootNamespaceName, TestPhysicalPath, CreateHost()));
        }

        [Fact]
        public void ConstructorAllowsEmptyRootNamespaceName()
        {
            new RazorChunkGenerator("Foo", string.Empty, TestPhysicalPath, CreateHost());
        }

        [Theory]
        [InlineData("StringLiterals")]
        [InlineData("NestedCSharp")]
        [InlineData("NullConditionalExpressions")]
        [InlineData("NestedCodeBlocks")]
        [InlineData("CodeBlock")]
        [InlineData("ExplicitExpression")]
        [InlineData("MarkupInCodeBlock")]
        [InlineData("Blocks")]
        [InlineData("ImplicitExpression")]
        [InlineData("Imports")]
        [InlineData("ExpressionsInCode")]
        [InlineData("FunctionsBlock")]
        [InlineData("FunctionsBlock_Tabs")]
        [InlineData("Templates")]
        [InlineData("Sections")]
        [InlineData("RazorComments")]
        [InlineData("InlineBlocks")]
        [InlineData("ConditionalAttributes")]
        [InlineData("Await")]
        [InlineData("CodeBlockWithTextElement")]
        [InlineData("ExplicitExpressionWithMarkup")]
        public void CSharpChunkGeneratorCorrectlyGeneratesRunTimeCode(string testType)
        {
            RunTest(testType);
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesMappingsForNestedCSharp()
        {
            RunTest(
                "NestedCSharp",
                "NestedCSharp.DesignTime",
                designTimeMode: true,
                tabTest: TabTest.NoTabs,
                expectedDesignTimePragmas: new List<LineMapping>
                {
                    BuildLineMapping(
                        documentAbsoluteIndex: 2,
                        documentLineIndex: 0,
                        generatedAbsoluteIndex: 545,
                        generatedLineIndex: 22,
                        characterOffsetIndex: 2,
                        contentLength: 6),
                    BuildLineMapping(
                        documentAbsoluteIndex: 9,
                        documentLineIndex: 1,
                        documentCharacterOffsetIndex: 5,
                        generatedAbsoluteIndex: 621,
                        generatedLineIndex: 29,
                        generatedCharacterOffsetIndex: 4,
                        contentLength: 53),
                    BuildLineMapping(
                        documentAbsoluteIndex: 82,
                        documentLineIndex: 4,
                        generatedAbsoluteIndex: 753,
                        generatedLineIndex: 37,
                        characterOffsetIndex: 13,
                        contentLength: 16),
                    BuildLineMapping(
                        documentAbsoluteIndex: 115,
                        documentLineIndex: 5,
                        generatedAbsoluteIndex: 848,
                        generatedLineIndex: 42,
                        characterOffsetIndex: 14,
                        contentLength: 7),
                    BuildLineMapping(
                        documentAbsoluteIndex: 122,
                        documentLineIndex: 6,
                        generatedAbsoluteIndex: 926,
                        generatedLineIndex: 49,
                        characterOffsetIndex: 5,
                        contentLength: 2),
                });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesMappingsForNullConditionalOperator()
        {
            RunTest("NullConditionalExpressions",
                    "NullConditionalExpressions.DesignTime",
                    designTimeMode: true,
                    tabTest: TabTest.NoTabs,
                    expectedDesignTimePragmas: new List<LineMapping>()
                    {
                        BuildLineMapping(
                            documentAbsoluteIndex: 2,
                            documentLineIndex: 0,
                            generatedAbsoluteIndex: 587,
                            generatedLineIndex: 22,
                            characterOffsetIndex: 2,
                            contentLength: 6),
                        BuildLineMapping(
                            documentAbsoluteIndex: 9,
                            documentLineIndex: 1,
                            documentCharacterOffsetIndex: 5,
                            generatedAbsoluteIndex: 679,
                            generatedLineIndex: 29,
                            generatedCharacterOffsetIndex: 6,
                            contentLength: 13),
                        BuildLineMapping(
                            documentAbsoluteIndex: 22,
                            documentLineIndex: 1,
                            generatedAbsoluteIndex: 789,
                            generatedLineIndex: 34,
                            characterOffsetIndex: 18,
                            contentLength: 6),
                        BuildLineMapping(
                            documentAbsoluteIndex: 29,
                            documentLineIndex: 2,
                            documentCharacterOffsetIndex: 5,
                            generatedAbsoluteIndex: 881,
                            generatedLineIndex: 41,
                            generatedCharacterOffsetIndex: 6,
                            contentLength: 22),
                        BuildLineMapping(
                            documentAbsoluteIndex: 51,
                            documentLineIndex: 2,
                            generatedAbsoluteIndex: 1009,
                            generatedLineIndex: 46,
                            characterOffsetIndex: 27,
                            contentLength: 6),
                        BuildLineMapping(
                            documentAbsoluteIndex: 58,
                            documentLineIndex: 3,
                            documentCharacterOffsetIndex: 5,
                            generatedAbsoluteIndex: 1101,
                            generatedLineIndex: 53,
                            generatedCharacterOffsetIndex: 6,
                            contentLength: 26),
                        BuildLineMapping(
                            documentAbsoluteIndex: 84,
                            documentLineIndex: 3,
                            generatedAbsoluteIndex: 1237,
                            generatedLineIndex: 58,
                            characterOffsetIndex: 31,
                            contentLength: 6),
                        BuildLineMapping(
                            documentAbsoluteIndex: 91,
                            documentLineIndex: 4,
                            documentCharacterOffsetIndex: 5,
                            generatedAbsoluteIndex: 1329,
                            generatedLineIndex: 65,
                            generatedCharacterOffsetIndex: 6,
                            contentLength: 41),
                        BuildLineMapping(
                            documentAbsoluteIndex: 132,
                            documentLineIndex: 4,
                            generatedAbsoluteIndex: 1495,
                            generatedLineIndex: 70,
                            characterOffsetIndex: 46,
                            contentLength: 2),
                        BuildLineMapping(
                            documentAbsoluteIndex: 140,
                            documentLineIndex: 7,
                            documentCharacterOffsetIndex: 1,
                            generatedAbsoluteIndex: 1581,
                            generatedLineIndex: 76,
                            generatedCharacterOffsetIndex: 6,
                            contentLength: 13),
                        BuildLineMapping(
                            documentAbsoluteIndex: 156,
                            documentLineIndex: 8,
                            documentCharacterOffsetIndex: 1,
                            generatedAbsoluteIndex: 1679,
                            generatedLineIndex: 81,
                            generatedCharacterOffsetIndex: 6,
                            contentLength: 22),
                        BuildLineMapping(
                            documentAbsoluteIndex: 181,
                            documentLineIndex: 9,
                            documentCharacterOffsetIndex: 1,
                            generatedAbsoluteIndex: 1787,
                            generatedLineIndex: 86,
                            generatedCharacterOffsetIndex: 6,
                            contentLength: 26),
                        BuildLineMapping(
                            documentAbsoluteIndex: 210,
                            documentLineIndex: 10,
                            documentCharacterOffsetIndex: 1,
                            generatedAbsoluteIndex: 1899,
                            generatedLineIndex: 91,
                            generatedCharacterOffsetIndex: 6,
                            contentLength: 41),
                    });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesMappingsForAwait()
        {
            RunTest("Await",
                    "Await.DesignTime",
                    designTimeMode: true,
                    tabTest: TabTest.Tabs,
                    expectedDesignTimePragmas: new List<LineMapping>()
                    {
                        BuildLineMapping(
                            documentAbsoluteIndex: 12,
                            documentLineIndex: 0,
                            documentCharacterOffsetIndex: 12,
                            generatedAbsoluteIndex: 173,
                            generatedLineIndex: 9,
                            generatedCharacterOffsetIndex: 0,
                            contentLength: 76),
                        BuildLineMapping(
                            documentAbsoluteIndex: 192,
                            documentLineIndex: 9,
                            documentCharacterOffsetIndex: 39,
                            generatedAbsoluteIndex: 669,
                            generatedLineIndex: 31,
                            generatedCharacterOffsetIndex: 15,
                            contentLength: 11),
                        BuildLineMapping(
                            documentAbsoluteIndex: 247,
                            documentLineIndex: 10,
                            documentCharacterOffsetIndex: 38,
                            generatedAbsoluteIndex: 753,
                            generatedLineIndex: 36,
                            generatedCharacterOffsetIndex: 14,
                            contentLength: 11),
                        BuildLineMapping(
                            documentAbsoluteIndex: 304,
                            documentLineIndex: 11,
                            documentCharacterOffsetIndex: 39,
                            generatedAbsoluteIndex: 835,
                            generatedLineIndex: 41,
                            generatedCharacterOffsetIndex: 12,
                            contentLength: 14),
                        BuildLineMapping(
                            documentAbsoluteIndex: 371,
                            documentLineIndex: 12,
                            documentCharacterOffsetIndex: 46,
                            generatedAbsoluteIndex: 922,
                            generatedLineIndex: 47,
                            generatedCharacterOffsetIndex: 13,
                            contentLength: 1),
                        BuildLineMapping(
                            documentAbsoluteIndex: 376,
                            documentLineIndex: 12,
                            documentCharacterOffsetIndex: 51,
                            generatedAbsoluteIndex: 1001,
                            generatedLineIndex: 53,
                            generatedCharacterOffsetIndex: 18,
                            contentLength: 11),
                        BuildLineMapping(
                            documentAbsoluteIndex: 391,
                            documentLineIndex: 12,
                            documentCharacterOffsetIndex: 66,
                            generatedAbsoluteIndex: 1089,
                            generatedLineIndex: 58,
                            generatedCharacterOffsetIndex: 18,
                            contentLength: 1),
                        BuildLineMapping(
                            documentAbsoluteIndex: 448,
                            documentLineIndex: 13,
                            documentCharacterOffsetIndex: 49,
                            generatedAbsoluteIndex: 1169,
                            generatedLineIndex: 64,
                            generatedCharacterOffsetIndex: 19,
                            contentLength: 5),
                        BuildLineMapping(
                            documentAbsoluteIndex: 578,
                            documentLineIndex: 18,
                            documentCharacterOffsetIndex: 42,
                            generatedAbsoluteIndex: 1248,
                            generatedLineIndex: 69,
                            generatedCharacterOffsetIndex: 15,
                            contentLength: 15),
                        BuildLineMapping(
                            documentAbsoluteIndex: 650,
                            documentLineIndex: 19,
                            documentCharacterOffsetIndex: 51,
                            generatedAbsoluteIndex: 1340,
                            generatedLineIndex: 74,
                            generatedCharacterOffsetIndex: 18,
                            contentLength: 19),
                        BuildLineMapping(
                            documentAbsoluteIndex: 716,
                            documentLineIndex: 20,
                            documentCharacterOffsetIndex: 41,
                            generatedAbsoluteIndex: 1435,
                            generatedLineIndex: 79,
                            generatedCharacterOffsetIndex: 17,
                            contentLength: 22),
                        BuildLineMapping(
                            documentAbsoluteIndex: 787,
                            documentLineIndex: 21,
                            documentCharacterOffsetIndex: 42,
                            generatedAbsoluteIndex: 1528,
                            generatedLineIndex: 84,
                            generatedCharacterOffsetIndex: 12,
                            contentLength: 39),
                        BuildLineMapping(
                            documentAbsoluteIndex: 884,
                            documentLineIndex: 22,
                            documentCharacterOffsetIndex: 51,
                            generatedAbsoluteIndex: 1642,
                            generatedLineIndex: 90,
                            generatedCharacterOffsetIndex: 15,
                            contentLength: 21),
                        BuildLineMapping(
                            documentAbsoluteIndex: 961,
                            documentLineIndex: 23,
                            documentCharacterOffsetIndex: 49,
                            generatedAbsoluteIndex: 1736,
                            generatedLineIndex: 96,
                            generatedCharacterOffsetIndex: 13,
                            contentLength: 1),
                        BuildLineMapping(
                            documentAbsoluteIndex: 966,
                            documentLineIndex: 23,
                            documentCharacterOffsetIndex: 54,
                            generatedAbsoluteIndex: 1815,
                            generatedLineIndex: 102,
                            generatedCharacterOffsetIndex: 18,
                            contentLength: 27),
                        BuildLineMapping(
                            documentAbsoluteIndex: 997,
                            documentLineIndex: 23,
                            documentCharacterOffsetIndex: 85,
                            generatedAbsoluteIndex: 1923,
                            generatedLineIndex: 107,
                            generatedCharacterOffsetIndex: 22,
                            contentLength: 1),
                        BuildLineMapping(
                            documentAbsoluteIndex: 1057,
                            documentLineIndex: 24,
                            documentCharacterOffsetIndex: 52,
                            generatedAbsoluteIndex: 2003,
                            generatedLineIndex: 113,
                            generatedCharacterOffsetIndex: 19,
                            contentLength: 19),
                    });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesMappingsForSimpleUnspacedIf()
        {
            RunTest("SimpleUnspacedIf",
                    "SimpleUnspacedIf.DesignTime.Tabs",
                    designTimeMode: true,
                    tabTest: TabTest.Tabs,
                    expectedDesignTimePragmas: new List<LineMapping>()
                    {
                        BuildLineMapping(
                            documentAbsoluteIndex: 1,
                            documentLineIndex: 0,
                            documentCharacterOffsetIndex: 1,
                            generatedAbsoluteIndex: 555,
                            generatedLineIndex: 22,
                            generatedCharacterOffsetIndex: 0,
                            contentLength: 15),
                        BuildLineMapping(
                            documentAbsoluteIndex: 27,
                            documentLineIndex: 2,
                            documentCharacterOffsetIndex: 12,
                            generatedAbsoluteIndex: 646,
                            generatedLineIndex: 30,
                            generatedCharacterOffsetIndex: 6,
                            contentLength: 3),
                    });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesMappingsForRazorCommentsAtDesignTime()
        {
            RunTest("RazorComments", "RazorComments.DesignTime", designTimeMode: true, tabTest: TabTest.NoTabs,
                expectedDesignTimePragmas: new List<LineMapping>()
                {
                    BuildLineMapping(
                        documentAbsoluteIndex: 81,
                        documentLineIndex: 3,
                        generatedAbsoluteIndex: 548,
                        generatedLineIndex: 22,
                        characterOffsetIndex: 2,
                        contentLength: 6),
                    BuildLineMapping(
                        documentAbsoluteIndex: 122,
                        documentLineIndex: 4,
                        documentCharacterOffsetIndex: 39,
                        generatedAbsoluteIndex: 659,
                        generatedLineIndex: 29,
                        generatedCharacterOffsetIndex: 38,
                        contentLength: 22),
                    BuildLineMapping(
                        documentAbsoluteIndex: 173,
                        documentLineIndex: 5,
                        documentCharacterOffsetIndex: 49,
                        generatedAbsoluteIndex: 796,
                        generatedLineIndex: 36,
                        generatedCharacterOffsetIndex: 48,
                        contentLength: 58),
                    BuildLineMapping(
                        documentAbsoluteIndex: 238,
                        documentLineIndex: 11,
                        generatedAbsoluteIndex: 922,
                        generatedLineIndex: 45,
                        characterOffsetIndex: 2,
                        contentLength: 24),
                    BuildLineMapping(
                        documentAbsoluteIndex: 310,
                        documentLineIndex: 12,
                        generatedAbsoluteIndex: 1059,
                        generatedLineIndex: 51,
                        characterOffsetIndex: 45,
                        contentLength: 3),
                    BuildLineMapping(
                        documentAbsoluteIndex: 323,
                        documentLineIndex: 14,
                        documentCharacterOffsetIndex: 2,
                        generatedAbsoluteIndex: 1135,
                        generatedLineIndex: 56,
                        generatedCharacterOffsetIndex: 6,
                        contentLength: 1),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGenerateMappingForOpenedCurlyIf()
        {
            OpenedIf(withTabs: true);
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGenerateMappingForOpenedCurlyIfSpaces()
        {
            OpenedIf(withTabs: false);
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesImportStatementsAtDesignTime()
        {
            RunTest("Imports", "Imports.DesignTime", designTimeMode: true, tabTest: TabTest.NoTabs, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 1,
                    documentLineIndex: 0,
                    documentCharacterOffsetIndex: 1,
                    generatedAbsoluteIndex: 51,
                    generatedLineIndex: 3,
                    generatedCharacterOffsetIndex: 0,
                    contentLength: 15),
                BuildLineMapping(
                    documentAbsoluteIndex: 19,
                    documentLineIndex: 1,
                    documentCharacterOffsetIndex: 1,
                    generatedAbsoluteIndex: 132,
                    generatedLineIndex: 9,
                    generatedCharacterOffsetIndex: 0,
                    contentLength: 32),
                BuildLineMapping(
                    documentAbsoluteIndex: 54,
                    documentLineIndex: 2,
                    documentCharacterOffsetIndex: 1,
                    generatedAbsoluteIndex: 230,
                    generatedLineIndex: 15,
                    generatedCharacterOffsetIndex: 0,
                    contentLength: 12),
                BuildLineMapping(
                    documentAbsoluteIndex: 71,
                    documentLineIndex: 4,
                    documentCharacterOffsetIndex: 1,
                    generatedAbsoluteIndex: 308,
                    generatedLineIndex: 21,
                    generatedCharacterOffsetIndex: 0,
                    contentLength: 19),
                BuildLineMapping(
                    documentAbsoluteIndex: 93,
                    documentLineIndex: 5,
                    documentCharacterOffsetIndex: 1,
                    generatedAbsoluteIndex: 393,
                    generatedLineIndex: 27,
                    generatedCharacterOffsetIndex: 0,
                    contentLength: 27),
                BuildLineMapping(
                    documentAbsoluteIndex: 123,
                    documentLineIndex: 6,
                    documentCharacterOffsetIndex: 1,
                    generatedAbsoluteIndex: 486,
                    generatedLineIndex: 33,
                    generatedCharacterOffsetIndex: 0,
                    contentLength: 41),
                BuildLineMapping(
                    documentAbsoluteIndex: 197,
                    documentLineIndex: 8,
                    generatedAbsoluteIndex: 1080,
                    generatedLineIndex: 57,
                    characterOffsetIndex: 29,
                    contentLength: 21),
                BuildLineMapping(
                    documentAbsoluteIndex: 259,
                    documentLineIndex: 9,
                    generatedAbsoluteIndex: 1197,
                    generatedLineIndex: 62,
                    characterOffsetIndex: 35,
                    contentLength: 20),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesFunctionsBlocksAtDesignTime()
        {
            RunTest("FunctionsBlock",
                    "FunctionsBlock.DesignTime",
                    designTimeMode: true,
                    tabTest: TabTest.NoTabs,
                    expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 12,
                    documentLineIndex: 0,
                    documentCharacterOffsetIndex: 12,
                    generatedAbsoluteIndex: 191,
                    generatedLineIndex: 9,
                    generatedCharacterOffsetIndex: 0,
                    contentLength: 4),
                BuildLineMapping(
                    documentAbsoluteIndex: 33,
                    documentLineIndex: 4,
                    documentCharacterOffsetIndex: 12,
                    generatedAbsoluteIndex: 259,
                    generatedLineIndex: 15,
                    generatedCharacterOffsetIndex: 0,
                    contentLength: 104),
                BuildLineMapping(
                    documentAbsoluteIndex: 167,
                    documentLineIndex: 11,
                    generatedAbsoluteIndex: 811,
                    generatedLineIndex: 37,
                    characterOffsetIndex: 25,
                    contentLength: 11),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesFunctionsBlocksAtDesignTimeTabs()
        {
            RunTest("FunctionsBlock",
                    "FunctionsBlock.DesignTime.Tabs",
                    designTimeMode: true,
                    tabTest: TabTest.Tabs,
                    expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 12,
                    documentLineIndex: 0,
                    documentCharacterOffsetIndex: 12,
                    generatedAbsoluteIndex: 191,
                    generatedLineIndex: 9,
                    generatedCharacterOffsetIndex: 0,
                    contentLength: 4),
                BuildLineMapping(
                    documentAbsoluteIndex: 33,
                    documentLineIndex: 4,
                    documentCharacterOffsetIndex: 12,
                    generatedAbsoluteIndex: 259,
                    generatedLineIndex: 15,
                    generatedCharacterOffsetIndex: 0,
                    contentLength: 104),
                BuildLineMapping(
                    documentAbsoluteIndex: 167,
                    documentLineIndex: 11,
                    documentCharacterOffsetIndex: 25,
                    generatedAbsoluteIndex: 799,
                    generatedLineIndex: 37,
                    generatedCharacterOffsetIndex: 13,
                    contentLength: 11),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesMinimalFunctionsBlocksAtDesignTimeTabs()
        {
            RunTest("FunctionsBlockMinimal",
                    "FunctionsBlockMinimal.DesignTime.Tabs",
                    designTimeMode: true,
                    tabTest: TabTest.Tabs,
                    expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(16, 2, 12, 205, 9, 0, 55)
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesHiddenSpansWithinCode()
        {
            RunTest("HiddenSpansInCode", designTimeMode: true, tabTest: TabTest.NoTabs, expectedDesignTimePragmas: new List<LineMapping>
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 2,
                    documentLineIndex: 0,
                    generatedAbsoluteIndex: 560,
                    generatedLineIndex: 22,
                    characterOffsetIndex: 2,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 9,
                    documentLineIndex: 1,
                    generatedAbsoluteIndex: 642,
                    generatedLineIndex: 29,
                    characterOffsetIndex: 5,
                    contentLength: 5),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorGeneratesCodeWithParserErrorsInDesignTimeMode()
        {
            RunTest("ParserError", designTimeMode: true, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 2,
                    documentLineIndex: 0,
                    generatedAbsoluteIndex: 542,
                    generatedLineIndex: 22,
                    characterOffsetIndex: 2,
                    contentLength: 31),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesInheritsAtRuntime()
        {
            RunTest("Inherits", baselineName: "Inherits.Runtime");
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesInheritsAtDesigntime()
        {
            RunTest("Inherits", baselineName: "Inherits.Designtime", designTimeMode: true, tabTest: TabTest.NoTabs, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 20,
                    documentLineIndex: 2,
                    generatedAbsoluteIndex: 321,
                    generatedLineIndex: 12,
                    characterOffsetIndex: 10,
                    contentLength: 25),
                BuildLineMapping(
                    documentAbsoluteIndex: 1,
                    documentLineIndex: 0,
                    documentCharacterOffsetIndex: 1,
                    generatedAbsoluteIndex: 685,
                    generatedLineIndex: 27,
                    generatedCharacterOffsetIndex: 6,
                    contentLength: 5),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesDesignTimePragmasForUnfinishedExpressionsInCode()
        {
            RunTest("UnfinishedExpressionInCode", tabTest: TabTest.NoTabs, designTimeMode: true, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 2,
                    documentLineIndex: 0,
                    generatedAbsoluteIndex: 587,
                    generatedLineIndex: 22,
                    characterOffsetIndex: 2,
                    contentLength: 2),
                BuildLineMapping(
                    documentAbsoluteIndex: 5,
                    documentLineIndex: 1,
                    documentCharacterOffsetIndex: 1,
                    generatedAbsoluteIndex: 673,
                    generatedLineIndex: 28,
                    generatedCharacterOffsetIndex: 6,
                    contentLength: 9),
                BuildLineMapping(
                    documentAbsoluteIndex: 14,
                    documentLineIndex: 1,
                    generatedAbsoluteIndex: 771,
                    generatedLineIndex: 33,
                    characterOffsetIndex: 10,
                    contentLength: 2),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesDesignTimePragmasForUnfinishedExpressionsInCodeTabs()
        {
            RunTest("UnfinishedExpressionInCode",
                    "UnfinishedExpressionInCode.Tabs",
                    tabTest: TabTest.Tabs,
                    designTimeMode: true, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 2,
                    documentLineIndex: 0,
                    generatedAbsoluteIndex: 587,
                    generatedLineIndex: 22,
                    characterOffsetIndex: 2,
                    contentLength: 2),
                BuildLineMapping(
                    documentAbsoluteIndex: 5,
                    documentLineIndex: 1,
                    documentCharacterOffsetIndex: 1,
                    generatedAbsoluteIndex: 673,
                    generatedLineIndex: 28,
                    generatedCharacterOffsetIndex: 6,
                    contentLength: 9),
                BuildLineMapping(
                    documentAbsoluteIndex: 14,
                    documentLineIndex: 1,
                    documentCharacterOffsetIndex: 10,
                    generatedAbsoluteIndex: 765,
                    generatedLineIndex: 33,
                    generatedCharacterOffsetIndex: 4,
                    contentLength: 2),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesDesignTimePragmasMarkupAndExpressions()
        {
            RunTest("DesignTime",
                designTimeMode: true,
                tabTest: TabTest.NoTabs,
                expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 20,
                    documentLineIndex: 1,
                    documentCharacterOffsetIndex: 13,
                    generatedAbsoluteIndex: 549,
                    generatedLineIndex: 22,
                    generatedCharacterOffsetIndex: 12,
                    contentLength: 36),
                BuildLineMapping(
                    documentAbsoluteIndex: 74,
                    documentLineIndex: 2,
                    generatedAbsoluteIndex: 671,
                    generatedLineIndex: 29,
                    characterOffsetIndex: 22,
                    contentLength: 1),
                BuildLineMapping(
                    documentAbsoluteIndex: 79,
                    documentLineIndex: 2,
                    generatedAbsoluteIndex: 762,
                    generatedLineIndex: 34,
                    characterOffsetIndex: 27,
                    contentLength: 15),
                BuildLineMapping(
                    documentAbsoluteIndex: 113,
                    documentLineIndex: 7,
                    documentCharacterOffsetIndex: 2,
                    generatedAbsoluteIndex: 847,
                    generatedLineIndex: 41,
                    generatedCharacterOffsetIndex: 6,
                    contentLength: 12),
                BuildLineMapping(
                    documentAbsoluteIndex: 129,
                    documentLineIndex: 8,
                    documentCharacterOffsetIndex: 1,
                    generatedAbsoluteIndex: 928,
                    generatedLineIndex: 46,
                    generatedCharacterOffsetIndex: 6,
                    contentLength: 4),
                BuildLineMapping(
                    documentAbsoluteIndex: 142,
                    documentLineIndex: 8,
                    generatedAbsoluteIndex: 1033,
                    generatedLineIndex: 48,
                    characterOffsetIndex: 14,
                    contentLength: 3),
                BuildLineMapping(
                    documentAbsoluteIndex: 204,
                    documentLineIndex: 13,
                    documentCharacterOffsetIndex: 5,
                    generatedAbsoluteIndex: 1219,
                    generatedLineIndex: 60,
                    generatedCharacterOffsetIndex: 6,
                    contentLength: 3),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesDesignTimePragmasForExplicitExpressionContainingMarkup()
        {
            RunTest(
                "ExplicitExpressionWithMarkup",
                "ExplicitExpressionWithMarkup.DesignTime",
                designTimeMode: true,
                expectedDesignTimePragmas: new List<LineMapping>());
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesDesignTimePragmasForImplicitExpressionStartedAtEOF()
        {
            RunTest("ImplicitExpressionAtEOF", designTimeMode: true, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 19,
                    documentLineIndex: 2,
                    documentCharacterOffsetIndex: 1,
                    generatedAbsoluteIndex: 582,
                    generatedLineIndex: 22,
                    generatedCharacterOffsetIndex: 6,
                    contentLength: 0),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesDesignTimePragmasForExplicitExpressionStartedAtEOF()
        {
            RunTest("ExplicitExpressionAtEOF", designTimeMode: true, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 20,
                    documentLineIndex: 2,
                    documentCharacterOffsetIndex: 2,
                    generatedAbsoluteIndex: 582,
                    generatedLineIndex: 22,
                    generatedCharacterOffsetIndex: 6,
                    contentLength: 0),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesDesignTimePragmasForCodeBlockStartedAtEOF()
        {
            RunTest("CodeBlockAtEOF", designTimeMode: true, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 2,
                    documentLineIndex: 0,
                    generatedAbsoluteIndex: 551,
                    generatedLineIndex: 22,
                    characterOffsetIndex: 2,
                    contentLength: 0),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesDesignTimePragmasForEmptyImplicitExpression()
        {
            RunTest("EmptyImplicitExpression", designTimeMode: true, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 19,
                    documentLineIndex: 2,
                    documentCharacterOffsetIndex: 1,
                    generatedAbsoluteIndex: 582,
                    generatedLineIndex: 22,
                    generatedCharacterOffsetIndex: 6,
                    contentLength: 0),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesDesignTimePragmasForEmptyImplicitExpressionInCode()
        {
            RunTest("EmptyImplicitExpressionInCode", tabTest: TabTest.NoTabs, designTimeMode: true, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 2,
                    documentLineIndex: 0,
                    generatedAbsoluteIndex: 596,
                    generatedLineIndex: 22,
                    characterOffsetIndex: 2,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 9,
                    documentLineIndex: 1,
                    documentCharacterOffsetIndex: 5,
                    generatedAbsoluteIndex: 691,
                    generatedLineIndex: 29,
                    generatedCharacterOffsetIndex: 6,
                    contentLength: 0),
                BuildLineMapping(
                    documentAbsoluteIndex: 9,
                    documentLineIndex: 1,
                    generatedAbsoluteIndex: 778,
                    generatedLineIndex: 34,
                    characterOffsetIndex: 5,
                    contentLength: 2),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesDesignTimePragmasForEmptyImplicitExpressionInCodeTabs()
        {
            RunTest("EmptyImplicitExpressionInCode",
                    "EmptyImplicitExpressionInCode.Tabs",
                    tabTest: TabTest.Tabs,
                    designTimeMode: true, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 2,
                    documentLineIndex: 0,
                    generatedAbsoluteIndex: 596,
                    generatedLineIndex: 22,
                    characterOffsetIndex: 2,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 9,
                    documentLineIndex: 1,
                    documentCharacterOffsetIndex: 5,
                    generatedAbsoluteIndex: 691,
                    generatedLineIndex: 29,
                    generatedCharacterOffsetIndex: 6,
                    contentLength: 0),
                BuildLineMapping(
                    documentAbsoluteIndex: 9,
                    documentLineIndex: 1,
                    documentCharacterOffsetIndex: 5,
                    generatedAbsoluteIndex: 775,
                    generatedLineIndex: 34,
                    generatedCharacterOffsetIndex: 2,
                    contentLength: 2),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesDesignTimePragmasForEmptyExplicitExpression()
        {
            RunTest("EmptyExplicitExpression", designTimeMode: true, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 20,
                    documentLineIndex: 2,
                    documentCharacterOffsetIndex: 2,
                    generatedAbsoluteIndex: 582,
                    generatedLineIndex: 22,
                    generatedCharacterOffsetIndex: 6,
                    contentLength: 0),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyGeneratesDesignTimePragmasForEmptyCodeBlock()
        {
            RunTest("EmptyCodeBlock", designTimeMode: true, expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 20,
                    documentLineIndex: 2,
                    generatedAbsoluteIndex: 551,
                    generatedLineIndex: 22,
                    characterOffsetIndex: 2,
                    contentLength: 0),
            });
        }

        [Fact]
        public void CSharpChunkGeneratorDoesNotRenderLinePragmasIfGenerateLinePragmasIsSetToFalse()
        {
            RunTest("NoLinePragmas", generatePragmas: false);
        }

        [Fact]
        public void CSharpChunkGeneratorCorrectlyInstrumentsRazorCodeWhenInstrumentationRequested()
        {
            RunTest("Instrumented", hostConfig: host =>
            {
                host.InstrumentedSourceFilePath = string.Format("~/{0}.cshtml", host.DefaultClassName);

                return host;
            });
        }

        [Fact]
        public void CSharpChunkGeneratorGeneratesUrlsCorrectlyWithCommentsAndQuotes()
        {
            RunTest("HtmlCommentWithQuote_Single",
                    tabTest: TabTest.NoTabs);

            RunTest("HtmlCommentWithQuote_Double",
                    tabTest: TabTest.NoTabs);
        }

        [Fact]
        public void CSharpChunkGenerator_CorrectlyGeneratesAttributes_AtDesignTime()
        {
            var expectedDesignTimePragmas = new[]
            {
                BuildLineMapping(
                    documentAbsoluteIndex: 2,
                    documentLineIndex: 0,
                    generatedAbsoluteIndex: 572,
                    generatedLineIndex: 22,
                    characterOffsetIndex: 2,
                    contentLength: 48),
                BuildLineMapping(
                    documentAbsoluteIndex: 66,
                    documentLineIndex: 3,
                    generatedAbsoluteIndex: 715,
                    generatedLineIndex: 31,
                    characterOffsetIndex: 20,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 83,
                    documentLineIndex: 4,
                    generatedAbsoluteIndex: 811,
                    generatedLineIndex: 38,
                    characterOffsetIndex: 15,
                    contentLength: 3),
                BuildLineMapping(
                    documentAbsoluteIndex: 90,
                    documentLineIndex: 4,
                    generatedAbsoluteIndex: 910,
                    generatedLineIndex: 43,
                    characterOffsetIndex: 22,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 111,
                    documentLineIndex: 5,
                    generatedAbsoluteIndex: 1010,
                    generatedLineIndex: 50,
                    characterOffsetIndex: 19,
                    contentLength: 3),
                BuildLineMapping(
                    documentAbsoluteIndex: 118,
                    documentLineIndex: 5,
                    generatedAbsoluteIndex: 1113,
                    generatedLineIndex: 55,
                    characterOffsetIndex: 26,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 135,
                    documentLineIndex: 6,
                    generatedAbsoluteIndex: 1209,
                    generatedLineIndex: 62,
                    characterOffsetIndex: 15,
                    contentLength: 3),
                BuildLineMapping(
                    documentAbsoluteIndex: 146,
                    documentLineIndex: 6,
                    generatedAbsoluteIndex: 1312,
                    generatedLineIndex: 67,
                    characterOffsetIndex: 26,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 185,
                    documentLineIndex: 7,
                    generatedAbsoluteIndex: 1430,
                    generatedLineIndex: 74,
                    characterOffsetIndex: 37,
                    contentLength: 2),
                BuildLineMapping(
                    documentAbsoluteIndex: 191,
                    documentLineIndex: 7,
                    generatedAbsoluteIndex: 1549,
                    generatedLineIndex: 79,
                    characterOffsetIndex: 43,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 234,
                    documentLineIndex: 8,
                    generatedAbsoluteIndex: 1671,
                    generatedLineIndex: 86,
                    characterOffsetIndex: 41,
                    contentLength: 2),
                BuildLineMapping(
                    documentAbsoluteIndex: 240,
                    documentLineIndex: 8,
                    generatedAbsoluteIndex: 1794,
                    generatedLineIndex: 91,
                    characterOffsetIndex: 47,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 257,
                    documentLineIndex: 9,
                    documentCharacterOffsetIndex: 15,
                    generatedAbsoluteIndex: 1890,
                    generatedLineIndex: 98,
                    generatedCharacterOffsetIndex: 14,
                    contentLength: 18),
                BuildLineMapping(
                    documentAbsoluteIndex: 276,
                    documentLineIndex: 9,
                    generatedAbsoluteIndex: 2018,
                    generatedLineIndex: 104,
                    characterOffsetIndex: 34,
                    contentLength: 3),
                BuildLineMapping(
                    documentAbsoluteIndex: 279,
                    documentLineIndex: 9,
                    generatedAbsoluteIndex: 2133,
                    generatedLineIndex: 109,
                    characterOffsetIndex: 37,
                    contentLength: 2),
                BuildLineMapping(
                    documentAbsoluteIndex: 285,
                    documentLineIndex: 9,
                    generatedAbsoluteIndex: 2254,
                    generatedLineIndex: 115,
                    characterOffsetIndex: 43,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 309,
                    documentLineIndex: 10,
                    generatedAbsoluteIndex: 2358,
                    generatedLineIndex: 122,
                    characterOffsetIndex: 22,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 329,
                    documentLineIndex: 11,
                    generatedAbsoluteIndex: 2458,
                    generatedLineIndex: 129,
                    characterOffsetIndex: 18,
                    contentLength: 44),
                BuildLineMapping(
                    documentAbsoluteIndex: 407,
                    documentLineIndex: 11,
                    generatedAbsoluteIndex: 2673,
                    generatedLineIndex: 134,
                    characterOffsetIndex: 96,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 427,
                    documentLineIndex: 12,
                    generatedAbsoluteIndex: 2773,
                    generatedLineIndex: 141,
                    characterOffsetIndex: 18,
                    contentLength: 60),
                BuildLineMapping(
                    documentAbsoluteIndex: 521,
                    documentLineIndex: 12,
                    generatedAbsoluteIndex: 3020,
                    generatedLineIndex: 146,
                    characterOffsetIndex: 112,
                    contentLength: 6),
                BuildLineMapping(
                    documentAbsoluteIndex: 638,
                    documentLineIndex: 13,
                    generatedAbsoluteIndex: 3217,
                    generatedLineIndex: 153,
                    characterOffsetIndex: 115,
                    contentLength: 2),
            };

            RunTest("ConditionalAttributes",
                    baselineName: "ConditionalAttributes.DesignTime",
                    designTimeMode: true,
                    tabTest: TabTest.NoTabs,
                    expectedDesignTimePragmas: expectedDesignTimePragmas);
        }

        private void OpenedIf(bool withTabs)
        {
            var tabOffsetForMapping = 7;

            // where the test is running with tabs, the offset into the CS buffer changes for the whitespace mapping
            // with spaces we get 7xspace -> offset of 8 (column = offset+1)
            // with tabs we get tab + 3 spaces -> offset of 4 chars + 1 = 5
            if (withTabs)
            {
                tabOffsetForMapping -= 3;
            }

            RunTest("OpenedIf",
                "OpenedIf.DesignTime" + (withTabs ? ".Tabs" : ""),
                    designTimeMode: true,
                    tabTest: withTabs ? TabTest.Tabs : TabTest.NoTabs,
                    spans: new TestSpan[]
            {
                new TestSpan(SpanKind.Markup, 0, 6),
                new TestSpan(SpanKind.Markup, 6, 8),
                new TestSpan(SpanKind.Markup, 8, 14),
                new TestSpan(SpanKind.Markup, 14, 16),
                new TestSpan(SpanKind.Transition, 16, 17),
                new TestSpan(SpanKind.Code, 17, 31),
                new TestSpan(SpanKind.Markup, 31, 38),
                new TestSpan(SpanKind.Code, 38, 40),
                new TestSpan(SpanKind.Markup, 40, 47),
                new TestSpan(SpanKind.Code, 47, 47),
            },
            expectedDesignTimePragmas: new List<LineMapping>()
            {
                BuildLineMapping(17, 2, 1, 531, 22, 0, 14),
                BuildLineMapping(38, 3, 7, 605 + tabOffsetForMapping, 28, tabOffsetForMapping, 2),
                // Multiply the tab offset absolute index by 2 to account for the first mapping
                BuildLineMapping(47, 4, 7, 667 + tabOffsetForMapping * 2, 34, tabOffsetForMapping, 0)
            });
        }

        protected static LineMapping BuildLineMapping(int documentAbsoluteIndex,
                                                      int documentLineIndex,
                                                      int generatedAbsoluteIndex,
                                                      int generatedLineIndex,
                                                      int characterOffsetIndex,
                                                      int contentLength)
        {
            return BuildLineMapping(documentAbsoluteIndex,
                                    documentLineIndex,
                                    characterOffsetIndex,
                                    generatedAbsoluteIndex,
                                    generatedLineIndex,
                                    characterOffsetIndex,
                                    contentLength);
        }

        protected static LineMapping BuildLineMapping(int documentAbsoluteIndex,
                                                      int documentLineIndex,
                                                      int documentCharacterOffsetIndex,
                                                      int generatedAbsoluteIndex,
                                                      int generatedLineIndex,
                                                      int generatedCharacterOffsetIndex,
                                                      int contentLength)
        {
            return new LineMapping(
                        documentLocation: new MappingLocation(
                            new SourceLocation(documentAbsoluteIndex,
                                               documentLineIndex,
                                               documentCharacterOffsetIndex),
                            contentLength),
                        generatedLocation: new MappingLocation(
                            new SourceLocation(generatedAbsoluteIndex,
                                               generatedLineIndex,
                                               generatedCharacterOffsetIndex),
                        contentLength)
                    );
        }
    }
}
