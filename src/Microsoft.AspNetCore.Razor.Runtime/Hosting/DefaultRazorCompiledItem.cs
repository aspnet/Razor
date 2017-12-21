// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Hosting
{
    internal class DefaultRazorCompiledItem : RazorCompiledItem
    {
        public DefaultRazorCompiledItem(Type type, string kind, string identifier)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (kind == null)
            {
                throw new ArgumentNullException(nameof(kind));
            }

            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            Type = type;
            Kind = kind;
            Identifier = identifier;
        }

        public override string Identifier { get; }

        public override string Kind { get; }

        public override Type Type { get; }
    }
}
