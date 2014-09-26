// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Runtime.Test.TagHelpers
{
    public class TagHelperTypeResolverTest
    {
        private static readonly Type[] AllValidTestableTagHelpers = new[]
        {
            typeof(Valid_PlainTagHelper),
            typeof(Valid_InheritedTagHelper)
        };
        private static readonly Type[] AllInvalidTestableTagHelpers = new[]
        {
            typeof(Invalid_AbstractTagHelper),
            typeof(Invalid_GenericTagHelper<>),
            typeof(Invalid_NestedPublicTagHelper),
            typeof(Invalid_NestedInternalTagHelper),
            typeof(Invalid_PrivateTagHelper),
            typeof(Invalid_ProtectedTagHelper),
            typeof(Invalid_InternalTagHelper)
        };
        private static readonly Type[] AllTestableTagHelpers =
            AllValidTestableTagHelpers.Concat(AllInvalidTestableTagHelpers).ToArray();

        [Fact]
        public void TypeResolver_OnlyReturnsValidTagHelpersForUnversionedAssemblyLookup()
        {
            // Arrange
            var tagHelperTypeResolver = new TestTagHelperTypeResolver(AllTestableTagHelpers);
            var expectedTypes = new[] {
                    typeof(Valid_PlainTagHelper),
                    typeof(Valid_InheritedTagHelper),
            };

            // Act
            var types = tagHelperTypeResolver.Resolve("Foo");

            // Assert
            Assert.Equal(expectedTypes, types);
        }

        [Fact]
        public void TypeResolver_OnlyReturnsValidTagHelpersForVersionedAssemblyLookup()
        {
            // Arrange
            var tagHelperTypeResolver = new TestTagHelperTypeResolver(AllTestableTagHelpers);
            var expectedTypes = new[] {
                    typeof(Valid_PlainTagHelper),
                    typeof(Valid_InheritedTagHelper),
            };

            // Act
            var types = tagHelperTypeResolver.Resolve("Foo, 1.0.0.0");

            // Assert
            Assert.Equal(expectedTypes, types);
        }

        [Fact]
        public void TypeResolver_ReturnsIndividualValidTagHelpersIfRequested()
        {
            // Arrange
            var tagHelperTypeResolver = new TestTagHelperTypeResolver(AllTestableTagHelpers);
            var expectedTypes = new[] {
                    typeof(Valid_PlainTagHelper)
            };

            // Act
            var types = tagHelperTypeResolver.Resolve(
                "Foo, 1.0.0.0, Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Valid_PlainTagHelper");

            // Assert
            Assert.Equal(expectedTypes, types);
        }

        [Theory]
        [InlineData("Foo,1.0.0.0,Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Valid_PlainTagHelper")]
        [InlineData("    Foo,1.0.0.0,Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Valid_PlainTagHelper")]
        [InlineData("Foo    ,1.0.0.0,Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Valid_PlainTagHelper")]
        [InlineData("     Foo    ,1.0.0.0,Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Valid_PlainTagHelper")]
        [InlineData("Foo,    1.0.0.0,Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Valid_PlainTagHelper")]
        [InlineData("Foo,1.0.0.0    ,Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Valid_PlainTagHelper")]
        [InlineData("Foo,     1.0.0.0    ,Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Valid_PlainTagHelper")]
        [InlineData("Foo,1.0.0.0,    Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Valid_PlainTagHelper")]
        [InlineData("Foo,1.0.0.0,Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Valid_PlainTagHelper    ")]
        [InlineData("Foo,1.0.0.0,     Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Valid_PlainTagHelper    ")]
        [InlineData("  Foo  ,  1.0.0.0  ,  Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Valid_PlainTagHelper  ")]
        public void TypeResolver_IgnoresSpaces(string lookupText)
        {
            // Arrange
            var expectedVersion = new Version("1.0.0.0");
            var tagHelperTypeResolver = new TestTagHelperTypeResolver(AllTestableTagHelpers)
            {
                OnGetAssemblyTypeInfos = (assemblyRef) =>
                {
                    Assert.Equal("Foo", assemblyRef.Name);
                    Assert.Equal(expectedVersion, assemblyRef.Version);
                }
            };
            var expectedTypes = new[] {
                    typeof(Valid_PlainTagHelper)
            };

            // Act
            var types = tagHelperTypeResolver.Resolve(lookupText);

            // Assert
            Assert.Equal(expectedTypes, types);
        }

        [Fact]
        public void TypeResolver_DoesntReturnInvalidTagHelpersEvenWhenSpecified()
        {
            // Arrange
            var tagHelperTypeResolver = new TestTagHelperTypeResolver(AllTestableTagHelpers);

            // Act
            var types = tagHelperTypeResolver.Resolve(
                "Foo, 1.0.0.0, Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_AbstractTagHelper");
            types = types.Concat(tagHelperTypeResolver.Resolve(
                "Foo, 1.0.0.0, Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_GenericTagHelper`"));
            types = types.Concat(tagHelperTypeResolver.Resolve(
                "Foo, 1.0.0.0, Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_NestedPublicTagHelper"));
            types = types.Concat(tagHelperTypeResolver.Resolve(
                "Foo, 1.0.0.0, Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_NestedInternalTagHelper"));
            types = types.Concat(tagHelperTypeResolver.Resolve(
                "Foo, 1.0.0.0, Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_PrivateTagHelper"));
            types = types.Concat(tagHelperTypeResolver.Resolve(
                "Foo, 1.0.0.0, Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_ProtectedTagHelper"));
            types = types.Concat(tagHelperTypeResolver.Resolve(
                "Foo, 1.0.0.0, Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_InternalTagHelper"));

            // Assert
            Assert.Empty(types);
        }

        [Fact]
        public void TypeResolver_ReturnsEmptyEnumerableIfNoValidTagHelpersFound()
        {
            // Arrange
            var tagHelperTypeResolver = new TestTagHelperTypeResolver(AllInvalidTestableTagHelpers);

            // Act
            var types = tagHelperTypeResolver.Resolve("Foo");

            // Assert
            Assert.Empty(types);
        }

        [Theory]
        [InlineData("Foo, 1.2.3.4", "1.2.3.4")]
        [InlineData("Foo, 1.2.3.4, Bar", "1.2.3.4")]
        public void TypeResolver_LookupTextVersionIsUsed(string lookupText, string version)
        {
            // Arrange
            var expectedVersion = new Version(version);
            var tagHelperTypeResolver = new TestTagHelperTypeResolver(AllInvalidTestableTagHelpers)
            {
                OnGetAssemblyTypeInfos = (assemblyRef) =>
                {
                    Assert.Equal(expectedVersion, assemblyRef.Version);
                }
            };

            // Act & Assert
            tagHelperTypeResolver.Resolve(lookupText);
        }

        [Fact]
        public void TypeResolver_ResolveThrowsIfEmptyLookupText()
        {
            // Arrange
            var tagHelperTypeResolver = new TestTagHelperTypeResolver(AllInvalidTestableTagHelpers);
            var expectedMessage = Resources.FormatTagHelperTypeResolver_InvalidTagHelperLookupText(string.Empty);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                tagHelperTypeResolver.Resolve(string.Empty);
            });

            Assert.Equal(expectedMessage, ex.Message);
        }


        [Theory]
        [InlineData("Foo, abcdefgh", "abcdefgh")]
        [InlineData("Foo, abcdefgh, Bar", "abcdefgh")]
        public void TypeResolver_ResolveThrowsIfInvalidVersion(string lookupText, string invalidVersion)
        {
            // Arrange
            var tagHelperTypeResolver = new TestTagHelperTypeResolver(AllInvalidTestableTagHelpers);
            var expectedMessage =
                Resources.FormatTagHelperTypeResolver_InvalidTagHelperLookupTextAssemblyVersion(invalidVersion);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                tagHelperTypeResolver.Resolve(lookupText);
            });

            Assert.Equal(expectedMessage, ex.Message);
        }

        private class TestTagHelperTypeResolver : TagHelperTypeResolver
        {
            private IEnumerable<TypeInfo> _assemblyTypeInfos;

            public TestTagHelperTypeResolver(IEnumerable<Type> assemblyTypes)
            {
                _assemblyTypeInfos = assemblyTypes.Select(type => type.GetTypeInfo());
                OnGetAssemblyTypeInfos = (_) => { };
            }

            public Action<AssemblyName> OnGetAssemblyTypeInfos { get; set; }

            internal override string GetAssemblyName(string lookupName)
            {
                // Doesn't matter because we override the way its used via GetAssemblyTypeInfos
                return lookupName;
            }

            internal override IEnumerable<TypeInfo> GetAssemblyTypeInfos(AssemblyName assemblyRef)
            {
                OnGetAssemblyTypeInfos(assemblyRef);

                return _assemblyTypeInfos;
            }
        }

        public class Invalid_NestedPublicTagHelper : TagHelper
        {
        }

        internal class Invalid_NestedInternalTagHelper : TagHelper
        {
        }

        private class Invalid_PrivateTagHelper : TagHelper
        {
        }

        protected class Invalid_ProtectedTagHelper : TagHelper
        {
        }
    }

    // These tag helper types must be unnested and public to be valid tag helpers.
    public class Valid_PlainTagHelper : TagHelper
    {
    }

    public class Valid_InheritedTagHelper : Valid_PlainTagHelper
    {
    }

    public abstract class Invalid_AbstractTagHelper : TagHelper
    {
    }

    public class Invalid_GenericTagHelper<T> : TagHelper
    {
    }

    internal class Invalid_InternalTagHelper : TagHelper
    {
    }
}