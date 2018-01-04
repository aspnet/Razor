// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.VisualStudio.LanguageServices.Razor;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Razor.Performance
{
    public class TagHelperSerializationBenchmark
    {
        private string _tagHelperFilePath;

        public TagHelperSerializationBenchmark()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null && !File.Exists(Path.Combine(current.FullName, "taghelpers.json")))
            {
                current = current.Parent;
            }

            _tagHelperFilePath = Path.Combine(current.FullName, "taghelpers.json");
        }

        [Benchmark(Description = "Razor TagHelper Serialization")]
        public void TagHelper_Serialization_RoundTrip()
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new RazorDiagnosticJsonConverter());
            serializer.Converters.Add(new TagHelperDescriptorJsonConverter());

            // Deserialize from json file.
            IReadOnlyList<TagHelperDescriptor> tagHelpers;
            using (var stream = File.OpenRead(_tagHelperFilePath))
            {
                var reader = new JsonTextReader(new StreamReader(stream));
                tagHelpers = serializer.Deserialize<IReadOnlyList<TagHelperDescriptor>>(reader);
            }

            // Serialize back to json.
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 4096, leaveOpen: true))
            {
                serializer.Serialize(writer, tagHelpers);
            }
        }
    }
}
