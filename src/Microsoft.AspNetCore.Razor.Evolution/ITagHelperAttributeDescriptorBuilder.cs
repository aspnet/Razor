// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public class ITagHelperAttributeDescriptorBuilder
    {
        public static readonly string DescriptorKind = "ITagHelper";
        public static readonly string ITagHelperPropertyNameKey = "ITagHelper.PropertyName";

        private static readonly IReadOnlyDictionary<string, string> PrimitiveDisplayTypeNameLookups = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [typeof(byte).FullName] = "byte",
            [typeof(sbyte).FullName] = "sbyte",
            [typeof(int).FullName] = "int",
            [typeof(uint).FullName] = "uint",
            [typeof(short).FullName] = "short",
            [typeof(ushort).FullName] = "ushort",
            [typeof(long).FullName] = "long",
            [typeof(ulong).FullName] = "ulong",
            [typeof(float).FullName] = "float",
            [typeof(double).FullName] = "double",
            [typeof(char).FullName] = "char",
            [typeof(bool).FullName] = "bool",
            [typeof(object).FullName] = "object",
            [typeof(string).FullName] = "string",
            [typeof(decimal).FullName] = "decimal",
        };

        private bool _isEnum;
        private string _dictionaryValueTypeName;
        private string _name;
        private string _propertyName;
        private string _typeName;
        private string _documentation;
        private List<RazorDiagnostic> _diagnostics;
        private readonly string _containingTypeName;
        private readonly Dictionary<string, string> _propertyBag;

        private ITagHelperAttributeDescriptorBuilder(string containingTypeName)
        {
            _containingTypeName = containingTypeName;
            _propertyBag = new Dictionary<string, string>();
        }

        public static ITagHelperAttributeDescriptorBuilder Create(string containingTypeName)
        {
            return new ITagHelperAttributeDescriptorBuilder(containingTypeName);
        }

        public ITagHelperAttributeDescriptorBuilder Name(string name)
        {
            _name = name;

            return this;
        }

        public ITagHelperAttributeDescriptorBuilder PropertyName(string propertyName)
        {
            _propertyName = propertyName;

            return this;
        }

        public ITagHelperAttributeDescriptorBuilder TypeName(string typeName)
        {
            _typeName = typeName;

            return this;
        }

        public ITagHelperAttributeDescriptorBuilder AsEnum()
        {
            _isEnum = true;

            return this;
        }

        public ITagHelperAttributeDescriptorBuilder DictionaryValueTypeName(string dictionaryValueTypeName)
        {
            _dictionaryValueTypeName = dictionaryValueTypeName;

            return this;
        }

        public ITagHelperAttributeDescriptorBuilder Documentation(string documentation)
        {
            _documentation = documentation;

            return this;
        }

        public ITagHelperAttributeDescriptorBuilder AddMetadata(string key, string value)
        {
            _propertyBag[key] = value;

            return this;
        }

        public TagHelperAttributeDescriptor Build()
        {
            if (!PrimitiveDisplayTypeNameLookups.TryGetValue(_typeName, out var simpleName))
            {
                simpleName = _typeName;
            }

            var displayName = $"{simpleName} {_containingTypeName}.{_propertyName}";
            var descriptor = new ITagHelperAttributeDescriptor(
                _isEnum,
                _name,
                _propertyName,
                _typeName,
                _dictionaryValueTypeName,
                _documentation,
                displayName,
                _propertyBag,
                _diagnostics ?? Enumerable.Empty<RazorDiagnostic>());

            return descriptor;
        }

        private class ITagHelperAttributeDescriptor : TagHelperAttributeDescriptor
        {
            public ITagHelperAttributeDescriptor(
                bool isEnum,
                string name,
                string propertyName,
                string typeName,
                string dictionaryValueTypeName,
                string documentation,
                string displayName,
                Dictionary<string, string> propertyBag,
                IEnumerable<RazorDiagnostic> diagnostics) : base(DescriptorKind)
            {
                IsEnum = isEnum;
                IsKeyValueStringProperty = dictionaryValueTypeName == typeof(string).FullName || dictionaryValueTypeName == "string";
                IsStringProperty = typeName == typeof(string).FullName || typeName == "string";
                Name = name;
                TypeName = typeName;
                KeyValueTypeName = dictionaryValueTypeName;
                Documentation = documentation;
                DisplayName = displayName;
                Diagnostics = diagnostics;

                propertyBag[ITagHelperPropertyNameKey] = propertyName;
                PropertyBag = propertyBag;
            }
        }
    }
}
