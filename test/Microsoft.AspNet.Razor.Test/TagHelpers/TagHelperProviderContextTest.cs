using System.Linq;
using Xunit;

namespace Microsoft.AspNet.Razor.Test
{
    public class TagHelperProviderContextTest
    {
        [Fact]
        public void TagHelperProviderContext_RegisterDoesntRegisterDuplicateCatchAllTagHelpers()
        {
            // Arrange
            var context = new TagHelperProviderContext();
            var catchAllDescriptor1 = new TagHelperDescriptor("*", "foo1", ContentBehavior.None);
            var catchAllDescriptor2 = new TagHelperDescriptor("*", "foo1", ContentBehavior.None);

            // Act
            context.Register(catchAllDescriptor1);
            context.Register(catchAllDescriptor2);
            var descriptors = context.GetTagHelpers("*");

            // Assert
            Assert.NotEmpty(descriptors);
            Assert.Equal(1, descriptors.Count());
            Assert.Equal(catchAllDescriptor1, descriptors.First());
        }

        [Fact]
        public void TagHelperProviderContext_RegisterDoesntRegisterDuplicateTagHelpers()
        {
            // Arrange
            var context = new TagHelperProviderContext();
            var divDescriptor1 = new TagHelperDescriptor("div", "foo", ContentBehavior.None);
            var divDescriptor2 = new TagHelperDescriptor("div", "foo", ContentBehavior.None);

            // Act
            context.Register(divDescriptor1);
            context.Register(divDescriptor2);
            var descriptors = context.GetTagHelpers("div");

            // Assert
            Assert.NotEmpty(descriptors);
            Assert.Equal(1, descriptors.Count());
            Assert.Equal(divDescriptor1, descriptors.First());
        }

        [Fact]
        public void TagHelperProviderContext_GetTagHelpersDoesntReturnNonCatchAllTagsForCatchAll()
        {
            // Arrange
            var context = new TagHelperProviderContext();
            var divDescriptor = new TagHelperDescriptor("div", "foo1", ContentBehavior.None);
            var spanDescriptor = new TagHelperDescriptor("span", "foo2", ContentBehavior.None);
            var catchAllDescriptor = new TagHelperDescriptor("*", "foo3", ContentBehavior.None);

            // Act
            context.Register(divDescriptor);
            context.Register(spanDescriptor);
            context.Register(catchAllDescriptor);
            var descriptors = context.GetTagHelpers("*");

            // Assert
            Assert.NotEmpty(descriptors);
            Assert.Equal(1, descriptors.Count());
            Assert.Equal(catchAllDescriptor, descriptors.First());
        }

        [Fact]
        public void TagHelperProviderContext_GetTagHelpersReturnsCatchAllsWithEveryTagName()
        {
            // Arrange
            var context = new TagHelperProviderContext();
            var divDescriptor = new TagHelperDescriptor("div", "foo1", ContentBehavior.None);
            var spanDescriptor = new TagHelperDescriptor("span", "foo2", ContentBehavior.None);
            var catchAllDescriptor = new TagHelperDescriptor("*", "foo3", ContentBehavior.None);

            // Act
            context.Register(divDescriptor);
            context.Register(spanDescriptor);
            context.Register(catchAllDescriptor);
            var divDescriptors = context.GetTagHelpers("div");
            var spanDescriptors = context.GetTagHelpers("span");

            // Assert
            // For divs
            Assert.NotEmpty(divDescriptors);
            Assert.Equal(2, divDescriptors.Count());
            Assert.Equal(divDescriptor, divDescriptors.First());
            Assert.Equal(catchAllDescriptor, divDescriptors.Last());

            // For spans
            Assert.NotEmpty(spanDescriptors);
            Assert.Equal(2, spanDescriptors.Count());
            Assert.Equal(spanDescriptor, spanDescriptors.First());
            Assert.Equal(catchAllDescriptor, spanDescriptors.Last());
        }

