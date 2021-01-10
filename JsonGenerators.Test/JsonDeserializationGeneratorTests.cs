using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace JsonGenerators.Test
{
    public class JsonDeserializationGeneratorTests
    {
        [Fact]
        public void Test()
        {
            var inputCompilation = CreateCompilation(@"
using JsonGenerators;

namespace SomeNamespace
{
    [GenerateJsonDeserializer]
    public class SomeClass
    {
        public string StringValue { get; set; }
    }
}
");

            var generator = new JsonDeserializationGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, 
                out var outputCompilation,
                out var diagnostics);

        }
        
        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(GenerateJsonDeserializerAttribute).Assembly.Location),
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}