// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers.Internal;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.TagHelpers
{
    public class TagHelperRegistrarTest
    {
        [Fact]
        public void TagHelperRegistrar_RegisterDoesntRegisterDuplicateCatchAllTagHelpers()
        {
            // Arrange
            var registrar = new TagHelperRegistrar();
            var provider = new TagHelperProvider(registrar);
            var catchAllDescriptor1 = new TagHelperDescriptor("*", "foo1", ContentBehavior.None);
            var catchAllDescriptor2 = new TagHelperDescriptor("*", "foo1", ContentBehavior.None);
            registrar.Register(catchAllDescriptor1);
            registrar.Register(catchAllDescriptor2);

            // Act
            var descriptors = provider.GetTagHelpers("*");

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Same(catchAllDescriptor1, descriptor);
        }

        [Fact]
        public void TagHelperRegistrar_RegisterDoesntRegisterDuplicateTagHelpers()
        {
            // Arrange
            var registrar = new TagHelperRegistrar();
            var provider = new TagHelperProvider(registrar);
            var divDescriptor1 = new TagHelperDescriptor("div", "foo", ContentBehavior.None);
            var divDescriptor2 = new TagHelperDescriptor("div", "foo", ContentBehavior.None);
            registrar.Register(divDescriptor1);
            registrar.Register(divDescriptor2);

            // Act
            var descriptors = provider.GetTagHelpers("div");

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Same(divDescriptor1, descriptor);
        }

        [Fact]
        public void TagHelperRegistrar_UnregisterRemovesCatchAllTagHelpersFromGetTagHelpers()
        {
            // Arrange
            var registrar = new TagHelperRegistrar();
            var provider = new TagHelperProvider(registrar);
            var divDescriptor = new TagHelperDescriptor("div", "foo1", ContentBehavior.None);
            var catchAllDescriptor1 = new TagHelperDescriptor("*", "foo2", ContentBehavior.None);
            var catchAllDescriptor2 = new TagHelperDescriptor("*", "foo3", ContentBehavior.None);
            registrar.Register(divDescriptor);
            registrar.Register(catchAllDescriptor1);
            registrar.Register(catchAllDescriptor2);

            // Act
            // This could also pass in catchAllDescriptor, this is just trying to show that
            // the descriptor just needs to have the right metadata.
            registrar.Unregister(new TagHelperDescriptor("*", "foo2", ContentBehavior.None));
            var catchAllDescriptors = provider.GetTagHelpers("*");
            var divDescriptors = provider.GetTagHelpers("div");

            // Assert
            var descriptor = Assert.Single(catchAllDescriptors);
            Assert.Same(catchAllDescriptor2, descriptor);

            Assert.Equal(2, divDescriptors.Count());
            Assert.Contains(divDescriptor, divDescriptors);
            Assert.Contains(catchAllDescriptor2, divDescriptors);
        }

        [Fact]
        public void TagHelperRegistrar_RegisterAllowsRetrievalFromGetTagHelpers()
        {
            // Arrange
            var registrar = new TagHelperRegistrar();
            var provider = new TagHelperProvider(registrar);
            var divDescriptor1 = new TagHelperDescriptor("div", "foo1", ContentBehavior.None);
            var divDescriptor2 = new TagHelperDescriptor("div", "foo2", ContentBehavior.None);

            // Act
            registrar.Register(divDescriptor1);
            registrar.Register(divDescriptor2);
            var descriptors = provider.GetTagHelpers("div");

            // Assert
            Assert.Equal(2, descriptors.Count());
            Assert.Contains(divDescriptor1, descriptors);
            Assert.Contains(divDescriptor2, descriptors);
        }

        [Fact]
        public void TagHelperRegistrar_UnregisterRemovesTagHelpersFromGetTagHelpers()
        {
            // Arrange
            var registrar = new TagHelperRegistrar();
            var provider = new TagHelperProvider(registrar);
            var divDescriptor1 = new TagHelperDescriptor("div", "foo1", ContentBehavior.None);
            var divDescriptor2 = new TagHelperDescriptor("div", "foo2", ContentBehavior.None);

            // Act
            registrar.Register(divDescriptor1);
            registrar.Register(divDescriptor2);
            // This could also pass in divDescriptor2, this is just trying to show that
            // the descriptor just needs to have the right metadata.
            registrar.Unregister(new TagHelperDescriptor("div", "foo2", ContentBehavior.None));
            var descriptors = provider.GetTagHelpers("div");

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Same(divDescriptor1, descriptor);
        }
    }
}