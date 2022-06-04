using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Text.Json;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace MEMConsole.AdminServiceCodeGen
{
    [Generator]
    public class GenerateAdminServiceClient : IIncrementalGenerator
    {
        private static readonly List<AdminServiceSourceFile> sourceFiles = new();
        private static readonly List<AdminServiceSourceFile> inputFiles = new();
        private static void GetResourceText()
        {
            foreach(var resource in typeof(GenerateAdminServiceClient).Assembly.GetManifestResourceNames())
            {
                if(!(resource.EndsWith("xml", StringComparison.OrdinalIgnoreCase) || resource.EndsWith("json", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                var actualName = resource.Replace("MEMConsole.AdminServiceCodeGen.XML.", "");
                var newas = new AdminServiceSourceFile
                {
                    FileName = actualName
                };
                using (var stream = typeof(GenerateAdminServiceClient).Assembly.GetManifestResourceStream(resource))
                {
                    using var sr = new StreamReader(stream);
                    newas.Source = sr.ReadToEnd();
                }
                inputFiles.Add(newas);
            }
        }
        private static void Execute(IncrementalGeneratorPostInitializationContext context)
        {
            if (sourceFiles.Count == 0)
            {
                GetResourceText();
                var adminServClientBuilder = new AdminServiceClientBuilder();
                var jsonFile = inputFiles.Where(p => p.FileName.EndsWith("Include.json", StringComparison.OrdinalIgnoreCase)).First();
                var jsonIncludes = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonFile.Source);
                foreach (var additionalFile in inputFiles.Where(p => p.FileName.EndsWith("xml", StringComparison.OrdinalIgnoreCase)))
                {
                    if (!additionalFile.FileName.ToLower().Contains("metadata")) { continue; }
                    var mdParser = new MetadataParser(additionalFile, jsonIncludes);
                    sourceFiles.AddRange(mdParser.MetadataTypeFiles);
                }
            }
            foreach (var file in sourceFiles)
            {
                context.AddSource(file.FileName, SourceText.From(file.Source, Encoding.UTF8));
            }
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx => Execute(ctx));
        }
    }
}
