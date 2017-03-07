//// Copyright (c) .NET Foundation. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//using System;
//using System.Collections.Generic;
//using Microsoft.AspNetCore.Razor.Evolution.Legacy;
//using Newtonsoft.Json;
//using Xunit;

//namespace Microsoft.AspNetCore.Razor.Evolution
//{
//    public class TagHelperDescriptorTest
//    {
//        [Fact]
//        public void Constructor_CorrectlyCreatesCopy()
//        {
//            // Arrange
//            var descriptor = new TagHelperDescriptor
//            {
//                Prefix = "prefix",
//                TagName = "tag-name",
//                TypeName = "TypeName",
//                AssemblyName = "AsssemblyName",
//                Attributes = new List<BoundAttributeDescriptor>
//                {
//                    new BoundAttributeDescriptor
//                    {
//                        Name = "test-attribute",
//                        PropertyName = "TestAttribute",
//                        TypeName = "string"
//                    }
//                },
//                RequiredAttributes = new List<RequiredAttributeDescriptor>
//                {
//                    new RequiredAttributeDescriptor
//                    {
//                        Name = "test-required-attribute"
//                    }
//                },
//                AllowedChildren = new[] { "child" },
//                RequiredParent = "required parent",
//                TagStructure = TagStructure.NormalOrSelfClosing,
//                DesignTimeDescriptor = new TagHelperDesignTimeDescriptor()
//            };

//            descriptor.Metadata.Add("foo", "bar");

//            // Act
//            var copyDescriptor = new TagHelperDescriptor(descriptor);

//            // Assert
//            Assert.Equal(descriptor, copyDescriptor, CaseSensitiveTagHelperDescriptorComparer.Default);
//            Assert.Same(descriptor.BoundAttributes, copyDescriptor.BoundAttributes);
//            Assert.Same(descriptor.RequiredAttributes, copyDescriptor.RequiredAttributes);
//        }

//        [Fact]
//        public void TagHelperDescriptor_CanBeSerialized()
//        {
//            // Arrange
//            var descriptor = new TagHelperDescriptor
//            {
//                Prefix = "prefix:",
//                TagName = "tag name",
//                TypeName = "type name",
//                AssemblyName = "assembly name",
//                RequiredAttributes = new[]
//                {
//                    new RequiredAttributeDescriptor
//                    {
//                        Name = "required attribute one",
//                        NameComparison = TagHelperRequiredAttributeNameComparison.PrefixMatch
//                    },
//                    new RequiredAttributeDescriptor
//                    {
//                        Name = "required attribute two",
//                        NameComparison = TagHelperRequiredAttributeNameComparison.FullMatch,
//                        Value = "something",
//                        ValueComparison = TagHelperRequiredAttributeValueComparison.PrefixMatch,
//                    }
//                },
//                AllowedChildren = new[] { "allowed child one" },
//                RequiredParent = "parent name",
//                DesignTimeDescriptor = new TagHelperDesignTimeDescriptor
//                {
//                    Summary = "usage summary",
//                    Remarks = "usage remarks",
//                    OutputElementHint = "some-tag"
//                },
//            };

