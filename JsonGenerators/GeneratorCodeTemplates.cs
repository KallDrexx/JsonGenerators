namespace JsonGenerators
{
    public static class GeneratorCodeTemplates
    {
        /// <summary>
        /// {0} == namespace, {1} == class name, {2} == property if statements
        /// </summary>
        public const string OverallCodeTemplate = @"
using System;
using JsonGenerators;

namespace {0}
{{
    public static class {1}Deserializer
    {{
        public static {1} Deserialize(string json)
        {{
            if (json == null)
            {{
                return null;
            }}

            var span = json.AsSpan();
            return JsonParseUtils.ParseObject<{1}>(span, 0, ParseProperties);  
        }}

        private static int? ParseProperties({1} result, ReadOnlySpan<char> json, JsonSection section)
        {{
            var propertyNameSlice = json.Slice(section.StartIndex + 1, section.EndIndex - section.StartIndex - 1);
            {2}

            var message = $""Unknown property found: '{{propertyNameSlice.ToString()}}"";
            throw new InvalidProgramException(message);
        }}
    }}
}}
";

        /// <summary>
        /// {0} == class name, {1} == property name
        /// </summary>
        public const string StringPropertyTemplate = @"
            if (propertyNameSlice.Equals(nameof({0}.{1}), StringComparison.OrdinalIgnoreCase))
            {{
                var nextStringSection = JsonParseUtils.FindNextString(json, section.NextCharIndex.Value + 1);
                var valueSlice = json.Slice(nextStringSection.StartIndex,
                    nextStringSection.EndIndex - nextStringSection.StartIndex);

                result.{1} = valueSlice.Equals(""null"", StringComparison.Ordinal) 
                    ? null 
                    : valueSlice.Slice(1).ToString();

                    return nextStringSection.NextCharIndex;
            }}            
";

        /// <summary>
        /// {0} == class name, {1} == property name
        /// </summary>
        public const string IntPropertyTemplate = @"
            if (propertyNameSlice.Equals(nameof({0}.{1}), StringComparison.OrdinalIgnoreCase))
            {{
                var numberSection = JsonParseUtils.FindNextNumber(json, section.NextCharIndex.Value + 1);
                var valueSlice = json.Slice(numberSection.StartIndex,
                    numberSection.EndIndex - numberSection.StartIndex);

                result.{1} = Convert.ToInt32(valueSlice.ToString());

                return numberSection.NextCharIndex;
            }}
";

        /// <summary>
        /// {0} == class name, {1} == property name
        /// </summary>
        public const string BoolPropertyTemplate = @"
            if (propertyNameSlice.Equals(nameof({0}.{1}), StringComparison.OrdinalIgnoreCase))
            {{
                var boolSection = JsonParseUtils.FindNextBool(json, section.NextCharIndex.Value + 1);
                var valueSlice = json.Slice(boolSection.StartIndex, boolSection.EndIndex - boolSection.StartIndex);

                if (valueSlice.Equals(""true"", StringComparison.Ordinal))
                {{
                    result.{1} = true;
                }}
                else if (valueSlice.Equals(""false"", StringComparison.Ordinal))
                {{
                    result.{1} = false;
                }}
                else
                {{
                    throw new InvalidOperationException($""Invalid bool value of '{{valueSlice.ToString()}}'"");
                }}

                return boolSection.NextCharIndex;
            }}
";

        /// <summary>
        /// {0} == class name, {1} == property name
        /// </summary>
        public const string NullableIntPropertyTemplate = @"
             if (propertyNameSlice.Equals(nameof({0}.{1}), StringComparison.OrdinalIgnoreCase))
             {{
                var numberSection = JsonParseUtils.FindNextNumber(json, section.NextCharIndex.Value + 1);
                var valueSlice = json.Slice(numberSection.StartIndex,
                    numberSection.EndIndex - numberSection.StartIndex);

                if (valueSlice.Equals(""null"", StringComparison.Ordinal))
                {{
                    result.{1} = null;
                }}
                else
                {{
                    result.{1} = Convert.ToInt32(valueSlice.ToString());
                }}

                return numberSection.NextCharIndex;
            }}
";

        /// <summary>
        /// {0} == class name, {1} == property name
        /// </summary>
        public const string DoublePropertyTemplate = @"
            if (propertyNameSlice.Equals(nameof({0}.{1}), StringComparison.OrdinalIgnoreCase))
            {{
                var numberSection = JsonParseUtils.FindNextNumber(json, section.NextCharIndex.Value + 1);
                var valueSlice = json.Slice(numberSection.StartIndex,
                    numberSection.EndIndex - numberSection.StartIndex);

                result.{1} = Convert.ToDouble(valueSlice.ToString());

                return numberSection.NextCharIndex;
            }}
";
    }
}