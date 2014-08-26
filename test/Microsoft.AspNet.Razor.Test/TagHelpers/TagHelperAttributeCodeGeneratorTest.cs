using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;
using Microsoft.AspNet.Razor.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.TagHelpers
{
    public class TagHelperAttributeCodeGeneratorTest
    {
        public static IEnumerable<object[]> AttributeCodeGeneratorPropertyNames
        {
            get
            {
                return typeof(CustomTagHelper).GetProperties()
                                              .Select(propertyInfo =>
                                                    new object[] {
                                                        propertyInfo,
                                                        new TagHelperAttributeCodeGenerator(propertyInfo)
                                                    });
            }
        }

        public static IEnumerable<object[]> ComplexTypeNames
        {
            get
            {
                yield return new object[] { typeof(byte).GetTypeInfo(), "System.Byte" };
                yield return new object[] { typeof(int[]).GetTypeInfo(), "System.Int32[]" };
                yield return new object[] { typeof(long[][]).GetTypeInfo(), "System.Int64[][]" };
                yield return new object[] { typeof(short[,]).GetTypeInfo(), "System.Int16[,]" };
                yield return new object[] { typeof(double[,,,][,][]).GetTypeInfo(), "System.Double[,,,][,][]" };
                yield return new object[] { typeof(IEnumerable<string>[]).GetTypeInfo(),
                    "System.Collections.Generic.IEnumerable<System.String>[]" };
                yield return new object[] { typeof(IEnumerable<string>[]).GetTypeInfo(),
                    "System.Collections.Generic.IEnumerable<System.String>[]" };
                yield return new object[] { typeof(IEnumerable<string[]>).GetTypeInfo(),
                    "System.Collections.Generic.IEnumerable<System.String[]>" };
                yield return new object[] { typeof(KeyValuePair<string, IEnumerable<int>>).GetTypeInfo(),
                    "System.Collections.Generic.KeyValuePair<System.String,System.Collections.Generic.IEnumerable<System.Int32>>" };
                yield return new object[] { typeof(IDictionary<string, IEnumerable<KeyValuePair<char, byte>>>).GetTypeInfo(),
                    "System.Collections.Generic.IDictionary<System.String,System.Collections.Generic.IEnumerable<"+
                    "System.Collections.Generic.KeyValuePair<System.Char,System.Byte>>>" };
                yield return new object[] { typeof(IDictionary<string[], IEnumerable<KeyValuePair<char[], byte>>>).GetTypeInfo(),
                    "System.Collections.Generic.IDictionary<System.String[],System.Collections.Generic.IEnumerable<"+
                    "System.Collections.Generic.KeyValuePair<System.Char[],System.Byte>>>" };
                yield return new object[] { typeof(IDictionary<string[], IEnumerable<KeyValuePair<char[], byte[,,][][,,,]>>[]>[][]).GetTypeInfo(),
                    "System.Collections.Generic.IDictionary<System.String[],System.Collections.Generic.IEnumerable<"+
                    "System.Collections.Generic.KeyValuePair<System.Char[],System.Byte[,,][][,,,]>>[]>[][]" };

            }
        }

        private CodeGeneratorContext EmptyContext
        {
            get
            {
                return CodeGeneratorContext.Create(
                    host: null,
                    className: null,
                    rootNamespace: null,
                    sourceFile: null,
                    shouldGenerateLinePragmas: true);
            }
        }

        [Theory]
        [MemberData("ComplexTypeNames")]
        public void TagHelperAttributeCodeGenerator_GetNameReturnsCorrectTypeNames(
            TypeInfo type,
            string expectedTypeName)
        {
            // Act
            var typeName = TagHelperAttributeCodeGenerator.GetName(type);

            // Assert
            Assert.Equal(expectedTypeName, typeName);
        }

        [Theory]
        [InlineData("System.String", "\"")]
        [InlineData("System.Char", "'")]
        [InlineData("System.Int32", "")]
        [InlineData("System.Double", "")]
        [InlineData("System.Object", "")]
        public void TagHelperAttributeCodeGenerator_GenerateValueSurroundsValuesCorrectly(
            string typeName, string surrounder)
        {
            // Arrange
            var type = Type.GetType(typeName);
            var generator = new CustomTagHelperAttributeCodeGenerator();
            var writer = new CSharpCodeWriter();
            var attributeValue = "Hello World";

            // Act
            generator.ExposeGenerateValue(writer, renderAttributeValue: () =>
            {
                writer.Write(attributeValue);
            }, attributeValueType: type);
            var code = writer.GenerateCode();

            // Assert
            Assert.Equal(string.Format("{0}{1}{0}", surrounder, attributeValue), code);
        }

        [Theory]
        [MemberData("AttributeCodeGeneratorPropertyNames")]
        public void TagHelperAttributeCodeGenerator_GenerateCodeNewsUpExpressionsCorrectly(
            PropertyInfo propertyInfo,
            TagHelperAttributeCodeGenerator attrCodeGenerator)
        {
            // Arrange
            var buildType = TagHelperAttributeCodeGenerator.GetBuildType(propertyInfo.PropertyType.GetTypeInfo());
            var expressionName = TagHelperAttributeCodeGenerator.GetNonGenericName(
                typeof(CustomTagHelperExpression<>).GetTypeInfo());
            var writer = new CSharpCodeWriter();

            // Act
            attrCodeGenerator.GenerateCode(writer, EmptyContext, renderAttributeValue: null);
            var code = writer.GenerateCode();

            // Assert
            Assert.True(code.StartsWith(
                string.Format("new {0}<{1}>(",
                    expressionName,
                    buildType.FullName)));
        }

        [Theory]
        [MemberData("AttributeCodeGeneratorPropertyNames")]
        public void TagHelperAttributeCodeGenerator_GeneratesIsSetCorrectly(
            PropertyInfo propertyInfo,
            TagHelperAttributeCodeGenerator attrCodeGenerator)
        {
            // Arrange
            var buildType = TagHelperAttributeCodeGenerator.GetBuildType(propertyInfo.PropertyType.GetTypeInfo());
            var isSetWriter = new CSharpCodeWriter();
            var isNotSetWriter = new CSharpCodeWriter();

            // Act
            attrCodeGenerator.GenerateCode(isSetWriter, EmptyContext, renderAttributeValue: () => { });
            var isSetCode = isSetWriter.GenerateCode();
            attrCodeGenerator.GenerateCode(isNotSetWriter, EmptyContext, renderAttributeValue: null);
            var isNotSetCode = isNotSetWriter.GenerateCode();


            // Assert
            Assert.True(isSetCode.EndsWith(" { IsSet = true }"));
            Assert.True(isNotSetCode.EndsWith("()"));
        }

        [Theory]
        [MemberData("AttributeCodeGeneratorPropertyNames")]
        public void TagHelperAttributeCodeGenerator_GeneratesValuesCorrectly(
            PropertyInfo propertyInfo,
            TagHelperAttributeCodeGenerator attrCodeGenerator)
        {
            // Arrange
            var buildType = TagHelperAttributeCodeGenerator.GetBuildType(propertyInfo.PropertyType.GetTypeInfo());
            var surrounding = string.Empty;

            if (buildType == typeof(string))
            {
                surrounding = "\"";
            }
            else if (buildType == typeof(char))
            {
                surrounding = "'";
            }
            var writer = new CSharpCodeWriter();

            // Act
            attrCodeGenerator.GenerateCode(writer, EmptyContext, renderAttributeValue: () =>
            {
                writer.Write("Hello World");
            });
            var code = writer.GenerateCode();

            // Assert
            Assert.True(code.EndsWith(string.Format("({0}Hello World{0}) {{ IsSet = true }}", surrounding)), code);
        }

        private class CustomTagHelper
        {
            public CustomTagHelperExpression<int> IntProp { get; set; }
            public CustomTagHelperExpression<string> StringProp { get; set; }
            public CustomTagHelperExpression<object> ObjectProp { get; set; }
            public CustomTagHelperExpression<char> CharProp { get; set; }
        }

        private class CustomTagHelperAttributeCodeGenerator : TagHelperAttributeCodeGenerator
        {
            public CustomTagHelperAttributeCodeGenerator()
                : base(null)
            {
            }

            public void ExposeGenerateValue(
                CSharpCodeWriter writer,
                Action renderAttributeValue,
                Type attributeValueType)
            {
                GenerateValue(writer, renderAttributeValue, attributeValueType);
            }
        }

        private class CustomTagHelperExpression<T> : TagHelperExpression<T>
        {
            public override T Build(TagHelperContext context)
            {
                return default(T);
            }
        }
    }
}