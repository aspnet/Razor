// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Internal.Web.Utils;

namespace Microsoft.AspNet.Razor.Generator.Compiler.CSharp
{
    /// <summary>
    /// Renders tag helper rendering code.
    /// </summary>
    public class CSharpTagHelperCodeRenderer
    {
        internal static readonly string ManagerVariableName = "__tagHelperManager";
        internal static readonly string BufferedStringValueVariableName = "__tagHelperBufferedStringValue";

        private static readonly TypeInfo StringTypeInfo = typeof(string).GetTypeInfo();
        private static readonly TagHelperAttributeDescriptorComparer AttributeDescriptorComparer =
            new TagHelperAttributeDescriptorComparer();

        // TODO: The work to properly implement this will be done in: https://github.com/aspnet/Razor/issues/74
        private static readonly TagHelperAttributeValueCodeRenderer AttributeValueCodeRenderer =
            new TagHelperAttributeValueCodeRenderer();

        private readonly CSharpCodeWriter _writer;
        private readonly CodeBuilderContext _context;
        private readonly IChunkVisitor _bodyVisitor;
        private readonly GeneratedTagHelperContext _tagHelperContext;

        /// <summary>
        /// Instantiates a new <see cref="CSharpTagHelperCodeRenderer"/>.
        /// </summary>
        /// <param name="bodyVisitor">The <see cref="IChunkVisitor"/> used to render chunks found in the body.</param>
        /// <param name="writer">The <see cref="CSharpCodeWriter"/> that's used to write code.</param>
        /// <param name="context">A <see cref="CodeGeneratorContext"/> instance that contains information about 
        /// the current code generation process.</param>
        public CSharpTagHelperCodeRenderer([NotNull] IChunkVisitor bodyVisitor,
                                           [NotNull] CSharpCodeWriter writer,
                                           [NotNull] CodeBuilderContext context)
        {
            _writer = writer;
            _context = context;
            _bodyVisitor = bodyVisitor;
            _tagHelperContext = context.Host.GeneratedClassContext.GeneratedTagHelperContext;
        }

        /// <summary>
        /// Renders the code to render the given <paramref name="chunk"/>.
        /// </summary>
        /// <param name="chunk">A <see cref="TagHelperChunk"/> to render.</param>
        public void RenderTagHelper(TagHelperChunk chunk)
        {
            // TODO: Implement design time support for tag helpers in https://github.com/aspnet/Razor/issues/83
            if (_context.Host.DesignTimeMode)
            {
                return;
            }

            var tagHelperDescriptors = chunk.Descriptors;

            // Find the first content behavior that doesn't have a default content behavior.
            // The resolver restricts content behavior collisions so the first one that's not the default will be
            // the content behavior we need to abide by.
            var contentBehavior = tagHelperDescriptors.Select(descriptor => descriptor.ContentBehavior)
                                                      .FirstOrDefault(
                                                            behavior => behavior != ContentBehavior.None);

            RenderTagHelperCreation(chunk);

            var attributeDescriptors = tagHelperDescriptors.SelectMany(descriptor => descriptor.Attributes);
            var boundHTMLAttributes = attributeDescriptors.Select(descriptor => descriptor.AttributeName);
            var htmlAttributes = chunk.Attributes;
            var unboundHTMLAttributes =
                htmlAttributes.Where(htmlAttribute => !boundHTMLAttributes.Contains(htmlAttribute.Key));

            RenderUnboundHTMLAttributes(unboundHTMLAttributes);
            RenderStartTagHelper(chunk.TagName, contentBehavior);
            RenderTagHelperBody(chunk.Children, contentBehavior);
            RenderEndTagHelper(contentBehavior);
        }

        internal static string GetVariableName(TagHelperDescriptor descriptor)
        {
            return string.Format(CultureInfo.InvariantCulture,
                                 "__{0}_{1}",
                                 descriptor.TagName.Replace('-', '_'),
                                 descriptor.TagHelperName.Replace('.', '_'));
        }

