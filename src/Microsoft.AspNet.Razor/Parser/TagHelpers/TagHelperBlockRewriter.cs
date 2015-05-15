// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;

namespace Microsoft.AspNet.Razor.Parser.TagHelpers.Internal
{
    public static class TagHelperBlockRewriter
    {
        private static readonly string StringTypeName = typeof(string).FullName;

        public static TagHelperBlockBuilder Rewrite(
            string tagName,
            bool validStructure,
            Block tag,
            IEnumerable<TagHelperDescriptor> descriptors,
            ErrorSink errorSink)
        {
            // There will always be at least one child for the '<'.
            var start = tag.Children.First().Start;
            var attributes = GetTagAttributes(tagName, validStructure, tag, descriptors, errorSink);
            var selfClosing = IsSelfClosing(tag);

            return new TagHelperBlockBuilder(tagName, selfClosing, start, attributes, descriptors);
        }

        private static IList<KeyValuePair<string, SyntaxTreeNode>> GetTagAttributes(
            string tagName,
            bool validStructure,
            Block tagBlock,
            IEnumerable<TagHelperDescriptor> descriptors,
            ErrorSink errorSink)
        {
            // Ignore all but one descriptor per type since this method uses the TagHelperDescriptors only to get the
            // contained TagHelperAttributeDescriptor's.
            descriptors = descriptors.Distinct(TypeBasedTagHelperDescriptorComparer.Default);

            var attributes = new List<KeyValuePair<string, SyntaxTreeNode>>();

            // We skip the first child "<tagname" and take everything up to the ending portion of the tag ">" or "/>".
            // The -2 accounts for both the start and end tags. If the tag does not have a valid structure then there's
            // no end tag to ignore.
            var symbolOffset = validStructure ? 2 : 1;
            var attributeChildren = tagBlock.Children.Skip(1).Take(tagBlock.Children.Count() - symbolOffset);

            foreach (var child in attributeChildren)
            {
                KeyValuePair<string, SyntaxTreeNode> attribute;
                bool isBoundAttribute;
                bool isBoundNonStringAttribute;
                bool succeeded;
                if (child.IsBlock)
                {
                    succeeded = TryParseBlock(
                        tagName,
                        (Block)child,
                        descriptors,
                        errorSink,
                        out attribute,
                        out isBoundAttribute,
                        out isBoundNonStringAttribute);
                }
                else
                {
                    succeeded = TryParseSpan(
                        (Span)child,
                        descriptors,
                        errorSink,
                        out attribute,
                        out isBoundAttribute,
                        out isBoundNonStringAttribute);
                }

                // Only want to track the attribute if we succeeded in parsing its corresponding Block/Span.
                if (succeeded)
                {
                    // Check if it's a bound attribute that is minimized or if it's a bound non-string attribute that
                    // is null or whitespace.
                    if ((isBoundAttribute && attribute.Value == null) ||
                        (isBoundNonStringAttribute && IsNullOrWhitespaceAttributeValue(attribute.Value)))
                    {
                        var errorLocation = GetAttributeNameStartLocation(child);

                        errorSink.OnError(
                            errorLocation,
                            RazorResources.FormatRewriterError_EmptyTagHelperBoundAttribute(
                                attribute.Key,
                                tagName,
                                GetPropertyType(attribute.Key, descriptors)),
                            attribute.Key.Length);
                    }

                    attributes.Add(new KeyValuePair<string, SyntaxTreeNode>(attribute.Key, attribute.Value));
                }
            }

            return attributes;
        }

        private static bool IsSelfClosing(Block beginTagBlock)
        {
            var childSpan = beginTagBlock.FindLastDescendentSpan();

            return childSpan?.Content.EndsWith("/>") ?? false;
        }