        [Fact]
        public void TagHelperProviderContext_GetTagHelpersAllowsRetrievalOfOnlyCatchAlls()
        {
            // Arrange
            var context = new TagHelperProviderContext();
            var catchAllDescriptor1 = new TagHelperDescriptor("*", "foo1", ContentBehavior.None);
            var catchAllDescriptor2 = new TagHelperDescriptor("*", "foo2", ContentBehavior.None);

            // Act
            context.Register(catchAllDescriptor1);
            context.Register(catchAllDescriptor2);
            var descriptors = context.GetTagHelpers("*");

            // Assert
            Assert.NotEmpty(descriptors);
            Assert.Equal(2, descriptors.Count());
            Assert.Equal(catchAllDescriptor1, descriptors.First());
            Assert.Equal(catchAllDescriptor2, descriptors.Last());
        }

        [Fact]
        public void TagHelperProviderContext_UnregisterRemovesCatchAllTagHelpersFromGetTagHelpers()
        {
            // Arrange
            var context = new TagHelperProviderContext();
            var divDescriptor = new TagHelperDescriptor("div", "foo1", ContentBehavior.None);
            var catchAllDescriptor1 = new TagHelperDescriptor("*", "foo2", ContentBehavior.None);
            var catchAllDescriptor2 = new TagHelperDescriptor("*", "foo3", ContentBehavior.None);

            // Act
            context.Register(divDescriptor);
            context.Register(catchAllDescriptor1);
            context.Register(catchAllDescriptor2);
            // This could also pass in catchAllDescriptor, this is just trying to show that
            // the descriptor just needs to have the right metadata.
            context.Unregister(new TagHelperDescriptor("*", "foo2", ContentBehavior.None));
            var catchAllDescriptors = context.GetTagHelpers("*");
            var divDescriptors = context.GetTagHelpers("div");

            // Assert
            Assert.NotEmpty(catchAllDescriptors);
            Assert.Equal(1, catchAllDescriptors.Count());
            Assert.Equal(catchAllDescriptor2, catchAllDescriptors.First());

            Assert.NotEmpty(divDescriptors);
            Assert.Equal(2, divDescriptors.Count());
            Assert.Equal(divDescriptor, divDescriptors.First());
            Assert.Equal(catchAllDescriptor2, divDescriptors.Last());
        }

        [Fact]
        public void TagHelperProviderContext_RegisterAllowsRetrievalFromGetTagHelpers()
        {
            // Arrange
            var context = new TagHelperProviderContext();
            var divDescriptor1 = new TagHelperDescriptor("div", "foo1", ContentBehavior.None);
            var divDescriptor2 = new TagHelperDescriptor("div", "foo2", ContentBehavior.None);

            // Act
            context.Register(divDescriptor1);
            context.Register(divDescriptor2);
            var descriptors = context.GetTagHelpers("div");

            // Assert
            Assert.NotEmpty(descriptors);
            Assert.Equal(2, descriptors.Count());
            Assert.Equal(divDescriptor1, descriptors.First());
            Assert.Equal(divDescriptor2, descriptors.Last());
        }

        [Fact]
        public void TagHelperProviderContext_UnregisterRemovesTagHelpersFromGetTagHelpers()
        {
            // Arrange
            var context = new TagHelperProviderContext();
            var divDescriptor1 = new TagHelperDescriptor("div", "foo1", ContentBehavior.None);
            var divDescriptor2 = new TagHelperDescriptor("div", "foo2", ContentBehavior.None);

            // Act
            context.Register(divDescriptor1);
            context.Register(divDescriptor2);
            // This could also pass in divDescriptor2, this is just trying to show that
            // the descriptor just needs to have the right metadata.
            context.Unregister(new TagHelperDescriptor("div", "foo2", ContentBehavior.None));
            var descriptors = context.GetTagHelpers("div");

            // Assert
            Assert.NotEmpty(descriptors);
            Assert.Equal(1, descriptors.Count());
            Assert.Equal(divDescriptor1, descriptors.First());
        }
    }
}