//            var expectedSerializedDescriptor =
//                $"{{\"{ nameof(TagHelperDescriptor.Prefix) }\":\"prefix:\"," +
//                $"\"{ nameof(TagHelperDescriptor.TagName) }\":\"tag name\"," +
//                $"\"{ nameof(TagHelperDescriptor.FullTagName) }\":\"prefix:tag name\"," +
//                $"\"{ nameof(TagHelperDescriptor.TypeName) }\":\"type name\"," +
//                $"\"{ nameof(TagHelperDescriptor.AssemblyName) }\":\"assembly name\"," +
//                $"\"{ nameof(TagHelperDescriptor.BoundAttributes) }\":[]," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredAttributes) }\":" +
//                $"[{{\"{ nameof(RequiredAttributeDescriptor.Name)}\":\"required attribute one\"," +
//                $"\"{ nameof(RequiredAttributeDescriptor.NameComparison) }\":1," +
//                $"\"{ nameof(RequiredAttributeDescriptor.Value) }\":null," +
//                $"\"{ nameof(RequiredAttributeDescriptor.ValueComparison) }\":0}}," +
//                $"{{\"{ nameof(RequiredAttributeDescriptor.Name)}\":\"required attribute two\"," +
//                $"\"{ nameof(RequiredAttributeDescriptor.NameComparison) }\":0," +
//                $"\"{ nameof(RequiredAttributeDescriptor.Value) }\":\"something\"," +
//                $"\"{ nameof(RequiredAttributeDescriptor.ValueComparison) }\":2}}]," +
//                $"\"{ nameof(TagHelperDescriptor.AllowedChildTags) }\":[\"allowed child one\"]," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredParent) }\":\"parent name\"," +
//                $"\"{ nameof(TagHelperDescriptor.TagStructure) }\":0," +
//                $"\"{ nameof(TagHelperDescriptor.DesignTimeDescriptor) }\":{{" +
//                $"\"{ nameof(TagHelperDesignTimeDescriptor.Summary) }\":\"usage summary\"," +
//                $"\"{ nameof(TagHelperDesignTimeDescriptor.Remarks) }\":\"usage remarks\"," +
//                $"\"{ nameof(TagHelperDesignTimeDescriptor.OutputElementHint) }\":\"some-tag\"}}," +
//                $"\"{ nameof(TagHelperDescriptor.Metadata) }\":{{}}}}";

//            // Act
//            var serializedDescriptor = JsonConvert.SerializeObject(descriptor);

//            // Assert
//            Assert.Equal(expectedSerializedDescriptor, serializedDescriptor, StringComparer.Ordinal);
//        }

//        [Fact]
//        public void TagHelperDescriptor_WithAttributes_CanBeSerialized()
//        {
//            // Arrange
//            var descriptor = new TagHelperDescriptor
//            {
//                Prefix = "prefix:",
//                TagName = "tag name",
//                TypeName = "type name",
//                AssemblyName = "assembly name",
//                Attributes = new[]
//                {
//                   new BoundAttributeDescriptor
//                   {
//                        Name = "attribute one",
//                        PropertyName = "property name",
//                        TypeName = "property type name",
//                        IsEnum = true,
//                   },
//                    new BoundAttributeDescriptor
//                   {
//                        Name = "attribute two",
//                        PropertyName = "property name",
//                        TypeName = typeof(string).FullName,
//                        IsStringProperty = true
//                    },
//                },
//                TagStructure = TagStructure.NormalOrSelfClosing
//            };

//            var expectedSerializedDescriptor =
//                $"{{\"{ nameof(TagHelperDescriptor.Prefix) }\":\"prefix:\"," +
//                $"\"{ nameof(TagHelperDescriptor.TagName) }\":\"tag name\"," +
//                $"\"{ nameof(TagHelperDescriptor.FullTagName) }\":\"prefix:tag name\"," +
//                $"\"{ nameof(TagHelperDescriptor.TypeName) }\":\"type name\"," +
//                $"\"{ nameof(TagHelperDescriptor.AssemblyName) }\":\"assembly name\"," +
//                $"\"{ nameof(TagHelperDescriptor.BoundAttributes) }\":[" +
//                $"{{\"{ nameof(BoundAttributeDescriptor.IsIndexer) }\":false," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsEnum) }\":true," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsStringProperty) }\":false," +
//                $"\"{ nameof(BoundAttributeDescriptor.Name) }\":\"attribute one\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.PropertyName) }\":\"property name\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.TypeName) }\":\"property type name\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.DesignTimeDescriptor) }\":null}}," +
//                $"{{\"{ nameof(BoundAttributeDescriptor.IsIndexer) }\":false," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsEnum) }\":false," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsStringProperty) }\":true," +
//                $"\"{ nameof(BoundAttributeDescriptor.Name) }\":\"attribute two\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.PropertyName) }\":\"property name\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.TypeName) }\":\"{ typeof(string).FullName }\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.DesignTimeDescriptor) }\":null}}]," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredAttributes) }\":[]," +
//                $"\"{ nameof(TagHelperDescriptor.AllowedChildTags) }\":null," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredParent) }\":null," +
//                $"\"{ nameof(TagHelperDescriptor.TagStructure) }\":1," +
//                $"\"{ nameof(TagHelperDescriptor.DesignTimeDescriptor) }\":null," +
//                $"\"{ nameof(TagHelperDescriptor.Metadata) }\":{{}}}}";

