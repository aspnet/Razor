﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.AspNet.Razor.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    public class TagHelperDescriptorFactoryTest
    {
        private static readonly string AssemblyName =
            typeof(TagHelperDescriptorFactoryTest).GetTypeInfo().Assembly.GetName().Name;

        [Fact]
        public void CreateDescriptor_OverridesAttributeNameFromAttribute()
        {
            // Arrange
            var validProperty1 = typeof(OverriddenAttributeTagHelper).GetProperty(
                nameof(OverriddenAttributeTagHelper.ValidAttribute1));
            var validProperty2 = typeof(OverriddenAttributeTagHelper).GetProperty(
                nameof(OverriddenAttributeTagHelper.ValidAttribute2));
            var expectedDescriptors = new[] {
                new TagHelperDescriptor(
                    "OverriddenAttribute",
                    typeof(OverriddenAttributeTagHelper).FullName,
                    AssemblyName,
                    new[] {
                        new TagHelperAttributeDescriptor("SomethingElse", validProperty1),
                        new TagHelperAttributeDescriptor("Something-Else", validProperty2)
                    })
            };

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(OverriddenAttributeTagHelper));

            // Assert
            Assert.Equal(expectedDescriptors, descriptors, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void CreateDescriptor_DoesNotInheritOverridenAttributeName()
        {
            // Arrange
            var validProperty1 = typeof(InheritedOverriddenAttributeTagHelper).GetProperty(
                nameof(InheritedOverriddenAttributeTagHelper.ValidAttribute1));
            var validProperty2 = typeof(InheritedOverriddenAttributeTagHelper).GetProperty(
                nameof(InheritedOverriddenAttributeTagHelper.ValidAttribute2));
            var expectedDescriptors = new[] {
                new TagHelperDescriptor(
                    "InheritedOverriddenAttribute",
                    typeof(InheritedOverriddenAttributeTagHelper).FullName,
                    AssemblyName,
                    new[] {
                        new TagHelperAttributeDescriptor(nameof(InheritedOverriddenAttributeTagHelper.ValidAttribute1),
                                                         validProperty1),
                        new TagHelperAttributeDescriptor("Something-Else", validProperty2)
                    })
            };

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(InheritedOverriddenAttributeTagHelper));

            // Assert
            Assert.Equal(expectedDescriptors, descriptors, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void CreateDescriptor_AllowsOverridenAttributeNameOnUnimplementedVirtual()
        {
            // Arrange
            var validProperty1 = typeof(InheritedNotOverriddenAttributeTagHelper).GetProperty(
                nameof(InheritedNotOverriddenAttributeTagHelper.ValidAttribute1));
            var validProperty2 = typeof(InheritedNotOverriddenAttributeTagHelper).GetProperty(
                nameof(InheritedNotOverriddenAttributeTagHelper.ValidAttribute2));
            var expectedDescriptors = new[] {
                new TagHelperDescriptor(
                    "InheritedNotOverriddenAttribute",
                    typeof(InheritedNotOverriddenAttributeTagHelper).FullName,
                    AssemblyName,
                    new[] {
                        new TagHelperAttributeDescriptor("SomethingElse", validProperty1),
                        new TagHelperAttributeDescriptor("Something-Else", validProperty2)
                    })
            };

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(InheritedNotOverriddenAttributeTagHelper));

            // Assert
            Assert.Equal(expectedDescriptors, descriptors, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void CreateDescriptor_BuildsDescriptorsFromSimpleTypes()
        {
            // Arrange
            var objectAssemblyName = typeof(object).GetTypeInfo().Assembly.GetName().Name;
            var expectedDescriptor =
                new TagHelperDescriptor("Object", "System.Object", objectAssemblyName);

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(object));

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Equal(expectedDescriptor, descriptor, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void CreateDescriptor_BuildsDescriptorsWithInheritedProperties()
        {
            // Arrange
            var intProperty = typeof(InheritedSingleAttributeTagHelper).GetProperty(
                nameof(InheritedSingleAttributeTagHelper.IntAttribute));
            var expectedDescriptor = new TagHelperDescriptor(
                "InheritedSingleAttribute",
                typeof(InheritedSingleAttributeTagHelper).FullName,
                AssemblyName,
                new[] {
                    new TagHelperAttributeDescriptor(nameof(InheritedSingleAttributeTagHelper.IntAttribute), intProperty)
                });

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(InheritedSingleAttributeTagHelper));

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Equal(expectedDescriptor, descriptor, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void CreateDescriptor_BuildsDescriptorsWithConventionNames()
        {
            // Arrange
            var intProperty = typeof(SingleAttributeTagHelper).GetProperty(nameof(SingleAttributeTagHelper.IntAttribute));
            var expectedDescriptor = new TagHelperDescriptor(
                "SingleAttribute",
                typeof(SingleAttributeTagHelper).FullName,
                AssemblyName,
                new[] {
                    new TagHelperAttributeDescriptor(nameof(SingleAttributeTagHelper.IntAttribute), intProperty)
                });

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(SingleAttributeTagHelper));

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Equal(expectedDescriptor, descriptor, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void CreateDescriptor_OnlyAcceptsPropertiesWithGetAndSet()
        {
            // Arrange
            var validProperty = typeof(MissingAccessorTagHelper).GetProperty(
                nameof(MissingAccessorTagHelper.ValidAttribute));
            var expectedDescriptor = new TagHelperDescriptor(
                "MissingAccessor",
                typeof(MissingAccessorTagHelper).FullName,
                AssemblyName,
                new[] {
                    new TagHelperAttributeDescriptor(nameof(MissingAccessorTagHelper.ValidAttribute), validProperty)
                });

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(MissingAccessorTagHelper));

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Equal(expectedDescriptor, descriptor, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void CreateDescriptor_OnlyAcceptsPropertiesWithPublicGetAndSet()
        {
            // Arrange
            var validProperty = typeof(PrivateAccessorTagHelper).GetProperty(
                nameof(PrivateAccessorTagHelper.ValidAttribute));
            var expectedDescriptor = new TagHelperDescriptor(
                "PrivateAccessor",
                typeof(PrivateAccessorTagHelper).FullName,
                AssemblyName,
                new[] {
                    new TagHelperAttributeDescriptor(
                        nameof(PrivateAccessorTagHelper.ValidAttribute), validProperty)
                });

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(PrivateAccessorTagHelper));

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Equal(expectedDescriptor, descriptor, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void CreateDescriptor_ResolvesMultipleTagHelperDescriptorsFromSingleType()
        {
            // Arrange
            var validProp = typeof(MultiTagTagHelper).GetProperty(nameof(MultiTagTagHelper.ValidAttribute));
            var expectedDescriptors = new[] {
                new TagHelperDescriptor(
                    "div",
                    typeof(MultiTagTagHelper).FullName,
                    AssemblyName,
                    new[] {
                        new TagHelperAttributeDescriptor(nameof(MultiTagTagHelper.ValidAttribute), validProp)
                    }),
                new TagHelperDescriptor(
                    "p",
                    typeof(MultiTagTagHelper).FullName,
                    AssemblyName,
                    new[] {
                        new TagHelperAttributeDescriptor(nameof(MultiTagTagHelper.ValidAttribute), validProp)
                    })
            };

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(MultiTagTagHelper));

            // Assert
            Assert.Equal(expectedDescriptors, descriptors, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void CreateDescriptor_DoesntResolveInheritedTagNames()
        {
            // Arrange
            var validProp = typeof(InheritedMultiTagTagHelper).GetProperty(nameof(InheritedMultiTagTagHelper.ValidAttribute));
            var expectedDescriptor = new TagHelperDescriptor(
                    "InheritedMultiTag",
                    typeof(InheritedMultiTagTagHelper).FullName,
                    AssemblyName,
                    new[] {
                        new TagHelperAttributeDescriptor(nameof(InheritedMultiTagTagHelper.ValidAttribute), validProp)
                    });

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(InheritedMultiTagTagHelper));

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Equal(expectedDescriptor, descriptor, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void CreateDescriptor_IgnoresDuplicateTagNamesFromAttribute()
        {
            // Arrange
            var expectedDescriptors = new[] {
                new TagHelperDescriptor(
                    "p",
                    typeof(DuplicateTagNameTagHelper).FullName,
                    AssemblyName),
                new TagHelperDescriptor(
                    "div",
                    typeof(DuplicateTagNameTagHelper).FullName,
                    AssemblyName)
            };

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(DuplicateTagNameTagHelper));

            // Assert
            Assert.Equal(expectedDescriptors, descriptors, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void CreateDescriptor_OverridesTagNameFromAttribute()
        {
            // Arrange
            var expectedDescriptors = new[] {
                new TagHelperDescriptor("data-condition",
                                        typeof(OverrideNameTagHelper).FullName,
                                        AssemblyName),
            };

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(OverrideNameTagHelper));

            // Assert
            Assert.Equal(expectedDescriptors, descriptors, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void CreateDescriptor_GetsTagNamesFromMultipleAttributes()
        {
            // Arrange
            var expectedDescriptors = new[] {
                new TagHelperDescriptor(
                    "span",
                    typeof(MultipleAttributeTagHelper).FullName,
                    AssemblyName),
                new TagHelperDescriptor(
                    "p",
                    typeof(MultipleAttributeTagHelper).FullName,
                    AssemblyName),
                new TagHelperDescriptor(
                    "div",
                    typeof(MultipleAttributeTagHelper).FullName,
                    AssemblyName)
            };

            // Act
            var descriptors = TagHelperDescriptorFactory.CreateDescriptors(typeof(MultipleAttributeTagHelper));

            // Assert
            Assert.Equal(expectedDescriptors, descriptors, CompleteTagHelperDescriptorComparer.Default);
        }

        [TagName("p", "div")]
        private class MultiTagTagHelper
        {
            public string ValidAttribute { get; set; }
        }

        private class InheritedMultiTagTagHelper : MultiTagTagHelper
        {
        }

        [TagName("p", "p", "div", "div")]
        private class DuplicateTagNameTagHelper
        {
        }

        [TagName("data-condition")]
        private class OverrideNameTagHelper
        {
        }

        [TagName("span")]
        [TagName("div", "p")]
        private class MultipleAttributeTagHelper
        {
        }

        private class InheritedSingleAttributeTagHelper : SingleAttributeTagHelper
        {
        }

        private class OverriddenAttributeTagHelper
        {
            [HtmlAttributeName("SomethingElse")]
            public virtual string ValidAttribute1 { get; set; }

            [HtmlAttributeName("Something-Else")]
            public string ValidAttribute2 { get; set; }
        }

        private class InheritedOverriddenAttributeTagHelper : OverriddenAttributeTagHelper
        {
            public override string ValidAttribute1 { get; set; }
        }

        private class InheritedNotOverriddenAttributeTagHelper : OverriddenAttributeTagHelper
        {
        }
    }
}