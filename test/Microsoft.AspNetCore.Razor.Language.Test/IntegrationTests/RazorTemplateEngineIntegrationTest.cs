// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNetCore.Razor.Language.IntegrationTests
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class RazorTemplateEngineIntegrationTest : IntegrationTestBase
    {
        [Fact]
        public void GenerateCodeWithDefaults()
        {
            // Arrange
            var fileSystem = new DefaultRazorProjectFileSystem(TestProjectRoot);
            var razorEngine = RazorEngine.Create(engine =>
            {
                engine.Features.Add(new SuppressChecksumOptionsFeature());
            });
            var templateEngine = new RazorTemplateEngine(razorEngine, fileSystem);

            // Act
            var cSharpDocument = templateEngine.GenerateCode($"{FileName}.cshtml");

            // Assert
            AssertCSharpDocumentMatchesBaseline(cSharpDocument);
        }

        [Fact]
        public void GenerateCodeWithBaseType()
        {
            // Arrange
            var fileSystem = new DefaultRazorProjectFileSystem(TestProjectRoot);
            var razorEngine = RazorEngine.Create(engine =>
            {
                engine.Features.Add(new SuppressChecksumOptionsFeature());

                engine.SetBaseType("MyBaseType");
            });
            var templateEngine = new RazorTemplateEngine(razorEngine, fileSystem);

            // Act
            var cSharpDocument = templateEngine.GenerateCode($"{FileName}.cshtml");

            // Assert
            AssertCSharpDocumentMatchesBaseline(cSharpDocument);
        }

        [Fact]
        public void GenerateCodeWithConfigureClass()
        {
            // Arrange
            var fileSystem = new DefaultRazorProjectFileSystem(TestProjectRoot);
            var razorEngine = RazorEngine.Create(engine =>
            {
                engine.Features.Add(new SuppressChecksumOptionsFeature());

                engine.ConfigureClass((document, @class) =>
                {
                    @class.ClassName = "MyClass";

                    @class.Modifiers.Clear();
                    @class.Modifiers.Add("protected");
                    @class.Modifiers.Add("internal");
                });

                engine.ConfigureClass((document, @class) =>
                {
                    @class.Interfaces = new[] { "global::System.IDisposable" };
                    @class.BaseType = "CustomBaseType";
                });
            });
            var templateEngine = new RazorTemplateEngine(razorEngine, fileSystem);

            // Act
            var cSharpDocument = templateEngine.GenerateCode($"{FileName}.cshtml");

            // Assert
            AssertCSharpDocumentMatchesBaseline(cSharpDocument);
        }

        [Fact]
        public void GenerateCodeWithSetNamespace()
        {
            // Arrange
            var fileSystem = new DefaultRazorProjectFileSystem(TestProjectRoot);
            var razorEngine = RazorEngine.Create(engine =>
            {
                engine.Features.Add(new SuppressChecksumOptionsFeature());

                engine.SetNamespace("MyApp.Razor.Views");
            });
            var templateEngine = new RazorTemplateEngine(razorEngine, fileSystem);

            // Act
            var cSharpDocument = templateEngine.GenerateCode($"{FileName}.cshtml");

            // Assert
            AssertCSharpDocumentMatchesBaseline(cSharpDocument);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
