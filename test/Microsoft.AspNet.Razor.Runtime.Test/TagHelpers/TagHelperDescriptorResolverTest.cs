// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Internal.Web.Utils;
using Xunit;

namespace Microsoft.AspNet.Razor.Runtime.Test.TagHelpers
{
    public class TagHelperDescriptorResolverTest
    {
        private static readonly CompleteTagHelperDescriptorComparer DefaultDescriptorComparer =
            new CompleteTagHelperDescriptorComparer();

        [Fact]
        public void DescriptorResolver_ResolvesOnlyTypeResolverProvidedTypes()
        {
            // Arrange
            var resolver = new TagHelperDescriptorResolver(
                new CustomTagHelperTypeResolver(
                    new Dictionary<string, IEnumerable<Type>>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "lookupText1", new Type[]{ typeof(string), typeof(int) } },
                        { "lookupText2", new Type[]{ typeof(object) } }
                    }));
            var expectedDescriptor = new TagHelperDescriptor("Object", "System.Object", ContentBehavior.None);

            // Act
            var descriptors = resolver.Resolve("lookupText2");

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Equal(descriptor, expectedDescriptor, DefaultDescriptorComparer);
        }

        [Fact]
        public void DescriptorResolver_ResolvesMultipleTypes()
        {
            // Arrange
            var resolver = new TagHelperDescriptorResolver(
                new CustomTagHelperTypeResolver(
                    new Dictionary<string, IEnumerable<Type>>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "lookupText", new Type[]{ typeof(string), typeof(int) } },
                    }));
            var expectedDescriptors = new TagHelperDescriptor[]
            {
                new TagHelperDescriptor("String", "System.String", ContentBehavior.None),
                new TagHelperDescriptor("Int32", "System.Int32", ContentBehavior.None),
            };

            // Act
            var descriptors = resolver.Resolve("lookupText").ToArray();

            // Assert
            Assert.Equal(descriptors.Length, 2);
            Assert.Equal(descriptors, expectedDescriptors, DefaultDescriptorComparer);
        }

        [Fact]
        public void DescriptorResolver_ResolvesCustomTypeWithConventionName()
        {
            // Arrange
            var resolver = new TagHelperDescriptorResolver(
                new CustomTagHelperTypeResolver(
                    new Dictionary<string, IEnumerable<Type>>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "lookupText", new Type[]{ typeof(SingleAttributeTagHelper) } },
                    }));
            var intProp = typeof(SingleAttributeTagHelper).GetProperty(nameof(SingleAttributeTagHelper.IntAttribute));
            var expectedDescriptor = new TagHelperDescriptor(
                "SingleAttribute",
                typeof(SingleAttributeTagHelper).FullName,
                ContentBehavior.None,
                new[] {
                    new TagHelperAttributeDescriptor(nameof(SingleAttributeTagHelper.IntAttribute), intProp)
                });

            // Act
            var descriptors = resolver.Resolve("lookupText").ToArray();

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Equal(descriptor, expectedDescriptor, DefaultDescriptorComparer);
        }

        [Fact]
        public void DescriptorResolver_OnlyResolvesPropertiesWithGetAndSet()
        {
            // Arrange
            var resolver = new TagHelperDescriptorResolver(
                new CustomTagHelperTypeResolver(
                    new Dictionary<string, IEnumerable<Type>>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "lookupText", new Type[]{ typeof(TwoInvalidAttributeTagHelper) } },
                    }));
            var validProp = typeof(TwoInvalidAttributeTagHelper).GetProperty(nameof(TwoInvalidAttributeTagHelper.ValidAttribute));
            var expectedDescriptor = new TagHelperDescriptor(
                "TwoInvalidAttribute",
                typeof(TwoInvalidAttributeTagHelper).FullName,
                ContentBehavior.None,
                new[] {
                    new TagHelperAttributeDescriptor(nameof(TwoInvalidAttributeTagHelper.ValidAttribute), validProp)
                });

            // Act
            var descriptors = resolver.Resolve("lookupText").ToArray();

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Equal(descriptor, expectedDescriptor, DefaultDescriptorComparer);
        }

        [Fact]
        public void DescriptorResolver_DoesntResolveTypesForInvalidLookupText()
        {
            // Arrange
            var resolver = new TagHelperDescriptorResolver(
                new CustomTagHelperTypeResolver(
                    new Dictionary<string, IEnumerable<Type>>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "lookupText1", new Type[]{ typeof(string), typeof(int) } },
                        { "lookupText2", new Type[]{ typeof(object) } }
                    }));

            // Act
            var descriptors = resolver.Resolve("lookupText").ToArray();

            // Assert
            Assert.Empty(descriptors);
        }

        private class CustomTagHelperTypeResolver : ITagHelperTypeResolver
        {
            private Dictionary<string, IEnumerable<Type>> _lookupValues;

            public CustomTagHelperTypeResolver(Dictionary<string, IEnumerable<Type>> lookupValues)
            {
                _lookupValues = lookupValues;
            }

            public IEnumerable<Type> Resolve(string lookupText)
            {
                IEnumerable<Type> types;

                _lookupValues.TryGetValue(lookupText, out types);

                return types ?? Enumerable.Empty<Type>();
            }
        }

        private class SingleAttributeTagHelper
        {
            public int IntAttribute { get; set; }
        }

        private class TwoInvalidAttributeTagHelper
        {
            public string ValidAttribute { get; set; }
            public string InvalidNoGetAttribute { set { } }
            public string InvalidNoSetAttribute { get { return string.Empty; } }
        }

        private class CompleteTagHelperDescriptorComparer : TagHelperDescriptorComparer
        {
            public new bool Equals(TagHelperDescriptor descriptorX, TagHelperDescriptor descriptorY)
            {
                return base.Equals(descriptorX, descriptorY) &&
                       descriptorX.Attributes.SequenceEqual(descriptorY.Attributes,
                                                            CompleteTagHelperAttributeDescriptorComparer.Default);
            }

            public new int GetHashCode(TagHelperDescriptor descriptor)
            {
                return HashCodeCombiner.Start()
                                       .Add(base.GetHashCode())
                                       .Add(descriptor.Attributes)
                                       .CombinedHash;
            }
        }

        private class CompleteTagHelperAttributeDescriptorComparer : IEqualityComparer<TagHelperAttributeDescriptor>
        {
            public static readonly CompleteTagHelperAttributeDescriptorComparer Default =
                new CompleteTagHelperAttributeDescriptorComparer();

            public bool Equals(TagHelperAttributeDescriptor descriptorX, TagHelperAttributeDescriptor descriptorY)
            {
                return descriptorX.AttributeName == descriptorY.AttributeName &&
                       descriptorX.AttributePropertyName == descriptorY.AttributePropertyName;
            }

            public int GetHashCode(TagHelperAttributeDescriptor descriptor)
            {
                return HashCodeCombiner.Start()
                                       .Add(descriptor.AttributeName)
                                       .Add(descriptor.AttributePropertyName)
                                       .CombinedHash;
            }
        }
    }
}