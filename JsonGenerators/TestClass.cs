using System;
using System.Collections.Generic;

namespace JsonGenerators
{
    public class TestClass
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
        public bool BoolValue { get; set; }
        public int? NullableIntValue { get; set; }
        //public List<int> IntArray { get; set; }
        //public Dictionary<string, int> DictionaryValue { get; set; }
    }

    public static class TestClassDeserializer
    {
        public static TestClass Deserialize(string json)
        {
            if (json == null)
            {
                return null;
            }

            var span = json.AsSpan();
            return JsonParseUtils.ParseObject<TestClass>(span, 0, ParseProperties);
        }

        private static int? ParseProperties(TestClass result, ReadOnlySpan<char> json, JsonSection section)
        {
            var propertyNameSlice = json.Slice(section.StartIndex + 1, section.EndIndex - section.StartIndex - 2);
            if (propertyNameSlice.Equals(nameof(TestClass.StringValue), StringComparison.OrdinalIgnoreCase))
            {
                var nextStringSection = JsonParseUtils.FindNextString(json, section.NextCharIndex.Value + 1);
                var valueSlice = json.Slice(nextStringSection.StartIndex,
                    nextStringSection.EndIndex - nextStringSection.StartIndex - 1);

                result.StringValue = valueSlice.Equals("null", StringComparison.Ordinal) 
                    ? null 
                    : valueSlice.Slice(1).ToString();

                return nextStringSection.NextCharIndex;
            }
            
            if (propertyNameSlice.Equals(nameof(TestClass.IntValue), StringComparison.OrdinalIgnoreCase))
            {
                var numberSection = JsonParseUtils.FindNextNumber(json, section.NextCharIndex.Value + 1);
                var valueSlice = json.Slice(numberSection.StartIndex,
                    numberSection.EndIndex - numberSection.StartIndex - 1);

                result.IntValue = Convert.ToInt32(valueSlice.ToString());

                return numberSection.NextCharIndex;
            }
            
            if (propertyNameSlice.Equals(nameof(TestClass.BoolValue), StringComparison.OrdinalIgnoreCase))
            {
                var boolSection = JsonParseUtils.FindNextBool(json, section.NextCharIndex.Value + 1);
                var valueSlice = json.Slice(boolSection.StartIndex, boolSection.EndIndex - boolSection.StartIndex - 1);

                if (valueSlice.Equals("true", StringComparison.Ordinal))
                {
                    result.BoolValue = true;
                }
                else if (valueSlice.Equals("false", StringComparison.Ordinal))
                {
                    result.BoolValue = false;
                }
                else
                {
                    throw new InvalidOperationException($"Invalid bool value of '{valueSlice.ToString()}'");
                }

                return boolSection.NextCharIndex;
            }

            if (propertyNameSlice.Equals(nameof(TestClass.NullableIntValue), StringComparison.OrdinalIgnoreCase))
            {
                var numberSection = JsonParseUtils.FindNextNumber(json, section.NextCharIndex.Value + 1);
                var valueSlice = json.Slice(numberSection.StartIndex,
                    numberSection.EndIndex - numberSection.StartIndex - 1);

                if (valueSlice.Equals("null", StringComparison.Ordinal))
                {
                    result.NullableIntValue = null;
                }
                else
                {
                    result.IntValue = Convert.ToInt32(valueSlice.ToString());
                }

                return numberSection.NextCharIndex;
            }

            if (propertyNameSlice.Equals(nameof(TestClass.DoubleValue), StringComparison.OrdinalIgnoreCase))
            {
                var numberSection = JsonParseUtils.FindNextNumber(json, section.NextCharIndex.Value + 1);
                var valueSlice = json.Slice(numberSection.StartIndex,
                    numberSection.EndIndex - numberSection.StartIndex - 1);

                result.DoubleValue = Convert.ToDouble(valueSlice.ToString());

                return numberSection.NextCharIndex;
            }

            var message = $"Unknown property found: '{propertyNameSlice.ToString()}";
            throw new InvalidProgramException(message);
        }
    }
}