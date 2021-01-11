using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

namespace JsonGenerators.Benchmarks
{
    [GenerateJsonDeserializer]
    public class TestClass
    {
        public int NumericValue { get; set; }
        public string SomeOtherContent { get; set; }
        public double DecimalValue { get; set; }
        public int? MaybeValue { get; set; }
        public bool IsCorrect { get; set; }
    }
    
    public class DeserializationBenchmark
    {
        private const string Json = @"{""SomeOtherContent"":""abcd efg"",""NumericValue"":23,""DecimalValue"":3.45,""IsCorrect"":true, ""MaybeValue"": 42}";

        [Benchmark]
        public TestClass JsonDotNet() => JsonConvert.DeserializeObject<TestClass>(Json);

        [Benchmark]
        public TestClass CustomDeserializer() => TestClassDeserializer.Deserialize(Json);

        [Benchmark]
        public TestClass SystemTextJson() => System.Text.Json.JsonSerializer.Deserialize<TestClass>(Json);
    }
}