        // This method handles cases when the attribute is a simple span attribute such as
        // class="something moresomething".  This does not handle complex attributes such as
        // class="@myclass". Therefore the span.Content is equivalent to the entire attribute.
        private static bool TryParseSpan(
            Span span,
            IEnumerable<TagHelperDescriptor> descriptors,
            ErrorSink errorSink,
            out KeyValuePair<string, SyntaxTreeNode> attribute,
            out bool isBoundAttribute,
            out bool isBoundNonStringAttribute)
        {
            var afterEquals = false;
            var builder = new SpanBuilder
            {
                CodeGenerator = span.CodeGenerator,
                EditHandler = span.EditHandler,
                Kind = span.Kind
            };

            // Will contain symbols that represent a single attribute value: <input| class="btn"| />
            var htmlSymbols = span.Symbols.OfType<HtmlSymbol>().ToArray();
            var capturedAttributeValueStart = false;
            var attributeValueStartLocation = span.Start;

            // The symbolOffset is initialized to 0 to expect worst case: "class=". If a quote is found later on for
            // the attribute value the symbolOffset is adjusted accordingly.
            var symbolOffset = 0;
            string name = null;

            // Iterate down through the symbols to find the name and the start of the value.
            // We subtract the symbolOffset so we don't accept an ending quote of a span.
            for (var i = 0; i < htmlSymbols.Length - symbolOffset; i++)
            {
                var symbol = htmlSymbols[i];

                if (afterEquals)
                {
                    // We've captured all leading whitespace, the attribute name, and an equals with an optional
                    // quote/double quote. We're now at: " asp-for='|...'" or " asp-for=|..."
                    // The goal here is to capture all symbols until the end of the attribute. Note this will not
                    // consume an ending quote due to the symbolOffset.

                    // When symbols are accepted into SpanBuilders, their locations get altered to be offset by the
                    // parent which is why we need to mark our start location prior to adding the symbol.
                    // This is needed to know the location of the attribute value start within the document.
                    if (!capturedAttributeValueStart)
                    {
                        capturedAttributeValueStart = true;

                        attributeValueStartLocation = span.Start + symbol.Start;
                    }

                    builder.Accept(symbol);
                }
                else if (name == null && symbol.Type == HtmlSymbolType.Text)
                {
                    // We've captured all leading whitespace prior to the attribute name.
                    // We're now at: " |asp-for='...'" or " |asp-for=..."
                    // The goal here is to capture the attribute name.

                    name = symbol.Content;
                    attributeValueStartLocation = SourceLocation.Advance(attributeValueStartLocation, name);
                }
                else if (symbol.Type == HtmlSymbolType.Equals)
                {
                    Debug.Assert(
                        name != null,
                        "Name should never be null here. The parser should guarantee an attribute has a name.");

                    // We've captured all leading whitespace and the attribute name.
                    // We're now at: " asp-for|='...'" or " asp-for|=..."
                    // The goal here is to consume the equal sign and the optional single/double-quote.

                    // The coming symbols will either be a quote or value (in the case that the value is unquoted).
                    // Spaces after/before the equal symbol are not yet supported:
                    // https://github.com/aspnet/Razor/issues/123

                    // TODO: Handle malformed tags, if there's an '=' then there MUST be a value.
                    // https://github.com/aspnet/Razor/issues/104

                    SourceLocation symbolStartLocation;

                    // Check for attribute start values, aka single or double quote
                    if ((i + 1) < htmlSymbols.Length && IsQuote(htmlSymbols[i + 1]))
                    {
                        // Move past the attribute start so we can accept the true value.
                        i++;
                        symbolStartLocation = htmlSymbols[i].Start;

                        // If there's a start quote then there must be an end quote to be valid, skip it.
                        symbolOffset = 1;
                    }
                    else
                    {
                        symbolStartLocation = symbol.Start;
                    }

                    attributeValueStartLocation =
                        span.Start +
                        symbolStartLocation +
                        new SourceLocation(absoluteIndex: 1, lineIndex: 0, characterIndex: 1);

                    afterEquals = true;
                }
                else if (symbol.Type == HtmlSymbolType.WhiteSpace)
                {
                    // We're at the start of the attribute, this branch may be hit on the first iterations of
                    // the loop since the parser separates attributes with their spaces included as symbols.
                    // We're at: "| asp-for='...'" or "| asp-for=..."
                    // Note: This will not be hit even for situations like asp-for  ="..." because the core Razor
                    // parser currently does not know how to handle attributes in that format. This will be addressed
                    // by https://github.com/aspnet/Razor/issues/123.

                    attributeValueStartLocation = SourceLocation.Advance(attributeValueStartLocation, symbol.Content);
                }
            }

            // After all symbols have been added we need to set the builders start position so we do not indirectly
            // modify each symbol's Start location.
            builder.Start = attributeValueStartLocation;

            if (name == null)
            {
                // We couldn't find a name, if the original span content was whitespace it ultimately means the tag
                // that owns this "attribute" is malformed and is expecting a user to type a new attribute.
                // ex: <myTH class="btn"| |
                if (!string.IsNullOrWhiteSpace(span.Content))
                {
                    errorSink.OnError(
                        span.Start,
                        RazorResources.TagHelperBlockRewriter_TagHelperAttributeListMustBeWelformed,
                        span.Content.Length);
                }

                attribute = default(KeyValuePair<string, SyntaxTreeNode>);
                isBoundAttribute = false;
                isBoundNonStringAttribute = false;

                return false;
            }

            isBoundAttribute = IsBoundAttribute(name, descriptors, out isBoundNonStringAttribute);

            // If we're not after an equal then we should treat the value as if it were a minimized attribute.
            var attributeValueBuilder = afterEquals ? builder : null;
            attribute = CreateMarkupAttribute(name, attributeValueBuilder, isBoundNonStringAttribute);

            return true;
        }

