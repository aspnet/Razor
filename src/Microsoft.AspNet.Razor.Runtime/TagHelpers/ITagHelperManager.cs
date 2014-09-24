// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Defines a contract that is used to render <see cref="TagHelper"/>s.
    /// </summary>
    public interface ITagHelperManager
    {
        /// <summary>
        /// Creates and tracks a tag helper of type <typeparamref name="TTagHelper"/>.
        /// </summary>
        /// <typeparam name="TTagHelper">The <see cref="TagHelper"/> type to create.</typeparam>
        /// <returns>The created <see cref="TagHelper"/>.</returns>
        TTagHelper InstantiateTagHelper<TTagHelper>() where TTagHelper : TagHelper;

        /// <summary>
        /// Initializes a scope for <see cref="TagHelper"/>s created by the
        /// <see cref="InstantiateTagHelper{TTagHelper}"/> method.
        /// </summary>
        /// <param name="tagName">The HTML tag name associated with the <see cref="TagHelper"/>s created by
        /// <see cref="InstantiateTagHelper{TTagHelper}"/>.</param>
        void StartTagHelpersScope(string tagName);

        /// <summary>
        /// Ends the scope for the tag helpers created by <see cref="InstantiateTagHelper{TTagHelper}"/>.
        /// </summary>
        void EndTagHelpersScope();

        /// <summary>
        /// Tracks the HTML attribute so it can be used by a <see cref="TagHelperOutput.Attributes"/>.
        /// </summary>
        /// <param name="name">The HTML attribute name.</param>
        /// <param name="value">The HTML attribute value.</param>
        void AddHtmlAttribute(string name, string value);

        /// <summary>
        /// Tracks the HTML attribute so it can be used by a <see cref="TagHelperContext.AllAttributes"/>.
        /// </summary>
        /// <param name="name">The HTML attribute name.</param>
        /// <param name="value">The attribute value.</param>
        void AddTagHelperAttribute(string name, object value);

        /// <summary>
        /// Creates a <see cref="TextWriter"/> that can be used to write to the current
        /// <see cref="TagHelperOutput.Content"/>.
        /// </summary>
        /// <returns>A <see cref="TextWriter"/> that can be used to write to the current
        /// <see cref="TagHelperOutput.Content"/>.</returns>
        TextWriter GetContentBuffer();

        /// <summary>
        /// Generates the start tag for the current set of <see cref="TagHelper"/>s.
        /// </summary>
        /// <returns>A string representation of the current start tag.</returns>
        string GenerateTagStart();

        /// <summary>
        /// Generates the content for the current set of <see cref="TagHelper"/>s.
        /// </summary>
        /// <returns>A string representation of the current HTML tags content.</returns>
        string GenerateTagContent();

        /// <summary>
        /// Generates the end tag for the current set of <see cref="TagHelper"/>s.
        /// </summary>
        /// <returns>A string representation of the current end tag.</returns>
        string GenerateTagEnd();

        /// <summary>
        /// Calls the <see cref="TagHelper.ProcessAsync(TagHelperOutput, TagHelperContext)"/> method on
        /// <see cref="TagHelper"/>s that have been created via the <see cref="InstantiateTagHelper{TTagHelper}"/>
        /// for the current tag helper scope.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing when all of the current <see cref="TagHelper"/>s have finished
        /// processing.</returns>
        Task ExecuteTagHelpersAsync();
    }
}