//            // Act
//            var serializedDescriptor = JsonConvert.SerializeObject(descriptor);

//            // Assert
//            Assert.Equal(expectedSerializedDescriptor, serializedDescriptor, StringComparer.Ordinal);
//        }

//        [Fact]
//        public void TagHelperDescriptor_WithIndexerAttributes_CanBeSerialized()
//        {
//            // Arrange
//            var descriptor = new TagHelperDescriptor
//            {
//                Prefix = "prefix:",
//                TagName = "tag name",
//                TypeName = "type name",
//                AssemblyName = "assembly name",
//                Attributes = new[]
//                {
//                    new BoundAttributeDescriptor
//                   {
//                        Name = "attribute one",
//                        PropertyName = "property name",
//                        TypeName = "property type name",
//                        IsIndexer = true,
//                        IsEnum = true,
//                    },
//                    new BoundAttributeDescriptor
//                   {
//                        Name = "attribute two",
//                        PropertyName = "property name",
//                        TypeName = typeof(string).FullName,
//                        IsIndexer = true,
//                        IsEnum = false,
//                        IsStringProperty = true
//                    },
//                },
//                AllowedChildren = new[] { "allowed child one", "allowed child two" },
//                RequiredParent = "parent name"
//            };

//            var expectedSerializedDescriptor =
//                $"{{\"{ nameof(TagHelperDescriptor.Prefix) }\":\"prefix:\"," +
//                $"\"{ nameof(TagHelperDescriptor.TagName) }\":\"tag name\"," +
//                $"\"{ nameof(TagHelperDescriptor.FullTagName) }\":\"prefix:tag name\"," +
//                $"\"{ nameof(TagHelperDescriptor.TypeName) }\":\"type name\"," +
//                $"\"{ nameof(TagHelperDescriptor.AssemblyName) }\":\"assembly name\"," +
//                $"\"{ nameof(TagHelperDescriptor.BoundAttributes) }\":[" +
//                $"{{\"{ nameof(BoundAttributeDescriptor.IsIndexer) }\":true," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsEnum) }\":true," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsStringProperty) }\":false," +
//                $"\"{ nameof(BoundAttributeDescriptor.Name) }\":\"attribute one\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.PropertyName) }\":\"property name\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.TypeName) }\":\"property type name\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.DesignTimeDescriptor) }\":null}}," +
//                $"{{\"{ nameof(BoundAttributeDescriptor.IsIndexer) }\":true," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsEnum) }\":false," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsStringProperty) }\":true," +
//                $"\"{ nameof(BoundAttributeDescriptor.Name) }\":\"attribute two\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.PropertyName) }\":\"property name\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.TypeName) }\":\"{ typeof(string).FullName }\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.DesignTimeDescriptor) }\":null}}]," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredAttributes) }\":[]," +
//                $"\"{ nameof(TagHelperDescriptor.AllowedChildTags) }\":[\"allowed child one\",\"allowed child two\"]," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredParent) }\":\"parent name\"," +
//                $"\"{ nameof(TagHelperDescriptor.TagStructure) }\":0," +
//                $"\"{ nameof(TagHelperDescriptor.DesignTimeDescriptor) }\":null," +
//                $"\"{ nameof(TagHelperDescriptor.Metadata) }\":{{}}}}";

//            // Act
//            var serializedDescriptor = JsonConvert.SerializeObject(descriptor);

