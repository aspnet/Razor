﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Evolution.Intermediate;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.AspNetCore.Razor.Evolution.IntegrationTests
{
    public class CodeGenerationIntegrationTest : IntegrationTestBase
    {
        #region Runtime
        [Fact]
        public void BasicImports_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void UnfinishedExpressionInCode_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void Templates_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void StringLiterals_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void SimpleUnspacedIf_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void Sections_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void RazorComments_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void ParserError_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void OpenedIf_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void NullConditionalExpressions_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void NoLinePragmas_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void NestedCSharp_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void NestedCodeBlocks_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void MarkupInCodeBlock_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void Instrumented_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void InlineBlocks_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void Inherits_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void Usings_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void ImplicitExpressionAtEOF_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void ImplicitExpression_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void HtmlCommentWithQuote_Double_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void HtmlCommentWithQuote_Single_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void HiddenSpansInCode_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void FunctionsBlock_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void FunctionsBlockMinimal_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void ExpressionsInCode_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void ExplicitExpressionWithMarkup_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void ExplicitExpressionAtEOF_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void ExplicitExpression_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void EmptyImplicitExpressionInCode_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void EmptyImplicitExpression_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void EmptyExplicitExpression_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void EmptyCodeBlock_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void ConditionalAttributes_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void CodeBlockWithTextElement_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void CodeBlockAtEOF_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void CodeBlock_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void Blocks_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void Await_Runtime()
        {
            // Arrange
            var engine = RazorEngine.Create(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        [Fact]
        public void SimpleTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.SimpleTagHelperDescriptors);
        }

        [Fact]
        public void TagHelpersWithBoundAttributes_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.SimpleTagHelperDescriptors);
        }

        [Fact]
        public void TagHelpersWithPrefix_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.SimpleTagHelperDescriptors);
        }

        [Fact]
        public void NestedTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.SimpleTagHelperDescriptors);
        }

        [Fact]
        public void SingleTagHelper_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void SingleTagHelperWithNewlineBeforeAttributes_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void TagHelpersWithWeirdlySpacedAttributes_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void IncompleteTagHelper_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void BasicTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void BasicTagHelpers_Prefixed_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors, tagHelperPrefix: "THS");
        }

        [Fact]
        public void BasicTagHelpers_RemoveTagHelper_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void CssSelectorTagHelperAttributes_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.CssSelectorTagHelperDescriptors);
        }

        [Fact]
        public void ComplexTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void EmptyAttributeTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void EscapedTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void AttributeTargetingTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.AttributeTargetingTagHelperDescriptors);
        }

        [Fact]
        public void PrefixedAttributeTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.PrefixedAttributeTagHelperDescriptors);
        }

        [Fact]
        public void DuplicateAttributeTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void DynamicAttributeTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DynamicAttributeTagHelpers_Descriptors);
        }

        [Fact]
        public void TransitionsInTagHelperAttributes_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void MinimizedTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.MinimizedTagHelpers_Descriptors);
        }

        [Fact]
        public void NestedScriptTagTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void SymbolBoundAttributes_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.SymbolBoundTagHelperDescriptors);
        }

        [Fact]
        public void EnumTagHelpers_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.EnumTagHelperDescriptors);
        }

        [Fact]
        public void TagHelpersInSection_Runtime()
        {
            // Arrange, Act & Assert
            RunRuntimeTagHelpersTest(TestTagHelperDescriptors.TagHelpersInSectionDescriptors);
        }
        #endregion

        #region DesignTime
        [Fact]
        public void BasicImports_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void UnfinishedExpressionInCode_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void Templates_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void StringLiterals_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void SimpleUnspacedIf_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void Sections_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void RazorComments_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void ParserError_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void OpenedIf_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void NullConditionalExpressions_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void NoLinePragmas_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void NestedCSharp_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void NestedCodeBlocks_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void MarkupInCodeBlock_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void Instrumented_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void InlineBlocks_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void Inherits_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void Usings_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void ImplicitExpressionAtEOF_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void ImplicitExpression_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void HtmlCommentWithQuote_Double_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void HtmlCommentWithQuote_Single_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void HiddenSpansInCode_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void FunctionsBlock_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void FunctionsBlockMinimal_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void ExpressionsInCode_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void ExplicitExpressionWithMarkup_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void ExplicitExpressionAtEOF_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void ExplicitExpression_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void EmptyImplicitExpressionInCode_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void EmptyImplicitExpression_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void EmptyExplicitExpression_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void EmptyCodeBlock_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void DesignTime_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void ConditionalAttributes_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void CodeBlockWithTextElement_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void CodeBlockAtEOF_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void CodeBlock_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void Blocks_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void Await_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void AddTagHelperDirective_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void RemoveTagHelperDirective_DesignTime()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder => builder.Features.Add(new ApiSetsIRTestAdapter()));
            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        [Fact]
        public void SimpleTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.SimpleTagHelperDescriptors);
        }

        [Fact]
        public void TagHelpersWithBoundAttributes_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.SimpleTagHelperDescriptors);
        }

        [Fact]
        public void TagHelpersWithPrefix_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.SimpleTagHelperDescriptors);
        }

        [Fact]
        public void NestedTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.SimpleTagHelperDescriptors);
        }

        [Fact]
        public void SingleTagHelper_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void SingleTagHelperWithNewlineBeforeAttributes_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void TagHelpersWithWeirdlySpacedAttributes_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void IncompleteTagHelper_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void BasicTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void BasicTagHelpers_Prefixed_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors, tagHelperPrefix: "THS");
        }

        [Fact]
        public void ComplexTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void EmptyAttributeTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void EscapedTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void AttributeTargetingTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.AttributeTargetingTagHelperDescriptors);
        }

        [Fact]
        public void PrefixedAttributeTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.PrefixedAttributeTagHelperDescriptors);
        }

        [Fact]
        public void DuplicateAttributeTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void DynamicAttributeTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DynamicAttributeTagHelpers_Descriptors);
        }

        [Fact]
        public void TransitionsInTagHelperAttributes_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void MinimizedTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.MinimizedTagHelpers_Descriptors);
        }

        [Fact]
        public void NestedScriptTagTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.DefaultPAndInputTagHelperDescriptors);
        }

        [Fact]
        public void SymbolBoundAttributes_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.SymbolBoundTagHelperDescriptors);
        }

        [Fact]
        public void EnumTagHelpers_DesignTime()
        {
            // Arrange, Act & Assert
            RunDesignTimeTagHelpersTest(TestTagHelperDescriptors.EnumTagHelperDescriptors);
        }
        #endregion

        protected override RazorCodeDocument CreateCodeDocument()
        {
            if (Filename == null)
            {
                var message = $"{nameof(CreateCodeDocument)} should only be called from an integration test ({nameof(Filename)} is null).";
                throw new InvalidOperationException(message);
            }

            var normalizedFileName = Filename.Substring(0, Filename.LastIndexOf("_"));
            var sourceFilename = Path.ChangeExtension(normalizedFileName, ".cshtml");
            var testFile = TestFile.Create(sourceFilename);
            if (!testFile.Exists())
            {
                throw new XunitException($"The resource {sourceFilename} was not found.");
            }

            var imports = new List<RazorSourceDocument>();
            while (true)
            {
                var importsFilename = Path.ChangeExtension(normalizedFileName + "_Imports" + imports.Count.ToString(), ".cshtml");
                if (!TestFile.Create(importsFilename).Exists())
                {
                    break;
                }

                imports.Add(
                    TestRazorSourceDocument.CreateResource(importsFilename, encoding: null, normalizeNewLines: true));
            }

            var codeDocument = RazorCodeDocument.Create(
                TestRazorSourceDocument.CreateResource(sourceFilename, encoding: null, normalizeNewLines: true), imports);

            // This will ensure that we're not putting any randomly generated data in a baseline.
            codeDocument.Items[DefaultRazorCSharpLoweringPhase.SuppressUniqueIds] = "test";

            // This is to make tests work cross platform.
            codeDocument.Items[DefaultRazorCSharpLoweringPhase.NewLineString] = "\r\n";

            return codeDocument;
        }

        private void RunRuntimeTagHelpersTest(IEnumerable<TagHelperDescriptor> descriptors, string tagHelperPrefix = null)
        {
            // Arrange
            var engine = RazorEngine.Create(
                builder =>
                {
                    builder.Features.Add(new ApiSetsIRTestAdapter());
                    builder.AddTagHelpers(descriptors);
                });
            var document = CreateCodeDocument();
            if (tagHelperPrefix != null)
            {
                document.SetTagHelperPrefix(tagHelperPrefix);
            }

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
        }

        private void RunDesignTimeTagHelpersTest(IEnumerable<TagHelperDescriptor> descriptors, string tagHelperPrefix = null)
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(
                builder =>
                {
                    builder.Features.Add(new ApiSetsIRTestAdapter());
                    builder.AddTagHelpers(descriptors);
                });
            var document = CreateCodeDocument();
            if (tagHelperPrefix != null)
            {
                document.SetTagHelperPrefix(tagHelperPrefix);
            }

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        private class ApiSetsIRTestAdapter : RazorIRPassBase, IRazorIROptimizationPass
        {
            public override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIRNode irDocument)
            {
                var walker = new ApiSetsIRWalker();
                walker.Visit(irDocument);
            }

            private class ApiSetsIRWalker : RazorIRNodeWalker
            {
                public override void VisitClass(ClassDeclarationIRNode node)
                {
                    node.Name = Filename.Replace('/', '_');
                    node.AccessModifier = "public";

                    VisitDefault(node);
                }

                public override void VisitNamespace(NamespaceDeclarationIRNode node)
                {
                    node.Content = typeof(CodeGenerationIntegrationTest).Namespace + ".TestFiles";

                    VisitDefault(node);
                }

                public override void VisitRazorMethodDeclaration(RazorMethodDeclarationIRNode node)
                {
                    node.AccessModifier = "public";
                    node.Modifiers = new[] { "async" };
                    node.ReturnType = typeof(Task).FullName;
                    node.Name = "ExecuteAsync";

                    VisitDefault(node);
                }
            }
        }
    }
}
