// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.Razor.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Generator
{
    // Really tests underlying BlockCodeGenerator
    public class RazorCommentCodeGeneratorTest
    {
        public static TheoryData<RazorCommentCodeGenerator, IBlockCodeGenerator> MatchingTestDataSet
        {
            get
            {
                return new TheoryData<RazorCommentCodeGenerator, IBlockCodeGenerator>
                {
                    { new RazorCommentCodeGenerator(), new RazorCommentCodeGenerator() },

                    // BlockCodeGenerator.Equals() only checks whether (other is IBlockCodeGenerator).
                    {
                        new RazorCommentCodeGenerator(),
                        new TagHelperCodeGenerator(Enumerable.Empty<TagHelperDescriptor>())
                    },
                    { new RazorCommentCodeGenerator(), new TemplateBlockCodeGenerator() },
                };
            }
        }

        // Following cases involve other IBlockCodeGenerator that BlockCodeGenerator thinks are equal. But they have
        // their own Equals() and GetHashCode() implementations, leading to X==Y but Y!=X and different hash codes.
        public static TheoryData<RazorCommentCodeGenerator, IBlockCodeGenerator> SlightlyMatchingTestDataSet
        {
            get
            {
                return new TheoryData<RazorCommentCodeGenerator, IBlockCodeGenerator>
                {
                    { new RazorCommentCodeGenerator(), BlockCodeGenerator.Null },
                    {
                        new RazorCommentCodeGenerator(),
                        new AttributeBlockCodeGenerator(name: null, prefix: null, suffix: null)
                    },
                    {
                        new RazorCommentCodeGenerator(),
                        new DynamicAttributeBlockCodeGenerator(prefix: null, offset: 0, line: 0, col: 0)
                    },
                    { new RazorCommentCodeGenerator(), new ExpressionCodeGenerator() },
                    { new RazorCommentCodeGenerator(), new SectionCodeGenerator(sectionName: null) },
                };
            }
        }

        public static TheoryData<IBlockCodeGenerator, object> NonMatchingTestDataSet
        {
            get
            {
                return new TheoryData<IBlockCodeGenerator, object>
                {
                    { new RazorCommentCodeGenerator(), null },
                    {
                        new RazorCommentCodeGenerator(),
                        new AddImportCodeGenerator(ns: "Fred", namespaceKeywordLength: 0)
                    },
                    { BlockCodeGenerator.Null, new object() },
                };
            }
        }

        [Theory]
        [MemberData(nameof(MatchingTestDataSet))]
        [MemberData(nameof(SlightlyMatchingTestDataSet))]
        public void Equals_True_WhenExpected(RazorCommentCodeGenerator leftObject, IBlockCodeGenerator rightObject)
        {
            // Arrange & Act
            var result = leftObject.Equals(rightObject);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(MatchingTestDataSet))]
        public void Equals_True_Symmetric(RazorCommentCodeGenerator leftObject, IBlockCodeGenerator rightObject)
        {
            // Arrange & Act
            var result = rightObject.Equals(leftObject);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(SlightlyMatchingTestDataSet))]
        public void Equals_False_NotSymmetric(RazorCommentCodeGenerator leftObject, IBlockCodeGenerator rightObject)
        {
            // Arrange & Act
            var result = rightObject.Equals(leftObject);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(NonMatchingTestDataSet))]
        public void Equals_False_WhenExpected(IBlockCodeGenerator leftObject, object rightObject)
        {
            // Arrange & Act
            var result = leftObject.Equals(rightObject);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(MatchingTestDataSet))]
        public void GetHashCode_ReturnsSameValue_WhenEqual(
            RazorCommentCodeGenerator leftObject,
            IBlockCodeGenerator rightObject)
        {
            // Arrange & Act
            var leftResult = leftObject.GetHashCode();
            var rightResult = rightObject.GetHashCode();

            // Assert
            Assert.Equal(leftResult, rightResult);
        }

        [Theory]
        [MemberData(nameof(SlightlyMatchingTestDataSet))]
        public void GetHashCode_ReturnsDifferentValues_NotSymmetric(
            RazorCommentCodeGenerator leftObject,
            IBlockCodeGenerator rightObject)
        {
            // Arrange & Act
            var leftResult = leftObject.GetHashCode();
            var rightResult = rightObject.GetHashCode();

            // Assert
            Assert.NotEqual(leftResult, rightResult);
        }
    }
}
