// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Used to resolve <see cref="TagHelperDescriptor"/>s.
    /// </summary>
    public class TagHelperDescriptorResolver : ITagHelperDescriptorResolver
    {
        private const string TagHelperNameEnding = "TagHelper";
        private const ContentBehavior DefaultContentBehavior = default(ContentBehavior);

        private ITagHelperTypeResolver _tagHelperTypeResolver;

        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperDescriptorResolver"/> class.
        /// </summary>
        /// <param name="tagHelperTypeResolver">
        /// A type resolver used to resolve tag helper <see cref="Type"/>s.
        /// </param>
        public TagHelperDescriptorResolver([NotNull] ITagHelperTypeResolver tagHelperTypeResolver)
        {
            _tagHelperTypeResolver = tagHelperTypeResolver;
        }

        /// <summary>
        /// Resolves <see cref="TagHelperDescriptor"/>s based on the given <paramref name="lookupText"/>.
        /// </summary>
        /// <param name="lookupText">
        /// A <see cref="string"/> location on where to find tag helper <see cref="Type"/>s.
        /// </param>
        /// <returns>An <see cref="IEnumerable{TagHelperDescriptor}"/> that represent tag helpers associated with the
        /// given <paramref name="lookupText"/>.</returns>
        public IEnumerable<TagHelperDescriptor> Resolve(string lookupText)
        {
            var tagHelperTypes = _tagHelperTypeResolver.Resolve(lookupText);

            var descriptors = tagHelperTypes.Select(GetTagHelperDescriptors);

            EnsureNoConflictingAttributes(descriptors);

            // TODO: Validate no conflicting ContentBehaviors after: 
            // https://github.com/aspnet/Razor/issues/122 and https://github.com/aspnet/Razor/issues/120

            return descriptors;
        }

        // TODO: Make this method return multiple TagHelperDescriptors based on a TagNameAttribute: 
        // https://github.com/aspnet/Razor/issues/120
        private static TagHelperDescriptor GetTagHelperDescriptors(Type type)
        {
            var tagName = GetTagNameTarget(type);
            var tagHelperTypeName = type.FullName;
            var attributeDescriptors = GetTagHelperAttributeDescriptors(type);
            var contentBehavior = GetContentBehavior(type);

            return new TagHelperDescriptor(tagName,
                                           tagHelperTypeName,
                                           contentBehavior,
                                           attributeDescriptors);
        }

        // TODO: Make this method support TagNameAttribute targets: https://github.com/aspnet/Razor/issues/120
        private static string GetTagNameTarget(Type tagHelperType)
        {
            var name = tagHelperType.Name;

            if (name.EndsWith(TagHelperNameEnding, StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - TagHelperNameEnding.Length);
            }

            return name;
        }

        private static IEnumerable<TagHelperAttributeDescriptor> GetTagHelperAttributeDescriptors(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            var properties = typeInfo.DeclaredProperties.Where(IsValidTagHelperProperty);
            var attributeDescriptors = properties.Select(ToTagHelperAttributeDescriptor);

            // TODO: Validate no conflicting HTML attribute names: https://github.com/aspnet/Razor/issues/121

            return attributeDescriptors;
        }

        // TODO: Make the HTML attribute name support names from a AttributeNameAttribute: 
        // https://github.com/aspnet/Razor/issues/121
        private static TagHelperAttributeDescriptor ToTagHelperAttributeDescriptor(PropertyInfo property)
        {
            return new TagHelperAttributeDescriptor(property.Name, property);
        }

        // TODO: Make the content behavior pull from a ContentBehaviorAttribute: https://github.com/aspnet/Razor/issues/122
        private static ContentBehavior GetContentBehavior(Type type)
        {
            return DefaultContentBehavior;
        }

        private static bool IsValidTagHelperProperty(PropertyInfo property)
        {
            return property.GetMethod != null && property.SetMethod != null;
        }

        private static void EnsureNoConflictingAttributes(IEnumerable<TagHelperDescriptor> descriptors)
        {
            var tagAttributes = new Dictionary<string, Dictionary<string, TagHelperAttributeErrorTracker>>(StringComparer.OrdinalIgnoreCase);

            foreach (var descriptor in descriptors)
            {
                Dictionary<string, TagHelperAttributeErrorTracker> attributeMappings;

                // Verify that there's a mapping dictionary to track the HTML tag names attributes
                if (!tagAttributes.TryGetValue(descriptor.TagName, out attributeMappings))
                {
                    attributeMappings = new Dictionary<string, TagHelperAttributeErrorTracker>(StringComparer.OrdinalIgnoreCase);
                    tagAttributes[descriptor.TagName] = attributeMappings;
                }

                foreach (var attributeDescriptor in descriptor.Attributes)
                {
                    if (attributeMappings.TryGetValue(attributeDescriptor.AttributeName, out var attributeTracker))
                    {
                        // If there's already an attribute descriptor under the current HTML tag name and it happens to
                        // target the same HTML attribute name: the types must be identical. Conflicting types means
                        // we can't expect a single type value from the HTML attribute.
                        if (attributeTracker.AttributeType != attributeDescriptor.PropertyInfo.PropertyType)
                        {
                            throw new InvalidOperationException(
                                Resources.FormatTagHelpers_CannotHaveConflictingAttributeTypes(
                                    descriptor.TagHelperName,
                                    attributeDescriptor.AttributeName,
                                    attributeDescriptor.PropertyInfo.PropertyType.ToString(),
                                    attributeTracker.TagHelperName,
                                    attributeTracker.AttributeType.ToString()));
                        }
                    }
                    else
                    {
                        attributeMappings[attributeDescriptor.AttributeName] = new TagHelperAttributeErrorTracker
                        {
                            TagHelperName = descriptor.TagHelperName,
                            AttributeType = attributeDescriptor.PropertyInfo.PropertyType
                        };
                    }
                }
            }
        }

        private class TagHelperAttributeErrorTracker
        {
            public string TagHelperName { get; set; }
            public Type AttributeType { get; set; }
        }
    }
}