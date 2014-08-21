using System.Collections.Generic;

namespace Microsoft.AspNet.Razor
{
    /// <summary>
    /// A tag helper provider context that is used to manager tag helpers found in the system.
    /// </summary>
    public interface ITagHelperProviderContext
    {
        /// <summary>
        /// Gets all tag helpers that match the provided <paramref name="tagName"/>.
        /// </summary>
        /// <param name="tagName">The name of the html tag to retrieve tags for.</param>
        /// <returns>Tag helpers that apply to the given <paramref name="tagName"/>.</returns>
        IEnumerable<TagHelperDescriptor> GetTagHelpers(string tagName);
        /// <summary>
        /// Registers a descriptor that can be retrieved via the <see cref="GetTagHelpers(string)"/> method.
        /// </summary>
        /// <param name="descriptor">The descriptor that will be maintained. Can be retrieved by calling
        /// <see cref="GetTagHelpers(string)"/> with the provided <see cref="TagHelperDescriptor.TagName"/>
        /// value.</param>
        void Register(TagHelperDescriptor descriptor);
        /// <summary>
        /// Unregisters a specific <see cref="TagHelperDescriptor"/> so it can no longer be retrieved via
        /// the <see cref="GetTagHelpers(string)"/> method.
        /// </summary>
        /// <param name="descriptor"></param>
        void Unregister(TagHelperDescriptor descriptor);
    }
}