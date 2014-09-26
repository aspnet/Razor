// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <inheritdoc />
    public class TagHelperTypeResolver : ITagHelperTypeResolver
    {
        private static readonly TypeInfo TagHelperTypeInfo = typeof(TagHelper).GetTypeInfo();

        private ILibraryManager _libraryManager;

        // Internal for testing
        internal TagHelperTypeResolver()
        {
        }

        /// <summary>
        /// Instantiates a new instance of <see cref="TagHelperTypeResolver"/>.
        /// </summary>
        /// <param name="libraryManager">The <see cref="ILibraryManager"/> used to locate assemblies.</param>
	    public TagHelperTypeResolver([NotNull] ILibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        /// <inheritdoc />
        public virtual IEnumerable<Type> Resolve(string lookupText)
        {
            var data = lookupText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            // Ensure that we have enough data to work with. Valid formats are:
            // "assemblyName"
            // "assemblyName, #.#.#.#"
            // "assemblyName, #.#.#.#, specificType"
            if (data.Length == 0 || data.Length > 3)
            {
                throw new InvalidOperationException(
                    Resources.FormatTagHelperTypeResolver_InvalidTagHelperLookupText(lookupText));
            }

            var assemblyRef = GetAssemblyRef(data);
            var types = GetAssemblyTypeInfos(assemblyRef);
            var typeLookup = GetTypeLookup(data);

            // Check if the lookupText specifies a type to add.
            if (typeLookup != null)
            {
                // Iterate through the assembly and find the specified type..
                types = types.Where(type => type.Namespace + "." + type.Name == typeLookup && IsTagHelper(type));
            }
            else
            {
                // The assembly was the only piece specified, pull all tag helpers from assembly.
                types = types.Where(IsTagHelper);
            }

            // Convert back from TypeInfo[] to Type[].
            return types.Select(type => type.AsType());
        }

        // Internal for testing
        internal virtual string GetAssemblyName(string lookupName)
        {
            return _libraryManager.GetLibraryInformation(lookupName).Name;
        }

        // Internal for testing
        internal virtual IEnumerable<TypeInfo> GetAssemblyTypeInfos(AssemblyName assemblyRef)
        {
            var assembly = Assembly.Load(assemblyRef);
            return assembly.DefinedTypes;
        }

        private AssemblyName GetAssemblyRef(string[] lookupTextData)
        {
            // Assembly name must always be provided
            var assemblyName = GetAssemblyName(lookupTextData[0].Trim());
            var assemblyRef = new AssemblyName(assemblyName);
            var assemblyVersionSpecified = lookupTextData.Length > 1;

            if (assemblyVersionSpecified)
            {
                Version assemblyVersion;
                var strAssemblyVersion = lookupTextData[1].Trim();

                // Try and parse the assembly version, if unsuccessful throw.
                if (!Version.TryParse(strAssemblyVersion, out assemblyVersion))
                {
                    throw new InvalidOperationException(
                        Resources.FormatTagHelperTypeResolver_InvalidTagHelperLookupTextAssemblyVersion(
                            strAssemblyVersion));
                }

                assemblyRef.Version = assemblyVersion;
            }

            return assemblyRef;
        }

        private string GetTypeLookup(string[] lookupTextData)
        {
            var typeSpecified = lookupTextData.Length == 3;

            if (typeSpecified)
            {
                return lookupTextData[2].Trim();
            }

            return null;
        }

        private static bool IsTagHelper(TypeInfo typeInfo)
        {
            return !typeInfo.IsAbstract &&
                   !typeInfo.IsGenericType &&
                   typeInfo.IsPublic &&
                   !typeInfo.IsNested &&
                   TagHelperTypeInfo.IsAssignableFrom(typeInfo);
        }
    }
}