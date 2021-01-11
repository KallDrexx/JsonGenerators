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
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
        public bool BoolValue { get; set; }
        public int? NullableIntValue { get; set; }
    }
    
    public class JsonDeserializationGeneratorTests
    {
        [Fact]
        public void Can_Deserialize_With_Generated_Deserializer()
        {
            const string json = @"{""StringValue"":""abcd efg"",""IntValue"":23,""DoubleValue"":3.45,""BoolValue"":true}";

            var result = AbcdefgDeserializer.Deserialize(json);
            result.ShouldNotBeNull();
            result.StringValue.ShouldBe("abcd efg");
            result.IntValue.ShouldBe(23);
            result.BoolValue.ShouldBeTrue();
            result.DoubleValue.ShouldBe(3.45);
            result.NullableIntValue.ShouldBeNull();
        }
        
        [Fact]
        public void Test()
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