//            // Assert
//            Assert.Equal(expectedSerializedDescriptor, serializedDescriptor, StringComparer.Ordinal);
//        }

//        [Fact]
//        public void TagHelperDescriptor_WithPropertyBagElements_CanBeSerialized()
//        {
//            // Arrange
//            var descriptor = new TagHelperDescriptor
//            {
//                Prefix = "prefix:",
//                TagName = "tag name",
//                TypeName = "type name",
//                AssemblyName = "assembly name"
//            };

//            descriptor.Metadata.Add("key one", "value one");
//            descriptor.Metadata.Add("key two", "value two");

//            var expectedSerializedDescriptor =
//                $"{{\"{ nameof(TagHelperDescriptor.Prefix) }\":\"prefix:\"," +
//                $"\"{ nameof(TagHelperDescriptor.TagName) }\":\"tag name\"," +
//                $"\"{ nameof(TagHelperDescriptor.FullTagName) }\":\"prefix:tag name\"," +
//                $"\"{ nameof(TagHelperDescriptor.TypeName) }\":\"type name\"," +
//                $"\"{ nameof(TagHelperDescriptor.AssemblyName) }\":\"assembly name\"," +
//                $"\"{ nameof(TagHelperDescriptor.BoundAttributes) }\":[]," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredAttributes) }\":[]," +
//                $"\"{ nameof(TagHelperDescriptor.AllowedChildTags) }\":null," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredParent) }\":null," +
//                $"\"{ nameof(TagHelperDescriptor.TagStructure) }\":0," +
//                $"\"{ nameof(TagHelperDescriptor.DesignTimeDescriptor) }\":null," +
//                $"\"{ nameof(TagHelperDescriptor.Metadata) }\":" +
//                    "{\"key one\":\"value one\",\"key two\":\"value two\"}}";

//            // Act
//            var serializedDescriptor = JsonConvert.SerializeObject(descriptor);

//            // Assert
//            Assert.Equal(expectedSerializedDescriptor, serializedDescriptor);
//        }

//        [Fact]
//        public void TagHelperDescriptor_CanBeDeserialized()
//        {
//            // Arrange
//            var serializedDescriptor =
//                $"{{\"{nameof(TagHelperDescriptor.Prefix)}\":\"prefix:\"," +
//                $"\"{nameof(TagHelperDescriptor.TagName)}\":\"tag name\"," +
//                $"\"{nameof(TagHelperDescriptor.FullTagName)}\":\"prefix:tag name\"," +
//                $"\"{nameof(TagHelperDescriptor.TypeName)}\":\"type name\"," +
//                $"\"{nameof(TagHelperDescriptor.AssemblyName)}\":\"assembly name\"," +
//                $"\"{nameof(TagHelperDescriptor.BoundAttributes)}\":[]," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredAttributes) }\":" +
//                $"[{{\"{ nameof(RequiredAttributeDescriptor.Name)}\":\"required attribute one\"," +
//                $"\"{ nameof(RequiredAttributeDescriptor.NameComparison) }\":1," +
//                $"\"{ nameof(RequiredAttributeDescriptor.Value) }\":null," +
//                $"\"{ nameof(RequiredAttributeDescriptor.ValueComparison) }\":0}}," +
//                $"{{\"{ nameof(RequiredAttributeDescriptor.Name)}\":\"required attribute two\"," +
//                $"\"{ nameof(RequiredAttributeDescriptor.NameComparison) }\":0," +
//                $"\"{ nameof(RequiredAttributeDescriptor.Value) }\":\"something\"," +
//                $"\"{ nameof(RequiredAttributeDescriptor.ValueComparison) }\":2}}]," +
//                $"\"{ nameof(TagHelperDescriptor.AllowedChildTags) }\":[\"allowed child one\",\"allowed child two\"]," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredParent) }\":\"parent name\"," +
//                $"\"{nameof(TagHelperDescriptor.TagStructure)}\":2," +
//                $"\"{ nameof(TagHelperDescriptor.DesignTimeDescriptor) }\":{{" +
//                $"\"{ nameof(TagHelperDesignTimeDescriptor.Summary) }\":\"usage summary\"," +
//                $"\"{ nameof(TagHelperDesignTimeDescriptor.Remarks) }\":\"usage remarks\"," +
//                $"\"{ nameof(TagHelperDesignTimeDescriptor.OutputElementHint) }\":\"some-tag\"}}}}";
//            var expectedDescriptor = new TagHelperDescriptor
//            {
//                Prefix = "prefix:",
//                TagName = "tag name",
//                TypeName = "type name",
//                AssemblyName = "assembly name",
//                RequiredAttributes = new[]
//                {
//                    new RequiredAttributeDescriptor
//                    {
//                        Name = "required attribute one",
//                        NameComparison = TagHelperRequiredAttributeNameComparison.PrefixMatch
//                    },
//                    new RequiredAttributeDescriptor
//                    {
//                        Name = "required attribute two",
//                        NameComparison = TagHelperRequiredAttributeNameComparison.FullMatch,
//                        Value = "something",
//                        ValueComparison = TagHelperRequiredAttributeValueComparison.PrefixMatch,
//                    }
//                },
//                AllowedChildren = new[] { "allowed child one", "allowed child two" },
//                RequiredParent = "parent name",
//                DesignTimeDescriptor = new TagHelperDesignTimeDescriptor
//                {
//                    Summary = "usage summary",
//                    Remarks = "usage remarks",
//                    OutputElementHint = "some-tag"
//                }
//            };

