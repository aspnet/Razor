﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.Razor.Editor;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;
using Xunit;

namespace Microsoft.AspNet.Razor.Parser.SyntaxTree
{
    public class AutoCompleteEditHandlerTest
    {
        public static TheoryData<AutoCompleteEditHandler, AutoCompleteEditHandler> MatchingTestDataSet
        {
            get
            {
                return new TheoryData<AutoCompleteEditHandler, AutoCompleteEditHandler>
                {
                    {
                        new AutoCompleteEditHandler(tokenizer: null)
                        {
                            AcceptedCharacters = AcceptedCharacters.AllWhiteSpace,
                            AutoCompleteAtEndOfSpan = false,
                            AutoCompleteString = "one string",
                            EditorHints = EditorHints.LayoutPage,
                        },
                        new AutoCompleteEditHandler(tokenizer: null)
                        {
                            AcceptedCharacters = AcceptedCharacters.AllWhiteSpace,
                            AutoCompleteAtEndOfSpan = false,
                            AutoCompleteString = "one string",
                            EditorHints = EditorHints.LayoutPage,
                        }
                    },
                    {
                        // Tokenizer not involved in equality check or hash code calculation.
                        new AutoCompleteEditHandler(tokenizer: null)
                        {
                            AcceptedCharacters = AcceptedCharacters.Any,
                            AutoCompleteAtEndOfSpan = true,
                            AutoCompleteString = "two string",
                            EditorHints = EditorHints.None,
                        },
                        new AutoCompleteEditHandler(tokenizer: _ => Enumerable.Empty<ISymbol>())
                        {
                            AcceptedCharacters = AcceptedCharacters.Any,
                            AutoCompleteAtEndOfSpan = true,
                            AutoCompleteString = "two string",
                            EditorHints = EditorHints.None,
                        }
                    },
                };
            }
        }

