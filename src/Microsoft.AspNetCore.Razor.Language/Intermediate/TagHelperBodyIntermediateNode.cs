﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Language.Intermediate
{
    public sealed class TagHelperBodyIntermediateNode : IntermediateNode
    {
        private RazorDiagnosticCollection _diagnostics;

        public override ItemCollection Annotations => ReadOnlyItemCollection.Empty;

        public override RazorDiagnosticCollection Diagnostics
        {
            get
            {
                if (_diagnostics == null)
                {
                    _diagnostics = new DefaultRazorDiagnosticCollection();
                }

                return _diagnostics;
            }
        }

        public override IntermediateNodeCollection Children { get; } = new DefaultIntermediateNodeCollection();

        public ICollection<TagHelperDescriptor> TagHelpers { get; } = new List<TagHelperDescriptor>();

        public override SourceSpan? Source { get; set; }

        public override bool HasDiagnostics => _diagnostics != null && _diagnostics.Count > 0;

        public override void Accept(IntermediateNodeVisitor visitor)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            visitor.VisitTagHelperBody(this);
        }
    }
}
