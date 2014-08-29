// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Razor.TagHelpers.Internal
{
    /// <summary>
    /// Retrieves <see cref="TagHelperDescriptor"/>'s from the <see cref="TagHelperRegistrar"/>
    /// given a <see cref="string"/> tag name.
    /// </summary>
    public class TagHelperProvider
    {
        private const string CatchAllDescriptorTarget = "*";

        private TagHelperRegistrar _registrar;

        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperProvider"/>.
        /// </summary>
        /// <param name="registrar">The registration system to retrieve <see cref="TagHelperDescriptor"/>'s
        /// from</param>
	    public TagHelperProvider(TagHelperRegistrar registrar)
        {
            _registrar = registrar;
        }

        /// <summary>
        /// Gets all tag helpers that match the given <paramref name="tagName"/>.
        /// </summary>
        /// <param name="tagName">The name of the HTML tag to match. Providing a '*' tag name 
        /// retrieves catch-all (descriptors that target every tag) 
        /// <see cref="TagHelperDescriptor"/>'s.</param>
        /// <returns>Tag helpers that apply to the given <paramref name="tagName"/>.</returns>
        public IEnumerable<TagHelperDescriptor> GetTagHelpers(string tagName)
        {
            var registrations = _registrar.Registrations;
            IEnumerable<TagHelperDescriptor> descriptors;

            if(registrations.ContainsKey(CatchAllDescriptorTarget))
            {
                descriptors = registrations[CatchAllDescriptorTarget];
            }
            else
            {
                descriptors = Enumerable.Empty<TagHelperDescriptor>();
            }

            // If we have a tag name associated with the requested name return the descriptors +
            // all of the catch all descriptors.
            if (registrations.ContainsKey(tagName))
            {
                return registrations[tagName].Union(descriptors);
            }

            // We couldn't find a tag name associated with the requested tag name, return all
            // of the "catch all" tag descriptors (there may not be any).
            return descriptors;
        }
    }
}