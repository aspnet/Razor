﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Span = Microsoft.AspNetCore.Razor.Language.Legacy.Span;

namespace Microsoft.VisualStudio.Editor.Razor
{
    internal class RazorSyntaxTreePartialParser
    {
        private Span _lastChangeOwner;
        private bool _lastResultProvisional;

        public RazorSyntaxTreePartialParser(RazorSyntaxTree syntaxTree)
        {
            if (syntaxTree == null)
            {
                throw new ArgumentNullException(nameof(syntaxTree));
            }

            // We mutate the existing syntax tree so we need to clone the one passed in so our mutations don't
            // impact external state.
            SyntaxTreeRoot = (Block)syntaxTree.Root.Clone();
        }

        // Internal for testing
        internal Block SyntaxTreeRoot { get; }

        public PartialParseResultInternal Parse(SourceChange change)
        {
            var result = GetPartialParseResult(change);

            // Remember if this was provisionally accepted for next partial parse.
            _lastResultProvisional = (result & PartialParseResultInternal.Provisional) == PartialParseResultInternal.Provisional;

            return result;
        }

        private PartialParseResultInternal GetPartialParseResult(SourceChange change)
        {
            var result = PartialParseResultInternal.Rejected;

            // Try the last change owner
            if (_lastChangeOwner != null && _lastChangeOwner.EditHandler.OwnsChange(_lastChangeOwner, change))
            {
                var editResult = _lastChangeOwner.EditHandler.ApplyChange(_lastChangeOwner, change);
                result = editResult.Result;
                if ((editResult.Result & PartialParseResultInternal.Rejected) != PartialParseResultInternal.Rejected)
                {
                    _lastChangeOwner.ReplaceWith(editResult.EditedSpan);
                }

                return result;
            }

            // Locate the span responsible for this change
            _lastChangeOwner = SyntaxTreeRoot.LocateOwner(change);

            if (_lastResultProvisional)
            {
                // Last change owner couldn't accept this, so we must do a full reparse
                result = PartialParseResultInternal.Rejected;
            }
            else if (_lastChangeOwner != null)
            {
                var editResult = _lastChangeOwner.EditHandler.ApplyChange(_lastChangeOwner, change);
                result = editResult.Result;
                if ((editResult.Result & PartialParseResultInternal.Rejected) != PartialParseResultInternal.Rejected)
                {
                    _lastChangeOwner.ReplaceWith(editResult.EditedSpan);
                }
            }

            return result;
        }
    }
}