        public static TheoryData<AutoCompleteEditHandler, object> NonMatchingTestDataSet
        {
            get
            {
                return new TheoryData<AutoCompleteEditHandler, object>
                {
                    {
                        new AutoCompleteEditHandler(tokenizer: _ => Enumerable.Empty<ISymbol>())
                        {
                            AcceptedCharacters = AcceptedCharacters.AllWhiteSpace,
                            AutoCompleteAtEndOfSpan = false,
                            AutoCompleteString = "three string",
                            EditorHints = EditorHints.VirtualPath,
                        },
                        null
                    },
                    {
                        new AutoCompleteEditHandler(tokenizer: _ => Enumerable.Empty<ISymbol>())
                        {
                            AcceptedCharacters = AcceptedCharacters.AllWhiteSpace,
                            AutoCompleteAtEndOfSpan = false,
                            AutoCompleteString = "three string",
                            EditorHints = EditorHints.VirtualPath,
                        },
                        new object()
                    },
                    {
                        new AutoCompleteEditHandler(tokenizer: _ => Enumerable.Empty<ISymbol>())
                        {
                            AcceptedCharacters = AcceptedCharacters.AllWhiteSpace,
                            AutoCompleteAtEndOfSpan = false,
                            AutoCompleteString = "three string",
                            EditorHints = EditorHints.VirtualPath,
                        },
                        new SpanEditHandler(tokenizer: _ => Enumerable.Empty<ISymbol>())
                    },
                    {
                        // Different AcceptedCharacters.
                        new AutoCompleteEditHandler(tokenizer: _ => Enumerable.Empty<ISymbol>())
                        {
                            AcceptedCharacters = AcceptedCharacters.AllWhiteSpace,
                            AutoCompleteAtEndOfSpan = false,
                            AutoCompleteString = "three string",
                            EditorHints = EditorHints.VirtualPath,
                        },
                        new AutoCompleteEditHandler(tokenizer: _ => Enumerable.Empty<ISymbol>())
                        {
                            AcceptedCharacters = AcceptedCharacters.AnyExceptNewline,
                            AutoCompleteAtEndOfSpan = false,
                            AutoCompleteString = "three string",
                            EditorHints = EditorHints.VirtualPath,
                        }
                    },
                    {
                        // Different AutoCompleteAtEndOfSpan.
                        new AutoCompleteEditHandler(tokenizer: null)
                        {
                            AcceptedCharacters = AcceptedCharacters.AnyExceptNewline,
                            AutoCompleteAtEndOfSpan = false,
                            AutoCompleteString = "four string",
                            EditorHints = EditorHints.LayoutPage,
                        },
                        new AutoCompleteEditHandler(tokenizer: null)
                        {
                            AcceptedCharacters = AcceptedCharacters.AnyExceptNewline,
                            AutoCompleteAtEndOfSpan = true,
                            AutoCompleteString = "four string",
                            EditorHints = EditorHints.LayoutPage,
                        }
                    },
                    {
                        // Different AutoCompleteString.
                        new AutoCompleteEditHandler(tokenizer: _ => Enumerable.Empty<ISymbol>())
                        {
                            AcceptedCharacters = AcceptedCharacters.NewLine,
                            AutoCompleteAtEndOfSpan = true,
                            AutoCompleteString = "some string",
                            EditorHints = EditorHints.None,
                        },
                        new AutoCompleteEditHandler(tokenizer: _ => Enumerable.Empty<ISymbol>())
                        {
                            AcceptedCharacters = AcceptedCharacters.NewLine,
                            AutoCompleteAtEndOfSpan = true,
                            AutoCompleteString = "different string",
                            EditorHints = EditorHints.None,
                        }
                    },
                    {
                        // Different AutoCompleteString (case sensitive).
                        new AutoCompleteEditHandler(tokenizer: null)
                        {
                            AcceptedCharacters = AcceptedCharacters.None,
                            AutoCompleteAtEndOfSpan = false,
                            AutoCompleteString = "some string",
                            EditorHints = EditorHints.VirtualPath,
                        },
                        new AutoCompleteEditHandler(tokenizer: null)
                        {
                            AcceptedCharacters = AcceptedCharacters.None,
                            AutoCompleteAtEndOfSpan = false,
                            AutoCompleteString = "Some String",
                            EditorHints = EditorHints.VirtualPath,
                        }
                    },
                    {
                        // Different EditorHints.
                        new AutoCompleteEditHandler(tokenizer: _ => Enumerable.Empty<ISymbol>())
                        {
                            AcceptedCharacters = AcceptedCharacters.NonWhiteSpace,
                            AutoCompleteAtEndOfSpan = true,
                            AutoCompleteString = "five string",
                            EditorHints = EditorHints.LayoutPage,
                        },
                        new AutoCompleteEditHandler(tokenizer: _ => Enumerable.Empty<ISymbol>())
                        {
                            AcceptedCharacters = AcceptedCharacters.NonWhiteSpace,
                            AutoCompleteAtEndOfSpan = true,
                            AutoCompleteString = "five string",
                            EditorHints = EditorHints.None,
                        }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(MatchingTestDataSet))]
        public void Equals_True_WhenExpected(AutoCompleteEditHandler leftObject, AutoCompleteEditHandler rightObject)
        {
            // Arrange & Act
            var result = leftObject.Equals(rightObject);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(NonMatchingTestDataSet))]
        public void Equals_False_WhenExpected(AutoCompleteEditHandler leftObject, object rightObject)
        {
            // Arrange & Act
            var result = leftObject.Equals(rightObject);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(MatchingTestDataSet))]
        public void GetHashCode_ReturnsSameValue_WhenEqual(
            AutoCompleteEditHandler leftObject,
            AutoCompleteEditHandler rightObject)
        {
            // Arrange & Act
            var leftResult = leftObject.GetHashCode();
            var rightResult = rightObject.GetHashCode();

            // Assert
            Assert.Equal(leftResult, rightResult);
        }
    }
}
