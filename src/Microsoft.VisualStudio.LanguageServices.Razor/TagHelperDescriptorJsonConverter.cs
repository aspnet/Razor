// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.AspNetCore.Razor.Evolution.Legacy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    internal class TagHelperDescriptorJsonConverter : JsonConverter
    {
        public static readonly TagHelperDescriptorJsonConverter Instance = new TagHelperDescriptorJsonConverter();
        private const string RazorDiagnosticMessageKey = "Message";
        private const string RazorDiagnosticTypeNameKey = "TypeName";

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TagHelperDescriptor) ||
                objectType == typeof(DefaultRazorDiagnostic) ||
                objectType == typeof(LegacyRazorDiagnostic);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                return null;
            }

            var descriptor = JObject.Load(reader);
            var descriptorKind = descriptor[nameof(TagHelperDescriptor.Kind)].Value<string>();
            if (descriptorKind != ITagHelperDescriptorBuilder.DescriptorKind)
            {
                throw new NotSupportedException();
            }

            var typeName = descriptor[nameof(TagHelperDescriptor.Name)].Value<string>();
            var assemblyName = descriptor[nameof(TagHelperDescriptor.AssemblyName)].Value<string>();
            var tagMatchingRules = descriptor[nameof(TagHelperDescriptor.TagMatchingRules)].Value<JArray>();
            var boundAttributes = descriptor[nameof(TagHelperDescriptor.BoundAttributes)].Value<JArray>();
            var childTags = descriptor[nameof(TagHelperDescriptor.AllowedChildTags)].Value<JArray>();
            var documentation = descriptor[nameof(TagHelperDescriptor.Documentation)].Value<string>();
            var tagOutputHint = descriptor[nameof(TagHelperDescriptor.TagOutputHint)].Value<string>();
            var diagnostics = descriptor[nameof(TagHelperDescriptor.Diagnostics)].Value<JArray>();
            var metadata = descriptor[nameof(TagHelperDescriptor.Metadata)].Value<JObject>();

            var builder = ITagHelperDescriptorBuilder.Create(typeName, assemblyName);

            builder
                .Documentation(documentation)
                .TagOutputHint(tagOutputHint);

            foreach (var tagMatchingRule in tagMatchingRules)
            {
                builder.TagMatchingRule(b => ReadTagMatchingRule(b, tagMatchingRule.Value<JObject>(), serializer));
            }

            foreach (var boundAttribute in boundAttributes)
            {
                builder.BindAttribute(b => ReadBoundAttribute(b, boundAttribute.Value<JObject>(), serializer));
            }

            foreach (var childTag in childTags)
            {
                builder.AllowChildTag(childTag.Value<string>());
            }

            foreach (var diagnostic in diagnostics)
            {
                builder.AddDiagnostic(ReadDiagnostic(diagnostic.Value<JObject>(), serializer));
            }

            var metadataValue = serializer.Deserialize<Dictionary<string, string>>(metadata.CreateReader());
            foreach (var item in metadataValue)
            {
                builder.AddMetadata(item.Key, item.Value);
            }

            return builder.Build();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var diagnostic = (RazorDiagnostic)value;
            writer.WriteStartObject();
            WriteProperty(writer, nameof(RazorDiagnostic.Id), diagnostic.Id);
            WriteProperty(writer, nameof(RazorDiagnostic.Severity), (int)diagnostic.Severity);
            WriteProperty(writer, RazorDiagnosticMessageKey, diagnostic.GetMessage());
            WriteProperty(writer, RazorDiagnosticTypeNameKey, diagnostic.GetType().FullName);

            var span = diagnostic.Span;
            writer.WritePropertyName(nameof(RazorDiagnostic.Span));
            writer.WriteStartObject();
            WriteProperty(writer, nameof(SourceSpan.FilePath), span.FilePath);
            WriteProperty(writer, nameof(SourceSpan.AbsoluteIndex), span.AbsoluteIndex);
            WriteProperty(writer, nameof(SourceSpan.LineIndex), span.LineIndex);
            WriteProperty(writer, nameof(SourceSpan.CharacterIndex), span.CharacterIndex);
            WriteProperty(writer, nameof(SourceSpan.Length), span.Length);
            writer.WriteEndObject();

            writer.WriteEndObject();
        }

        private void ReadTagMatchingRule(TagMatchingRuleBuilder builder, JObject rule, JsonSerializer serializer)
        {
            var tagName = rule[nameof(TagMatchingRule.TagName)].Value<string>();
            var attributes = rule[nameof(TagMatchingRule.Attributes)].Value<JArray>();
            var parentTag = rule[nameof(TagMatchingRule.ParentTag)].Value<string>();
            var tagStructure = rule[nameof(TagMatchingRule.TagStructure)].Value<int>();
            var diagnostics = rule[nameof(TagMatchingRule.Diagnostics)].Value<JArray>();

            builder
                .RequireTagName(tagName)
                .RequireParentTag(parentTag)
                .RequireTagStructure((TagStructure)tagStructure);

            foreach (var attribute in attributes)
            {
                builder.RequireAttribute(b => ReadRequiredAttribute(b, attribute.Value<JObject>(), serializer));
            }

            foreach (var diagnostic in diagnostics)
            {
                builder.AddDiagnostic(ReadDiagnostic(diagnostic.Value<JObject>(), serializer));
            }
        }

        private void ReadRequiredAttribute(RequiredAttributeDescriptorBuilder builder, JObject attribute, JsonSerializer serializer)
        {
            var name = attribute[nameof(RequiredAttributeDescriptor.Name)].Value<string>();
            var nameComparison = attribute[nameof(RequiredAttributeDescriptor.NameComparison)].Value<int>();
            var value = attribute[nameof(RequiredAttributeDescriptor.Value)].Value<string>();
            var valueComparison = attribute[nameof(RequiredAttributeDescriptor.ValueComparison)].Value<int>();
            var diagnostics = attribute[nameof(RequiredAttributeDescriptor.Diagnostics)].Value<JArray>();

            builder
                .Name(name)
                .NameComparisonMode((RequiredAttributeDescriptor.NameComparisonMode)nameComparison)
                .Value(value)
                .ValueComparisonMode((RequiredAttributeDescriptor.ValueComparisonMode)valueComparison);

            foreach (var diagnostic in diagnostics)
            {
                builder.AddDiagnostic(ReadDiagnostic(diagnostic.Value<JObject>(), serializer));
            }
        }

        private void ReadBoundAttribute(ITagHelperBoundAttributeDescriptorBuilder builder, JObject attribute, JsonSerializer serializer)
        {
            var descriptorKind = attribute[nameof(BoundAttributeDescriptor.Kind)].Value<string>();
            if (descriptorKind != ITagHelperBoundAttributeDescriptorBuilder.DescriptorKind)
            {
                throw new NotSupportedException();
            }

            var name = attribute[nameof(BoundAttributeDescriptor.Name)].Value<string>();
            var typeName = attribute[nameof(BoundAttributeDescriptor.TypeName)].Value<string>();
            var isEnum = attribute[nameof(BoundAttributeDescriptor.IsEnum)].Value<bool>();
            var indexerNamePrefix = attribute[nameof(BoundAttributeDescriptor.IndexerNamePrefix)].Value<string>();
            var indexerTypeName = attribute[nameof(BoundAttributeDescriptor.IndexerTypeName)].Value<string>();
            var documentation = attribute[nameof(BoundAttributeDescriptor.Documentation)].Value<string>();
            var diagnostics = attribute[nameof(BoundAttributeDescriptor.Diagnostics)].Value<JArray>();
            var metadata = attribute[nameof(BoundAttributeDescriptor.Metadata)].Value<JObject>();

            builder
                .Name(name)
                .TypeName(typeName)
                .Documentation(documentation)
                .AsDictionary(indexerNamePrefix, indexerTypeName);

            if (isEnum)
            {
                builder.AsEnum();
            }

            foreach (var diagnostic in diagnostics)
            {
                builder.AddDiagnostic(ReadDiagnostic(diagnostic.Value<JObject>(), serializer));
            }

            var metadataValue = serializer.Deserialize<Dictionary<string, string>>(metadata.CreateReader());
            foreach (var item in metadataValue)
            {
                if (string.Equals(item.Key, ITagHelperBoundAttributeDescriptorBuilder.PropertyNameKey, StringComparison.Ordinal))
                {
                    builder.PropertyName(item.Value);
                }

                builder.AddMetadata(item.Key, item.Value);
            }
        }

        private RazorDiagnostic ReadDiagnostic(JObject diagnostic, JsonSerializer serializer)
        {
            var span = diagnostic[nameof(RazorDiagnostic.Span)].Value<JObject>();
            var absoluteIndex = span[nameof(SourceSpan.AbsoluteIndex)].Value<int>();
            var lineIndex = span[nameof(SourceSpan.LineIndex)].Value<int>();
            var characterIndex = span[nameof(SourceSpan.CharacterIndex)].Value<int>();
            var length = span[nameof(SourceSpan.Length)].Value<int>();
            var filePath = span[nameof(SourceSpan.FilePath)].Value<string>();
            var message = diagnostic[RazorDiagnosticMessageKey].Value<string>();
            var typeName = diagnostic[RazorDiagnosticTypeNameKey].Value<string>();

            if (string.Equals(typeName, typeof(DefaultRazorDiagnostic).FullName, StringComparison.Ordinal))
            {
                var id = diagnostic[nameof(RazorDiagnostic.Id)].Value<string>();
                var severity = diagnostic[nameof(RazorDiagnostic.Severity)].Value<int>();

                var descriptor = new RazorDiagnosticDescriptor(id, () => message, (RazorDiagnosticSeverity)severity);
                var sourceSpan = new SourceSpan(filePath, absoluteIndex, lineIndex, characterIndex, length);

                return RazorDiagnostic.Create(descriptor, sourceSpan);
            }
            else if (string.Equals(typeName, typeof(LegacyRazorDiagnostic).FullName, StringComparison.Ordinal))
            {
                var error = new RazorError(message, absoluteIndex, lineIndex, characterIndex, length);

                return RazorDiagnostic.Create(error);
            }

            throw new NotSupportedException(
                Resources.FormatTagHelperDescriptorJsonConverter_UnsupportedRazorDiagnosticType(typeName));
        }

        private void WriteProperty<T>(JsonWriter writer, string key, T value)
        {
            writer.WritePropertyName(key);
            writer.WriteValue(value);
        }
    }
}
