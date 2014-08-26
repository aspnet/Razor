using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// Provides a way of indicating how to generate code for a tag helper attribute's creation.
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
        /// Called during Razor's code generation process to generate code that instantiates the value of the tag helpers
        /// property. Values that are written should not end the line.
        /// </summary>
        /// <example>
        /// Writes the string: "new MyPropertyType(...)" to the output where the "..." is rendered by calling the
        /// <paramref name="renderAttributeValue"/> <see cref="Action"/>.
        /// </example>
        /// <param name="writer">The code writer that writes directly to the generated Razor class.</param>
        /// <param name="context">An informational object that contains information about the current code generation
        /// process.</param>
        /// <param name="renderAttributeValue">Renders the raw value of the attribute. Will be null if there is no attribute 
        /// value. Example: If the attribute value is '3' and we want to new up an object that takes the attribute value we'd 
        /// write "new MyObjectType(" then call into <paramref name="renderAttributeValue"/> and finally write the ending ")".</param>
        public virtual void GenerateCode(CSharpCodeWriter writer, CodeGeneratorContext context, Action renderAttributeValue)
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
                GenerateValue(writer, renderAttributeValue, attributeValueType);

                writer.WriteEndMethodInvocation(endLine: false);

                // Since there's an attribute value set the IsSet property, AKA: { IsSet = true }
                GenerateIsSetProperty(writer, isSet: true);
            }
            else
            {
                writer.WriteEndMethodInvocation(endLine: false);
            }
        }

        /// <summary>
        /// A helper method to call into <paramref name="renderAttributeValue"/> and surrounds it with the appropriate
        /// prefix/suffix. For example, if the attribute value is a <see cref="string"/> we want to make sure we surround it with quotes;
        /// if it's something like an <see cref="int"/> we don't want to surround it with anything.
        /// </summary>
        /// <param name="writer">The code writer that writes directly to the generated Razor class.</param>
        /// <param name="renderAttributeValue">Renders the raw value of the attribute. Example: If the attribute value is '3'
        /// and we want to new up an object that takes the attribute value we'd write "new MyObjectType(" then call into 
        /// <paramref name="renderAttributeValue"/> and finally write the ending ")".</param>
        /// <param name="attributeValueType">The <see cref="Type"/> of the output that will be rendered by <paramref name="renderAttributeValue"/>.</param>
        protected virtual void GenerateValue(
            CSharpCodeWriter writer,
            Action renderAttributeValue,
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
                renderAttributeValue();
                return;
            }

            writer.Write(surrounding);
            renderAttributeValue();
            writer.Write(surrounding);
        }

        /// <summary>
        /// A helper method to write the "setting" of the <see cref="TagHelperExpression.IsSet"/> property.
        /// </summary>
        /// <param name="writer">The code writer that writes directly to the generated Razor class.</param>
        /// <param name="isSet">Whether the <see cref="TagHelperExpression.IsSet"/> is set or not.</param>
        protected virtual void GenerateIsSetProperty(CSharpCodeWriter writer, bool isSet)
        {
            writer.Write(" { ")
                  .WriteStartAssignment(TagHelperExpression.IsSetPropertyName)
                  .Write(isSet.ToString().ToLowerInvariant())
                  .Write(" }");
        }

        internal static Type GetBuildType(TypeInfo type)
        {
            // Iterate through the base types and find the generic tag helper expression.
            while (!type.IsGenericType ||
                   !(type.GetGenericTypeDefinition() == typeof(TagHelperExpression<>)))
            {
                type = type.BaseType.GetTypeInfo();
            }

            var genericArguments = type.GenericTypeArguments;
            // The first argument is the build type.
            var buildType = genericArguments.FirstOrDefault();

            return buildType;
        }

        internal static string GetName(TypeInfo type)
        {
            var suffix = string.Empty;

            // If we're an array we need to build out a valid array suffix
            if (type.IsArray)
            {
                do
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
                while (type.IsArray);
            }

            string name = null;
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