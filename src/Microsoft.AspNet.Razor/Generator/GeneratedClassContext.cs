// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNet.Razor.Generator
{
    public class GeneratedClassContext
    {
        public static readonly string DefaultWriteMethodName = "Write";
        public static readonly string DefaultWriteLiteralMethodName = "WriteLiteral";
        public static readonly string DefaultExecuteMethodName = "ExecuteAsync";
        public static readonly string DefaultLayoutPropertyName = "Layout";
        public static readonly string DefaultWriteAttributeMethodName = "WriteAttribute";
        public static readonly string DefaultWriteAttributeToMethodName = "WriteAttributeTo";

        public static readonly GeneratedClassContext Default = 
            new GeneratedClassContext(GeneratedTagHelperContext.Default,
                                      DefaultExecuteMethodName,
                                      DefaultWriteMethodName,
                                      DefaultWriteLiteralMethodName);

        public GeneratedClassContext(GeneratedTagHelperContext generatedTagHelperContext,
                                     string executeMethodName,
                                     string writeMethodName,
                                     string writeLiteralMethodName)
        {
            if (generatedTagHelperContext == null)
            {
                throw new ArgumentNullException(nameof(generatedTagHelperContext));
            }
            if (string.IsNullOrEmpty(executeMethodName))
            {
                throw new ArgumentException(
                    CommonResources.Argument_Cannot_Be_Null_Or_Empty, 
                    nameof(executeMethodName));
            }
            if (string.IsNullOrEmpty(writeMethodName))
            {
                throw new ArgumentException(
                    CommonResources.Argument_Cannot_Be_Null_Or_Empty, 
                    nameof(writeMethodName));
            }
            if (string.IsNullOrEmpty(writeLiteralMethodName))
            {
                throw new ArgumentException(
                    CommonResources.Argument_Cannot_Be_Null_Or_Empty, 
                    nameof(writeLiteralMethodName));
            }

            GeneratedTagHelperContext = generatedTagHelperContext;

            WriteMethodName = writeMethodName;
            WriteLiteralMethodName = writeLiteralMethodName;
            ExecuteMethodName = executeMethodName;

            WriteToMethodName = null;
            WriteLiteralToMethodName = null;
            TemplateTypeName = null;
            DefineSectionMethodName = null;

            LayoutPropertyName = DefaultLayoutPropertyName;
            WriteAttributeMethodName = DefaultWriteAttributeMethodName;
            WriteAttributeToMethodName = DefaultWriteAttributeToMethodName;
        }

        public GeneratedClassContext(GeneratedTagHelperContext generatedTagHelperContext,
                                     string executeMethodName,
                                     string writeMethodName,
                                     string writeLiteralMethodName,
                                     string writeToMethodName,
                                     string writeLiteralToMethodName,
                                     string templateTypeName)
            : this(generatedTagHelperContext,
                   executeMethodName, 
                   writeMethodName, 
                   writeLiteralMethodName)
        {
            WriteToMethodName = writeToMethodName;
            WriteLiteralToMethodName = writeLiteralToMethodName;
            TemplateTypeName = templateTypeName;
        }

        public GeneratedClassContext(GeneratedTagHelperContext generatedTagHelperContext,
                                     string executeMethodName,
                                     string writeMethodName,
                                     string writeLiteralMethodName,
                                     string writeToMethodName,
                                     string writeLiteralToMethodName,
                                     string templateTypeName,
                                     string defineSectionMethodName)
            : this(generatedTagHelperContext,
                   executeMethodName,
                   writeMethodName,
                   writeLiteralMethodName,
                   writeToMethodName,
                   writeLiteralToMethodName,
                   templateTypeName)
        {
            DefineSectionMethodName = defineSectionMethodName;
        }

        public GeneratedClassContext(GeneratedTagHelperContext generatedTagHelperContext,
                                     string executeMethodName,
                                     string writeMethodName,
                                     string writeLiteralMethodName,
                                     string writeToMethodName,
                                     string writeLiteralToMethodName,
                                     string templateTypeName,
                                     string defineSectionMethodName,
                                     string beginContextMethodName,
                                     string endContextMethodName)
            : this(generatedTagHelperContext,
                   executeMethodName,
                   writeMethodName,
                   writeLiteralMethodName,
                   writeToMethodName,
                   writeLiteralToMethodName,
                   templateTypeName,
                   defineSectionMethodName)
        {
            BeginContextMethodName = beginContextMethodName;
            EndContextMethodName = endContextMethodName;
        }

        public GeneratedClassContext(GeneratedTagHelperContext generatedTagHelperContext,
                                     string executeMethodName,
                                     string writeMethodName,
                                     string writeLiteralMethodName,
                                     string writeToMethodName,
                                     string writeLiteralToMethodName,
                                     string templateTypeName,
                                     string defineSectionMethodName,
                                     string beginContextMethodName,
                                     string endContextMethodName,
                                     string activateAttributeName)
            : this(generatedTagHelperContext,
                   executeMethodName,
                   writeMethodName,
                   writeLiteralMethodName,
                   writeToMethodName,
                   writeLiteralToMethodName,
                   templateTypeName,
                   defineSectionMethodName,
                   beginContextMethodName,
                   endContextMethodName)
        {
            ActivateAttributeName = activateAttributeName;
        }

        // Required Items
        public string WriteMethodName { get; private set; }
        public string WriteLiteralMethodName { get; private set; }
        public string WriteToMethodName { get; private set; }
        public string WriteLiteralToMethodName { get; private set; }
        public string ExecuteMethodName { get; private set; }
        public GeneratedTagHelperContext GeneratedTagHelperContext { get; private set; }


        // Optional Items
        public string ActivateAttributeName { get; set; }
        public string BeginContextMethodName { get; set; }
        public string EndContextMethodName { get; set; }
        public string LayoutPropertyName { get; set; }
        public string DefineSectionMethodName { get; set; }
        public string TemplateTypeName { get; set; }
        public string WriteAttributeMethodName { get; set; }
        public string WriteAttributeToMethodName { get; set; }


        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Property is not a URL property")]
        public string ResolveUrlMethodName { get; set; }

        public bool AllowSections
        {
            get { return !string.IsNullOrEmpty(DefineSectionMethodName); }
        }

        public bool AllowTemplates
        {
            get { return !string.IsNullOrEmpty(TemplateTypeName); }
        }

        public bool SupportsInstrumentation
        {
            get { return !string.IsNullOrEmpty(BeginContextMethodName) && !string.IsNullOrEmpty(EndContextMethodName); }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GeneratedClassContext))
            {
                return false;
            }
            var other = (GeneratedClassContext)obj;
            return string.Equals(DefineSectionMethodName, other.DefineSectionMethodName, StringComparison.Ordinal) &&
                   string.Equals(WriteMethodName, other.WriteMethodName, StringComparison.Ordinal) &&
                   string.Equals(WriteLiteralMethodName, other.WriteLiteralMethodName, StringComparison.Ordinal) &&
                   string.Equals(WriteToMethodName, other.WriteToMethodName, StringComparison.Ordinal) &&
                   string.Equals(WriteLiteralToMethodName, other.WriteLiteralToMethodName, StringComparison.Ordinal) &&
                   string.Equals(ExecuteMethodName, other.ExecuteMethodName, StringComparison.Ordinal) &&
                   string.Equals(TemplateTypeName, other.TemplateTypeName, StringComparison.Ordinal) &&
                   string.Equals(BeginContextMethodName, other.BeginContextMethodName, StringComparison.Ordinal) &&
                   string.Equals(EndContextMethodName, other.EndContextMethodName, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            // TODO: Use HashCodeCombiner
            return DefineSectionMethodName.GetHashCode() ^
                   WriteMethodName.GetHashCode() ^
                   WriteLiteralMethodName.GetHashCode() ^
                   WriteToMethodName.GetHashCode() ^
                   WriteLiteralToMethodName.GetHashCode() ^
                   ExecuteMethodName.GetHashCode() ^
                   TemplateTypeName.GetHashCode() ^
                   BeginContextMethodName.GetHashCode() ^
                   EndContextMethodName.GetHashCode();
        }

        public static bool operator ==(GeneratedClassContext left, GeneratedClassContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GeneratedClassContext left, GeneratedClassContext right)
        {
            return !left.Equals(right);
        }
    }
}
