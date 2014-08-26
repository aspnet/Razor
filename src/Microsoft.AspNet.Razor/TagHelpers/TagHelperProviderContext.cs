using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Razor.TagHelpers
{
    internal class TagHelperProviderContext : ITagHelperProviderContext
    {
        private const string CatchAllDescriptorTarget = "*";

        public static readonly TagHelperProviderContext Default = new TagHelperProviderContext();

        private IDictionary<string, List<TagHelperDescriptor>> _registrations;
        private IEnumerable<TagHelperDescriptor> _catchAllCache;

        public TagHelperProviderContext()
        {
            _registrations = new Dictionary<string, List<TagHelperDescriptor>>(StringComparer.OrdinalIgnoreCase);
            _registrations[CatchAllDescriptorTarget] = new List<TagHelperDescriptor>();

            UpdateCatchAllCache();
        }

        public void Register(TagHelperDescriptor descriptor)
        {
            // If the tag helper has not been registered before create a new list to manage tag helpers
            // for the given tag name.
            if (!_registrations.ContainsKey(descriptor.TagName))
            {
                _registrations[descriptor.TagName] = new List<TagHelperDescriptor>();
            }

            // As long as there is not an identical tag descriptor already registered.
            if (!_registrations[descriptor.TagName].Any(thd => thd.Equals(descriptor)))
            {
                _registrations[descriptor.TagName].Add(descriptor);
            }

            // Updates the catch all cache.
            UpdateCatchAllCache();
        }

        public IEnumerable<TagHelperDescriptor> GetTagHelpers(string tagName)
        {
            // If we have a tag name associated with the requested name return the descriptors +
            // all of the catch all descriptors.
            if (_registrations.ContainsKey(tagName))
            {
                return _registrations[tagName].Union(_catchAllCache);
            }

            // We couldn't find a tag name associated with the requested tag name, return all
            // of the "catch all" tag descriptors.
            return _catchAllCache;
        }

        public void Unregister(TagHelperDescriptor descriptor)
        {
            if (_registrations.ContainsKey(descriptor.TagName))
            {
                _registrations[descriptor.TagName] = _registrations[descriptor.TagName].Where(desc => !desc.Equals(descriptor)).ToList();

                UpdateCatchAllCache();
            }
        }

        private void UpdateCatchAllCache()
        {
            _catchAllCache = _registrations[CatchAllDescriptorTarget];
        }
    }
}