        private static bool TryParseBlock(
            string tagName,
            Block block,
            IEnumerable<TagHelperDescriptor> descriptors,
            ErrorSink errorSink,
            out KeyValuePair<string, SyntaxTreeNode> attribute,
            out bool isBoundAttribute,
            out bool isBoundNonStringAttribute)
        {
            // TODO: Accept more than just spans: https://github.com/aspnet/Razor/issues/96.
            // The first child will only ever NOT be a Span if a user is doing something like:
            // <input @checked />

            var childSpan = block.Children.First() as Span;

            if (childSpan == null || childSpan.Kind != SpanKind.Markup)
            {
                errorSink.OnError(block.Children.First().Start,
                                  RazorResources.FormatTagHelpers_CannotHaveCSharpInTagDeclaration(tagName));

                attribute = default(KeyValuePair<string, SyntaxTreeNode>);
                isBoundAttribute = false;
                isBoundNonStringAttribute = false;

                return false;
            }

            var builder = new BlockBuilder(block);

            // If there's only 1 child it means that it's plain text inside of the attribute.
            // i.e. <div class="plain text in attribute">
            if (builder.Children.Count == 1)
            {
                return TryParseSpan(
                    childSpan,
                    descriptors,
                    errorSink,
                    out attribute,
                    out isBoundAttribute,
                    out isBoundNonStringAttribute);
            }

            var textSymbol = childSpan.Symbols.FirstHtmlSymbolAs(HtmlSymbolType.Text);
            var name = textSymbol != null ? textSymbol.Content : null;

            if (name == null)
            {
                errorSink.OnError(childSpan.Start, RazorResources.FormatTagHelpers_AttributesMustHaveAName(tagName));

                attribute = default(KeyValuePair<string, SyntaxTreeNode>);
                isBoundAttribute = false;
                isBoundNonStringAttribute = false;

                return false;
            }

            // Have a name now. Able to determine correct isBoundNonStringAttribute value.
            isBoundAttribute = IsBoundAttribute(name, descriptors, out isBoundNonStringAttribute);

            // TODO: Support no attribute values: https://github.com/aspnet/Razor/issues/220

            // Remove first child i.e. foo="
            builder.Children.RemoveAt(0);

            // Grabbing last child to check if the attribute value is quoted.
            var endNode = block.Children.Last();
            if (!endNode.IsBlock)
            {
                var endSpan = (Span)endNode;

                // In some malformed cases e.g. <p bar="false', the last Span (false' in the ex.) may contain more
                // than a single HTML symbol. Do not ignore those other symbols.
                var symbolCount = endSpan.Symbols.Count();
                var endSymbol = symbolCount == 1 ? (HtmlSymbol)endSpan.Symbols.First() : null;

                // Checking to see if it's a quoted attribute, if so we should remove end quote
                if (endSymbol != null && IsQuote(endSymbol))
                {
                    builder.Children.RemoveAt(builder.Children.Count - 1);
                }
            }

            // We need to rebuild the code generators of the builder and its children (this is needed to
            // ensure we don't do special attribute code generation since this is a tag helper).
            block = RebuildCodeGenerators(builder.Build());

            // If there's only 1 child at this point its value could be a simple markup span (treated differently than
            // block level elements for attributes).
            if (block.Children.Count() == 1)
            {
                var child = block.Children.First() as Span;

                if (child != null)
                {
                    // After pulling apart the block we just have a value span.
                    var spanBuilder = new SpanBuilder(child);

                    attribute = CreateMarkupAttribute(name, spanBuilder, isBoundNonStringAttribute);

                    return true;
                }
            }

            attribute = new KeyValuePair<string, SyntaxTreeNode>(name, block);

            return true;
        }

