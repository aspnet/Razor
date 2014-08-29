// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Razor.TagHelpers.Internal
{
    /// <summary>
    /// A tag helper provider context used to manage tag helpers found in the system.
    /// </summary>
    public class TagHelperRegistrar
    {
        private IDictionary<string, List<TagHelperDescriptor>> _registrations;

        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperRegistrar"/> object.
        /// </summary>
        public TagHelperRegistrar()
        {
            _registrations = new Dictionary<string, List<TagHelperDescriptor>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the current list of <see cref="TagHelperDescriptor"/> registrations.
        /// </summary>
        public IDictionary<string, List<TagHelperDescriptor>> Registrations
        {
            get
            {
                return _registrations;
            }
        }

        /// <summary>
        /// Registers a descriptor that can be retrieved via the <see cref="GetTagHelpers(string)"/> method.
        /// </summary>
        /// <param name="descriptor">The descriptor that will be maintained. Can be retrieved by calling
        /// <see cref="TagHelperProvider.GetTagHelpers(string)"/> with the given <see cref="TagHelperDescriptor.TagName"/>
        /// value.</param>
        public void Register(TagHelperDescriptor descriptor)
        {
            // If the tag helper has not been registered before, create a new list to manage tag helpers
            // for the given tag name.
            if (!_registrations.ContainsKey(descriptor.TagName))
            {
                _registrations[descriptor.TagName] = new List<TagHelperDescriptor>();
            }

            // As long as there is not an identical tag descriptor already registered add the descriptor.
            if (!_registrations[descriptor.TagName].Any(thd => thd.Equals(descriptor)))
            {
                _registrations[descriptor.TagName].Add(descriptor);
            }
        }

        /// <summary>
        /// Unregisters a specific <see cref="TagHelperDescriptor"/> so it can no longer be retrieved via
        /// the <see cref="GetTagHelpers(string)"/> method.
        /// </summary>
        /// <param name="descriptor">The descriptor to be unregistered.</param>
        public void Unregister(TagHelperDescriptor descriptor)
        {
            if (_registrations.ContainsKey(descriptor.TagName))
            {
                _registrations[descriptor.TagName].Remove(descriptor);
            }
        }
    }
}