//            // Act
//            var descriptor = JsonConvert.DeserializeObject<TagHelperDescriptor>(serializedDescriptor);

//            // Assert
//            Assert.NotNull(descriptor);
//            Assert.Equal(expectedDescriptor.Prefix, descriptor.Prefix, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.TagName, descriptor.TagName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.FullTagName, descriptor.FullTagName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.TypeName, descriptor.TypeName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.AssemblyName, descriptor.AssemblyName, StringComparer.Ordinal);
//            Assert.Empty(descriptor.BoundAttributes);
//            Assert.Equal(expectedDescriptor.RequiredAttributes, descriptor.RequiredAttributes, TagHelperRequiredAttributeDescriptorComparer.Default);
//            Assert.Equal(
//                expectedDescriptor.DesignTimeDescriptor,
//                descriptor.DesignTimeDescriptor,
//                TagHelperDesignTimeDescriptorComparer.Default);
//            Assert.Empty(descriptor.Metadata);
//        }

//        [Fact]
//        public void TagHelperDescriptor_WithAttributes_CanBeDeserialized()
//        {
//            // Arrange
//            var serializedDescriptor =
//                $"{{\"{ nameof(TagHelperDescriptor.Prefix) }\":\"prefix:\"," +
//                $"\"{ nameof(TagHelperDescriptor.TagName) }\":\"tag name\"," +
//                $"\"{ nameof(TagHelperDescriptor.FullTagName) }\":\"prefix:tag name\"," +
//                $"\"{ nameof(TagHelperDescriptor.TypeName) }\":\"type name\"," +
//                $"\"{ nameof(TagHelperDescriptor.AssemblyName) }\":\"assembly name\"," +
//                $"\"{ nameof(TagHelperDescriptor.BoundAttributes) }\":[" +
//                $"{{\"{ nameof(BoundAttributeDescriptor.IsIndexer) }\":false," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsEnum) }\":true," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsStringProperty) }\":false," +
//                $"\"{ nameof(BoundAttributeDescriptor.Name) }\":\"attribute one\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.PropertyName) }\":\"property name\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.TypeName) }\":\"property type name\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.DesignTimeDescriptor) }\":null}}," +
//                $"{{\"{ nameof(BoundAttributeDescriptor.IsIndexer) }\":false," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsEnum) }\":false," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsStringProperty) }\":true," +
//                $"\"{ nameof(BoundAttributeDescriptor.Name) }\":\"attribute two\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.PropertyName) }\":\"property name\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.TypeName) }\":\"{ typeof(string).FullName }\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.DesignTimeDescriptor) }\":null}}]," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredAttributes) }\":[]," +
//                $"\"{ nameof(TagHelperDescriptor.AllowedChildTags) }\":null," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredParent) }\":null," +
//                $"\"{nameof(TagHelperDescriptor.TagStructure)}\":0," +
//                $"\"{ nameof(TagHelperDescriptor.DesignTimeDescriptor) }\":null}}";
//            var expectedDescriptor = new TagHelperDescriptor
//            {
//                Prefix = "prefix:",
//                TagName = "tag name",
//                TypeName = "type name",
//                AssemblyName = "assembly name",
//                Attributes = new[]
//                {
//                    new BoundAttributeDescriptor
//                   {
//                        Name = "attribute one",
//                        PropertyName = "property name",
//                        TypeName = "property type name",
//                        IsEnum = true,
//                    },
//                    new BoundAttributeDescriptor
//                   {
//                        Name = "attribute two",
//                        PropertyName = "property name",
//                        TypeName = typeof(string).FullName,
//                        IsEnum = false,
//                        IsStringProperty = true
//                    },
//                },
//                AllowedChildren = new[] { "allowed child one", "allowed child two" }
//            };