        private static Block RebuildCodeGenerators(Block block)
        {
            var builder = new BlockBuilder(block);

            var isDynamic = builder.CodeGenerator is DynamicAttributeBlockCodeGenerator;

            // We don't want any attribute specific logic here, null out the block code generator.
            if (isDynamic || builder.CodeGenerator is AttributeBlockCodeGenerator)
            {
                builder.CodeGenerator = BlockCodeGenerator.Null;
            }

            for (var i = 0; i < builder.Children.Count; i++)
            {
                var child = builder.Children[i];

                if (child.IsBlock)
                {
                    // The child is a block, recurse down into the block to rebuild its children
                    builder.Children[i] = RebuildCodeGenerators((Block)child);
                }
                else
                {
                    var childSpan = (Span)child;
                    ISpanCodeGenerator newCodeGenerator = null;
                    var literalGenerator = childSpan.CodeGenerator as LiteralAttributeCodeGenerator;

                    if (literalGenerator != null)
                    {
                        if (literalGenerator.ValueGenerator == null || literalGenerator.ValueGenerator.Value == null)
                        {
                            newCodeGenerator = new MarkupCodeGenerator();
                        }
                        else
                        {
                            newCodeGenerator = literalGenerator.ValueGenerator.Value;
                        }
                    }
                    else if (isDynamic && childSpan.CodeGenerator == SpanCodeGenerator.Null)
                    {
                        // Usually the dynamic code generator handles rendering the null code generators underneath
                        // it. This doesn't make sense in terms of tag helpers though, we need to change null code
                        // generators to markup code generators.

                        newCodeGenerator = new MarkupCodeGenerator();
                    }

                    // If we have a new code generator we'll need to re-build the child
                    if (newCodeGenerator != null)
                    {
                        var childSpanBuilder = new SpanBuilder(childSpan)
                        {
                            CodeGenerator = newCodeGenerator
                        };

                        builder.Children[i] = childSpanBuilder.Build();
                    }
                }
            }

            return builder.Build();
        }

        private static SourceLocation GetAttributeNameStartLocation(SyntaxTreeNode node)
        {
            Span span;
            var nodeStart = SourceLocation.Undefined;

            if (node.IsBlock)
            {
                span = ((Block)node).FindFirstDescendentSpan();
                nodeStart = span.Parent.Start;
            }
            else
            {
                span = (Span)node;
                nodeStart = span.Start;
            }

            // Span should never be null here, this should only ever be called if an attribute was successfully parsed.
            Debug.Assert(span != null);

            // Attributes must have at least one non-whitespace character to represent the tagName (even if its a C#
            // expression).
            var firstNonWhitespaceSymbol = span
                .Symbols
                .OfType<HtmlSymbol>()
                .First(sym => sym.Type != HtmlSymbolType.WhiteSpace && sym.Type != HtmlSymbolType.NewLine);

            return nodeStart + firstNonWhitespaceSymbol.Start;
        }

        private static KeyValuePair<string, SyntaxTreeNode> CreateMarkupAttribute(
            string name,
            SpanBuilder builder,
            bool isBoundNonStringAttribute)
        {
            Span value = null;

            // Builder will be null in the case of minimized attributes
            if (builder != null)
            {
                // If the attribute was requested by a tag helper but the corresponding property was not a string,
                // then treat its value as code. A non-string value can be any C# value so we need to ensure the
                // SyntaxTreeNode reflects that.
                if (isBoundNonStringAttribute)
                {
                    builder.Kind = SpanKind.Code;
                }

                value = builder.Build();
            }

            return new KeyValuePair<string, SyntaxTreeNode>(name, value);
        }

        private static bool IsNullOrWhitespaceAttributeValue(SyntaxTreeNode attributeValue)
        {
            if (attributeValue.IsBlock)
            {
                foreach (var span in ((Block)attributeValue).Flatten())
                {
                    if (!string.IsNullOrWhiteSpace(span.Content))
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return string.IsNullOrWhiteSpace(((Span)attributeValue).Content);
            }
        }

        // Determines the full name of the <see cref="Type"/> of the property corresponding to an attribute named
        // <paramref name="name"/>.
        private static string GetPropertyType(string name, IEnumerable<TagHelperDescriptor> descriptors)
        {
            var firstBoundAttribute = FindFirstBoundAttribute(name, descriptors);

            return firstBoundAttribute?.TypeName;
        }

        // Determines whether an attribute named <paramref name="name"/> is bound to a non-<see cref="string"/> tag
        // helper property.
        private static bool IsBoundAttribute(
            string name,
            IEnumerable<TagHelperDescriptor> descriptors,
            out bool isBoundNonStringAttribute)
        {
            var firstBoundAttribute = FindFirstBoundAttribute(name, descriptors);
            var isBoundAttribute = firstBoundAttribute != null;
            isBoundNonStringAttribute = isBoundAttribute && !firstBoundAttribute.IsStringProperty;

            return isBoundAttribute;
        }

        // Finds first TagHelperAttributeDescriptor matching given name.
        private static TagHelperAttributeDescriptor FindFirstBoundAttribute(
            string name,
            IEnumerable<TagHelperDescriptor> descriptors)
        {
            return descriptors
                .SelectMany(descriptor => descriptor.Attributes)
                .FirstOrDefault(attribute => string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsQuote(HtmlSymbol htmlSymbol)
        {
            return htmlSymbol.Type == HtmlSymbolType.DoubleQuote ||
                   htmlSymbol.Type == HtmlSymbolType.SingleQuote;
        }
    }
}