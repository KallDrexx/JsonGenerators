using Shouldly;
using Xunit;

namespace JsonGenerators.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            const string json = @"{""StringValue"":""abcd efg"",""IntValue"":23,""DoubleValue"":3.45,""BoolValue"":true}";

            var result = TestClassDeserializer.Deserialize(json);
            result.ShouldNotBeNull();
            result.StringValue.ShouldBe("abcd efg");
            result.IntValue.ShouldBe(23);
            result.BoolValue.ShouldBeTrue();
            result.DoubleValue.ShouldBe(3.45);
            result.NullableIntValue.ShouldBeNull();
        }
    }
}