//            // Act
//            var descriptor = JsonConvert.DeserializeObject<TagHelperDescriptor>(serializedDescriptor);

//            // Assert
//            Assert.NotNull(descriptor);
//            Assert.Equal(expectedDescriptor.Prefix, descriptor.Prefix, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.TagName, descriptor.TagName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.FullTagName, descriptor.FullTagName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.TypeName, descriptor.TypeName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.AssemblyName, descriptor.AssemblyName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.BoundAttributes, descriptor.BoundAttributes, CaseSensitiveBoundAttributeDescriptorComparer.Default);
//            Assert.Empty(descriptor.RequiredAttributes);
//            Assert.Empty(descriptor.Metadata);
//        }

//        [Fact]
//        public void TagHelperDescriptor_WithIndexerAttributes_CanBeDeserialized()
//        {
//            // Arrange
//            var serializedDescriptor =
//                $"{{\"{ nameof(TagHelperDescriptor.Prefix) }\":\"prefix:\"," +
//                $"\"{ nameof(TagHelperDescriptor.TagName) }\":\"tag name\"," +
//                $"\"{ nameof(TagHelperDescriptor.FullTagName) }\":\"prefix:tag name\"," +
//                $"\"{ nameof(TagHelperDescriptor.TypeName) }\":\"type name\"," +
//                $"\"{ nameof(TagHelperDescriptor.AssemblyName) }\":\"assembly name\"," +
//                $"\"{ nameof(TagHelperDescriptor.BoundAttributes) }\":[" +
//                $"{{\"{ nameof(BoundAttributeDescriptor.IsIndexer) }\":true," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsEnum) }\":true," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsStringProperty) }\":false," +
//                $"\"{ nameof(BoundAttributeDescriptor.Name) }\":\"attribute one\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.PropertyName) }\":\"property name\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.TypeName) }\":\"property type name\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.DesignTimeDescriptor) }\":null}}," +
//                $"{{\"{ nameof(BoundAttributeDescriptor.IsIndexer) }\":true," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsEnum) }\":false," +
//                $"\"{ nameof(BoundAttributeDescriptor.IsStringProperty) }\":true," +
//                $"\"{ nameof(BoundAttributeDescriptor.Name) }\":\"attribute two\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.PropertyName) }\":\"property name\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.TypeName) }\":\"{ typeof(string).FullName }\"," +
//                $"\"{ nameof(BoundAttributeDescriptor.DesignTimeDescriptor) }\":null}}]," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredAttributes) }\":[]," +
//                $"\"{ nameof(TagHelperDescriptor.AllowedChildTags) }\":null," +
//                $"\"{ nameof(TagHelperDescriptor.RequiredParent) }\":null," +
//                $"\"{nameof(TagHelperDescriptor.TagStructure)}\":1," +
//                $"\"{ nameof(TagHelperDescriptor.DesignTimeDescriptor) }\":null," +
//                $"\"{ nameof(TagHelperDescriptor.Metadata) }\":{{}}}}";