        private void RenderTagHelperCreation(TagHelperChunk chunk)
        {
            var tagHelperDescriptors = chunk.Descriptors;
            var htmlAttributeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var tagHelperDescriptor in tagHelperDescriptors)
            {
                var tagHelperVariableName = GetVariableName(tagHelperDescriptor);

                // Create the tag helper
                _writer.WriteStartAssignment(tagHelperVariableName)
                       .Write(ManagerVariableName)
                       .Write(".")
                       .Write(_tagHelperContext.InstantiateTagHelperMethodName)
                       .Write("<")
                       .Write(tagHelperDescriptor.TagHelperName)
                       .WriteLine(">();");

                // Render all of the bound attribute values for the tag helper.
                RenderBoundHTMLAttributes(chunk.Attributes,
                                          tagHelperVariableName,
                                          tagHelperDescriptor.Attributes,
                                          htmlAttributeValues);
            }
        }

        private void RenderBoundHTMLAttributes(IDictionary<string, Chunk> chunkAttributes,
                                               string tagHelperVariableName,
                                               IEnumerable<TagHelperAttributeDescriptor> attributeDescriptors,
                                               Dictionary<string, string> htmlAttributeValues)
        {
            foreach (var attributeDescriptor in attributeDescriptors)
            {
                Chunk attributeValueChunk;

                var providedAttribute = chunkAttributes.TryGetValue(attributeDescriptor.AttributeName,
                                                                    out attributeValueChunk);

                if (providedAttribute)
                {
                    var attributeValueRecorded = htmlAttributeValues.ContainsKey(attributeDescriptor.AttributeName);

                    // Bufferable attributes are attributes that can have Razor code inside of them.
                    var bufferableAttribute = AcceptsRazorCode(attributeDescriptor);

                    // Plain text values are non razor code values. If an attribute is bufferable it may still have
                    // a plain text value if the value does not contain Razor code.
                    string textValue;
                    var isPlainTextValue = TryGetPlainTextValue(attributeValueChunk, out textValue);

                    // If we haven't recorded a value and we need to buffer an attribute value and the value is not 
                    // plain text then we need to prepare the value prior to setting it below.
                    if (!attributeValueRecorded && bufferableAttribute && !isPlainTextValue)
                    {
                        BuildBufferedAttributeValue(attributeValueChunk);
                    }

                    // We capture the tag helpers property value accessor so we can retrieve it later (if we need to).
                    var valueAccessor = string.Format(CultureInfo.InvariantCulture,
                                                      "{0}.{1}",
                                                      tagHelperVariableName,
                                                      attributeDescriptor.AttributePropertyName);

                    _writer.WriteStartAssignment(valueAccessor);

                    // If we haven't recorded this attribute value before then we need to record its value.
                    if (!attributeValueRecorded)
                    {
                        // We only need to create attribute values once per HTML element (not once per tag helper).
                        // We're saving the value accessor so we can retrieve it later if there are more tag helpers that
                        // need the value.
                        htmlAttributeValues.Add(attributeDescriptor.AttributeName, valueAccessor);

                        if (bufferableAttribute)
                        {
                            // If the attribute is bufferable but has a plain text value that means the value
                            // is a string which needs to be surrounded in quotes.
                            if (isPlainTextValue)
                            {
                                RenderQuotedAttributeValue(textValue, attributeDescriptor);
                            }
                            else
                            {
                                // The value contains more than plain text. i.g. someAttribute="Time: @DateTime.Now"
                                RenderBufferedAttributeValue(attributeDescriptor);
                            }
                        }
                        else
                        {
                            // We aren't a bufferable attribute which means we have no Razor code in our value.
                            // Therefore we can just use the "textValue" as the attribute value.
                            RenderRawAttributeValue(textValue, attributeDescriptor);
                        }

                        // Ends the assignment to the attribute.
                        _writer.WriteLine(";");

                        _writer.WriteStartInstanceMethodInvocation(ManagerVariableName,
                                                                   _tagHelperContext.AddTagHelperAttributeMethodName);
                        _writer.WriteStringLiteral(attributeDescriptor.AttributeName)
                               .WriteParameterSeparator()
                               .Write(tagHelperVariableName)
                               .Write(".")
                               .Write(attributeDescriptor.AttributePropertyName)
                               .WriteEndMethodInvocation();
                    }
                    else
                    {
                        // The attribute value has already been recorded, lets retrieve it from the stored value accessors.

                        _writer.Write(htmlAttributeValues[attributeDescriptor.AttributeName])
                               .WriteLine(";");
                    }
                }
            }
        }

