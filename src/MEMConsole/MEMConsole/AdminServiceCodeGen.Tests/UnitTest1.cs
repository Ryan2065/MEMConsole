using MEMConsole.AdminServiceCodeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Reflection;

namespace MEMConsole.AdminServiceCodeGen.Tests;

public class UnitTest1
{
    [Fact]
    public void SimpleTest()
    {
        Compilation inputCompilation = CreateCompilation(@"
namespace MEMConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
namespace MEMConsole.Client.AdminServiceModels
{
    public class AdminserviceClient
    {
        
    }
}
");
        var generator = new GenerateAdminServiceClient();
        
        var additionalFiles = new List<AdditionalText>();
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if(assemblyPath == null)
        {
            throw new ApplicationException("Could not find assembly path");
        }
        var dirXmlFiles = System.IO.Directory.GetFiles(assemblyPath, "*metadata.xml");
        foreach(var xmlFile in dirXmlFiles)
        {
            AdditionalText adText = new TestText(xmlFile);
            additionalFiles.Add(adText);
        }
        var dirJsonFiles = System.IO.Directory.GetFiles(assemblyPath, "Include.json");
        foreach (var jsonFile in dirJsonFiles)
        {
            AdditionalText adText = new TestText(jsonFile);
            additionalFiles.Add(adText);
        }
        GeneratorDriver driv = CSharpGeneratorDriver.Create(generator).AddAdditionalTexts(ImmutableArray.CreateRange(additionalFiles));
        _ = driv.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputComp, out var diagnostics);
        Assert.True(diagnostics.IsEmpty, "Diagnostics is not empty!");
        Assert.True(outputComp.SyntaxTrees.Count() > 1, "Did not generate any new syntax trees");
        
    }
    private static Compilation CreateCompilation(string source)
    => CSharpCompilation.Create("compilation",
        new[] { CSharpSyntaxTree.ParseText(source) },
        new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
        new CSharpCompilationOptions(OutputKind.ConsoleApplication));
}

public class TestText : AdditionalText
{
    private readonly string _path = default!;
    public TestText(string filePath)
    {
        _path = filePath;
    }
    public override string Path
    {
        get
        {
            return _path;
        }
    }

    public override SourceText? GetText(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