//            var expectedDescriptor = new TagHelperDescriptor
//            {
//                Prefix = "prefix:",
//                TagName = "tag name",
//                TypeName = "type name",
//                AssemblyName = "assembly name",
//                Attributes = new[]
//                {
//                    new BoundAttributeDescriptor
//                   {
//                        Name = "attribute one",
//                        PropertyName = "property name",
//                        TypeName = "property type name",
//                        IsIndexer = true,
//                        IsEnum = true,
//                    },
//                    new BoundAttributeDescriptor
//                   {
//                        Name = "attribute two",
//                        PropertyName = "property name",
//                        TypeName = typeof(string).FullName,
//                        IsIndexer = true,
//                        IsEnum = false,
//                        IsStringProperty = true
//                    }
//                },
//                TagStructure = TagStructure.NormalOrSelfClosing
//            };

//            // Act
//            var descriptor = JsonConvert.DeserializeObject<TagHelperDescriptor>(serializedDescriptor);

//            // Assert
//            Assert.NotNull(descriptor);
//            Assert.Equal(expectedDescriptor.Prefix, descriptor.Prefix, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.TagName, descriptor.TagName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.FullTagName, descriptor.FullTagName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.TypeName, descriptor.TypeName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.AssemblyName, descriptor.AssemblyName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.BoundAttributes, descriptor.BoundAttributes, CaseSensitiveBoundAttributeDescriptorComparer.Default);
//            Assert.Empty(descriptor.RequiredAttributes);
//            Assert.Empty(descriptor.Metadata);
//        }

//        [Fact]
//        public void TagHelperDescriptor_WithPropertyBagElements_CanBeDeserialized()
//        {
//            // Arrange
//            var serializedDescriptor =
//                $"{{\"{nameof(TagHelperDescriptor.Prefix)}\":\"prefix:\"," +
//                $"\"{nameof(TagHelperDescriptor.TagName)}\":\"tag name\"," +
//                $"\"{nameof(TagHelperDescriptor.TypeName)}\":\"type name\"," +
//                $"\"{nameof(TagHelperDescriptor.AssemblyName)}\":\"assembly name\"," +
//                $"\"{ nameof(TagHelperDescriptor.Metadata) }\":" +
//                    "{\"key one\":\"value one\",\"key two\":\"value two\"}}";
//            var expectedDescriptor = new TagHelperDescriptor
//            {
//                Prefix = "prefix:",
//                TagName = "tag name",
//                TypeName = "type name",
//                AssemblyName = "assembly name"
//            };

//            expectedDescriptor.Metadata.Add("key one", "value one");
//            expectedDescriptor.Metadata.Add("key two", "value two");

//            // Act
//            var descriptor = JsonConvert.DeserializeObject<TagHelperDescriptor>(serializedDescriptor);

//            // Assert
//            Assert.NotNull(descriptor);
//            Assert.Equal(expectedDescriptor.Prefix, descriptor.Prefix, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.TagName, descriptor.TagName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.TypeName, descriptor.TypeName, StringComparer.Ordinal);
//            Assert.Equal(expectedDescriptor.AssemblyName, descriptor.AssemblyName, StringComparer.Ordinal);
//            Assert.Empty(descriptor.BoundAttributes);
//            Assert.Empty(descriptor.RequiredAttributes);
//            Assert.Equal(expectedDescriptor.Metadata["key one"], descriptor.Metadata["key one"]);
//            Assert.Equal(expectedDescriptor.Metadata["key two"], descriptor.Metadata["key two"]);
//        }
//    }
//}
