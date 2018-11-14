﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;

namespace Microsoft.VisualStudio.Editor.Razor
{
    internal class RazorDirectiveCompletionSource : IAsyncCompletionSource
    {
        // Internal for testing
        internal static readonly object DescriptionKey = new object();
        // Hardcoding the Guid here to avoid a reference to Microsoft.VisualStudio.ImageCatalog.dll
        // that is not present in Visual Studio for Mac
        internal static readonly Guid ImageCatalogGuid = new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}");
        internal static readonly ImageElement DirectiveImageGlyph = new ImageElement(
            new ImageId(ImageCatalogGuid, 3233), // KnownImageIds.Type = 3233
            "Razor Directive.");
        internal static readonly ImmutableArray<CompletionFilter> DirectiveCompletionFilters = new[] {
            new CompletionFilter("Razor Directive", "r", DirectiveImageGlyph)
        }.ToImmutableArray();

        // Internal for testing
        internal readonly VisualStudioRazorParser _parser;
        private readonly RazorCompletionFactsService _completionFactsService;
        private readonly ForegroundDispatcher _foregroundDispatcher;

        public RazorDirectiveCompletionSource(
            ForegroundDispatcher foregroundDispatcher,
            VisualStudioRazorParser parser,
            RazorCompletionFactsService completionFactsService)
        {
            if (foregroundDispatcher == null)
            {
                throw new ArgumentNullException(nameof(foregroundDispatcher));
            }

            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (completionFactsService == null)
            {
                throw new ArgumentNullException(nameof(completionFactsService));
            }

            _foregroundDispatcher = foregroundDispatcher;
            _parser = parser;
            _completionFactsService = completionFactsService;
        }

        public Task<CompletionContext> GetCompletionContextAsync(
            IAsyncCompletionSession session,
            CompletionTrigger trigger,
            SnapshotPoint triggerLocation,
            SnapshotSpan applicableSpan,
            CancellationToken token)
        {
            _foregroundDispatcher.AssertBackgroundThread();

            var syntaxTree = _parser.CodeDocument?.GetSyntaxTree();
            var location = new SourceSpan(applicableSpan.Start.Position, applicableSpan.Length);
            var razorCompletionItems = _completionFactsService.GetCompletionItems(syntaxTree, location);

            var completionItems = new List<CompletionItem>();
            foreach (var razorCompletionItem in razorCompletionItems)
            {
                if (razorCompletionItem.Kind != RazorCompletionItemKind.Directive)
                {
                    // Don't support any other types of completion kinds other than directives.
                    continue;
                }

                var completionItem = new CompletionItem(
                    displayText: razorCompletionItem.DisplayText,
                    filterText: razorCompletionItem.DisplayText,
                    insertText: razorCompletionItem.InsertText,
                    source: this,
                    icon: DirectiveImageGlyph,
                    filters: DirectiveCompletionFilters,
                    suffix: string.Empty,
                    sortText: razorCompletionItem.DisplayText,
                    attributeIcons: ImmutableArray<ImageElement>.Empty);
                completionItem.Properties.AddProperty(DescriptionKey, razorCompletionItem.Description);
                completionItems.Add(completionItem);
            }
            var context = new CompletionContext(completionItems.ToImmutableArray());
            return Task.FromResult(context);
        }

        public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {
            if (!item.Properties.TryGetProperty<string>(DescriptionKey, out var directiveDescription))
            {
                directiveDescription = string.Empty;
            }

            return Task.FromResult<object>(directiveDescription);
        }

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            // The applicable span for completion is the piece of text a completion is for. For example:
            //      @Date|Time.Now
            // If you trigger completion at the | then the applicable span is the region of 'DateTime'; however, Razor
            // doesn't know this information so we rely on Roslyn to define what the applicable span for a completion is.
            return CompletionStartData.ParticipatesInCompletionIfAny;
        }
    }
}
