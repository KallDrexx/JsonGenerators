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
        public List<int> IntArray { get; set; }
        public Dictionary<string, int> DictionaryValue { get; set; }
    }

    public static class TestClassDeserializer
    {
        public static TestClass Deserialize(string json)
        {
            return json == null 
                ? null 
                : ParseRootObject(json.AsSpan());
        }
        
        private static TestClass ParseRootObject(ReadOnlySpan<char> json)
        {
            var result = (TestClass) null;
            var endBraceEncountered = false;
            var nullEncountered = false;
            for (var currentIndex = 0; currentIndex < json.Length; currentIndex++)
            {
                var currentChar = json[currentIndex];
                switch (currentChar)
                {
                    case var whitespace when char.IsWhiteSpace(whitespace):
                        break;

                    case '{' when result == null && !nullEncountered:
                        result = new TestClass();
                        break;

                    case '}' when result != null && !endBraceEncountered:
                        endBraceEncountered = true;
                        break;

                    case 'n' when currentIndex + 3 < json.Length &&
                                  result == null &&
                                  json[currentIndex + 1] == 'u' &&
                                  json[currentIndex + 2] == 'l' &&
                                  json[currentIndex + 3] == 'l':
                        // Since we aren't in an object, and we encountered "null", assume it's valid and move
                        // past it.  If there's non-whitespace after it the for loop will catch it and exception.
                        currentIndex += 3;
                        nullEncountered = true;
                        break;

                    case '"' when result != null:
                        var endIndex = currentIndex + 1;
                        var slashEncountered = false;
                        while (endIndex < json.Length)
                        {
                            if (!slashEncountered && json[endIndex] == '"')
                            {
                                break;
                            }

                            if (!slashEncountered && json[endIndex] == '\\')
                            {
                                slashEncountered = true;
                            }
                            else
                            {
                                slashEncountered = false;
                            }

                            endIndex++;
                        }

                        if (endIndex >= json.Length)
                        {
                            var message = $"Beginning quote encountered at index {currentIndex} without an unescaped ending quote";
                            throw new InvalidOperationException(message);
                        }

                        var propertyName = json.Slice(currentIndex + 1, endIndex);
                        
                        // Move to the next colon
                        currentIndex = endIndex + 1;
                        var colonFound = false;
                        while (currentIndex < json.Length && !colonFound)
                        {
                            switch (json[currentIndex])
                            {
                                case ':':
                                    colonFound = true;
                                    break;

                                case var whitespace when char.IsWhiteSpace(whitespace):
                                    break;
                                
                                default:
                                    var message = $"Unexpected character found at index {currentIndex} (expected colon)";
                                    throw new InvalidOperationException(message);
                            }

                            currentIndex++;
                        }

                        if (!colonFound)
                        {
                            throw new InvalidOperationException("No colon found for property");
                        }

                        bool propertyEndsOnBrace;
                        if (propertyName.Equals(nameof(TestClass.StringValue), StringComparison.OrdinalIgnoreCase))
                        {
                            ParseStringValueProperty(json, ref currentIndex, result, out propertyEndsOnBrace);
                        }
                        else if (propertyName.Equals(nameof(TestClass.IntValue), StringComparison.OrdinalIgnoreCase))
                        {
                            
                        }
                        else if (propertyName.Equals(nameof(TestClass.DoubleValue), StringComparison.OrdinalIgnoreCase))
                        {
                            
                        }
                        else if (propertyName.Equals(nameof(TestClass.BoolValue), StringComparison.OrdinalIgnoreCase))
                        {
                            
                        }
                        else if (propertyName.Equals(nameof(TestClass.NullableIntValue), StringComparison.OrdinalIgnoreCase))
                        {
                            
                        }
                        else if (propertyName.Equals(nameof(TestClass.DictionaryValue), StringComparison.OrdinalIgnoreCase))
                        {
                            
                        }
                        else
                        {
                            throw new InvalidOperationException($"No mapping for property '{propertyName.ToString()}'");
                        }

                        break;
                }
            }

            if (result != null && !endBraceEncountered)
            {
                throw new InvalidOperationException("End encountered without a closing brace");
            }

            return result;
        }

        private static void ParseStringValueProperty(ReadOnlySpan<char> json,
            ref int currentIndex,
            TestClass result,
            out bool endsOnEndingBrace)
        {
            var startIndex = (int?) null;
            var endIndex = (int?) null;
            var inEscapeMode = false;
            var isNull = false;
            while (currentIndex < json.Length && endIndex == null && !isNull)
            {
                switch (json[currentIndex])
                {
                    case '"' when startIndex == null:
                        startIndex = currentIndex;
                        break;
                    
                    case '"' when !inEscapeMode:
                        endIndex = currentIndex;
                        break;
                    
                    case var whitespace when char.IsWhiteSpace(whitespace):
                        break;
                    
                    case 'n' when startIndex != null &&
                                    currentIndex + 3 < json.Length &&
                                    json[currentIndex + 1] == 'u' &&
                                    json[currentIndex + 2] == 'l' &&
                                    json[currentIndex + 3] == 'l':
                        isNull = true;
                        currentIndex += 3;
                        break;
                    
                    case var _ when startIndex == null:
                        throw new InvalidOperationException($"Unexpected token at index {currentIndex}");
                    
                    case '\\' when !inEscapeMode:
                        inEscapeMode = true;
                        break;
                    
                    case var _ when inEscapeMode:
                        inEscapeMode = false;
                        break;
                }

                currentIndex++;
            }

            if (!isNull && startIndex == null)
            {
                throw new InvalidOperationException("No value found for the property");
            }

            if (!isNull && endIndex == null)
            {
                throw new InvalidOperationException("No ending quote was found");
            }

            result.StringValue = isNull 
                ? null 
                : json.Slice(startIndex.Value + 1, endIndex.Value - startIndex.Value - 1).ToString();
            
            while (currentIndex < json.Length)
            {
                switch (json[currentIndex])
                {
                    case var ch when ch == '}' || ch == ',':
                        endsOnEndingBrace = ch == '}';
                        currentIndex++;
                        return;

                    case var whitespace when char.IsWhiteSpace(whitespace):
                        break;
                    
                    default:
                        var message = $"Unexpected character at index {currentIndex}";
                        throw new InvalidOperationException(message);
                }

                currentIndex++;
            }

            throw new InvalidOperationException("Expected a comma or end brace, but reached the end of the string");
        }

        private static void ParseIntValueProperty(ReadOnlySpan<char> json,
            ref int currentIndex,
            TestClass result,
            out bool endsOnEndingBrace)
        {
            var startIndex = (int?) null;
            var endIndex = (int?) null;
            while (currentIndex < json.Length && endIndex == null)
            {
                switch (json[currentIndex])
                {
                    case var ch when char.IsDigit(ch):
                        startIndex ??= currentIndex;
                        break;

                    case var whitespace when char.IsWhiteSpace(whitespace) && startIndex == null:
                        break;
                    
                    case var ch when startIndex != null &&
                                     (ch == ',' || ch == '}' || char.IsWhiteSpace(ch)):
                        endIndex = currentIndex - 1;
                        break;
                    
                    default:
                        throw new InvalidOperationException($"Unexpected character at index {currentIndex}");
                }

                currentIndex++;
            }
            
            
        }
    }
}