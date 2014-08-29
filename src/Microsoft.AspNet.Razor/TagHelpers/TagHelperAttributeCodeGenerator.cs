// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// Generates code for a tag helper property initialization.
    /// </summary>
    public class TagHelperAttributeCodeGenerator
    {
        /// <summary>
        /// Instantiates an instance of the <see cref="TagHelperAttributeCodeGenerator"/> class.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="System.Reflection.PropertyInfo"/> for the tag helper property
        /// (HTML attribute) to generate code for.</param>
        public TagHelperAttributeCodeGenerator(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        /// <summary>
        /// The <see cref="System.Reflection.PropertyInfo"/> for the tag helper property (HTML attribute) to generate
        /// code for.
        /// </summary>
        protected PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Called during Razor's code generation process to generate code that instantiates the value of the tag helper's
        /// property. Last value that is written should not be or end in a semicolon.
        /// </summary>
        /// <remarks>
        /// Writes the string: "new MyPropertyType(...)" to the output where the "..." is rendered by calling the
        /// <paramref name="renderAttributeValue"/> <see cref="Action"/>.
        /// </remarks>
        /// <param name="writer">The <see cref="CSharpCodeWriter"/> that writes directly to the generated Razor 
        /// class.</param>
        /// <param name="context">An <see cref="CodeGeneratorContext"/> instance that contains information about 
        /// the current code generation process.</param>
        /// <param name="renderAttributeValue"><see cref="Action"/> that renders the raw value of the HTML attribute. Will be null 
        /// if there is no attribute value. Example: If the HTML attribute value is '3' and we want to new up an object that takes 
        /// the attribute value we'd write "new MyObjectType(" then call into <paramref name="renderAttributeValue"/> and finally 
        /// write the ending ")".</param>
        public virtual void GenerateCode(CSharpCodeWriter writer, 
                                         CodeGeneratorContext context, 
                                         Action<CSharpCodeWriter> renderAttributeValue)
        {
            var propertyType = PropertyInfo.PropertyType.GetTypeInfo();

            writer.Write("new ")
                  .Write(GetName(propertyType))
                  .Write("(");

            // Verify that there is an attribute value
            if (renderAttributeValue != null)
            {
                // If the build type is null that means that the type is not a generic expression, therefore just use what was given to us.
                var attributeValueType = GetBuildType(propertyType) ?? propertyType.DeclaringType;
                RenderValue(writer, renderAttributeValue, attributeValueType);

                writer.WriteEndMethodInvocation(endLine: false);

                // Since there's an attribute value, add { IsSet = true }
                GenerateIsSetProperty(writer, isSet: true);
            }
            else
            {
                writer.WriteEndMethodInvocation(endLine: false);
            }
        }

        /// <summary>
        /// A helper method to call into <paramref name="renderAttributeValue"/> and surrounds it with the appropriate
        /// prefix/suffix. For example, if the attribute value is a <see cref="string"/> we want to make sure 
        /// we surround it with quotes; if it's something like an <see cref="int"/> we don't want to surround 
        /// it with anything.
        /// </summary>S
        /// <param name="writer">The code writer that's used to render the value.</param>
        /// <param name="renderAttributeValue">Used to render the raw attribute value. Depending on the 
        /// <see cref="PropertyInfo"/> the attribute value may be surrounded by single or double quotes.</param>
        /// <param name="attributeValueType">The <see cref="Type"/> of the output that will be rendered by 
        /// <paramref name="renderAttributeValue"/>.</param>
        protected void RenderValue(
            CSharpCodeWriter writer,
            Action<CSharpCodeWriter> renderAttributeValue,
            Type attributeValueType)
        {
            var surrounding = string.Empty;

            if (attributeValueType == typeof(string))
            {
                surrounding = "\"";
            }
            else if (attributeValueType == typeof(char))
            {
                surrounding = "'";
            }
            else
            {
                // Short circuit, the surrounding is empty, no need to continue
                renderAttributeValue(writer);
                return;
            }

            writer.Write(surrounding);
            renderAttributeValue(writer);
            writer.Write(surrounding);
        }

        /// <summary>
        /// A helper method to write the "setting" of the <see cref="TagHelperExpression.IsSet"/> property.
        /// </summary>
        /// <param name="writer">The code writer that's used to render the value.</param>
        /// <param name="isSet">Whether the <see cref="TagHelperExpression.IsSet"/> is set or not.</param>
        protected void GenerateIsSetProperty(CSharpCodeWriter writer, bool isSet)
        {
            writer.Write(" { ")
                  .WriteStartAssignment(TagHelperExpression.IsSetPropertyName)
                  .Write(isSet.ToString().ToLowerInvariant())
                  .Write(" }");
        }

        // Internal for testing purposes
        internal static Type GetBuildType(TypeInfo type)
        {
            // Iterate through the base types and find the generic tag helper expression.
            while (!type.IsGenericType ||
                   !(type.GetGenericTypeDefinition() == typeof(TagHelperExpression<>)))
            {
                type = type.BaseType.GetTypeInfo();
            }

            var genericArguments = type.GenericTypeArguments;
            // The first argument refers to the attribute type.
            var buildType = genericArguments.FirstOrDefault();

            return buildType;
        }

        internal static string GetName(TypeInfo type)
        {
            var suffix = string.Empty;

            // If we're an array we need to build out a valid array suffix
            while (type.IsArray)
            {
                var arrayRank = type.GetArrayRank();
                if (arrayRank == 1)
                {
                    suffix += "[]";
                }
                else
                {
                    suffix += "[";
                    suffix += new string(',', arrayRank - 1);
                    suffix += "]";
                }

                // Iterate down through the array's types
                type = type.GetElementType().GetTypeInfo();
            }

            string name;
            var genericArguments = string.Empty;

            if (!type.IsGenericType)
            {
                name = type.FullName;
            }
            else
            {
                name = GetNonGenericName(type);
                genericArguments = GetGenericParameterString(type);
            }

            return string.Format("{0}{1}{2}", name, genericArguments, suffix);
        }

        internal static string GetNonGenericName(TypeInfo type)
        {
            Debug.Assert(type.IsGenericType);

            var name = type.FullName;
            var index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }

        private static string GetGenericParameterString(TypeInfo type)
        {
            var values = string.Join(",", type.GenericTypeArguments.Select(arg => GetName(arg.GetTypeInfo())));

            if (!string.IsNullOrEmpty(values))
            {
                return string.Format("<{0}>", values);
            }

            return string.Empty;
        }
    }
}