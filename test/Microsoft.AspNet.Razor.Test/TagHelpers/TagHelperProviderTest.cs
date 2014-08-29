// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers.Internal;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.TagHelpers
{
    public class TagHelperProviderTest
    {
        [Fact]
        public void TagHelperProvider_GetTagHelpersReturnsNothingForUnregisteredTags()
        {
            var registrar = new TagHelperRegistrar();
            var provider = new TagHelperProvider(registrar);
            var divDescriptor = new TagHelperDescriptor("div", "foo1", ContentBehavior.None);
            var spanDescriptor = new TagHelperDescriptor("span", "foo2", ContentBehavior.None);
            registrar.Register(divDescriptor);
            registrar.Register(spanDescriptor);

            // Act
            var descriptors = provider.GetTagHelpers("foo");

            // Assert
            Assert.Empty(descriptors);
        }

        [Fact]
        public void TagHelperProvider_GetTagHelpersDoesntReturnNonCatchAllTagsForCatchAll()
        {
            // Arrange
            var registrar = new TagHelperRegistrar();
            var provider = new TagHelperProvider(registrar);
            var divDescriptor = new TagHelperDescriptor("div", "foo1", ContentBehavior.None);
            var spanDescriptor = new TagHelperDescriptor("span", "foo2", ContentBehavior.None);
            var catchAllDescriptor = new TagHelperDescriptor("*", "foo3", ContentBehavior.None);
            registrar.Register(divDescriptor);
            registrar.Register(spanDescriptor);
            registrar.Register(catchAllDescriptor);

            // Act
            var descriptors = provider.GetTagHelpers("*");

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Same(catchAllDescriptor, descriptor);
        }

        [Fact]
        public void TagHelperProvider_GetTagHelpersReturnsCatchAllsWithEveryTagName()
        {
            // Arrange
            var registrar = new TagHelperRegistrar();
            var provider = new TagHelperProvider(registrar);
            var divDescriptor = new TagHelperDescriptor("div", "foo1", ContentBehavior.None);
            var spanDescriptor = new TagHelperDescriptor("span", "foo2", ContentBehavior.None);
            var catchAllDescriptor = new TagHelperDescriptor("*", "foo3", ContentBehavior.None);
            registrar.Register(divDescriptor);
            registrar.Register(spanDescriptor);
            registrar.Register(catchAllDescriptor);

            // Act
            var divDescriptors = provider.GetTagHelpers("div");
            var spanDescriptors = provider.GetTagHelpers("span");

            // Assert
            // For divs
            Assert.Equal(2, divDescriptors.Count());
            Assert.Contains(divDescriptor, divDescriptors);
            Assert.Contains(catchAllDescriptor, divDescriptors);

            // For spans
            Assert.Equal(2, spanDescriptors.Count());
            Assert.Contains(spanDescriptor, spanDescriptors);
            Assert.Contains(catchAllDescriptor, spanDescriptors);
        }
    }
}