        private void RenderUnboundHTMLAttributes(IEnumerable<KeyValuePair<string, Chunk>> unboundHTMLAttributes)
        {
            // Build out the unbound HTML attributes for the tag builder
            foreach (var htmlAttribute in unboundHTMLAttributes)
            {
                string textValue;
                var attributeValue = htmlAttribute.Value;
                var isPlainTextValue = TryGetPlainTextValue(attributeValue, out textValue);

                // HTML attributes are always strings, so if it's not a plain text value, aka
                // something that has C# code then we need to buffer the value.
                if (!isPlainTextValue)
                {
                    BuildBufferedAttributeValue(attributeValue);
                }

                _writer.WriteStartInstanceMethodInvocation(ManagerVariableName,
                                                           _tagHelperContext.AddHtmlAttributeMethodName);
                _writer.WriteStringLiteral(htmlAttribute.Key)
                       .WriteParameterSeparator();

                // If it's a plain text value then we need to surround the value with quotes.
                if (isPlainTextValue)
                {
                    _writer.WriteStringLiteral(textValue);
                }
                else
                {
                    _writer.Write(BufferedStringValueVariableName);
                }

                _writer.WriteEndMethodInvocation();
            }
        }

        private void RenderStartTagHelper(string tagName, ContentBehavior contentBehavior)
        {
            // Call into the Start of the tag helper
            _writer.WriteStartInstanceMethodInvocation(ManagerVariableName, _tagHelperContext.StartTagHelpersScope)
                   .WriteStringLiteral(tagName)
                   .WriteEndMethodInvocation();

            // If the content behavior is NOT modify then we need to execute the tag helpers and render the start tag
            // now
            if (contentBehavior != ContentBehavior.Modify)
            {
                RenderExecuteAndStartTag();
            }

            if (contentBehavior == ContentBehavior.Prepend)
            {
                RenderTagContent();
            }
        }

        private void RenderTagHelperBody(IList<Chunk> children, ContentBehavior contentBehavior)
        {
            // No need to render children if the body is being replaced anyways
            if (contentBehavior != ContentBehavior.Replace)
            {
                // If content behavior is modify we need to use a different writer to buffer the body.
                if (contentBehavior == ContentBehavior.Modify)
                {
                    var bodyWriterName = string.Format("{0}.{1}()",
                                                      ManagerVariableName,
                                                      _tagHelperContext.GetContentBuffer);

                    BuildBufferedWritingScope(bodyWriterName, () =>
                    {
                        // Render all of the tag helper children
                        _bodyVisitor.Accept(children);
                    });
                }
                else
                {
                    // Render all of the tag helper children
                    _bodyVisitor.Accept(children);
                }
            }
        }

        private void RenderEndTagHelper(ContentBehavior contentBehavior)
        {
            // If content behavior is modify then we need to execute AFTER now (after the children)
            // and then render the start tag.
            if (contentBehavior == ContentBehavior.Modify)
            {
                RenderExecuteAndStartTag();
            }

            if (contentBehavior != ContentBehavior.None &&
                contentBehavior != ContentBehavior.Prepend)
            {
                RenderTagContent();
            }

            RenderEndTag();

            _writer.WriteInstanceMethodInvocation(ManagerVariableName, _tagHelperContext.EndTagHelpersScope);
        }

        private void RenderExecuteAndStartTag()
        {
            _writer.Write("await ")
                   .WriteInstanceMethodInvocation(ManagerVariableName, _tagHelperContext.ExecuteTagHelpersAsyncMethodName);

            CSharpCodeVisitor.RenderPreWriteStart(_writer, _context);

            _writer.WriteInstanceMethodInvocation(ManagerVariableName,
                                                  _tagHelperContext.GenerateTagStartMethodName,
                                                  endLine: false);

            _writer.WriteEndMethodInvocation();
        }

