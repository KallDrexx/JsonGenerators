using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

namespace JsonGenerators.Benchmarks
{
    [SimpleJob()]
    public class HardCodedBenchmark
    {
        private const string Json = @"{""StringValue"":""abcd efg"",""IntValue"":23,""DoubleValue"":3.45,""BoolValue"":true}";

        [Benchmark]
        public TestClass JsonDotNet() => JsonConvert.DeserializeObject<TestClass>(Json);

        [Benchmark]
        public TestClass CustomDeserializer() => TestClassDeserializer.Deserialize(Json);
    }
}