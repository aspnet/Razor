﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Used to resolve <see cref="TagHelperDescriptor"/>s.
    /// </summary>
    public class TagHelperDescriptorResolver : ITagHelperDescriptorResolver
    {
        private readonly TagHelperTypeResolver _typeResolver;

        // internal for testing
        internal TagHelperDescriptorResolver(TagHelperTypeResolver typeResolver)
        {
            _typeResolver = typeResolver;
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperDescriptorResolver"/> class.
        /// </summary>
        public TagHelperDescriptorResolver()
            : this(new TagHelperTypeResolver())
        {
        }

        /// <inheritdoc />
        public IEnumerable<TagHelperDescriptor> Resolve([NotNull] TagHelperDescriptorResolutionContext context)
        {
            var resolvedDescriptors = new HashSet<TagHelperDescriptor>(TagHelperDescriptorComparer.Default);

            foreach (var directiveDescriptor in context.DirectiveDescriptors)
            {
                var lookupInfo = GetLookupInfo(directiveDescriptor);

                if (directiveDescriptor.DirectiveType == TagHelperDirectiveType.RemoveTagHelper)
                {
                    resolvedDescriptors.RemoveWhere(descriptor => MatchesLookupInfo(descriptor, lookupInfo));
                }
                else if (directiveDescriptor.DirectiveType == TagHelperDirectiveType.AddTagHelper)
                {
                    var descriptors = ResolveDescriptorsInAssembly(lookupInfo.AssemblyName);

                    // Only use descriptors that match our lookup info
                    descriptors = descriptors.Where(descriptor => MatchesLookupInfo(descriptor, lookupInfo));

                    resolvedDescriptors.UnionWith(descriptors);
                }
            }

            return resolvedDescriptors;
        }

        /// <summary>
        /// Resolves all <see cref="TagHelperDescriptor"/>s for <see cref="ITagHelper"/>s from the given 
        /// <paramref name="assemblyName"/>.
        /// </summary>
        /// <param name="assemblyName">
        /// The name of the assembly to resolve <see cref="TagHelperDescriptor"/>s from.
        /// </param>
        /// <returns><see cref="TagHelperDescriptor"/>s for <see cref="ITagHelper"/>s from the given
        /// <paramref name="assemblyName"/>.</returns>
        // This is meant to be overridden by tooling to enable assembly level caching.
        protected virtual IEnumerable<TagHelperDescriptor> ResolveDescriptorsInAssembly(string assemblyName)
        {
            // Resolve valid tag helper types from the assembly.
            var tagHelperTypes = _typeResolver.Resolve(assemblyName);

            // Convert types to TagHelperDescriptors
            var descriptors = tagHelperTypes.SelectMany(TagHelperDescriptorFactory.CreateDescriptors);

            return descriptors;
        }

        private static bool MatchesLookupInfo(TagHelperDescriptor descriptor, LookupInfo lookupInfo)
        {
            if (!string.Equals(descriptor.AssemblyName, lookupInfo.AssemblyName, StringComparison.Ordinal))
            {
                return false;
            }

            return string.IsNullOrEmpty(lookupInfo.TypeName) ||
              string.Equals(descriptor.TypeName, lookupInfo.TypeName, StringComparison.Ordinal);
        }

        private static LookupInfo GetLookupInfo(TagHelperDirectiveDescriptor directiveDescriptor)
        {
            var lookupText = directiveDescriptor.LookupText;
            var lookupStrings = lookupText?.Split(new[] { ',' });

            // Ensure that we have valid lookupStrings to work with. Valid formats are:
            // "assemblyName"
            // "typeName, assemblyName"
            if (lookupStrings == null ||
                lookupStrings.Any(string.IsNullOrWhiteSpace) ||
                (lookupStrings.Length != 1 && lookupStrings.Length != 2))
            {
                throw new ArgumentException(
                    Resources.FormatTagHelperDescriptorResolver_InvalidTagHelperLookupText(lookupText),
                    nameof(lookupText));
            }

            // Grab the assembly name from the lookup text strings. Due to our supported lookupText formats it will 
            // always be the last element provided.
            var assemblyName = lookupStrings.Last().Trim();
            string typeName = null;

            // Check if the lookupText specifies a type to search for.
            if (lookupStrings.Length == 2)
            {
                // The user provided a type name. Retrieve it so we can prune our descriptors.
                typeName = lookupStrings[0].Trim();
            }

            return new LookupInfo
            {
                AssemblyName = assemblyName,
                TypeName = typeName
            };
        }

        private class LookupInfo
        {
            public string AssemblyName { get; set; }

            public string TypeName { get; set; }
        }
    }
}