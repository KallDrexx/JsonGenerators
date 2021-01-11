using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace JsonGenerators.Test
{
    [GenerateJsonDeserializer]
    public class Abcdefg
    {
        public int NumericValue { get; set; }
        public string SomeOtherContent { get; set; }
        public double DecimalValue { get; set; }
        public bool IsCorrect { get; set; }
        public int? MaybeValue { get; set; }
    }
    
    public class JsonDeserializationGeneratorTests
    {
        [Fact]
        public void Can_Deserialize_With_Generated_Deserializer()
        {
            const string json = @"{""SomeOtherContent"":""abcd efg"",""NumericValue"":23,""DecimalValue"":3.45,""IsCorrect"":true, ""MaybeValue"": 42}";

            var result = AbcdefgDeserializer.Deserialize(json);
            result.ShouldNotBeNull();
            result.SomeOtherContent.ShouldBe("abcd efg");
            result.NumericValue.ShouldBe(23);
            result.IsCorrect.ShouldBeTrue();
            result.DecimalValue.ShouldBe(3.45);
            result.MaybeValue.ShouldBe(42);
        }
        
        [Fact]
        public void Can_Run_Code_Through_Generator_Without_Roslyn_Diagnostic_Errors()
        {
            var inputCompilation = CreateCompilation(@"
using JsonGenerators;

namespace SomeNamespace.OtherPart
{
    [GenerateJsonDeserializer]
    public class SomeClass
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
        public bool BoolValue { get; set; }
        public int? NullableIntValue { get; set; }
    }
}
");

            var generator = new JsonDeserializationGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, 
                out var outputCompilation,
                out var diagnostics);

            if (diagnostics.Any())
            {
                var messages = string.Join(Environment.NewLine + Environment.NewLine,
                    diagnostics.Select(x => x.Descriptor.Description));
                
                Assert.True(false, messages);
            }

            outputCompilation.SyntaxTrees.Count().ShouldBe(2); // Should have the original syntax tree and the one we generated
        }
        
        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(GenerateJsonDeserializerAttribute).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}