        private void RenderTagContent()
        {
            CSharpCodeVisitor.RenderPreWriteStart(_writer, _context);

            _writer.WriteInstanceMethodInvocation(ManagerVariableName,
                                                  _tagHelperContext.GenerateTagContentMethodName,
                                                  endLine: false);

            _writer.WriteEndMethodInvocation();
        }

        private void RenderEndTag()
        {
            CSharpCodeVisitor.RenderPreWriteStart(_writer, _context);

            _writer.WriteInstanceMethodInvocation(ManagerVariableName,
                                                  _tagHelperContext.GenerateTagEndMethodName,
                                                  endLine: false);

            _writer.WriteEndMethodInvocation();
        }


        private void RenderBufferedAttributeValue(TagHelperAttributeDescriptor attributeDescriptor)
        {
            RenderAttribute(
                attributeDescriptor,
                valueRenderer: (writer) =>
                {
                    _writer.Write(BufferedStringValueVariableName);
                });
        }

        private void RenderRawAttributeValue(string value, TagHelperAttributeDescriptor attributeDescriptor)
        {
            RenderAttribute(
                attributeDescriptor,
                valueRenderer: (writer) =>
                {
                    _writer.Write(value);
                });
        }

        private void RenderQuotedAttributeValue(string value, TagHelperAttributeDescriptor attributeDescriptor)
        {
            RenderAttribute(
                attributeDescriptor,
                valueRenderer: (writer) =>
                {
                    _writer.WriteStringLiteral(value);
                });
        }

        private void BuildBufferedAttributeValue(Chunk htmlAttributeChunk)
        {
            BuildBufferedWritingScope(writerVariable: null, renderCode: () =>
            {
                // Render the HTML's attribute chunk(s).
                _bodyVisitor.Accept(htmlAttributeChunk);
            });
        }

        private void BuildBufferedWritingScope(string writerVariable, Action renderCode)
        {
            _writer.Write("try");
            using (_writer.BuildScope())
            {
                _writer.WriteMethodInvocation(_tagHelperContext.NewWritingScopeMethodName, writerVariable);

                renderCode();
            }
            _writer.Write("finally");
            using (_writer.BuildScope())
            {
                _writer.WriteStartAssignment(BufferedStringValueVariableName)
                       .WriteMethodInvocation(_tagHelperContext.EndWritingScopeMethodName);
            }
        }

        private void RenderAttribute(TagHelperAttributeDescriptor attributeDescriptor,
                                     Action<CSharpCodeWriter> valueRenderer)
        {
            AttributeValueCodeRenderer.RenderAttributeValue(attributeDescriptor, _writer, _context, valueRenderer);
        }

        private static bool AcceptsRazorCode(TagHelperAttributeDescriptor attributeDescriptor)
        {
            var attributeType = attributeDescriptor.PropertyInfo.PropertyType.GetTypeInfo();

            return StringTypeInfo.IsAssignableFrom(attributeType);
        }

        private static bool TryGetPlainTextValue(Chunk chunk, out string plainText)
        {
            var chunkBlock = chunk as ChunkBlock;

            plainText = null;

            if (chunkBlock == null || chunkBlock.Children.Count != 1)
            {
                return false;
            }

            var literalChildChunk = chunkBlock.Children[0] as LiteralChunk;

            if (literalChildChunk == null)
            {
                return false;
            }

            plainText = literalChildChunk.Text;

            return true;
        }

        // This class is used to compare tag helper attributes by validating the HTML attribute name only.
        private class TagHelperAttributeDescriptorComparer : IEqualityComparer<TagHelperAttributeDescriptor>
        {
            public bool Equals(TagHelperAttributeDescriptor descriptorX, TagHelperAttributeDescriptor descriptorY)
            {
                return descriptorX.AttributeName.Equals(descriptorY.AttributeName, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(TagHelperAttributeDescriptor descriptor)
            {
                return HashCodeCombiner.Start()
                                       .Add(descriptor.AttributeName)
                                       .CombinedHash;
            }
        }
    }
}