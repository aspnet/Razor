// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor.Generator
{
    /// <summary>
    /// Contains information associated with rendering TagHelpers.
    /// </summary>
    public class GeneratedTagHelperRenderingContext
    {
        /// <summary>
        /// Default value of <see cref="TagHelperRendererName"/>.
        /// </summary>
        public static readonly string DefaultTagHelperRendererName = "__tagHelperRenderer";
        /// <summary>
        /// Default value of <see cref="TagHelperPrepareContext"/>.
        /// </summary>
        public static readonly string DefaultTagHelperPrepareContext = "ViewContext";
        /// <summary>
        /// Default value of <see cref="TagHelperRendererAddAttributeBuilderName"/>.
        /// </summary>
        public static readonly string DefaultTagHelperRendererAddAttributeBuilderName = "AddAttributeBuilder";
        /// <summary>
        /// Default value of <see cref="TagHelperRendererAddBufferedAttributeBuilderName"/>.
        /// </summary>
        public static readonly string DefaultTagHelperRendererAddBufferedAttributeBuilderName = "AddBufferedAttributeBuilder";
        /// <summary>
        /// Default value of <see cref="TagHelperRendererAddAttributeName"/>.
        /// </summary>
        public static readonly string DefaultTagHelperRendererAddAttributeName = "AddAttribute";
        /// <summary>
        /// Default value of <see cref="TagHelperRendererTagBuilderName"/>.
        /// </summary>
        public static readonly string DefaultTagHelperRendererTagBuilderName = "TagBodyWriter";
        /// <summary>
        /// Default value of <see cref="TagHelperRendererPrepareMethodName"/>.
        /// </summary>
        public static readonly string DefaultTagHelperRendererPrepareMethodName = "PrepareTagHelper";
        /// <summary>
        /// Default value of <see cref="TagHelperRendererStartMethodName"/>.
        /// </summary>
        public static readonly string DefaultTagHelperRendererStartMethodName = "StartTagHelper";
        /// <summary>
        /// Default value of <see cref="TagHelperRendererEndMethodName"/>.
        /// </summary>
        public static readonly string DefaultTagHelperRendererEndMethodName = "EndTagHelper";
        /// <summary>
        /// Default value of <see cref="DefaultTagHelperRendererBodyBufferName"/>.
        /// </summary>
        public static readonly string DefaultTagHelperRendererBodyBufferName = "TagBodyBuffer";
        /// <summary>
        /// Default value of <see cref="TagHelperUseWriterName"/>.
        /// </summary>
        public static readonly string DefaultTagHelperUseWriterName = "ViewContext.UseWriter";
        /// <summary>
        /// Default value of <see cref="TagHelperNextWriterName"/>.
        /// </summary>
        public static readonly string DefaultTagHelperNextWriterName = "ViewContext.NextWriter";

        /// <summary>
        /// Default implementation of <see cref="GeneratedTagHelperRenderingContext"/>.
        /// </summary>
        public static readonly GeneratedTagHelperRenderingContext Default =
            new GeneratedTagHelperRenderingContext(DefaultTagHelperRendererName,
                                                   DefaultTagHelperPrepareContext,
                                                   DefaultTagHelperRendererAddAttributeBuilderName,
                                                   DefaultTagHelperRendererAddBufferedAttributeBuilderName,
                                                   DefaultTagHelperRendererAddAttributeName,
                                                   DefaultTagHelperRendererTagBuilderName,
                                                   DefaultTagHelperRendererPrepareMethodName,
                                                   DefaultTagHelperRendererStartMethodName,
                                                   DefaultTagHelperRendererEndMethodName,
                                                   DefaultTagHelperRendererBodyBufferName,
                                                   DefaultTagHelperUseWriterName,
                                                   DefaultTagHelperNextWriterName);

        /// <summary>
        /// Instantiates a new instance of <see cref="GeneratedTagHelperRenderingContext"/>.
        /// </summary>
        /// <param name="tagHelperRendererName">The variable name of the renderer that will be used to render 
        /// <see cref="TagHelpers.TagHelper"/>'s.</param>
        /// <param name="tagHelperPrepareContext">The context value to pass into 
        /// <see cref="TagHelperRendererPrepareMethodName"/>.</param>
        /// <param name="tagHelperRendererAddAttributeBuilderName">The name of the method to invoke when adding an
        /// attribute builder to the tag helper renderer.</param>
        /// <param name="tagHelperRendererAddBufferedAttributeBuilderName">The name of the method to invoke when adding 
        /// a buffered attribute builder to the tag helper renderer.</param>
        /// <param name="tagHelperRendererAddAttributeName">The name of the method to invoke when adding 
        /// a attribute to the tag helper renderer.</param>
        /// <param name="tagHelperRendererTagBuilderName">The access to the tag builder object that represents the
        /// current <see cref="TagHelpers.TagHelper"/>'s tag builder object.</param>
        /// <param name="tagHelperRendererPrepareMethodName">The name of the method to invoke when preparing a 
        /// <see cref="TagHelpers.TagHelper"/>.</param>
        /// <param name="tagHelperRendererStartMethodName">The name of the method to invoke when starting a 
        /// <see cref="TagHelpers.TagHelper"/>.</param>
        /// <param name="tagHelperRendererEndMethodName">The name of the method to invoke when ending a 
        /// <see cref="TagHelpers.TagHelper"/>.</param>
        /// <param name="tagHelperRendererBodyBufferName">The name of the access point to get a bodies buffered contents.</param>
        /// <param name="tagHelperUseWriterName">The name of the method to invoke to start using a provided writer.</param>
        /// <param name="tagHelperNextWriterName">The name of the method to invoke when done using the existing writer
        /// that was provided via <paramref name="tagHelperUseWriterName"/>.</param>
        public GeneratedTagHelperRenderingContext(string tagHelperRendererName,
                                                  string tagHelperPrepareContext,
                                                  string tagHelperRendererAddAttributeBuilderName,
                                                  string tagHelperRendererAddBufferedAttributeBuilderName,
                                                  string tagHelperRendererAddAttributeName,
                                                  string tagHelperRendererTagBuilderName,
                                                  string tagHelperRendererPrepareMethodName,
                                                  string tagHelperRendererStartMethodName,
                                                  string tagHelperRendererEndMethodName,
                                                  string tagHelperRendererBodyBufferName,
                                                  string tagHelperUseWriterName,
                                                  string tagHelperNextWriterName)
        {
            TagHelperRendererName = tagHelperRendererName;
            TagHelperPrepareContext = tagHelperPrepareContext;
            TagHelperRendererAddAttributeBuilderName = tagHelperRendererAddAttributeBuilderName;
            TagHelperRendererAddBufferedAttributeBuilderName = tagHelperRendererAddBufferedAttributeBuilderName;
            TagHelperRendererAddAttributeName = tagHelperRendererAddAttributeName;
            TagHelperRendererTagBuilderName = tagHelperRendererTagBuilderName;
            TagHelperRendererPrepareMethodName = tagHelperRendererPrepareMethodName;
            TagHelperRendererStartMethodName = tagHelperRendererStartMethodName;
            TagHelperRendererEndMethodName = tagHelperRendererEndMethodName;
            TagHelperRendererBodyBufferName = tagHelperRendererBodyBufferName;
            TagHelperUseWriterName = tagHelperUseWriterName;
            TagHelperNextWriterName = tagHelperNextWriterName;
        }

        /// <summary>
        /// The variable name of the renderer that will be used to render <see cref="TagHelpers.TagHelper"/>'s.
        /// </summary>
        public string TagHelperRendererName { get; private set; }

        /// <summary>
        /// The context value to pass into <see cref="TagHelperRendererPrepareMethodName"/>.
        /// </summary>
        public string TagHelperPrepareContext { get; private set; }

        /// <summary>
        /// The name of the method to invoke when adding an attribute builder to the tag helper renderer.
        /// </summary>
        public string TagHelperRendererAddAttributeBuilderName { get; private set; }

        /// <summary>
        /// The name of the method to invoke when adding an attribute builder to the tag helper renderer.
        /// </summary>
        public string TagHelperRendererAddBufferedAttributeBuilderName { get; private set; }

        /// <summary>
        /// The name of the method to invoke when adding a buffered attribute builder to the tag helper renderer.
        /// </summary>
        public string TagHelperRendererAddAttributeName { get; private set; }

        /// <summary>
        /// The access to the tag builder object that represents the current <see cref="TagHelpers.TagHelper"/>'s 
        /// tag builder object.
        /// </summary>
        public string TagHelperRendererTagBuilderName { get; private set; }

        /// <summary>
        /// The name of the method to invoke when preparing a <see cref="TagHelpers.TagHelper"/>.
        /// </summary>
        public string TagHelperRendererPrepareMethodName { get; private set; }

        /// <summary>
        /// The name of the method to invoke when starting a <see cref="TagHelpers.TagHelper"/>.
        /// </summary>
        public string TagHelperRendererStartMethodName { get; private set; }

        /// <summary>
        /// The name of the method to invoke when ending a <see cref="TagHelpers.TagHelper"/>.
        /// </summary>
        public string TagHelperRendererEndMethodName { get; private set; }

        /// <summary>
        /// The name of the access point to get a bodies buffered contents.
        /// </summary>
        public string TagHelperRendererBodyBufferName { get; private set; }

        /// <summary>
        /// The name of the method to invoke to start using a provided writer.
        /// </summary>
        public string TagHelperUseWriterName { get; private set; }

        /// <summary>
        /// The name of the method to invoke when done using the existing writer that was provided via 
        /// <see cref="TagHelperUseWriterName"/>.
        /// </summary>
        public string TagHelperNextWriterName { get; private set; }
    }
}